using System;
namespace vm.Aspects
{
    /// <summary>
    /// Class RegistrationLookup. The instances of this class are used as composite lookup keys in the maps returned by the method 
    /// <see cref="M:vm.Aspects.DIContainer.GetRegistrationDictionary"/>. The keys consist of a registered type and 
    /// possibly registration name.
    /// </summary>
    public class RegistrationLookup : IEquatable<RegistrationLookup>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationLookup"/> class with a registered type and possibly registration name.
        /// </summary>
        /// <param name="type">The registered type.</param>
        /// <param name="name">The registration name.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="type"/> is <see langword="null"/></exception>
        public RegistrationLookup(
            Type type,
            string name = null)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            RegisteredType = type;

            if (string.IsNullOrEmpty(name))
                Name = string.Empty;
            else
                Name = name;
        }

        /// <summary>
        /// Gets the registered type.
        /// </summary>
        public Type RegisteredType { get; }

        /// <summary>
        /// Gets the registration name.
        /// </summary>
        public string Name { get; }

        #region Identity rules implementation.
        #region IEquatable<RegistrationLookup> Members
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        public virtual bool Equals(RegistrationLookup other)
        {
            if (ReferenceEquals(other, null))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return RegisteredType == other.RegisteredType &&
                   Name           == other.Name;
        }
        #endregion

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><see langword="true"/> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj) => Equals(obj as RegistrationLookup);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            var hashCode = Constants.HashInitializer;

            hashCode = Constants.HashMultiplier * hashCode + RegisteredType.GetHashCode();
            hashCode = Constants.HashMultiplier * hashCode + Name.GetHashCode();

            return hashCode;
        }

        /// <summary>
        /// Implements the == operator.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the operation.</returns>
        public static bool operator==(RegistrationLookup left, RegistrationLookup right) => ReferenceEquals(left, null) 
                                                                                                ? ReferenceEquals(right, null) 
                                                                                                : left.Equals(right);

        /// <summary>
        /// Implements the != operator.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The result of the operation.</returns>
        public static bool operator!=(RegistrationLookup left, RegistrationLookup right) => !(left==right);
        #endregion
    }
}
