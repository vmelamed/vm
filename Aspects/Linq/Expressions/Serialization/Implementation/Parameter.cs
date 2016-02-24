using System;

namespace vm.Aspects.Linq.Expressions.Serialization.Implementation
{
    class Parameter : IEquatable<Parameter>
    {
        public string Name
        { get; set; }
        public Type Type
        { get; set; }
        public bool IsByRef
        { get; set; }

        #region Identity rules implementation.

        #region IEquatable<Parameter> Members
        /// <summary>
        /// Indicates whether the current object is equal to a reference to another object of the same type.
        /// </summary>
        /// <param name="other">A reference to another object of type <see cref="Parameter"/> to compare with this object.</param>
        /// <returns>
        /// <see langword="false"/> if <paramref name="other"/> is equal to <see langword="null"/>, otherwise
        /// <see langword="true"/> if <paramref name="other"/> refers to <c>this</c> object, otherwise
        /// <see langword="true"/> if <i>the business identities</i> of the current object and the <paramref name="other"/> are equal by value,
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// The <see cref="Equals(Parameter)"/> methods and the overloaded <c>operator==</c>-s test for business identity, 
        /// i.e. they test for business <i>same-ness</i> by comparing the business keys.
        /// </remarks>
        public virtual bool Equals(Parameter other)
        {
            if (ReferenceEquals(other, null))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return Name    == other.Name &&
                   Type    == other.Type &&
                   IsByRef == other.IsByRef;
        }
        #endregion

        /// <summary>
        /// Determines whether this <see cref="Parameter"/> instance is equal to the specified <see cref="System.Object"/> reference.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> reference to compare with this <see cref="Parameter"/> object.</param>
        /// <returns>
        /// <see langword="false"/> if <paramref name="obj"/> is equal to <see langword="null"/>, otherwise
        /// <see langword="true"/> if <paramref name="obj"/> refers to <c>this</c> object, otherwise
        /// <see langword="true"/> if <paramref name="obj"/> <i>is an instance of</i> <see cref="Parameter"/> and 
        /// <i>the business identities</i> of the current object and the <paramref name="obj"/> are equal by value; otherwise, 
        /// <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// The <see cref="Equals(object)"/> methods and the overloaded <c>operator==</c>-s test for business identity, 
        /// i.e. they test for business <i>same-ness</i> by comparing the business keys.
        /// </remarks>
        public override bool Equals(object obj) => Equals(obj as Parameter);

        /// <summary>
        /// Serves as a hash function for the objects of <see cref="Parameter"/> and its derived types.
        /// </summary>
        /// <returns>A hash code for the current <see cref="Parameter"/> instance.</returns>
        public override int GetHashCode()
        {
            var hashCode = 23;

            hashCode = 17 * hashCode + Name.GetHashCode();
            hashCode = 17 * hashCode + Type.GetHashCode();
            hashCode = 17 * hashCode + IsByRef.GetHashCode();

            return hashCode;
        }

        /// <summary>
        /// Compares two <see cref="Parameter"/> objects.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// <see langword="true"/> if the objects are considered to be equal (<see cref="Equals(Parameter)"/>);
        /// otherwise <see langword="false"/>.
        /// </returns>
        public static bool operator ==(Parameter left, Parameter right) => ReferenceEquals(left, null)
                                                                                ? ReferenceEquals(right, null)
                                                                                : left.Equals(right);

        /// <summary>
        /// Compares two <see cref="Parameter"/> objects.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// <see langword="true"/> if the objects are not considered to be equal (<see cref="Equals(Parameter)"/>);
        /// otherwise <see langword="false"/>.
        /// </returns>
        public static bool operator !=(Parameter left, Parameter right) => !(left==right);
        #endregion
    }
}
