using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Reflection;

namespace vm.Aspects
{
    /// <summary>
    /// Class EnumFlagsExtensions defines a few extension methods to the enum-s similar to the method <see cref="M:Enum.HasFlag"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags", Justification = "Makes it clearer that it is all about flags.")]
    public static class EnumFlagsExtensions
    {
        /// <summary>
        /// The method is only applicable to enum types marked with attribute <see cref="T:System.FlagsAttribute"/>.
        /// It determines whether the specified enum value has no others but only some (or none) of the specified flags.
        /// </summary>
        /// <typeparam name="TEnum">The enum type which must be marked with the attribute <see cref="T:System.FlagsAttribute"/>.</typeparam>
        /// <param name="value">The value to be tested.</param>
        /// <returns><see langword="true" /> if the value has no others but only some (or none) of the specified flags; otherwise, <see langword="false" />.</returns>
        public static bool IsEmpty<TEnum>(
            this TEnum value) where TEnum : struct
        {
            Contract.Requires<ArgumentException>(typeof(TEnum).IsEnum, "The method is applicable only to enum types marked with attribute FlagsAttribute only.");

            if (typeof(TEnum).GetCustomAttribute<FlagsAttribute>() == null)
                throw new ArgumentException("The method is applicable only to enum types marked with attribute FlagsAttribute only.", nameof(value));

            var tested = ((IConvertible)value).ToUInt64(CultureInfo.InvariantCulture);

            return tested == 0ul;
        }

        /// <summary>
        /// The method is only applicable to enum types marked with attribute <see cref="T:System.FlagsAttribute"/>.
        /// It determines whether the specified enum value has any of the specified flags.
        /// </summary>
        /// <typeparam name="TEnum">The enum type which must be marked with the attribute <see cref="T:System.FlagsAttribute"/>.</typeparam>
        /// <param name="value">The value to be tested.</param>
        /// <param name="flags">The flags to be tested for.</param>
        /// <returns><see langword="true" /> if the value has any of the specified flags; otherwise, <see langword="false" />.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "flags", Justification = "Makes it clearer that it is all about flags.")]
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags", Justification = "Makes it clearer that it is all about flags.")]
        public static bool HasAnyFlags<TEnum>(
            this TEnum value,
            TEnum flags) where TEnum : struct
        {
            Contract.Requires<ArgumentException>(typeof(TEnum).IsEnum, "The method is applicable only to enum types marked with attribute FlagsAttribute only.");

            if (typeof(TEnum).GetCustomAttribute<FlagsAttribute>() == null)
                throw new ArgumentException("The method is applicable only to enum types marked with attribute FlagsAttribute only.", nameof(value));

            var tested = ((IConvertible)value).ToUInt64(CultureInfo.InvariantCulture);
            var testFor = ((IConvertible)flags).ToUInt64(CultureInfo.InvariantCulture);

            return (tested & testFor) != 0ul;
        }

        /// <summary>
        /// The method is only applicable to enum types marked with attribute <see cref="T:System.FlagsAttribute"/>.
        /// It determines whether the specified enum value has all of the specified flags.
        /// </summary>
        /// <typeparam name="TEnum">The enum type which must be marked with the attribute <see cref="T:System.FlagsAttribute"/>.</typeparam>
        /// <param name="value">The value to be tested.</param>
        /// <param name="flags">The flags to be tested for.</param>
        /// <returns><see langword="true" /> if the value has all of the specified flags; otherwise, <see langword="false" />.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags", Justification = "Makes it clearer that it is all about flags.")]
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "flags", Justification = "Makes it clearer that it is all about flags.")]
        public static bool HasAllFlags<TEnum>(
            this TEnum value,
            TEnum flags) where TEnum : struct
        {
            Contract.Requires<ArgumentException>(typeof(TEnum).IsEnum, "The method is applicable only to enum types marked with attribute FlagsAttribute only.");

            if (typeof(TEnum).GetCustomAttribute<FlagsAttribute>() == null)
                throw new ArgumentException("The method is applicable only to enum types marked with attribute FlagsAttribute only.", nameof(value));

            var tested = ((IConvertible)value).ToUInt64(CultureInfo.InvariantCulture);
            var testFor = ((IConvertible)flags).ToUInt64(CultureInfo.InvariantCulture);

            return (tested & testFor) == testFor;
        }

        /// <summary>
        /// The method is only applicable to enum types marked with attribute <see cref="T:System.FlagsAttribute"/>.
        /// It determines whether the specified enum value has no flags outside the specified set of flags.
        /// </summary>
        /// <typeparam name="TEnum">The enum type which must be marked with the attribute <see cref="T:System.FlagsAttribute"/>.</typeparam>
        /// <param name="value">The value to be tested.</param>
        /// <param name="flags">The flags to be tested for.</param>
        /// <returns><see langword="true" /> if the value has has no flags outside the specified set of flags; otherwise, <see langword="false" />.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags", Justification = "Makes it clearer that it is all about flags.")]
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "flags", Justification = "Makes it clearer that it is all about flags.")]
        public static bool HasNoFlagsBut<TEnum>(
            this TEnum value,
            TEnum flags) where TEnum : struct
        {
            Contract.Requires<ArgumentException>(typeof(TEnum).IsEnum, "The method is applicable only to enum types marked with attribute FlagsAttribute only.");

            if (typeof(TEnum).GetCustomAttribute<FlagsAttribute>() == null)
                throw new ArgumentException("The method is applicable only to enum types marked with attribute FlagsAttribute only.", nameof(value));

            var tested = ((IConvertible)value).ToUInt64(CultureInfo.InvariantCulture);
            var testFor = ((IConvertible)flags).ToUInt64(CultureInfo.InvariantCulture);

            return (tested & ~testFor) == 0ul;
        }
    }
}
