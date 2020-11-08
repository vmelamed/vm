using System;
using System.Reflection;

namespace vm.Aspects.Diagnostics.Implementation
{
    /// <summary>
    /// Compares two PropertyInfo objects by the property Order of the dump attribute and then by Name.
    /// </summary>
    struct MemberInfoComparer : IMemberInfoComparer
    {
        Type _metadata;

        #region IComparer<MemberInfo> Members
        public int Compare(
            MemberInfo? x,
            MemberInfo? y)
        {
            if (x is null)
                return y is null ? 0 : -1;
            if (y is null)
                return 1;

            if (x.DeclaringType?.IsAssignableFrom(y.DeclaringType) == false &&
                y.DeclaringType?.IsAssignableFrom(x.DeclaringType) == false)
                throw new InvalidOperationException("Cannot compare the order weights of properties from unrelated classes.");

            // get the order of each property:
            var orderX = (long)PropertyDumpResolver.GetPropertyDumpAttribute(x, _metadata).Order;
            var orderY = (long)PropertyDumpResolver.GetPropertyDumpAttribute(y, _metadata).Order;

            // if the properties are from different types
            if (x.DeclaringType != y.DeclaringType)
                // dump the properties of the base class after the non-negative properties of the inheriting class
                return y.DeclaringType?.IsAssignableFrom(x.DeclaringType) == true
                            ? (orderX >= 0 ? -1 : 1)
                            : (orderY >= 0 ? 1 : -1);

            // negative orders should be dumped after the positive orders (and the base class' properties but this is not the case here)
            if (orderX < 0)
                orderX = unchecked(int.MaxValue + (-orderX));
            if (orderY < 0)
                orderY = unchecked(int.MaxValue + (-orderY));

            return orderX < orderY
                    ? -1
                    : orderX > orderY
                        ? 1
                        : x is PropertyInfo ^ y is PropertyInfo
                            ? y is PropertyInfo ? -1 : 1                // dump fields before properties
                            : string.CompareOrdinal(x.Name, y.Name);    // dump in alphabetical order
        }
        #endregion

        #region IPropertyInfoComparer Members
        public IMemberInfoComparer SetMetadata(Type metadata)
        {
            _metadata = metadata;
            return this;
        }
        #endregion
    }
}
