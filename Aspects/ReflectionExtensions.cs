using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using vm.Aspects.Threading;

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
            if (attributeProvider == null)
                throw new ArgumentNullException(nameof(attributeProvider));

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
            if (attributeProvider == null)
                throw new ArgumentNullException(nameof(attributeProvider));
            
            if (!attributeProvider.IsDefined(typeof(T), inherit))
                return new T[0];

            return attributeProvider.GetCustomAttributes(typeof(T), inherit).OfType<T>();
        }
        */

        /// <summary>
        /// Gets a custom attribute 
        /// from a method's <see cref="MethodBase"/> reflection object, or if not found,
        /// from the declaring type of the method, or if not found,
        /// from the reflected type if different from the declaring type (as it happens with an interface and implementing class).
        /// The method is useful when attributes are used to modify or parameterize the behavior of a call handler.
        /// </summary>
        /// <param name="methodBase">The method base.</param>
        /// <returns>CustomDataContextTypeAttribute.</returns>
        public static T GetMethodCustomAttribute<T>(
            this MethodBase methodBase) where T : Attribute
        {
            if (methodBase == null)
                throw new ArgumentNullException(nameof(methodBase));

            return methodBase.GetCustomAttribute<T>(true)            ??
                   methodBase.DeclaringType.GetCustomAttribute<T>()  ??
                   (methodBase.ReflectedType != methodBase.DeclaringType ? methodBase.ReflectedType.GetCustomAttribute<T>() : null);
        }

        /// <summary>
        /// Determines whether the <paramref name="type"/> is or inherits from the type <paramref name="baseType"/>.
        /// </summary>
        /// <param name="type">The type to test.</param>
        /// <param name="baseType">The base type to test against.</param>
        /// <returns><c>true</c> if [is] [the specified base type]; otherwise, <c>false</c>.</returns>
        public static bool Is(
            this Type type,
            Type baseType) => baseType.IsAssignableFrom(type);

        /// <summary>
        /// Determines whether the <paramref name="type" /> is or inherits from the type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The base type to test against.</typeparam>
        /// <param name="type">The type to test.</param>
        /// <returns><c>true</c> if [is] [the specified base type]; otherwise, <c>false</c>.</returns>
        public static bool Is<T>(
            this Type type) => type.Is(typeof(T));

        /// <summary>
        /// Determines whether the specified <see cref="PropertyInfo"/> object represents a virtual property.
        /// </summary>
        /// <param name="pi">The <see cref="PropertyInfo"/> object.</param>
        /// <returns><see langword="true"/> if the specified <see cref="PropertyInfo"/> object represents a virtual property; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="pi"/> is <see langword="null"/>.</exception>
        public static bool? IsVirtual(
            this PropertyInfo pi)
        {
            if (pi == null)
                throw new ArgumentNullException(nameof(pi));

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
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return type.IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        static ReaderWriterLockSlim _sync = new ReaderWriterLockSlim();
        static IDictionary<Type,object> _defaultValues = new Dictionary<Type,object>
        {
            [typeof(char)]           = '\x0',
            [typeof(bool)]           = false,
            [typeof(byte)]           = (byte)0,
            [typeof(sbyte)]          = (sbyte)0,
            [typeof(short)]          = (short)0,
            [typeof(ushort)]         = (ushort)0u,
            [typeof(int)]            = 0,
            [typeof(uint)]           = 0U,
            [typeof(long)]           = 0L,
            [typeof(ulong)]          = 0UL,
            [typeof(decimal)]        = 0M,
            [typeof(float)]          = (float)0.0,
            [typeof(double)]         = 0.0,
            [typeof(DateTime)]       = default(DateTime),
            [typeof(DateTimeOffset)] = default(DateTimeOffset),
            [typeof(TimeSpan)]       = default(TimeSpan),
            [typeof(Guid)]           = default(Guid),
            [typeof(IntPtr)]         = IntPtr.Zero,
            [typeof(UIntPtr)]        = UIntPtr.Zero,
        };

        /// <summary>
        /// Gets the default value of the specified type - a runtime equivalent of default(T).
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>System.Object.</returns>
        public static object Default(
            this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (type == typeof(void)  ||  !type.IsValueType)
                return null;

            object value;

            using (_sync.UpgradableReaderLock())
                if (!_defaultValues.TryGetValue(type, out value))
                {
                    value = typeof(ReflectionExtensions)
                                .GetMethod(nameof(GenericDefault), BindingFlags.Static|BindingFlags.NonPublic)
                                .MakeGenericMethod(type)
                                .Invoke(null, null);

                    using (_sync.WriterLock())
                        _defaultValues[type] = value;
                }

            return value;
        }

        static T GenericDefault<T>() => default(T);
    }
}
