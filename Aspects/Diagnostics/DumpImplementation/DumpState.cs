using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using vm.Aspects.Diagnostics.Properties;

namespace vm.Aspects.Diagnostics.DumpImplementation
{
    class DumpState : IEnumerator<MemberInfo>
    {
        #region fileds
        ObjectTextDumper _dumper;
        bool _isTopLevelClass;
        /// <summary>
        /// The current field or property's property info object
        /// </summary>
        MemberInfo _currentMemberInfo;
        /// <summary>
        /// The current field or property's dump attribute
        /// </summary>
        DumpAttribute _currentDumpAttribute;
        #endregion

        public DumpScript DumpScript { get; }

        public DumpState(
            ObjectTextDumper dumper,
            object instance,
            ClassDumpData classDumpData,
            bool buildScript)
            : this(dumper, instance, instance.GetType(), classDumpData, classDumpData.DumpAttribute, null, true)
        {
            Contract.Requires<ArgumentNullException>(dumper        != null, nameof(dumper));
            Contract.Requires<ArgumentNullException>(instance      != null, nameof(instance));
            Contract.Requires<ArgumentNullException>(classDumpData != null, nameof(classDumpData));

            // let's not create script if we don't need it or are not doing anything here.
            if (buildScript  &&  ObjectType != typeof(object))
                DumpScript = new DumpScript(dumper, instance.GetType());

            DecrementMaxDepth();
        }

        private DumpState(
            ObjectTextDumper dumper,
            object instance,
            Type type,
            ClassDumpData classDumpData,
            DumpAttribute instanceDumpAttribute,
            DumpScript dumpScript,
            bool isTopLevelClass)
        {
            Contract.Requires<ArgumentNullException>(instance              != null, nameof(instance));
            Contract.Requires<ArgumentNullException>(type                  != null, nameof(type));
            Contract.Requires<ArgumentNullException>(classDumpData         != null, nameof(classDumpData));
            Contract.Requires<ArgumentNullException>(instanceDumpAttribute != null, nameof(instanceDumpAttribute));
            Contract.Requires<ArgumentNullException>(dumper                != null, nameof(dumper));

            _dumper               = dumper;
            Instance              = instance;
            ObjectType            = type;
            ClassDumpData         = classDumpData;
            InstanceDumpAttribute = instanceDumpAttribute;
            DumpScript            = dumpScript;
            _isTopLevelClass      = isTopLevelClass;

            if (_isTopLevelClass)
            {
                var defaultProperty = DefaultProperty;

                if (!defaultProperty.IsNullOrWhiteSpace())
                {
                    var pi = ObjectType.GetProperty(defaultProperty);

                    if (pi != null)
                        Enumerator = (new MemberInfo[] { pi }).AsEnumerable().GetEnumerator();
                    else
                        Enumerator = (new MemberInfo[] { }).AsEnumerable().GetEnumerator();

                    return;
                }
            }

            Enumerator            = ObjectType
                                        .GetProperties(_dumper.PropertiesBindingFlags)
                                        .Union<MemberInfo>(ObjectType.GetFields(_dumper.FieldsBindingFlags))
                                        .Where(mi => !mi.Name.StartsWith("<", StringComparison.Ordinal))
                                        .OrderBy(p => p, ServiceResolver.Default
                                                                            .GetInstance<IMemberInfoComparer>()
                                                                            .SetMetadata(ClassDumpData.Metadata))
                                        .GetEnumerator();
        }

        public DumpState GetBaseTypeDumpState()
            => new DumpState(
                    _dumper,
                    Instance,
                    ObjectType.BaseType,
                    ClassMetadataResolver.GetClassDumpData(ObjectType.BaseType),
                    InstanceDumpAttribute,
                    DumpScript,
                    false);

        /// <summary>
        /// Gets or sets the currently dumped instance.
        /// </summary>
        public object Instance { get; }

        /// <summary>
        /// Gets or sets the current type (maybe base type) of the instance being dumped.
        /// </summary>
        public Type ObjectType { get; private set; }

        /// <summary>
        /// Gets or sets the dump attribute applied to the instance .
        /// </summary>
        /// <value>
        /// The instance dump attribute.
        /// </value>
        public DumpAttribute InstanceDumpAttribute { get; private set; }

        /// <summary>
        /// Gets or sets the class dump data pair - the metadata type and the class dump attribute.
        /// </summary>
        public ClassDumpData ClassDumpData { get; }

        /// <summary>
        /// Gets the current property being dumped.
        /// </summary>
        public MemberInfo CurrentProperty => Current;

        /// <summary>
        /// Gets the property dump attribute applied to the current property being dumped.
        /// </summary>
        public DumpAttribute CurrentPropertyDumpAttribute => _currentDumpAttribute;

        #region Calculated properties
        /// <summary>
        /// Calculates whether null property values of the current instance should be dumped.
        /// </summary>
        public ShouldDump DumpNullValues => ClassDumpData.DumpNullValues(InstanceDumpAttribute);

        /// <summary>
        /// Calculates whether to dump recursively the current instance.
        /// </summary>
        /// <value>This property never returns <see cref="ShouldDump.Default"/>.</value>
        public ShouldDump RecurseDump => ClassDumpData.RecurseDump(InstanceDumpAttribute);

        /// <summary>
        /// Gets the representative property of the current type that should not be dumped recursively.
        /// </summary>
        public string DefaultProperty => ClassDumpData.DefaultProperty(InstanceDumpAttribute);
        #endregion

        #region IEnumerator<MemberInfo> Members
        public IEnumerator<MemberInfo> Enumerator { get; private set; }

        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        /// <returns>The element in the collection at the current position of the enumerator.</returns>
        public MemberInfo Current => _currentMemberInfo;
        #endregion

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_isTopLevelClass)
                IncrementMaxDepth();

            Enumerator.Dispose();
        }

        #endregion

        #region IEnumerator Members
        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        /// <returns>The element in the collection at the current position of the enumerator.</returns>
        object IEnumerator.Current => Current;

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns>
        /// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
        /// </returns>
        public bool MoveNext()
        {
            if (!Enumerator.MoveNext())
            {
                _currentMemberInfo    = null;
                _currentDumpAttribute = null;
                return false;
            }

            _currentMemberInfo    = Enumerator.Current;
            _currentDumpAttribute = PropertyDumpResolver.GetPropertyDumpAttribute(_currentMemberInfo, ClassDumpData.Metadata);
            return true;
        }

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        public void Reset()
        {
            Enumerator.Reset();
            _currentMemberInfo    = null;
            _currentDumpAttribute = null;
        }

        #endregion

        public bool DumpedAlready()
        {
            var type = Instance.GetType();

            // stop the recursion at circular references
            if (!_dumper.DumpedObjects.Contains(new DumpedObject(Instance, type)))
                return false;

            DumpSeenAlready();

            return true;
        }

        public void DumpSeenAlready()
        {
            var type = Instance.GetType();

            _dumper.Writer.Write(
                DumpFormat.CyclicalReference,
                type.GetTypeName(),
                type.Namespace,
                type.AssemblyQualifiedName);

            DumpScript?.DumpSeenAlready();
        }

        public void DumpType()
        {
            var type = Instance.GetType();

            _dumper.Writer.Write(
                DumpFormat.Type,
                type.GetTypeName(),
                type.Namespace,
                type.AssemblyQualifiedName);

            DumpScript?.DumpType();
        }

        internal void IncrementMaxDepth()
        {
            _dumper._maxDepth++;
            DumpScript?.AddIncrementMaxDepth();
        }

        internal void DecrementMaxDepth()
        {
            _dumper._maxDepth--;
            DumpScript?.AddDecrementMaxDepth();
        }

        internal void Indent()
        {
            _dumper.Indent();
            DumpScript?.AddIndent();
        }

        internal void Unindent()
        {
            _dumper.Unindent();
            DumpScript?.AddUnindent();
        }

        /// <summary>
        /// The dumping traverses depth-first a dump tree consisting of the object's properties its base classes' properties and the properties' 
        /// properties etc. This method determines if the recursion reached a leaf in the dump tree and that it should stop drilling down and return to 
        /// dump other branches of the dump tree. Recursion stops when:
        /// <list type="bullet">
        /// <item>
        /// The current examined type is <see cref="object"/>. The method dumps the object contained in <see cref="Instance"/>-s type name.
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
        /// <returns><c>true</c> if the recursion should stop; otherwise <c>false</c>.</returns>
        public bool IsAtDumpTreeLeaf() => DumpedRootClass()       ||  // Stop the recursion if: at System.Object
                                          DumpedDelegate()        ||  // at delegates
                                          DumpedMemberInfo();         // at MemberInfo values

        public bool DumpedRootClass()
        {
            if (ObjectType!=typeof(object))
                return false;

            DumpType();
            return true;
        }

        public bool DumpedDelegate()
        {
            if (!typeof(Delegate).IsAssignableFrom(ObjectType))
                return false;

            // it will be dumped at the descendant level
            if (ObjectType == typeof(MulticastDelegate) ||
                ObjectType == typeof(Delegate))
                return true;

            DumpScript?.DumpedDelegate();
            return _dumper.Writer.Dumped((Delegate)Instance);
        }

        public bool DumpedMemberInfo()
        {
            DumpScript?.DumpedMemberInfo();
            return _dumper.Writer.Dumped(Instance as MemberInfo);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "It's OK in the dumper")]
        public void DumpProperty()
        {
            // should we dump it at all?
            if (!CurrentProperty.CanRead()  ||  CurrentPropertyDumpAttribute.Skip == ShouldDump.Skip)
                return;

            var isVirtual = CurrentProperty.IsVirtual().GetValueOrDefault();

            if (isVirtual)
            {
                // don't dump virtual properties that were already dumped
                if (_dumper.DumpedVirtualProperties.Contains(new DumpedProperty(Instance, CurrentProperty.Name)))
                    return;

                // or will be dumped by the base classes
                if (CurrentPropertyDumpAttribute.IsDefaultAttribute() &&
                    PropertyDumpResolver.PropertyHasNonDefaultDumpAttribute(CurrentProperty))
                    return;
            }

            // can't dump indexers
            var pi = CurrentProperty as PropertyInfo;
            var fi = CurrentProperty as FieldInfo;

            if (pi != null  &&
                pi.GetIndexParameters().Length > 0)
                return;

            Type type = null;
            object value = null;

            try
            {
                if (pi != null)
                {
                    type  = pi.PropertyType;
                    value = pi.GetValue(Instance, null);
                }
                else
                {
                    type  = fi.FieldType;
                    value = fi.GetValue(Instance);
                }
            }
            catch (Exception x)
            {
                // this should not happen but...
                value = $"<{x.Message}>";
            }

            // should we dump a null value of the current property
            if (value == null  &&
                (CurrentPropertyDumpAttribute.DumpNullValues==ShouldDump.Skip  ||
                 CurrentPropertyDumpAttribute.DumpNullValues==ShouldDump.Default && DumpNullValues==ShouldDump.Skip))
                return;

            if (isVirtual)
                _dumper.DumpedVirtualProperties.Add(new DumpedProperty(Instance, CurrentProperty.Name));

            // write the property header
            _dumper.Writer.WriteLine();
            _dumper.Writer.Write(
                CurrentPropertyDumpAttribute.LabelFormat,
                CurrentProperty.Name);

            DumpScript?.AddWriteLine();
            DumpScript?.AddWrite(
                CurrentPropertyDumpAttribute.LabelFormat,
                CurrentProperty.Name);

            if (DumpedPropertyCustom(value, type))  // dump the property value using caller's customization (see ValueFormat="ToString", DumpClass, DumpMethod) if any.
                return;

            if (_dumper.Writer.DumpedBasicValue(value, CurrentPropertyDumpAttribute))
            {
                DumpScript?.DumpedBasicValue(CurrentProperty, CurrentPropertyDumpAttribute);
                return;
            }
            if (_dumper.Writer.Dumped(value as Delegate))
            {
                DumpScript?.DumpedDelegate(CurrentProperty);
                return;
            }
            if (_dumper.Writer.Dumped(value as MemberInfo))
            {
                DumpScript?.DumpedMemberInfo(CurrentProperty);
                return;
            }
            if (DumpedCollection(value))      // dump sequences (array, collection, etc. IEnumerable)
                return;

            // dump a property representing an associated class or struct object
            _dumper.DumpObject(
                value,
                null,
                !CurrentPropertyDumpAttribute.IsDefaultAttribute() ? CurrentPropertyDumpAttribute : null);
            DumpScript.DumpObject(
                CurrentProperty,
                null,
                !CurrentPropertyDumpAttribute.IsDefaultAttribute() ? CurrentPropertyDumpAttribute : null);
        }

        public bool DumpedCollection(
            bool enumerateCustom)
        {
            var dictionary = Instance as IDictionary;

            if (dictionary != null  &&
                _dumper.Writer.DumpedDictionary(
                                        dictionary,
                                        ClassDumpData.DumpAttribute,
                                        o => _dumper.DumpObject(o),
                                        _dumper.Indent,
                                        _dumper.Unindent))
            {
                DumpScript?.DumpedDictionary(ClassDumpData.DumpAttribute);
                return true;
            }

            var sequence = Instance as IEnumerable;

            if (sequence != null  &&
                _dumper.Writer.DumpedCollection(
                                        sequence,
                                        ClassDumpData.DumpAttribute,
                                        enumerateCustom,
                                        o => _dumper.DumpObject(o),
                                        _dumper.Indent,
                                        _dumper.Unindent))
            {
                DumpScript?.DumpedCollection(ClassDumpData.DumpAttribute);
                return true;
            }

            return false;
        }

        bool DumpedCollection(
            object currentPropertyValue)
        {
            var dictionary = currentPropertyValue as IDictionary;

            if (dictionary != null  &&
                _dumper.Writer.DumpedDictionary(
                                        dictionary,
                                        ClassDumpData.DumpAttribute,
                                        o => _dumper.DumpObject(o),
                                        _dumper.Indent,
                                        _dumper.Unindent))
            {
                DumpScript?.DumpedDictionary(CurrentProperty, ClassDumpData.DumpAttribute);
                return true;
            }

            var sequence = currentPropertyValue as IEnumerable;

            if (sequence != null  &&
                _dumper.Writer.DumpedCollection(
                                        sequence,
                                        CurrentPropertyDumpAttribute,
                                        false,
                                        o => _dumper.DumpObject(o),
                                        _dumper.Indent,
                                        _dumper.Unindent))
            {
                DumpScript?.DumpedCollection(CurrentProperty, CurrentPropertyDumpAttribute);
                return true;
            }

            return false;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "vm.Aspects.Diagnostics.DumpImplementation.DumpScript.AddWrite(System.String)")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "It's OK.")]
        bool DumpedPropertyCustom(
            object value,
            Type type)
        {
            if (value==null)
                return false;

            // did they specify DumpAttribute.ValueFormat="ToString"?
            if (CurrentPropertyDumpAttribute.ValueFormat == Resources.ValueFormatToString)
            {
                _dumper.Writer.Write(value.ToString());
                DumpScript?.WritePropertyOrField(CurrentProperty, null);
                return true;
            }

            // did they specify DumpAttribute.DumpMethod?
            var dumpMethodName = CurrentPropertyDumpAttribute.DumpMethod;
            var dumpClass = CurrentPropertyDumpAttribute.DumpClass;

            if (dumpClass==null  &&  dumpMethodName.IsNullOrWhiteSpace())
                return false;

            if (dumpMethodName.IsNullOrWhiteSpace())
                dumpMethodName = "Dump";

            MethodInfo dumpMethod = null;   // best match
            MethodInfo dumpMethod2 = null;  // second best

            // try the external class if specified
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
                {
                    _dumper.Writer.Write((string)dumpMethod.Invoke(null, new object[] { value }));
                    DumpScript?.WritePropertyOrField(CurrentProperty, dumpMethod);
                }
                else
                {
                    var message0 = $"*** Could not find a public, static, method {dumpMethodName}, with return type of System.String, with a single parameter of type {type.FullName} in the class {dumpClass.FullName}.";

                    _dumper.Writer.Write(message0);
                    DumpScript?.AddWrite(message0);
                }
                return true;
            }

            // try the property's class or base class, or metadata class
            foreach (var mi in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                                   .Where(mi => mi.Name == dumpMethodName  &&
                                                mi.ReturnType == typeof(string))
                                   .Union(type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                                                .Where(mi => mi.Name == dumpMethodName  &&
                                                             mi.ReturnType == typeof(string)  &&
                                                             mi.GetParameters().Count() == 1))
                                   .Union(ClassDumpData.Metadata.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
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
                    {
                        if (mi.GetParameters()[0].ParameterType.IsAssignableFrom(type))
                            dumpMethod2 = mi;
                    }
                }
            }

            if (dumpMethod == null)
                dumpMethod = dumpMethod2;

            if (dumpMethod != null)
            {
                if (dumpMethod.IsStatic)
                    _dumper.Writer.Write((string)dumpMethod.Invoke(null, new object[] { value }));
                else
                    _dumper.Writer.Write((string)dumpMethod.Invoke(value, null));
                DumpScript?.WritePropertyOrField(CurrentProperty, dumpMethod);
                return true;
            }

            if (dumpMethod2 != null)
            {
                if (dumpMethod2.IsStatic)
                    _dumper.Writer.Write((string)dumpMethod2.Invoke(null, new object[] { value }));
                else
                    _dumper.Writer.Write((string)dumpMethod2.Invoke(value, null));
                DumpScript?.WritePropertyOrField(CurrentProperty, dumpMethod2);
                return true;
            }

            var message = $"*** Could not find a public instance method with name {dumpMethodName} and no parameters or static method with a single parameter of type {type.FullName}, with return type of System.String in the class {type.FullName}.";

            _dumper.Writer.Write(message);
            DumpScript?.AddWrite(message);

            return true;
        }
    }
}
