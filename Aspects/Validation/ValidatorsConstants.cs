using System;
namespace vm.Aspects.Validation
{
    /// <summary>
    /// Class ValidatorsConstants. Has a method that returns the appropriate zero-value for a specific type.
    /// </summary>
    static class ValidatorsConstants
    {
        /// <summary>
        /// Gets the appropriate zero-value for the specified type <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type. Must implement <see cref="IComparable"/>.</param>
        /// <returns>IComparable.</returns>
        public static IComparable GetZero(
            Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (!typeof(IComparable).IsAssignableFrom(type))
                throw new ArgumentException("The type does not implement IComparable.");

            switch (Type.GetTypeCode(type))
            {
            case TypeCode.Int32:
                return 0;

            case TypeCode.Int64:
                return 0L;

            case TypeCode.Int16:
                return (short)0;

            case TypeCode.Decimal:
                return decimal.Zero;

            case TypeCode.Double:
                return 0.0;

            case TypeCode.Single:
                return (float)0.0;

            case TypeCode.Byte:
                return (byte)0;

            case TypeCode.SByte:
                return (sbyte)0;

            default:
                throw new ArgumentException("Don't know the zero value of the specified type.", nameof(type));
            }
        }
    }
}
