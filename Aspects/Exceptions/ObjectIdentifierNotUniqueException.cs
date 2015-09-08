using System;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using vm.Aspects.Properties;

namespace vm.Aspects.Exceptions
{
    /// <summary>
    /// Class ObjectIdentifierNotUniqueException represents a situation where a new business/domain object is introduced in the system but its
    /// unique identifier (business key) is already in use by other object.
    /// </summary>
    [Serializable]
    public class ObjectIdentifierNotUniqueException : ObjectException
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectIdentifierNotUniqueException"/> class.
        /// </summary>
        public ObjectIdentifierNotUniqueException()
            : this(null, (Exception)null)
        {
        }

        /// <summary>
        /// Initializes a ObjectIdentifierNotUniqueException with a custom <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The custom message.</param>
        public ObjectIdentifierNotUniqueException(string message)
            : this(message, (Exception)null)
        {
        }

        /// <summary>
        /// Initializes a ObjectIdentifierNotUniqueException with an inner exception which lead to generating this one.
        /// </summary>
        /// <param name="innerException">The exception which lead to generating this one.</param>
        public ObjectIdentifierNotUniqueException(Exception innerException)
            : this(null, innerException)
        {
        }

        /// <summary>
        /// Initializes a ObjectIdentifierNotUniqueException with a custom <paramref name="message"/> and
        /// an inner exception which lead to generating this one.
        /// </summary>
        /// <param name="message">The custom message.</param>
        /// <param name="innerException">The exception which lead to generating this one.</param>
        public ObjectIdentifierNotUniqueException(
            string message,
            Exception innerException)
            : base(string.IsNullOrWhiteSpace(message) ? "The specified object identifier is not unique." : message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectIdentifierNotUniqueException" /> class.
        /// </summary>
        /// <param name="objectIdentifier">The object unique identifier.</param>
        /// <param name="objectType">Type of the object.</param>
        public ObjectIdentifierNotUniqueException(
            string objectIdentifier,
            Type objectType)
            : this(objectIdentifier, objectType, null, null)
        {
        }

        /// <summary>
        /// Initializes a ObjectIdentifierNotUniqueException with a custom <paramref name="message" />.
        /// </summary>
        /// <param name="objectIdentifier">The unique object identifier.</param>
        /// <param name="objectType">The type of the object.</param>
        /// <param name="message">The custom message.</param>
        public ObjectIdentifierNotUniqueException(
            string objectIdentifier,
            Type objectType,
            string message)
            : this(objectIdentifier, objectType, message, null)
        {
        }

        /// <summary>
        /// Initializes a ObjectIdentifierNotUniqueException with a object identifier and object type.
        /// </summary>
        /// <param name="objectIdentifier">The object identifier.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="innerException">The exception which lead to generating this one.</param>
        public ObjectIdentifierNotUniqueException(
            string objectIdentifier,
            Type objectType,
            Exception innerException)
            : this(objectIdentifier, objectType, null, innerException)
        {
        }

        /// <summary>
        /// Initializes a ObjectIdentifierNotUniqueException with a custom <paramref name="message" /> and
        /// an inner exception which lead to generating this one.
        /// </summary>
        /// <param name="objectIdentifier">The object identifier.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="message">The custom message.</param>
        /// <param name="innerException">The exception which lead to generating this one.</param>
        public ObjectIdentifierNotUniqueException(
            string objectIdentifier,
            Type objectType,
            string message,
            Exception innerException)
            : base(objectIdentifier, objectType, string.IsNullOrWhiteSpace(message) ? "The specified object identifier is not unique." : message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectIdentifierNotUniqueException" /> class (part of the .NET objects serialization pattern).
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected ObjectIdentifierNotUniqueException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
            Contract.Requires<ArgumentNullException>(info != null, nameof(info));
        }
        #endregion
    }
}
