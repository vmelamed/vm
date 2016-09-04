using System;

namespace vm.Aspects.FtpTransfer
{
    /// <summary>
    /// Represents a file list entry in the list of file (Unix or DOS style) coming from the FTP site.
    /// </summary>
    /// <seealso cref="IEquatable{FileListEntry}" />
    public class FtpFileListEntry : IEquatable<FtpFileListEntry>
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is folder or a file.
        /// </summary>
        public bool IsFolder { get; set; }

        /// <summary>
        /// Gets or sets the access rights (Unix only).
        /// </summary>
        public string AccessRights { get; set; }

        /// <summary>
        /// Gets or sets the file number (Unix only).
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Gets or sets the owner's identifier.
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// Gets or sets the group (Unix).
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// Gets or sets the size of the file.
        /// </summary>
        public int FileSize { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the file was created.
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        public string Name { get; set; }

        #region IEquatable<FtpFileListEntry> Members
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        public virtual bool Equals(FtpFileListEntry other)
        {
            if (ReferenceEquals(other, null))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return IsFolder     == other.IsFolder     &&
                   AccessRights == other.AccessRights &&
                   Number       == other.Number       &&
                   Owner        == other.Owner        &&
                   Group        == other.Group        &&
                   FileSize     == other.FileSize     &&
                   Created      == other.Created      &&
                   Name         == other.Name;
        }
        #endregion

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><see langword="true" /> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <see langword="false" />.</returns>
        public override bool Equals(object obj)
            => Equals(obj as FtpFileListEntry);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            var hashCode = Constants.HashInitializer;

            hashCode = Constants.HashMultiplier * hashCode + IsFolder.GetHashCode();
            hashCode = Constants.HashMultiplier * hashCode + (AccessRights!=null ? AccessRights.GetHashCode() : 0);
            hashCode = Constants.HashMultiplier * hashCode + Number.GetHashCode();
            hashCode = Constants.HashMultiplier * hashCode + (Owner!=null ? Owner.GetHashCode() : 0);
            hashCode = Constants.HashMultiplier * hashCode + (Group!=null ? Group.GetHashCode() : 0);
            hashCode = Constants.HashMultiplier * hashCode + FileSize.GetHashCode();
            hashCode = Constants.HashMultiplier * hashCode + Created.GetHashCode();
            hashCode = Constants.HashMultiplier * hashCode + (Name!=null ? Name.GetHashCode() : 0);

            return hashCode;
        }

        /// <summary>
        /// Compares two <see cref="FtpFileListEntry"/> objects.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// <c>true</c> if the objects are considered to be equal (<see cref="Equals(FtpFileListEntry)"/>);
        /// otherwise <c>false</c>.
        /// </returns>
        public static bool operator ==(
            FtpFileListEntry left,
            FtpFileListEntry right)
            => ReferenceEquals(left, null)
                        ? ReferenceEquals(right, null)
                        : left.Equals(right);

        /// <summary>
        /// Compares two <see cref="FtpFileListEntry"/> objects.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// <c>true</c> if the objects are not considered to be equal (<see cref="Equals(FtpFileListEntry)"/>);
        /// otherwise <c>false</c>.
        /// </returns>
        public static bool operator !=(
            FtpFileListEntry left,
            FtpFileListEntry right)
            => !(left==right);
    }
}
