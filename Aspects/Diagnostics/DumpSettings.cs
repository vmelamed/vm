using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using vm.Aspects.Diagnostics.Implementation;

namespace vm.Aspects.Diagnostics
{
    /// <summary>
    /// Contains some setting values used to initialize instances of <see cref="ObjectTextDumper"/>
    /// </summary>
    public struct DumpSettings : IEquatable<DumpSettings>
    {
        /// <summary>
        /// The default initial indent level.
        /// </summary>
        public const int DefaultInitialIndentLevel = 0;

        /// <summary>
        /// The default initial indent size
        /// </summary>
        public const int DefaultIndentSize = 2;

        /// <summary>
        /// The default maximum dump length.
        /// </summary>
        public const int DefaultMaxDumpLength = DumpTextWriter.DefaultMaxLength;

        /// <summary>
        /// The initial default properties' binding flags.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags")]
        public const BindingFlags InitialDefaultPropertiesBindingFlags = BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance;

        /// <summary>
        /// The initial default fields' binding flags
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags")]
        public const BindingFlags InitialDefaultFieldsBindingFlags = BindingFlags.Public|BindingFlags.Instance;

        int _indentSize;

        /// <summary>
        /// Gets a default set of settings
        /// </summary>
        public static DumpSettings Default => new DumpSettings(
                                                        true,
                                                        DefaultIndentSize,
                                                        DumpTextWriter.DefaultMaxLength,
                                                        InitialDefaultPropertiesBindingFlags,
                                                        InitialDefaultFieldsBindingFlags);

        /// <summary>
        /// Initializes a new instance of the <see cref="DumpSettings" /> struct.
        /// </summary>
        /// <param name="useDumpScriptCache">if set to <c>true</c> [use dump script cache].</param>
        /// <param name="indentSize">Size of the indent.</param>
        /// <param name="maxDumpLength">Maximum length of the dump.</param>
        /// <param name="propertiesBindingFlags">The properties binding flags.</param>
        /// <param name="fieldsBindingFlags">The fields binding flags.</param>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags")]
        public DumpSettings(
            bool useDumpScriptCache = true,
            int indentSize = DefaultIndentSize,
            int maxDumpLength = DumpTextWriter.DefaultMaxLength,
            BindingFlags propertiesBindingFlags = InitialDefaultPropertiesBindingFlags,
            BindingFlags fieldsBindingFlags = InitialDefaultFieldsBindingFlags)
        {
            UseDumpScriptCache     = useDumpScriptCache;
            _indentSize            = indentSize  >= DefaultIndentSize ? indentSize : DefaultIndentSize;
            MaxDumpLength          = maxDumpLength;
            PropertiesBindingFlags = propertiesBindingFlags;
            FieldsBindingFlags     = fieldsBindingFlags;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use the dump script cache.
        /// </summary>
        public bool UseDumpScriptCache { get; set; }

        /// <summary>
        /// Gets or sets the size of the indent.
        /// </summary>
        public int IndentSize
        {
            get => _indentSize;
            set => _indentSize = value >= DefaultIndentSize ? value : DefaultIndentSize;
        }

        /// <summary>
        /// Gets or sets the maximum length of the dump.
        /// </summary>
        public int MaxDumpLength { get; set; }

        /// <summary>
        /// Gets or sets the properties binding flags controlling which properties should be dumped, e.g. private vs public, static vs. instance, etc.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags")]
        public BindingFlags PropertiesBindingFlags { get; set; }

        /// <summary>
        /// Gets or sets the fields binding flags controlling which fields should be dumped, e.g. private vs public, static vs. instance, etc.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags")]
        public BindingFlags FieldsBindingFlags { get; set; }

        #region Identity rules implementation.
        #region IEquatable<DumpSettings> Members
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">Another object of type <see cref="DumpSettings"/> to compare with the current object.</param>
        /// <returns>
        /// <list type="number">
        ///     <item><see langword="true"/> if the current object and the <paramref name="other"/> are considered to be equal, 
        ///                                  e.g. their business identities are equal; otherwise, <see langword="false"/>.</item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// The <see cref="Equals(DumpSettings)"/> and <see cref="Equals(object)"/> methods and
        /// the overloaded <c>operator==</c> and <c>operator!=</c> test for business identity, 
        /// i.e. they test for business same-ness by comparing the types and the business keys.
        /// </remarks>
        public bool Equals(DumpSettings other)
            => UseDumpScriptCache     == other.UseDumpScriptCache     &&
               IndentSize             == other.IndentSize             &&
               MaxDumpLength          == other.MaxDumpLength          &&
               PropertiesBindingFlags == other.PropertiesBindingFlags &&
               FieldsBindingFlags     == other.FieldsBindingFlags;
        #endregion

        /// <summary>
        /// Determines whether this <see cref="DumpSettings"/> instance is equal to the specified <see cref="object"/> reference.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> reference to compare with this <see cref="DumpSettings"/> object.</param>
        /// <returns>
        /// <list type="number">
        ///     <item><see langword="false"/> if <paramref name="obj"/> cannot be cast to <see cref="DumpSettings"/>, otherwise</item>
        ///     <item><see langword="false"/> if <paramref name="obj"/> is equal to <see langword="null"/>, otherwise</item>
        ///     <item><see langword="true"/> if <paramref name="obj"/> refers to <c>this</c> object, otherwise</item>
        ///     <item><see langword="false"/> if <paramref name="obj"/> is not the same type as <c>this</c> object, otherwise</item>
        ///     <item><see langword="true"/> if the current object and the <paramref name="obj"/> are considered to be equal, 
        ///                                  e.g. their business identities are equal; otherwise, <see langword="false"/>.</item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// The <see cref="Equals(DumpSettings)"/> and <see cref="Equals(object)"/> methods and
        /// the overloaded <c>operator==</c> and <c>operator!=</c> test for business identity, 
        /// i.e. they test for business same-ness by comparing the types and the business keys.
        /// </remarks>
        public override bool Equals(object obj)
            => obj is DumpSettings ? Equals((DumpSettings)obj) : false;

        /// <summary>
        /// Serves as a hash function for the objects of <see cref="DumpSettings"/> and its derived types.
        /// </summary>
        /// <returns>A hash code for the current <see cref="DumpSettings"/> instance.</returns>
        public override int GetHashCode()
        {
            var hashCode = Constants.HashInitializer;

            hashCode = Constants.HashMultiplier * hashCode + UseDumpScriptCache.GetHashCode();
            hashCode = Constants.HashMultiplier * hashCode + IndentSize.GetHashCode();
            hashCode = Constants.HashMultiplier * hashCode + MaxDumpLength.GetHashCode();
            hashCode = Constants.HashMultiplier * hashCode + PropertiesBindingFlags.GetHashCode();
            hashCode = Constants.HashMultiplier * hashCode + FieldsBindingFlags.GetHashCode();

            return hashCode;
        }

        /// <summary>
        /// Compares two <see cref="DumpSettings"/> objects.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// <see langword="true"/> if the objects are considered to be equal (<see cref="Equals(DumpSettings)"/>);
        /// otherwise <see langword="false"/>.
        /// </returns>
        public static bool operator ==(DumpSettings left, DumpSettings right) => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="DumpSettings"/> objects.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// <see langword="true"/> if the objects are not considered to be equal (<see cref="Equals(DumpSettings)"/>);
        /// otherwise <see langword="false"/>.
        /// </returns>
        public static bool operator !=(DumpSettings left, DumpSettings right) => !(left==right);
        #endregion

    }
}
