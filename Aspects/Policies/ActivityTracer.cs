using System;

using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace vm.Aspects.Policies
{
    /// <summary>
    /// Struct ActivityTracer encapsulates a tracer.
    /// </summary>
    public struct ActivityTracer : IEquatable<ActivityTracer>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityTracer"/> struct.
        /// </summary>
        /// <param name="tracer">The tracer.</param>
        /// <exception cref="ArgumentNullException">tracer</exception>
        public ActivityTracer(
            Tracer tracer)
        {
            Tracer = tracer  ??  throw new ArgumentNullException(nameof(tracer));
        }

        /// <summary>
        /// Gets or sets the tracer.
        /// </summary>
        public Tracer Tracer { get; set; }

        #region Identity rules implementation.
        #region IEquatable<ActivityTracer> Members
        /// <summary>
        /// Indicates whether the current object is equal to a reference to another object of the same type.
        /// </summary>
        /// <param name="other">A reference to another object of type <see cref="ActivityTracer"/> to compare with the current object.</param>
        /// <returns>
        /// <list type="number">
        ///     <item><see langword="false"/> if <paramref name="other"/> is equal to <see langword="null"/>, otherwise</item>
        ///     <item><see langword="true"/> if <paramref name="other"/> refers to <c>this</c> object, otherwise</item>
        ///     <item><see langword="false"/> if <paramref name="other"/> is not the same type as <c>this</c> object, otherwise</item>
        ///     <item><see langword="true"/> if the current object and the <paramref name="other"/> are considered to be equal,
        ///                                  e.g. their business identities are equal; otherwise, <see langword="false"/>.</item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// The <see cref="Equals(ActivityTracer)"/> and <see cref="Equals(object)"/> methods and
        /// the overloaded <c>operator==</c> and <c>operator!=</c> test for business identity,
        /// i.e. they test for business same-ness by comparing the types and the business keys.
        /// </remarks>
        public bool Equals(ActivityTracer other)
        {
            return Tracer == other.Tracer;
        }
        #endregion

        /// <summary>
        /// Determines whether this <see cref="ActivityTracer"/> instance is equal to the specified <see cref="object"/> reference.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> reference to compare with this <see cref="ActivityTracer"/> object.</param>
        /// <returns>
        /// <list type="number">
        ///     <item><see langword="false"/> if <paramref name="obj"/> cannot be cast to <see cref="ActivityTracer"/>, otherwise</item>
        ///     <item><see langword="false"/> if <paramref name="obj"/> is equal to <see langword="null"/>, otherwise</item>
        ///     <item><see langword="true"/> if <paramref name="obj"/> refers to <c>this</c> object, otherwise</item>
        ///     <item><see langword="false"/> if <paramref name="obj"/> is not the same type as <c>this</c> object, otherwise</item>
        ///     <item><see langword="true"/> if the current object and the <paramref name="obj"/> are considered to be equal,
        ///                                  e.g. their business identities are equal; otherwise, <see langword="false"/>.</item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// The <see cref="Equals(ActivityTracer)"/> and <see cref="Equals(object)"/> methods and
        /// the overloaded <c>operator==</c> and <c>operator!=</c> test for business identity,
        /// i.e. they test for business same-ness by comparing the types and the business keys.
        /// </remarks>
        public override bool Equals(object obj)
            => obj is ActivityTracer ? Equals((ActivityTracer)obj) : false;

        /// <summary>
        /// Serves as a hash function for the objects of <see cref="ActivityTracer"/> and its derived types.
        /// </summary>
        /// <returns>A hash code for the current <see cref="ActivityTracer"/> instance.</returns>
        public override int GetHashCode()
        {
            var hashCode = Constants.HashInitializer;

            unchecked
            {
                hashCode = Constants.HashMultiplier * hashCode + Tracer.GetHashCode();
            }

            return hashCode;
        }

        /// <summary>
        /// Compares two <see cref="ActivityTracer"/> objects.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// <see langword="true"/> if the objects are considered to be equal (<see cref="Equals(ActivityTracer)"/>);
        /// otherwise <see langword="false"/>.
        /// </returns>
        public static bool operator ==(ActivityTracer left, ActivityTracer right)
            => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="ActivityTracer"/> objects.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// <see langword="true"/> if the objects are not considered to be equal (<see cref="Equals(ActivityTracer)"/>);
        /// otherwise <see langword="false"/>.
        /// </returns>
        public static bool operator !=(ActivityTracer left, ActivityTracer right)
            => !(left==right);
        #endregion

    }
}
