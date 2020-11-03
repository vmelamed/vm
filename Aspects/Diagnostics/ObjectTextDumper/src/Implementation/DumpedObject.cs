using System;

namespace vm.Aspects.Diagnostics.Implementation
{
    /// <summary>
    /// Stores pair of type and object which has already been dumped in the current call.
    /// </summary>
    struct DumpedObject : IEquatable<DumpedObject>
    {
        public readonly object Object;
        public readonly Type Type;

        public DumpedObject(object obj, Type type)
        {
            Object = obj;
            Type = type;
        }

        #region Identity rules implementation
        #region IEquatable<DumpedObject> Members
        public bool Equals(DumpedObject other)
            => Object.Equals(other.Object)  &&
               Type.Equals(other.Type);
        #endregion

        /// <summary>
        /// Determines whether the specified <see cref="object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
            => obj is DumpedObject dobj && Equals(dobj);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            var hash = Constants.HashInitializer;

            unchecked
            {
                hash = hash * Constants.HashMultiplier + Object.GetHashCode();
                hash = hash * Constants.HashMultiplier + Type.GetHashCode();
            }

            return hash;
        }

        /// <summary>
        /// Overloads the equals operator <c>==</c> to compare two values of this struct by invoking the <see cref="DumpedObject.Equals(object)"/> method.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// <c>true</c> if the left operand is equal by value to the right operand; otherwise <c>false</c>.
        /// </returns>
        public static bool operator ==(DumpedObject left, DumpedObject right)
            => left.Equals(right);

        /// <summary>
        /// Overloads the not equals operator <c>!=</c> to compare two values of this struct by invoking the <see cref="DumpedObject.Equals(object)"/> method.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// <c>true</c> if the left operand is not equal by value to the right operand; otherwise <c>false</c>.
        /// </returns>
        public static bool operator !=(DumpedObject left, DumpedObject right)
            => !(left==right);
        #endregion
    }
}
