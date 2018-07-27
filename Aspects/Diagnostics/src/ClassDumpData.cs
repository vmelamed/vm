using System;

namespace vm.Aspects.Diagnostics
{
    /// <summary>
    /// Encapsulates a pair of a class or struct metadata and the corresponding DumpAttribute.
    /// </summary>
    internal struct ClassDumpData : IEquatable<ClassDumpData>
    {
        public ClassDumpData(
            Type metadata,
            DumpAttribute dumpAttribute = null)
        {
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));

            if (dumpAttribute != null)
                DumpAttribute = dumpAttribute;
            else
            if (Attribute.IsDefined(metadata, typeof(DumpAttribute)))
                DumpAttribute = metadata.GetCustomAttribute<DumpAttribute>();
            else
                DumpAttribute = DumpAttribute.Default;
        }

        /// <summary>
        /// Gets or sets the metadata associated with a given class or struct (which may be the class or struct itself).
        /// </summary>
        public Type Metadata { get; set; }

        /// <summary>
        /// Gets or sets the dump attribute applied on a given class or struct.
        /// </summary>
        public DumpAttribute DumpAttribute { get; set; }

        /// <summary>
        /// Implements the instance over type priority rule for the <see cref="DumpAttribute.DumpNullValues"/> property.
        /// </summary>
        /// <param name="instanceAttribute">The instance associated attribute.</param>
        /// <returns>
        /// The property value that should be in effect.
        /// </returns>
        public ShouldDump DumpNullValues(DumpAttribute instanceAttribute) => instanceAttribute != null  &&
                                                                             instanceAttribute.DumpNullValues != ShouldDump.Default
                                                                                  ? instanceAttribute.DumpNullValues
                                                                                  : DumpAttribute.DumpNullValues == ShouldDump.Default
                                                                                      ? ShouldDump.Dump
                                                                                      : DumpAttribute.DumpNullValues;

        /// <summary>
        /// Implements the instance over type priority rule for the <see cref="DumpAttribute.DefaultProperty"/> property.
        /// </summary>
        /// <param name="instanceAttribute">The attribute associated with the instance.</param>
        /// <returns>
        /// The property value that should be in effect.
        /// </returns>
        public string DefaultProperty(DumpAttribute instanceAttribute) => instanceAttribute != null  &&
                                                                          !instanceAttribute.DefaultProperty.IsNullOrWhiteSpace()
                                                                               ? instanceAttribute.DefaultProperty
                                                                               : DumpAttribute.DefaultProperty;

        /// <summary>
        /// Implements the instance over type priority rule for the <see cref="DumpAttribute.RecurseDump"/> property.
        /// </summary>
        /// <param name="instanceAttribute">The instance associated attribute.</param>
        /// <returns>
        /// The property value that should be in effect. Never returns <see cref="ShouldDump.Default"/>.
        /// </returns>
        public ShouldDump RecurseDump(DumpAttribute instanceAttribute = null) => instanceAttribute != null  &&
                                                                                 instanceAttribute.RecurseDump != ShouldDump.Default
                                                                                      ? instanceAttribute.RecurseDump
                                                                                      : DumpAttribute.RecurseDump != ShouldDump.Default
                                                                                          ? DumpAttribute.RecurseDump
                                                                                          : ShouldDump.Dump;

        #region Identity rules
        #region IEquatable<ClassDumpData> Members
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">A copy of another object of type <see cref="ClassDumpData"/> to compare with this object.</param>
        /// <returns>
        /// <c>true</c> if the values of the fields are equal; otherwise <c>false</c>.
        /// </returns>
        public bool Equals(ClassDumpData other)
            => Metadata      == other.Metadata  &&
               DumpAttribute == other.DumpAttribute;
        #endregion

        /// <summary>
        /// Determines whether this <see cref="ClassDumpData"/> instance is equal to the specified <see cref="System.Object"/> reference.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> reference to compare with this <see cref="ClassDumpData"/> object.</param>
        /// <returns>
        /// <c>false</c> if <paramref name="obj"/> is equal to <c>null</c>, otherwise
        /// <c>true</c> if <paramref name="obj"/> refers to <c>this</c> object, otherwise
        /// <c>true</c> if <paramref name="obj"/> <i>is an instance of</i> <see cref="ClassDumpData"/> and 
        /// the fields values of the current object and the <paramref name="obj"/> are equal by value; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
            => obj is ClassDumpData ? Equals((ClassDumpData)obj) : false;

        /// <summary>
        /// Serves as a hash function for the objects of <see cref="ClassDumpData"/> and its derived types.
        /// </summary>
        /// <returns>A hash code for the current <see cref="ClassDumpData"/> instance.</returns>
        public override int GetHashCode()
        {
            var hashCode = Constants.HashInitializer;

            unchecked
            {
                hashCode = Constants.HashMultiplier * hashCode + Metadata.GetHashCode();
                hashCode = Constants.HashMultiplier * hashCode + DumpAttribute.GetHashCode();
            }

            return hashCode;
        }

        /// <summary>
        /// Compares two <see cref="ClassDumpData"/> objects.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// <c>true</c> if the objects are considered to be equal (<see cref="M:IEquatable.Equals{ClassDumpData}"/>);
        /// otherwise <c>false</c>.
        /// </returns>
        public static bool operator ==(ClassDumpData left, ClassDumpData right)
            => left.Equals(right);

        /// <summary>
        /// Compares two <see cref="ClassDumpData"/> objects.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// <c>true</c> if the objects are not considered to be equal (<see cref="M:IEquatable.Equals{ClassDumpData}"/>);
        /// otherwise <c>false</c>.
        /// </returns>
        public static bool operator !=(ClassDumpData left, ClassDumpData right)
            => !(left==right);
        #endregion
    }
}
