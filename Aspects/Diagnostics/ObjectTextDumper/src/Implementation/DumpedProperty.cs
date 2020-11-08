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
            if (property is "")
                throw new ArgumentException("The argument cannot consist of whitespace characters only.", nameof(property));

            Instance = instance;
            Property = property;
        }

        #region Identity rules implementation.
        #region IEquatable<DumpedProperty> Members
        public bool Equals(DumpedProperty other) =>
            Instance.Equals(other.Instance)  &&
            Property.Equals(other.Property, StringComparison.Ordinal);
        #endregion

        public override bool Equals(object? obj) => obj is DumpedProperty dp  &&  Equals(dp);

        public override int GetHashCode() => HashCode.Combine(Instance, Property);

        public static bool operator ==(DumpedProperty left, DumpedProperty right) => left.Equals(right);

        public static bool operator !=(DumpedProperty left, DumpedProperty right) => !(left==right);
        #endregion
    }
}
