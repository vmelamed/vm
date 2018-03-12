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

            var a = methodBase.GetCustomAttribute<T>(true);

            if (a != null)
                return a;

            a = methodBase.DeclaringType.GetCustomAttribute<T>(true);

            if (a != null)
                return a;

            if (methodBase.ReflectedType != methodBase.DeclaringType)
                a = methodBase.ReflectedType.GetCustomAttribute<T>(true);

            return a;

            //return methodBase.GetCustomAttribute<T>(true)            ??
            //       methodBase.DeclaringType.GetCustomAttribute<T>()  ??
            //       (methodBase.ReflectedType != methodBase.DeclaringType ? methodBase.ReflectedType.GetCustomAttribute<T>() : null);
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
        /// Determines whether the specified <see cref="MethodInfo"/> object represents a method that is overridden.
        /// </summary>
        /// <param name="mi">The <see cref="MethodInfo"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="MethodInfo"/> is overridden; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">mi</exception>
        public static bool IsOverridden(
            this MethodInfo mi)
        {
            if (mi == null)
                throw new ArgumentNullException(nameof(mi));

            return mi.IsVirtual  &&
                   mi.GetBaseDefinition().DeclaringType != mi.DeclaringType;
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
        static IDictionary<Type, object> _defaultValues = new Dictionary<Type, object>
        {
            [typeof(char)] = '\x0',
            [typeof(bool)] = false,
            [typeof(byte)] = (byte)0,
            [typeof(sbyte)] = (sbyte)0,
            [typeof(short)] = (short)0,
            [typeof(ushort)] = (ushort)0u,
            [typeof(int)] = 0,
            [typeof(uint)] = 0U,
            [typeof(long)] = 0L,
            [typeof(ulong)] = 0UL,
            [typeof(decimal)] = 0M,
            [typeof(float)] = (float)0.0,
            [typeof(double)] = 0.0,
            [typeof(DateTime)] = default(DateTime),
            [typeof(DateTimeOffset)] = default(DateTimeOffset),
            [typeof(TimeSpan)] = default(TimeSpan),
            [typeof(Guid)] = default(Guid),
            [typeof(IntPtr)] = IntPtr.Zero,
            [typeof(UIntPtr)] = UIntPtr.Zero,
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

            if (type == typeof(void) || !type.IsValueType)
                return null;

            object value;

            using (_sync.UpgradableReaderLock())
                if (!_defaultValues.TryGetValue(type, out value))
                {
                    value = typeof(ReflectionExtensions)
                                .GetMethod(nameof(GenericDefault), BindingFlags.Static | BindingFlags.NonPublic)
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
