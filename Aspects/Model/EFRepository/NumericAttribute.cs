using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace vm.Aspects.Model.EFRepository
{
    /// <summary>
    /// Specifies that the field to which it is applied should be mapped to SQL data type NUMERIC with the specified precision and scale.
    /// This class cannot be inherited.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(
        AttributeTargets.Property |
        AttributeTargets.Field,
        AllowMultiple = false)]
    public sealed class NumericAttribute : Attribute
    {
        byte _precision;
        byte _scale;

        /// <summary>
        /// Initializes a new instance of the <see cref="NumericAttribute" /> class.
        /// </summary>
        /// <param name="precision">The precision.</param>
        /// <param name="scale">The scale.</param>
        public NumericAttribute(
            byte precision,
            byte scale = 0)
        {
            Contract.Requires<ArgumentException>(precision > 0, "The precision parameter must be positive.");
            Contract.Requires<ArgumentException>(scale > 0  &&  scale <= precision, "The scale parameter must be positive and less than the precision.");

            _precision = precision;
            _scale     = scale;
        }

        /// <summary>
        /// Gets the precision.
        /// </summary>
        public byte Precision => _precision;

        /// <summary>
        /// Gets the scale.
        /// </summary>
        public byte Scale => _scale;
    }

    /// <summary>
    /// Class NumericAttributeConvention enables the attribute <see cref="NumericAttribute"/>
    /// </summary>
    /// <seealso cref="System.Data.Entity.ModelConfiguration.Conventions.Convention" />
    public sealed class NumericAttributeConvention : Convention
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NumericAttributeConvention"/> class.
        /// </summary>
        public NumericAttributeConvention()
        {
            Properties<decimal>()
                .Having(
                    p => p.GetCustomAttribute<NumericAttribute>()  ??
                         p.DeclaringType
                          .GetCustomAttribute<MetadataTypeAttribute>()
                          ?.MetadataClassType
                          ?.GetProperty(p.Name)
                          ?.GetCustomAttribute<NumericAttribute>())
                .Configure(
                    (c, a) => c.HasPrecision(a.Precision, a.Scale))
                ;
        }
    }
}
