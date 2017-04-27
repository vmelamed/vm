using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
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
        /// Contains references to all dumped objects to avoid infinite dumping due to cyclical references.
        /// </summary>
        readonly HashSet<DumpedObject> _dumpedObjects = new HashSet<DumpedObject>();
        #endregion

        static BindingFlags defaultPropertiesBindingFlags = BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly;

        /// <summary>
        /// The default binding flags determining which properties to be dumped
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags")]
        public static BindingFlags DefaultPropertiesBindingFlags
        {
            get { return defaultPropertiesBindingFlags; }
            set { defaultPropertiesBindingFlags = value; }
        }

        static BindingFlags defaultFieldsBindingFlags = BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly;

        /// <summary>
        /// The default binding flags determining which fields to be dumped
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags")]
        public static BindingFlags DefaultFieldsBindingFlags
        {
            get { return defaultFieldsBindingFlags; }
            set { defaultFieldsBindingFlags = value; }
        }

        #region Internal properties for access by the DumpState
        /// <summary>
        /// The binding flags determining which properties to be dumped
        /// </summary>
        internal BindingFlags PropertiesBindingFlags { get; }

        /// <summary>
        /// The binding flags determining which fields to be dumped
        /// </summary>
        internal BindingFlags FieldsBindingFlags { get; }

        /// <summary>
        /// The dump writer.
        /// </summary>
        internal TextWriter Writer { get; }

        /// <summary>
        /// Contains references to all dumped virtual properties to avoid dumping them more than once in the derived classes.
        /// </summary>
        internal readonly HashSet<DumpedProperty> DumpedVirtualProperties = new HashSet<DumpedProperty>();
        #endregion

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
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
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
                Writer = new DumpTextWriter((StringWriter)writer, maxDumpLength);
                _isDumpWriter = true;
            }
            else
                Writer = writer;

            _indentLevel           = indentLevel  >= 0 ? indentLevel : 0;
            _indentLength          = indentLength >= 2 ? indentLength : 2;
            PropertiesBindingFlags = propertiesBindingFlags;
            FieldsBindingFlags     = fieldsBindingFlags;
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
            var revertPermission = false;

            try
            {
                // assert the permission and dump
                reflectionPermission.Demand();
                revertPermission = true;

                // assert the permission and dump
                reflectionPermission.Assert();
                _maxDepth = int.MinValue;

                if (!Writer.DumpedBasicValue(value, dumpAttribute))
                {
                    Writer.Indent(_indentLevel, _indentLength)
                           .WriteLine();

                    DumpObjectOfNonBasicValue(value, dumpMetadata, dumpAttribute);
                }
            }
            catch (SecurityException)
            {
                Writer.WriteLine();
                Writer.Write(Resources.CallerDoesNotHavePermissionFormat, value?.ToString() ?? DumpUtilities.Null);
            }
            catch (Exception x)
            {
                Writer.WriteLine($"\n\nATTENTION:\nThe TextDumper threw an exception:\n{x.ToString()}");
            }
            finally
            {
                // revert the permission assert
                if (revertPermission)
                    CodeAccessPermission.RevertAssert();

                // restore the original indent
                Writer.Unindent(_indentLevel, _indentLength);

                // clear the dumped objects register
                _dumpedObjects.Clear();
                // clear the dumped properties register
                DumpedVirtualProperties.Clear();
            }

            return this;
        }
        #endregion

        internal void Indent()
        {
            Writer.Indent(++_indentLevel, _indentLength);
        }

        internal void Unindent()
        {
            Writer.Unindent(--_indentLevel, _indentLength);
        }

        #region Private methods
        internal void DumpObject(
            object obj,
            Type dumpMetadata = null,
            DumpAttribute dumpAttribute = null)
        {
            if (Writer.DumpedBasicValue(obj, dumpAttribute))
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
                Writer.Write(Resources.DumpReachedMaxDepth);
                return;
            }

            _maxDepth--;

            using (var state = new DumpState(this, obj, classDumpData))
                if (!DumpedAlready(state))     // the object has been dumped already (block circular and repeating references)
                {
                    // this object will be dumped below.
                    // Add it to the dumped objects so that if nested property refers back to it, it won't be dumped in an infinite recursive chain.
                    _dumpedObjects.Add(new DumpedObject(obj, type));

                    if (!DumpedCollectionObject(state, false))
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

            var baseDumpState = state.GetBaseTypeDumpState();

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
                state.DumpProperty();
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

                    state.DumpProperty();
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
                        state.DumpProperty();
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

            // Stop the recursion if: 
            return state.DumpedRootClass()                  ||  // at System.Object or at max depth
                   state.DumpedDefaultProperty()            ||  // classDump.Recurse is false
                   state.DumpedDelegate()                   ||  // at delegates
                   state.DumpedMemberInfo();                    // at MemberInfo values
        }

        bool DumpedCollectionObject(
            DumpState state,
            bool enumerateCustom)
        {
            Contract.Requires(state != null);

            var sequence = state.Instance as IEnumerable;

            if (sequence == null)
                return false;

            if (Writer.DumpedDictionary(
                        sequence,
                        state.ClassDumpData.DumpAttribute,
                        o => DumpObject(o),
                        Indent,
                        Unindent))
                return true;

            return Writer.DumpedSequence(
                        sequence,
                        state.ClassDumpData.DumpAttribute,
                        enumerateCustom,
                        o => DumpObject(o),
                        Indent,
                        Unindent);
        }

        bool DumpedAlready(DumpState state)
        {
            Contract.Requires(state!=null);

            var type = state.Instance.GetType();

            // stop the recursion at circular references
            if (!_dumpedObjects.Contains(new DumpedObject(state.Instance, type)))
                return false;

            Writer.Write(
                DumpFormat.CyclicalReference,
                type.GetTypeName(),
                type.Namespace,
                type.AssemblyQualifiedName);

            if (state.SetToDefault())
                state.DumpProperty();

            return true;
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
        public bool IsDisposed => Interlocked.CompareExchange(ref _disposed, 1, 1) == 1;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>Invokes the protected virtual <see cref="Dispose(bool)"/>.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "It is correct.")]
        public void Dispose()
        {
            // these will be called only if the instance is not disposed and is not in a process of disposing.
            Dispose(true);
            GC.SuppressFinalize(this);
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
            // if it is disposed or in a process of disposing - return.
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            if (disposing && _isDumpWriter)
                // if the writer wraps foreign StringWriter, it is smart enough not to dispose it
                Writer.Dispose();
        }
        #endregion

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [ContractInvariantMethod]
        void Invariant()
        {
            Contract.Invariant(Writer!=null, "The text writer cannot be null at any time.");
            Contract.Invariant(_indentLength>=0, "The length of the indent cannot be negative.");
            Contract.Invariant(_indentLevel>=0, "The the indent level cannot be negative.");
            Contract.Invariant(_dumpedObjects!=null);
            Contract.Invariant(DumpedVirtualProperties!=null);
        }
    }
}
