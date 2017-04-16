using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using vm.Aspects.Diagnostics.DumpImplementation;
using vm.Aspects.Diagnostics.Properties;

namespace vm.Aspects.Diagnostics
{
    class DumpState : IEnumerator<MemberInfo>
    {
        #region fileds
        readonly ObjectTextDumper _dumper;
        IEnumerator<MemberInfo> _enumerator;
        MemberInfo _property;
        DumpAttribute _propertyDumpAttribute;
        #endregion

        public DumpState(
            ObjectTextDumper dumper,
            object instance,
            ClassDumpData classDumpData)
            : this(dumper, instance, instance.GetType(), classDumpData, classDumpData.DumpAttribute)
        {
            Contract.Requires<ArgumentNullException>(dumper        != null, nameof(dumper));
            Contract.Requires<ArgumentNullException>(instance      != null, nameof(instance));
            Contract.Requires<ArgumentNullException>(classDumpData != null, nameof(classDumpData));
        }

        private DumpState(
            ObjectTextDumper dumper,
            object instance,
            Type type,
            ClassDumpData classDumpData,
            DumpAttribute instanceDumpAttribute)
        {
            Contract.Requires<ArgumentNullException>(instance              != null, nameof(instance));
            Contract.Requires<ArgumentNullException>(type                  != null, nameof(type));
            Contract.Requires<ArgumentNullException>(classDumpData         != null, nameof(classDumpData));
            Contract.Requires<ArgumentNullException>(instanceDumpAttribute != null, nameof(instanceDumpAttribute));
            Contract.Requires<ArgumentNullException>(dumper                != null, nameof(dumper));

            _dumper                 = dumper;
            Instance                = instance;
            Type                    = type;
            ClassDumpData           = classDumpData;
            InstanceDumpAttribute   = instanceDumpAttribute ?? DumpAttribute.Default;
        }

        public DumpState GetBaseTypeDumpState() => new DumpState(
                                                            _dumper,
                                                            Instance,
                                                            Type.BaseType,
                                                            ClassMetadataResolver.GetClassDumpData(Type.BaseType),
                                                            InstanceDumpAttribute);

        /// <summary>
        /// Gets or sets the currently dumped instance.
        /// </summary>
        public object Instance { get; }

        /// <summary>
        /// Gets or sets the current type (maybe base type) of the instance being dumped.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets or sets the dump attribute applied to the instance .
        /// </summary>
        /// <value>
        /// The instance dump attribute.
        /// </value>
        public DumpAttribute InstanceDumpAttribute { get; }

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
        public DumpAttribute CurrentPropertyDumpAttribute => _propertyDumpAttribute;

        /// <summary>
        /// Sets the state to point to the default (representative) property if the type is not to be dumped recursively.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the state was successfully set to the default property; otherwise <c>false</c>.
        /// </returns>
        public bool SetToDefault()
        {
            Contract.Requires<ArgumentNullException>(Type != null, nameof(Type));

            var defaultProperty = DefaultProperty;

            if (defaultProperty.IsNullOrWhiteSpace())
                return false;

            var pi = Type.GetProperty(defaultProperty);

            if (pi==null)
                return false;

            _property              = pi;
            _propertyDumpAttribute = PropertyDumpResolver.GetPropertyDumpAttribute(_property, ClassDumpData.Metadata);

            return true;
        }

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
        IEnumerator<MemberInfo> Enumerator
        {
            get
            {
                if (_enumerator == null)
                    _enumerator = Type.GetProperties(_dumper.PropertiesBindingFlags)
                                      .Union<MemberInfo>(Type.GetFields(_dumper.FieldsBindingFlags))
                                      .Where(mi => !mi.Name.StartsWith("<", StringComparison.Ordinal))
                                      .OrderBy(p => p, ServiceResolver.Default
                                                                      .GetInstance<IMemberInfoComparer>()
                                                                      .SetMetadata(ClassDumpData.Metadata))
                                      .GetEnumerator();
                return _enumerator;
            }
        }

        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        /// <returns>The element in the collection at the current position of the enumerator.</returns>
        public MemberInfo Current => _property;

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_enumerator != null)
                _enumerator.Dispose();
        }

        #endregion

        #region IEnumerator Members

        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        /// <returns>The element in the collection at the current position of the enumerator.</returns>
        object System.Collections.IEnumerator.Current => Current;

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
                _property              = null;
                _propertyDumpAttribute = null;
                return false;
            }

            _property              = Enumerator.Current;
            _propertyDumpAttribute = PropertyDumpResolver.GetPropertyDumpAttribute(_property, ClassDumpData.Metadata);
            return true;
        }

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        public void Reset()
        {
            Enumerator.Reset();
            _property              = null;
            _propertyDumpAttribute = null;
        }

        #endregion
        #endregion

        public bool DumpedRootClass()
        {
            if (Type!=typeof(object))
                return false;

            var type = Instance.GetType();

            _dumper.Writer.Write(
                DumpFormat.Type,
                type.GetTypeName(),
                type.Namespace,
                type.AssemblyQualifiedName);

            _dumper.Indent();

            return true;
        }

        public bool DumpedDefaultProperty()
        {
            Contract.Requires(RecurseDump != ShouldDump.Default);

            if (RecurseDump == ShouldDump.Dump)
                return false;

            var type = Instance.GetType();

            _dumper.Writer.Write(
                        DumpFormat.Type,
                        type.GetTypeName(),
                        type.Namespace,
                        type.AssemblyQualifiedName);

            _dumper.Indent();

            if (SetToDefault())
                DumpProperty();

            _dumper.Unindent();

            return true;
        }

        public bool DumpedDelegate()
        {
            if (!typeof(Delegate).IsAssignableFrom(Type))
                return false;

            // it will be dumped at the descendant level
            if (Type == typeof(MulticastDelegate) ||
                Type == typeof(Delegate))
                return true;

            return _dumper.Writer.Dumped((Delegate)Instance);
        }

        public bool DumpedMemberInfo() => _dumper.Writer.Dumped(Instance as MemberInfo);

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

            object value = null;
            Type type = null;

            try
            {
                type = pi != null
                            ? pi.PropertyType
                            : fi.FieldType;
                value = pi != null
                            ? pi.GetValue(Instance, null)
                            : fi.GetValue(Instance);
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

            if (DumpedPropertyCustom(value, type)                              ||     // dump the property value using caller's customization (ValueFormat, DumpClass, DumpMethod) if any.
                _dumper.Writer.DumpedBasicValue(value, CurrentPropertyDumpAttribute)  ||
                _dumper.Writer.Dumped(value as Delegate)                              ||
                _dumper.Writer.Dumped(value as MemberInfo)                            ||
                DumpedSequenceProperty(value))                                       // dump sequences (array, collection, etc. IEnumerable)
                return;

            // dump a property representing an associated class or struct object
            _dumper.DumpObject(
                value,
                null,
                CurrentPropertyDumpAttribute.IsDefaultAttribute()
                    ? null
                    : CurrentPropertyDumpAttribute);
        }

        bool DumpedSequenceProperty(
            object value)
        {
            var sequence = value as IEnumerable;

            if (sequence == null)
                return false;

            return _dumper.Writer.DumpedDictionary(
                                        sequence,
                                        ClassDumpData.DumpAttribute,
                                        o => _dumper.DumpObject(o),
                                        _dumper.Indent,
                                        _dumper.Unindent)
                   ||
                   _dumper.Writer.DumpedSequence(
                                        sequence,
                                        CurrentPropertyDumpAttribute,
                                        false,
                                        o => _dumper.DumpObject(o),
                                        _dumper.Indent,
                                        _dumper.Unindent);
        }

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
                    _dumper.Writer.Write((string)dumpMethod.Invoke(null, new object[] { value }));
                else
                    _dumper.Writer.Write($"*** Could not find a public, static, method {dumpMethodName}, with return type of System.String, with a single parameter of type {type.FullName} in the class {dumpClass.FullName}.");

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
                        if (mi.GetParameters()[0].ParameterType.IsAssignableFrom(type))
                        dumpMethod2 = mi;
                }
            }

            if (dumpMethod != null)
            {
                if (dumpMethod.IsStatic)
                    _dumper.Writer.Write((string)dumpMethod.Invoke(null, new object[] { value }));
                else
                    _dumper.Writer.Write((string)dumpMethod.Invoke(value, null));

                return true;
            }

            if (dumpMethod2 != null)
            {
                if (dumpMethod2.IsStatic)
                    _dumper.Writer.Write((string)dumpMethod2.Invoke(null, new object[] { value }));
                else
                    _dumper.Writer.Write((string)dumpMethod2.Invoke(value, null));

                return true;
            }

            _dumper.Writer.Write($"*** Could not find a public instance method with name {dumpMethodName} and no parameters or static method with a single parameter of type {type.FullName}, with return type of System.String in the class {type.FullName}.");

            return true;
        }
    }
}
