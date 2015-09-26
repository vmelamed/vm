using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace vm.Aspects.Model.EFRepository.HiLoIdentity
{
    /// <summary>
    /// Class HiLoIdentityGenerator. Implements the Hi-Lo algorithm for generating unique ID-s.
    /// </summary>
    public class HiLoIdentityGenerator
    {
        #region Constants
        /// <summary>
        /// The maximum length of the property <see cref="EntitySetName"/>: 100.
        /// </summary>
        public const int EntitySetNameMaxLength = 100;

        /// <summary>
        /// The default value of the maximum value of the low value: 100
        /// </summary>
        public const int DefaultMaxLowValue = 1000;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="HiLoIdentityGenerator"/> class.
        /// </summary>
        public HiLoIdentityGenerator()
        {
            MaxLowValue = DefaultMaxLowValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HiLoIdentityGenerator"/> class.
        /// </summary>
        /// <param name="entitySetName">Name of the entity set.</param>
        /// <param name="maxLowValue">The max low value.</param>
        public HiLoIdentityGenerator(
            string entitySetName,
            int maxLowValue = DefaultMaxLowValue)
        {
            if (string.IsNullOrWhiteSpace(entitySetName))
                throw new ArgumentNullException("entitySetName");

            EntitySetName = entitySetName;
            MaxLowValue   = maxLowValue;
        }
        #endregion

        #region Persisted properties
        /// <summary>
        /// Gets the name of the entities set.
        /// </summary>
        /// <value>
        /// The name of the entities set.
        /// </value>
        [StringLengthValidator(EntitySetNameMaxLength)]
        [RegexValidator(@"^[_\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Mc}\p{Cf}\p{Pc}\p{Lm}][_\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}\p{Nl}\p{Mc}\p{Cf}\p{Pc}\p{Lm}]*$")]
        public string EntitySetName { get; protected internal set; }

        /// <summary>
        /// Gets the next value of the high value.
        /// </summary>
        public int HighValue { get; protected internal set; }

        /// <summary>
        /// Gets the low value.
        /// </summary>
        public int LowValue { get; protected internal set; }

        /// <summary>
        /// Gets the maximum value of the low value.
        /// </summary>
        public int MaxLowValue { get; protected internal set; }
        #endregion

        #region Methods
        /// <summary>
        /// Increments the high value and initializes the low value.
        /// I.e. Moves the generator to the next locked range of values.
        /// </summary>
        internal void IncrementHighValue()
        {
            unchecked
            {
                if (HighValue+1 == 0L)
                    throw new InvalidOperationException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Fatal error: the HighValue of the ID generator for entity set {0} has reached the maximum value.",
                            EntitySetName));

                HighValue++;
                LowValue = 0;
            }
        }

        /// <summary>
        /// Gets the next available value for an entity id. If the low values are exhausted the method will return -1L.
        /// </summary>
        /// <returns>The next available value for an entity id.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification="n/a")]
        public long GetId()
        {
            if (HighValue < 1L || LowValue >= MaxLowValue)
                return -1L;

            return unchecked(((long)(HighValue-1) * MaxLowValue) + ++LowValue);
        }
        #endregion
    }
}
