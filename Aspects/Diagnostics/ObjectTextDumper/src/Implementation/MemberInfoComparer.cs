using System;
using System.Reflection;

namespace vm.Aspects.Diagnostics.Implementation
{
    /// <summary>
    /// Compares two PropertyInfo objects by the property Order of the dump attribute and then by Name.
    /// </summary>
    class MemberInfoComparer : IMemberInfoComparer
    {
        Type _metadata;

        #region IComparer<MemberInfo> Members
        public int Compare(
            MemberInfo x,
            MemberInfo y)
        {
            if (x == null)
                throw new ArgumentNullException(nameof(x));
            if (y == null)
                throw new ArgumentNullException(nameof(y));
            if (!x.DeclaringType.IsAssignableFrom(y.DeclaringType) &&
                !y.DeclaringType.IsAssignableFrom(x.DeclaringType))
                throw new InvalidOperationException("Cannot compare the order weights of properties from unrelated classes.");

            // get the order of each property:
            var orderX = (long)PropertyDumpResolver.GetPropertyDumpAttribute(x, _metadata).Order;
            var orderY = (long)PropertyDumpResolver.GetPropertyDumpAttribute(y, _metadata).Order;

            // if the properties are from the same type
            if (x.DeclaringType == y.DeclaringType)
            {
                // negative orders should be dumped after the positive orders (and the base class' properties but this is not the case here)
                if (orderX < 0)
                    orderX = int.MaxValue + (-orderX);
                if (orderY < 0)
                    orderY = int.MaxValue + (-orderY);

                if (orderX < orderY)
                    return -1;

                if (orderX > orderY)
                    return 1;

                var xpi = x as PropertyInfo;
                var ypi = y as PropertyInfo;

                // if the orders are the same - dump fields before properties in alphabetical order:
                if (xpi==null ^ ypi==null)
                {
                    // dump fields before properties
                    if (x is FieldInfo)
                        return -1;
                    else
                        return 1;
                }
                else
                    // if both types of the member infos are either PropertyInfo or FieldInfo - dump in alphabetical order
                    return string.CompareOrdinal(x.Name, y.Name);
            }

            // dump the properties of the base class after the non-negative properties of the inheriting class
            return y.DeclaringType.IsAssignableFrom(x.DeclaringType)
                        ? (orderX >= 0 ? -1 : 1)
                        : (orderY >= 0 ? 1 : -1);
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
