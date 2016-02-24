using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Threading;
using vm.Aspects.Diagnostics.DumpImplementation;
using vm.Aspects.Diagnostics.Properties;

namespace vm.Aspects.Diagnostics
{
    /// <summary>
    /// Class ObjectTextDumper. This class cannot be inherited. The main class which dumps the requested object.
    /// </summary>
    public sealed partial class ObjectTextDumper : IDisposable
    {
        #region Fields
        /// <summary>
        /// The dump writer.
        /// </summary>
        readonly TextWriter _writer;

        /// <summary>
        /// Flag that the current writer is actually our DumpWriter.
        /// </summary>
        readonly bool _isDumpWriter;

        /// <summary>
        /// The current indent.
        /// </summary>
        int _indentLevel;

        /// <summary>
        /// The number of spaces in a single indent.
        /// </summary>
        readonly int _indentLength;

        /// <summary>
        /// The current maximum depth of recursing into the aggregated objects. When it goes down to 0 - the recursion should stop.
        /// </summary>
        int _maxDepth = int.MinValue;

        /// <summary>
        /// The binding flags determining which properties to be dumped
        /// </summary>
        readonly BindingFlags _propertiesBindingFlags;

        /// <summary>
        /// The binding flags determining which fields to be dumped
        /// </summary>
        readonly BindingFlags _fieldsBindingFlags;

        /// <summary>
        /// Contains references to all dumped objects to avoid infinite dumping due to cyclical references.
        /// </summary>
        readonly HashSet<DumpedObject> _dumpedObjects = new HashSet<DumpedObject>();

        /// <summary>
        /// Contains references to all dumped virtual properties to avoid dumping them more than once in the derived classes.
        /// </summary>
        readonly HashSet<DumpedProperty> _dumpedVirtualProperties = new HashSet<DumpedProperty>();

        /// <summary>
        /// Matches the name space of the types within System
        /// </summary>
        static readonly Regex _systemNameSpace = new Regex(Resources.RegexSystemNamespace, RegexOptions.Compiled);

        /// <summary>
        /// Matches a type name with hexadecimal suffix.
        /// </summary>
        static readonly Regex _hexadecimalSuffix = new Regex(@"_[0-9A-F]{64}", RegexOptions.Compiled);
        #endregion

        static BindingFlags defaultPropertiesBindingFlags = BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly;

        /// <summary>
        /// The default binding flags determining which properties to be dumped
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId="Flags")]
        public static BindingFlags DefaultPropertiesBindingFlags
        {
            get { return defaultPropertiesBindingFlags; }
            set { defaultPropertiesBindingFlags = value; }
        }

        static BindingFlags defaultFieldsBindingFlags = BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly;

        /// <summary>
        /// The default binding flags determining which fields to be dumped
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId="Flags")]
        public static BindingFlags DefaultFieldsBindingFlags
        {
            get { return defaultFieldsBindingFlags; }
            set { defaultFieldsBindingFlags = value; }
        }

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectTextDumper" /> class with a text writer where to dump the object and initial indent
        /// level.
        /// </summary>
        /// <param name="writer">The text writer where to dump the object to.</param>
        /// <param name="indentLevel">The initial indent level.</param>
        /// <param name="indentLength">The length of the indent.</param>
        /// <param name="maxDumpLength">Maximum length of the dump text.</param>
        /// <param name="propertiesBindingFlags">The binding flags of the properties.</param>
        /// <param name="fieldsBindingFlags">The binding flags of the fields.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="writer" /> is <c>null</c>.</exception>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId="Flags")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="0")]
        public ObjectTextDumper(
            TextWriter writer,
            int indentLevel = 0,
            int indentLength = 2,
            int maxDumpLength = DumpTextWriter.DefaultMaxLength,
            BindingFlags propertiesBindingFlags = BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly,
            BindingFlags fieldsBindingFlags = BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly)
        {
            Contract.Requires<ArgumentNullException>(writer != null, nameof(writer));

            if (writer.GetType() == typeof(StringWriter))
            {
                // wrap the writer in DumpTextWriter
                _writer = new DumpTextWriter((StringWriter)writer, maxDumpLength);
                _isDumpWriter = true;
            }
            else
                _writer = writer;

            _indentLevel            = indentLevel > 0 ? indentLevel : 0;
            _indentLength           = indentLength > 0 ? indentLength : 2;
            _propertiesBindingFlags = propertiesBindingFlags;
            _fieldsBindingFlags     = fieldsBindingFlags;
        }
        #endregion

        #region Public method/s
        /// <summary>
        /// Dumps the specified object in a text form to this object's <see cref="TextWriter"/> instance.
        /// </summary>
        /// <param name="value">
        /// The object to be dumped.
        /// </param>
        /// <param name="dumpMetadata">
        /// Optional metadata class to use to extract the dump parameters, options and settings. If not specified, the dump metadata will be
        /// extracted from the <see cref="MetadataTypeAttribute"/> attribute applied to <paramref name="value"/>'s class if specified otherwise from 
        /// the attributes applied within the class itself.
        /// </param>
        /// <param name="dumpAttribute">
        /// An explicit dump attribute to be applied at a class level. If not specified the <see cref="MetadataTypeAttribute"/> attribute applied to 
        /// <paramref name="value"/>'s class or <see cref="DumpAttribute.Default"/> will be assumed.
        /// </param>
        /// <returns>
        /// The <paramref name="value"/> parameter.
        /// </returns>
        public ObjectTextDumper Dump(
            object value,
            Type dumpMetadata = null,
            DumpAttribute dumpAttribute = null)
        {
            Contract.Ensures(Contract.OldValue(_indentLevel) == _indentLevel, "The indent level was not preserved.");

            var reflectionPermission = new ReflectionPermission(PermissionState.Unrestricted);
            var callerHasPermission = true;

            try
            {
                // make sure all callers on the stack have the permission to reflect on the object
                reflectionPermission.Demand();
            }
            catch (SecurityException)
            {
                callerHasPermission = false;
            }

            try
            {
                if (!callerHasPermission)
                {
                    _writer.WriteLine();
                    DumpReflectionNotPermitted(value);
                    return this;
                }

                // assert the permission and dump
                reflectionPermission.Assert();
                _maxDepth = int.MinValue;

                if (!DumpedBasicValue(value, dumpAttribute))
                {
                    _writer.Indent(_indentLevel, _indentLength)
                           .WriteLine();

                    DumpObjectOfNonBasicValue(value, dumpMetadata, dumpAttribute);
                }

                return this;
            }
            finally
            {
                // revert the permission assert
                if (callerHasPermission)
                    CodeAccessPermission.RevertAssert();

                // restore the original indent
                _writer.Unindent(_indentLevel, _indentLength);

                // clear the dumped objects register
                _dumpedObjects.Clear();
                // clear the dumped properties register
                _dumpedVirtualProperties.Clear();
            }
        }
        #endregion

        #region Private methods
        void Indent()
        {
            _writer.Indent(++_indentLevel, _indentLength);
        }

        void Unindent()
        {
            _writer.Unindent(--_indentLevel, _indentLength);
        }

        void DumpReflectionNotPermitted(
            object v)
        {
            // the caller does not have the permission - just call ToString() on the object.
            _writer.Write(
                Resources.CallerDoesNotHavePermissionFormat,
                v!=null ? v.ToString() : DumpUtilities.Null);
        }

        void DumpObject(
            object obj,
            Type dumpMetadata = null,
            DumpAttribute dumpAttribute = null)
        {
            if (DumpedBasicValue(obj, dumpAttribute))
                return;

            DumpObjectOfNonBasicValue(obj, dumpMetadata, dumpAttribute);
        }

        void DumpObjectOfNonBasicValue(
            object obj,
            Type dumpMetadata,
            DumpAttribute dumpAttribute)
        {
            Contract.Requires(obj != null);

            var type = obj.GetType();
            ClassDumpData classDumpData;

            if (dumpMetadata==null)
            {
                classDumpData = ClassMetadataResolver.GetClassDumpData(type);
                if (dumpAttribute != null)
                    classDumpData.DumpAttribute = dumpAttribute;
            }
            else
                classDumpData = new ClassDumpData(dumpMetadata, dumpAttribute);

            if (_maxDepth == int.MinValue)
                _maxDepth = classDumpData.DumpAttribute.MaxDepth;

            if (_maxDepth < 0)
            {
                _writer.Write(Resources.DumpReachedMaxDepth);
                return;
            }

            _maxDepth--;

            using (var state = new DumpState(
                                        obj,
                                        type,
                                        classDumpData,
                                        dumpAttribute,
                                        _propertiesBindingFlags,
                                        _fieldsBindingFlags))
                if (!DumpedAlready(state))     // the object has been dumped already (block circular and repeating references)
                {
                    // this object will be dumped below.
                    // Add it to the dumped objects so that if nested property refers back to it, it won't be dumped in an infinite recursive chain.
                    _dumpedObjects.Add(new DumpedObject(obj, type));

                    if (!DumpedCollectionObject(state))
                    {
                        Stack<DumpState> statesWithRemainingProperties = new Stack<DumpState>();
                        Queue<DumpState> statesWithTailProperties = new Queue<DumpState>();

                        // dump all properties with non-negative order in class inheritance descending order (base classes' properties first)
                        if (!DumpTopProperties(state, statesWithRemainingProperties))
                        {
                            // dump all properties with negative order in class ascending order (derived classes' properties first)
                            DumpRemainingProperties(statesWithRemainingProperties, statesWithTailProperties);

                            // dump all properties with Order=int.MinValue in ascending order (derived classes' properties first)
                            DumpTailProperties(statesWithTailProperties);

                            // if the object implements IEnumerable and the state allows it - dump the elements.
                            DumpedCollectionObject(state, true);

                            Unindent();
                        }
                    }
                }

            _maxDepth++;
        }

        /// <summary>
        /// Dumps the properties with non-negative dump order.
        /// </summary>
        /// <param name="state">The current dump state.</param>
        /// <param name="statesWithRemainingProperties">The stack containing the states which have remaining properties.</param>
        /// <returns><c>true</c> if this is a dump tree leaf object, the current dump item is one of:
        /// <list type="bullet">
        /// <item><c>typeof(System.Object)</c></item><item><see cref="MemberInfo" /></item>
        /// <item>a delegate</item>
        /// <item>should not be dumped marked with <c>DumpAttribute(false)</c></item>
        /// </list>
        /// ; otherwise <c>false</c>.</returns>
        bool DumpTopProperties(
            DumpState state,
            Stack<DumpState> statesWithRemainingProperties)
        {
            if (IsAtDumpTreeLeaf(state))
                return true;

            var baseDumpState = new DumpState(
                                         state.Instance,
                                         state.Type.BaseType,
                                         ClassMetadataResolver.GetClassDumpData(state.Type.BaseType),
                                         state.InstanceDumpAttribute,
                                         _propertiesBindingFlags,
                                         _fieldsBindingFlags);

            // 1. dump the properties of the base class with non-negative order 
            DumpTopProperties(baseDumpState, statesWithRemainingProperties);

            // 2. dump the non-negative order properties of the current class
            while (state.MoveNext())
            {
                if (state.CurrentPropertyDumpAttribute.Order < 0)
                {
                    // if we reached a negative order property - set the flag that there are more to be dumped and suspend the dumping at this level
                    statesWithRemainingProperties.Push(state);
                    return false;
                }
                DumpProperty(state);
            }

            state.Dispose();
            return false;
        }

        /// <summary>
        /// Dumps the properties with negative dump order.
        /// </summary>
        /// <param name="statesWithRemainingProperties">The stack containing the states which have remaining properties.</param>
        /// <param name="statesWithTailProperties">The queue containing the states which have tail properties.</param>
        void DumpRemainingProperties(
            Stack<DumpState> statesWithRemainingProperties,
            Queue<DumpState> statesWithTailProperties)
        {
            Contract.Requires(statesWithRemainingProperties != null);

            while (statesWithRemainingProperties.Any())
            {
                var state = statesWithRemainingProperties.Pop();

                do
                {
                    if (state.CurrentPropertyDumpAttribute.Order == int.MinValue)
                    {
                        statesWithTailProperties.Enqueue(state);
                        break;
                    }

                    DumpProperty(state);
                }
                while (state.MoveNext());

                state.Dispose();
            }
        }

        /// <summary>
        /// Dumps the properties with order int.MinValue
        /// </summary>
        /// <param name="statesWithTailProperties">The bottom properties collection (usually a queue).</param>
        void DumpTailProperties(IEnumerable<DumpState> statesWithTailProperties)
        {
            Contract.Requires(statesWithTailProperties != null);

            foreach (var state in statesWithTailProperties)
                using (state)
                    do
                        DumpProperty(state);
                    while (state.MoveNext());
        }

        /// <summary>
        /// The dumping traverses depth-first a dump tree consisting of the object's properties its base classes' properties and the properties' 
        /// properties etc. This method determines if the recursion reached a leaf in the dump tree and that it should stop drilling down and return to 
        /// dump other branches of the dump tree. Recursion stops when:
        /// <list type="bullet">
        /// <item>
        /// The current examined type is <see cref="object"/>. The method dumps the object contained in <paramref name="state"/>-s type name.
        /// </item>
        /// <item>
        /// The current examined base class has <see cref="DumpAttribute"/> with property <see cref="DumpAttribute.RecurseDump"/> set to <see cref="ShouldDump.Skip"/>. 
        /// If the attribute also defines the property <see cref="DumpAttribute.DefaultProperty"/> it will dump that property only as a representing
        /// property of the entire class.
        /// </item>
        /// <item>
        /// The examined object is a delegate. The method will dump the delegate type.
        /// </item>
        /// <item>
        /// The method determines that the current object has already been dumped (discovers circular reference). The method will dump a short 
        /// reference text.
        /// </item>
        /// </list>
        /// The current base class has class dump attribute with  examined is System.Object
        /// </summary>
        /// <param name="state">The current dump state.</param>
        /// <returns><c>true</c> if the recursion should stop; otherwise <c>false</c>.</returns>
        bool IsAtDumpTreeLeaf(DumpState state)
        {
            Contract.Requires(state != null);

            return DumpedRootClass(state)                  ||     // stop the recursion at System.Object or at max depth
                   DumpedDefaultProperty(state)            ||     // stop if classDump.Recurse is false
                   DumpedDelegate(state)                   ||     // stop at delegates
                   DumpedMemberInfoValue(state.Instance);         // stop at MemberInfo
        }

        bool DumpedCollectionObject(
            DumpState state,
            bool enumerateCustom = false)
        {
            Contract.Requires(state != null);

            var sequence = state.Instance as IEnumerable;

            if (sequence == null)
                return false;

            return DumpedSequenceObject(
                        sequence,
                        state.ClassDumpData.DumpAttribute,
                        enumerateCustom);
        }

        void DumpProperty(
            DumpState state)
        {
            Contract.Requires(state!=null);

            // should we dump it at all?
            if (!state.CurrentProperty.CanRead()  ||  
                state.CurrentPropertyDumpAttribute.Skip == ShouldDump.Skip)
                return;

            var isVirtual = state.CurrentProperty.IsVirtual().GetValueOrDefault();

            if (isVirtual)
            {
                // don't dump virtual properties that were already dumped
                if (_dumpedVirtualProperties.Contains(new DumpedProperty(state.Instance, state.CurrentProperty.Name)))
                    return;

                // or will be dumped by the base classes
                if (state.CurrentPropertyDumpAttribute.IsDefaultAttribute() &&
                    PropertyDumpResolver.PropertyHasNonDefaultDumpAttribute(state.CurrentProperty))
                    return;
            }

            // can't dump indexers
            var pi = state.CurrentProperty as PropertyInfo;
            var fi = state.CurrentProperty as FieldInfo;

            if (pi != null  &&
                pi.GetIndexParameters().Length > 0)
                return;

            Type type = pi != null
                            ? pi.PropertyType
                            : fi.FieldType;
            object value = null;

            try
            {
                value = pi != null 
                            ? pi.GetValue(state.Instance, null)
                            : fi.GetValue(state.Instance);
            }
            catch (TargetInvocationException)
            {
                // this should not happen but...
                value = null;
            }

            // should we dump a null value of the current property
            if (value == null  &&  
                (state.CurrentPropertyDumpAttribute.DumpNullValues==ShouldDump.Skip  ||
                 state.CurrentPropertyDumpAttribute.DumpNullValues==ShouldDump.Default && state.DumpNullValues==ShouldDump.Skip))
                return;

            if (isVirtual)
                _dumpedVirtualProperties.Add(new DumpedProperty(state.Instance, state.CurrentProperty.Name));

            // write the property header
            _writer.WriteLine();
            _writer.Write(
                state.CurrentPropertyDumpAttribute.LabelFormat,
                state.CurrentProperty.Name);

            // dump the property value using caller's customization (ValueFormat, DumpClass, DumpMethod) if any.
            if (DumpedPropertyCustom(state, value, type))
                return;

            // dump primitive values, including string and DateTime
            if (DumpedBasicValue(value, state.CurrentPropertyDumpAttribute))
                return;

            Contract.Assume(value!=null, "The null value should have been dumped by now.");

            if (DumpedDelegate(value))
                return;

            // dump a member which is a reflection object
            if (DumpedMemberInfoValue(value))
                return;

            // dump sequences (array, collection, etc. IEnumerable)
            if (DumpedSequenceProperty(state, value))
                return;

            // dump a property representing an associated class or struct object
            DumpObjectOfNonBasicValue(
                value,
                null,
                state.CurrentPropertyDumpAttribute.IsDefaultAttribute() 
                    ? null
                    : state.CurrentPropertyDumpAttribute);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification="It's OK.")]
        bool DumpedPropertyCustom(
            DumpState state,
            object value,
            Type type)
        {
            Contract.Requires<ArgumentNullException>(state != null, nameof(state));

            if (value==null)
                return false;

            // did they specify DumpAttribute.ValueFormat="ToString"?
            if (state.CurrentPropertyDumpAttribute.ValueFormat == Resources.ValueFormatToString)
            {
                _writer.Write("{0}", value.ToString());
                return true;
            }

            // did they specify DumpAttribute.DumpMethod?
            var dumpMethodName = state.CurrentPropertyDumpAttribute.DumpMethod;
            var dumpClass = state.CurrentPropertyDumpAttribute.DumpClass;

            if (dumpClass==null  &&  string.IsNullOrWhiteSpace(dumpMethodName))
                return false;

            if (string.IsNullOrWhiteSpace(dumpMethodName))
                dumpMethodName = nameof(Dump);

            MethodInfo dumpMethod = null;   // best match
            MethodInfo dumpMethod2 = null;  // second best

            // try external class if specified
            if (dumpClass != null)
            {
                foreach (var mi in dumpClass.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                                            .Where(mi => mi.Name == dumpMethodName  &&  
                                                            mi.ReturnType == typeof(string)  &&  
                                                            mi.GetParameters().Count() == 1))
                {
                    if (mi.GetParameters()[0].ParameterType == type)
                    {
                        // exact match
                        dumpMethod = mi;
                        break;
                    }

                    if (mi.GetParameters()[0].ParameterType.IsAssignableFrom(type))
                        // candidate
                        dumpMethod = mi;
                }

                if (dumpMethod != null)
                    _writer.Write("{0}", (string)dumpMethod.Invoke(null, new object[] { value }));
                else
                    _writer.Write("*** Could not find a public, static, method {0}, with return type of System.String, "+
                                    "with a single parameter of type {1} in the class {2}.",
                                    dumpMethodName,
                                    type.FullName,
                                    dumpClass.FullName);

                return true;
            }

            // try the property's class or base class
            foreach (var mi in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                                    .Where(mi => mi.Name == dumpMethodName  &&  
                                                mi.ReturnType == typeof(string))
                                    .Union(type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                                                .Where(mi => mi.Name == dumpMethodName  &&  
                                                            mi.ReturnType == typeof(string)  &&  
                                                            mi.GetParameters().Count() == 1)))
            {
                // found an instance method
                if (!mi.IsStatic && mi.GetParameters().Count() == 0)
                {
                    dumpMethod = mi;
                    break;
                }

                if (mi.IsStatic)
                {
                    if (mi.GetParameters()[0].ParameterType == type)
                        dumpMethod = mi;
                    else
                        if (mi.GetParameters()[0].ParameterType.IsAssignableFrom(type))
                            dumpMethod2 = mi;
                }
            }

            if (dumpMethod != null)
            {
                if (dumpMethod.IsStatic)
                    _writer.Write("{0}", (string)dumpMethod.Invoke(null, new object[] { value }));
                else
                    _writer.Write("{0}", (string)dumpMethod.Invoke(value, null));

                return true;
            }

            if (dumpMethod2 != null)
            {
                if (dumpMethod2.IsStatic)
                    _writer.Write("{0}", (string)dumpMethod2.Invoke(null, new object[] { value }));
                else
                    _writer.Write("{0}", (string)dumpMethod2.Invoke(value, null));

                return true;
            }

            _writer.Write("*** Could not find a public instance method with name {0} and no parameters or "+
                          "static method with a single parameter of type {1}, "+
                          "with return type of System.String in the class {1}.",
                          dumpMethodName,
                          type.FullName);

            return true;
        }

        bool DumpedAlready(DumpState state)
        {
            Contract.Requires(state!=null);

            var type = state.Instance.GetType();

            // stop the recursion at circular references
            if (!_dumpedObjects.Contains(new DumpedObject(state.Instance, type)))
                return false;

            _writer.Write(
                DumpFormat.CyclicalReference,
                GetTypeName(type),
                type.Namespace,
                type.AssemblyQualifiedName);

            if (state.SetToDefault())
                DumpProperty(state);

            return true;
        }

        bool DumpedRootClass(DumpState state)
        {
            Contract.Requires(state != null);

            if (state.Type!=typeof(object))
                return false;

            var type = state.Instance.GetType();

            _writer.Write(
                DumpFormat.Type,
                GetTypeName(type),
                type.Namespace,
                type.AssemblyQualifiedName);
            Indent();

            return true;
        }

        bool DumpedDelegate(DumpState state)
        {
            Contract.Requires(state != null);

            if (!typeof(Delegate).IsAssignableFrom(state.Type))
                return false;

            // it will be dumped at the descendant level
            if (state.Type == typeof(MulticastDelegate) ||
                state.Type == typeof(Delegate))
                return true;

            return DumpedDelegate(state.Instance);
        }

        bool DumpedDelegate(object value)
        {
            var d = value as Delegate;

            if (d == null)
                return false;   // if it is null delegate, it will be dumped as basic value <null>

            _writer.Write(
                DumpFormat.Delegate,
                d.Method.DeclaringType!=null ? d.Method.DeclaringType.Name : string.Empty,
                d.Method.DeclaringType!=null ? d.Method.DeclaringType.Namespace : string.Empty,
                d.Method.DeclaringType!=null ? d.Method.DeclaringType.AssemblyQualifiedName : string.Empty,
                d.Method.Name,
                d.Target==null 
                    ? Resources.ClassMethodDesignator 
                    : Resources.InstanceMethodDesignator);

            return true;
        }

        bool DumpedSequenceProperty(
            DumpState state,
            object value)
        {
            var sequence = value as IEnumerable;

            if (sequence == null)
                return false;

            return DumpedSequenceObject(sequence, state.CurrentPropertyDumpAttribute);
        }

        bool DumpedSequenceObject(
            IEnumerable sequence,
            DumpAttribute dumpAttribute,
            bool enumerateCustom = false)
        {
            Contract.Requires<ArgumentNullException>(dumpAttribute != null, nameof(dumpAttribute));
            Contract.Requires(sequence != null);

            var sequenceType = sequence.GetType();

            if (!sequenceType.IsArray &&
                !_systemNameSpace.IsMatch(sequenceType.Namespace))
            {
                if (!enumerateCustom)
                    return false;

                if (dumpAttribute.Enumerate != ShouldDump.Dump)
                    return false;

                _writer.WriteLine();
            }

            var collection = sequence as ICollection;
            var elementsType = sequenceType.IsArray
                                    ? new Type[] { sequenceType.GetElementType() }
                                    : sequenceType.IsGenericType
                                        ? sequenceType.GetGenericArguments()
                                        : new Type[] { typeof(object) };

            _writer.Write(
                DumpFormat.SequenceTypeName,
                sequenceType.IsArray
                        ? GetTypeName(elementsType[0])
                        : GetTypeName(sequenceType),
                collection != null
                        ? collection.Count.ToString(CultureInfo.InvariantCulture)
                        : string.Empty);

            // how many items to dump max?
            var max = dumpAttribute.MaxLength;

            if (max < 0)
                max = int.MaxValue;
            else
                if (max == 0)        // limit sequences of primitive types (can be very big)
                    max = DumpAttribute.DefaultMaxElements;

            if (sequenceType == typeof(byte[]))
            {
                // dump no more than max elements from the sequence:
                var array = (byte[])sequence;
                var length = Math.Min(max, array.Length);

                _writer.Write(BitConverter.ToString(array, 0, length));
                if (length < array.Length)
                    _writer.Write(DumpFormat.SequenceDumpTruncated, max);

                return true;
            }

            _writer.Write(
                    DumpFormat.SequenceType,
                    GetTypeName(sequenceType),
                    sequenceType.Namespace,
                    sequenceType.AssemblyQualifiedName);

            // stop the recursion if dump.Recurse is false
            if (dumpAttribute.RecurseDump==ShouldDump.Skip)
                return true;

            var n = 0;

            Indent();

            foreach (var item in sequence)
            {
                _writer.WriteLine();
                if (n++ >= max)
                {
                    _writer.Write(DumpFormat.SequenceDumpTruncated, max);
                    break;
                }
                DumpObject(item);
            }

            Unindent();

            return true;
        }

        bool DumpedDefaultProperty(
            DumpState state)
        {
            Contract.Requires(state != null);
            Contract.Requires(state.RecurseDump != ShouldDump.Default);

            if (state.RecurseDump == ShouldDump.Dump)
                return false;

            var type = state.Instance.GetType();

            _writer.Write(
                        DumpFormat.Type,
                        GetTypeName(type),
                        type.Namespace,
                        type.AssemblyQualifiedName);

            Indent();

            if (state.SetToDefault())
                DumpProperty(state);

            Unindent();

            return true;
        }

        /// <summary>
        /// Gets the name of a type. In case the type is a EF dynamic proxy it will return only the first portion of the name, e.g.
        /// from the name "SomeTypeName_CFFF21E2EAC773F63711A0F93BE77F1CBC891DE8F0E5FFC46E7C4BB2E4BCC8D3" it will return only "SomeTypeName"
        /// </summary>
        /// <param name="type">The object which type name needs to be retrieved.</param>
        /// <returns>The type name.</returns>
        static string GetTypeName(
            Type type)
        {
            Contract.Requires(type != null);

            string typeName = type.Name;

            if (typeName.Length > 65 && 
                _hexadecimalSuffix.IsMatch(typeName.Substring(typeName.Length - 65-1)))
                typeName = type.BaseType.Name.Substring(0, typeName.Length-65);

            if (type.IsGenericType)
            {
                var tickIndex = typeName.IndexOf('`');

                if (tickIndex > -1)
                    typeName = typeName.Substring(0, tickIndex);

                typeName = string.Format(
                                CultureInfo.InvariantCulture,
                                "{0}<{1}>",
                                typeName,
                                string.Join(Resources.GenericParamSeparator, type.GetGenericArguments().Select(t => GetTypeName(t))));
            }

            if (type.IsArray)
            {
                var bracketIndex = typeName.IndexOf('[');

                if (bracketIndex > -1)
                    typeName = typeName.Substring(0, bracketIndex);
            }

            return typeName;
        }
        #endregion

        #region IDisposable pattern implementation
        /// <summary>
        /// The flag is being set when the object gets disposed.
        /// </summary>
        /// <value>0 - if the object is not disposed yet, any other value - the object is already disposed.</value>
        /// <remarks>
        /// Do not test or manipulate this flag outside of the property <see cref="IsDisposed"/> or the method <see cref="Dispose()"/>.
        /// The type of this field is Int32 so that it can be easily passed to the members of the class <see cref="Interlocked"/>.
        /// </remarks>
        int _disposed;

        /// <summary>
        /// Returns <c>true</c> if the object has already been disposed, otherwise <c>false</c>.
        /// </summary>
        public bool IsDisposed => !Interlocked.Equals(_disposed, 0);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>Invokes the protected virtual <see cref="Dispose(bool)"/>.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification="It is correct.")]
        public void Dispose()
        {
            // if it is disposed or in a process of disposing - return.
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            // these will be called only if the instance is not disposed and is not in a process of disposing.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Allows the object to attempt to free resources and perform other cleanup operations before the Object is reclaimed by garbage collection. 
        /// </summary>
        /// <remarks>Invokes the protected virtual <see cref="Dispose(bool)"/>.</remarks>
        ~ObjectTextDumper()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs the actual job of disposing the object.
        /// </summary>
        /// <param name="disposing">
        /// Passes the information whether this method is called by <see cref="Dispose()"/> (explicitly or
        /// implicitly at the end of a <c>using</c> statement), or by the finalizer of <see cref="ObjectTextDumper"/>.
        /// </param>
        /// <remarks>
        /// If the method is called with <paramref name="disposing"/><c>==true</c>, i.e. from <see cref="Dispose()"/>, it will try to release all managed resources 
        /// (usually aggregated objects which implement <see cref="IDisposable"/> as well) and then it will release all unmanaged resources if any.
        /// If the parameter is <c>false</c> then the method will only try to release the unmanaged resources.
        /// </remarks>
        void Dispose(bool disposing)
        {
            if (disposing && _isDumpWriter)
                // if the writer wraps foreign StringWriter, it is smart enough not to dispose it
                _writer.Dispose();
        }
        #endregion

        [ContractInvariantMethod]
        void Invariant()
        {
            Contract.Invariant(_writer!=null, "The text writer cannot be null at any time.");
            Contract.Invariant(_indentLength>=0, "The length of the indent cannot be negative.");
            Contract.Invariant(_indentLevel>=0, "The the indent level cannot be negative.");
            Contract.Invariant(_dumpedObjects!=null);
            Contract.Invariant(_dumpedVirtualProperties!=null);
        }
    }
}
