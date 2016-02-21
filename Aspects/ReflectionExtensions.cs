using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace vm.Aspects
{
    /// <summary>
    /// Class ReflectionExtensions.
    /// </summary>
    public static class ReflectionExtensions
    {
        /*
        /// <summary>
        /// Retrieves a custom attribute of a specified type that is applied to a specified member.
        /// </summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="attributeProvider">The member to inspect.</param>
        /// <param name="inherit"><see langword="true" /> to inspect the ancestors of element; otherwise, <see langword="false" /> (the default).</param>
        /// <returns>A custom attribute that matches <typeparamref name="T"/>, or <see langword="null" /> if no such attribute is found.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="attributeProvider"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if more than one attribute of the specified type <typeparamref name="T"/> is found.</exception>
        internal static T GetCustomAttribute<T>(
            this ICustomAttributeProvider attributeProvider,
            bool inherit = false) where T : class
        {
            Contract.Requires<ArgumentNullException>(attributeProvider != null, nameof(attributeProvider));

            if (!attributeProvider.IsDefined(typeof(T), inherit))
                return null;

            return (T)attributeProvider.GetCustomAttributes(typeof(T), inherit).Single();
        }

        /// <summary>
        /// Retrieves all custom attributes of a specified type that are applied to a specified member.
        /// </summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="attributeProvider">The member to inspect.</param>
        /// <param name="inherit"><see langword="true" /> to inspect the ancestors of element; otherwise, <see langword="false" /> (the default).</param>
        /// <returns>A sequence of custom attributes that match <typeparamref name="T"/> (possibly empty).</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="attributeProvider"/> is <see langword="null"/>.</exception>
        internal static IEnumerable<T> GetCustomAttributes<T>(
            this ICustomAttributeProvider attributeProvider,
            bool inherit = false) where T : class
        {
            Contract.Requires<ArgumentNullException>(attributeProvider != null, nameof(attributeProvider));
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            if (!attributeProvider.IsDefined(typeof(T), inherit))
                return new T[0];

            return attributeProvider.GetCustomAttributes(typeof(T), inherit).OfType<T>();
        }
        */

        /// <summary>
        /// Determines whether the <paramref name="type"/> inherits the type <paramref name="baseType"/>.
        /// </summary>
        /// <param name="type">The type to test.</param>
        /// <param name="baseType">The base type to test against.</param>
        /// <returns><c>true</c> if [is] [the specified base type]; otherwise, <c>false</c>.</returns>
        [Pure]
        public static bool Is(
            this Type type,
            Type baseType)
        {
            while (type != null  &&  type != baseType  &&  type != typeof(object))
                type = type.BaseType;

            return (type == baseType);
        }

        /// <summary>
        /// Determines whether the specified <see cref="PropertyInfo"/> object represents a virtual property.
        /// </summary>
        /// <param name="pi">The <see cref="PropertyInfo"/> object.</param>
        /// <returns><see langword="true"/> if the specified <see cref="PropertyInfo"/> object represents a virtual property; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="pi"/> is <see langword="null"/>.</exception>
        [Pure]
        public static bool? IsVirtual(
            this PropertyInfo pi)
        {
            Contract.Requires<ArgumentNullException>(pi != null, nameof(pi));

            bool? isVirtual = null;

            foreach (var accessor in pi.GetAccessors())
                if (isVirtual.HasValue)
                {
                    if (isVirtual.Value != accessor.IsVirtual)
                        return null;
                }
                else
                    isVirtual = accessor.IsVirtual;

            return isVirtual;
        }

        /// <summary>
        /// Determines whether the specified type is nullable.
        /// </summary>
        /// <param name="type">The type to be tested.</param>
        /// <returns><see langword="true" /> if the specified type is nullable; otherwise, <see langword="false" />.</returns>
        public static bool IsNullable(
            this Type type)
        {
            Contract.Requires<ArgumentNullException>(type != null, nameof(type));

            return type.IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}
