#if NETCOREAPP2_1
namespace System.ComponentModel.DataAnnotations
{
    /// <summary>
    /// Specifies the metadata class to associate with a data model class.
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class MetadataTypeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataTypeAttribute" /> class.
        /// </summary>
        /// <param name="metadataClassType">Type of the metadata class to reference.</param>
        /// <exception cref="ArgumentNullException">metadataClassType cannot be <see langword="null"/></exception>
        public MetadataTypeAttribute(
            Type metadataClassType)
        {
            MetadataClassType = metadataClassType ?? throw new ArgumentNullException(nameof(metadataClassType));
        }

        /// <summary>
        /// Gets the metadata class that is associated with a data-model partial class.
        /// </summary>
        public Type MetadataClassType { get; }
    }
}
#endif