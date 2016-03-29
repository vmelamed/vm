using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace vm.Aspects.Model.EFRepository
{
    /// <summary>
    /// Specifies several DB character column facets in one attribute. This class cannot be inherited.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(
        AttributeTargets.Property |
        AttributeTargets.Field,
        AllowMultiple = false)]
    public sealed class StringAttribute : Attribute
    {
        int _length;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringAttribute"/> class.
        /// </summary>
        /// <param name="length">The length of the column. If specified -1, the length will be the unlimited length (BLOB).</param>
        public StringAttribute(
            int length)
        {
            Contract.Requires<ArgumentException>(length > 0 || length == -1, "The length can be either a positive number greater than 0 or -1.");

            _length = length;
        }

        /// <summary>
        /// Gets the length of the column.
        /// </summary>
        public int Length => _length;

        /// <summary>
        /// Gets or sets a value indicating whether the column is UNICODE.
        /// </summary>
        public bool IsUnicode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the column length is fixed vs variable.
        /// </summary>
        public bool IsFixed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the value is required vs. nullable - non-required.
        /// </summary>
        public bool IsRequired { get; set; }
    }

    /// <summary>
    /// Class NumericAttributeConvention enables the attribute <see cref="NumericAttribute" /></summary>
    /// <seealso cref="Convention" />
    /// <seealso cref="System.Data.Entity.ModelConfiguration.Conventions.Convention" />
    public class StringAttributeConvention : Convention
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NumericAttributeConvention"/> class.
        /// </summary>
        public StringAttributeConvention()
        {
            Properties<string>()
                .Having(
                    p => p.GetCustomAttribute<StringAttribute>()  ??
                         p.DeclaringType
                          .GetCustomAttribute<MetadataTypeAttribute>()
                          ?.MetadataClassType
                          ?.GetProperty(p.Name)
                          ?.GetCustomAttribute<StringAttribute>())
                .Configure(
                    (c, a) =>
                    {
                        if (a.Length > 0)
                        {
                            c.HasMaxLength(a.Length);
                            if (a.IsFixed)
                                c.IsFixedLength();
                        }
                        else
                            c.IsMaxLength();

                        c.IsUnicode(a.IsUnicode);

                        if (a.IsRequired)
                            c.IsRequired();
                    });
        }
    }
}
