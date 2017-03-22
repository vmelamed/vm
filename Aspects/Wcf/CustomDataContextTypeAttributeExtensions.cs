using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace vm.Aspects.Wcf
{
    /// <summary>
    /// Extensions that return <see cref="CustomDataContextTypeAttribute"/> from a method or its declaring or defining type.
    /// </summary>
    public static class CustomDataContextTypeAttributeExtensions
    {
        /// <summary>
        /// Gets the custom data context type attribute from the type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>CustomDataContextTypeAttribute.</returns>
        public static CustomDataContextTypeAttribute GetCustomDataContextTypeAttribute(
            this Type type)
        {
            Contract.Requires<ArgumentNullException>(type != null, nameof(type));

            return type.GetCustomAttribute<CustomDataContextTypeAttribute>(true);
        }

        /// <summary>
        /// Gets the custom data context type attribute from the type.
        /// </summary>
        /// <param name="methodBase">The method base.</param>
        /// <returns>CustomDataContextTypeAttribute.</returns>
        public static CustomDataContextTypeAttribute GetCustomDataContextTypeAttribute(
            this MethodBase methodBase)
        {
            Contract.Requires<ArgumentNullException>(methodBase != null, nameof(methodBase));

            return methodBase.GetCustomAttribute<CustomDataContextTypeAttribute>(true) ??
                   methodBase.DeclaringType.GetCustomDataContextTypeAttribute()        ??
                   (methodBase.ReflectedType != methodBase.DeclaringType
                        ? methodBase.ReflectedType.GetCustomDataContextTypeAttribute()
                        : null);
        }
    }
}
