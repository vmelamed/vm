using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using vm.Aspects.Diagnostics.DumpImplementation;

namespace vm.Aspects.Diagnostics
{
    sealed partial class ObjectTextDumper
    {
        class DumpState : IEnumerator<MemberInfo>
        {
            #region fileds
            IEnumerator<MemberInfo> _enumerator;
            MemberInfo _property;
            DumpAttribute _propertyDumpAttribute;
            readonly BindingFlags _propertiesBindingFlags;
            readonly BindingFlags _fieldsBindingFlags;
            #endregion

            public DumpState(
                object instance,
                Type type,
                ClassDumpData classDumpData,
                DumpAttribute instanceDumpAttribute,
                BindingFlags propertiesBindingFlags,
                BindingFlags fieldsBindingFlags)
            {
                Instance                = instance;
                Type                    = type;
                ClassDumpData           = classDumpData;
                InstanceDumpAttribute   = instanceDumpAttribute ?? DumpAttribute.Default;
                _propertiesBindingFlags = propertiesBindingFlags;
                _fieldsBindingFlags     = fieldsBindingFlags;
            }

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
                        _enumerator = Type.GetProperties(_propertiesBindingFlags)
                                          .Union<MemberInfo>(Type.GetFields(_fieldsBindingFlags))
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
        }
    }
}
