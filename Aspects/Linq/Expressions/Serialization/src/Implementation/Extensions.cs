using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vm.Aspects.Linq.Expressions.Serialization.Implementation
{
    static class Extensions
    {

        /// <summary>
        /// Determines whether the specified type is basic: primitive, enum, decimal, string, Guid, Uri, DateTime, TimeSpan, DateTimeOffset, IntPtr, 
        /// UIntPtr.
        /// </summary>
        /// <param name="type">The type to be tested.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified type is one of the basic types; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsBasicType(
            this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return
                type.IsEnum ||
                type.IsPrimitive ||
                type == typeof(DBNull) ||
                type == typeof(decimal) ||
                type == typeof(string) ||
                type == typeof(Guid) ||
                type == typeof(Uri) ||
                type == typeof(DateTime) ||
                type == typeof(TimeSpan) ||
                type == typeof(IntPtr) ||
                type == typeof(UIntPtr) ||
                type == typeof(DateTimeOffset);
        }
    }
}
