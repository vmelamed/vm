using System;

namespace vm.Aspects.Diagnostics.Implementation
{
    struct DumpedProperty : IEquatable<DumpedProperty>
    {
        public readonly object Instance;
        public readonly string Property;

        public DumpedProperty(
            object instance,
            string property)
        {
            if (property.IsNullOrWhiteSpace())
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(property));

            Instance = instance ?? throw new ArgumentNullException(nameof(instance));
            Property = property;
        }

        #region Identity rules implementation.
        #region IEquatable<DumpedProperty> Members
        public bool Equals(DumpedProperty other)
            => Instance.Equals(other.Instance) &&
               Property.Equals(other.Property, StringComparison.OrdinalIgnoreCase);
        #endregion

        public override bool Equals(object obj)
            => obj is DumpedProperty ? Equals((DumpedProperty)obj) : false;

        public override int GetHashCode()
        {
            var hash = Constants.HashInitializer;

            unchecked
            {
                hash = hash * Constants.HashMultiplier + Instance.GetHashCode();
                hash = hash * Constants.HashMultiplier + Property.GetHashCode();
            }

            return hash;
        }

        public static bool operator ==(DumpedProperty left, DumpedProperty right)
            => left.Equals(right);

        public static bool operator !=(DumpedProperty left, DumpedProperty right)
            => !(left==right);
        #endregion
    }
}
