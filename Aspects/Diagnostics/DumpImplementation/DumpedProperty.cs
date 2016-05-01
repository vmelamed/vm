using System;
using System.Diagnostics.Contracts;

namespace vm.Aspects.Diagnostics.DumpImplementation
{
    struct DumpedProperty : IEquatable<DumpedProperty>
    {
        public readonly object Instance;
        public readonly string Property;

        public DumpedProperty(
            object instance, 
            string property)
        {
            Contract.Requires<ArgumentNullException>(instance != null, nameof(instance));
            Contract.Requires<ArgumentNullException>(property != null, nameof(property));
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(property));

            Instance = instance;
            Property = property;
        }

        #region Identity rules implementation.
        #region IEquatable<DumpedProperty> Members
        public bool Equals(DumpedProperty other) => Instance.Equals(other.Instance) &&
                                                    Property.Equals(other.Property, StringComparison.OrdinalIgnoreCase);
        #endregion

        public override bool Equals(object obj)
        {
            if (obj is DumpedProperty)
                return Equals((DumpedProperty)obj);

            return false;
        }

        public override int GetHashCode()
        {
            var hash = Constants.HashInitializer;

            hash = hash * Constants.HashMultiplier + Instance.GetHashCode();
            hash = hash * Constants.HashMultiplier + Property.GetHashCode();

            return hash;
        }

        public static bool operator==(DumpedProperty left, DumpedProperty right) => left.Equals(right);

        public static bool operator!=(DumpedProperty left, DumpedProperty right) => !(left==right);
        #endregion
    }
}
