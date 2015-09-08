using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace vm.Aspects.Exceptions
{
    /// <summary>
    /// Class ObjectNotFoundException represents an exceptional situation where a certain business/domain object is not found in the system.
    /// </summary>
    [Serializable]
    public class ObjectNotFoundException : ObjectException
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectNotFoundException"/> class.
        /// </summary>
        public ObjectNotFoundException()
            : this("The specified object was not found.", (Exception)null)
        {
        }

        /// <summary>
        /// Initializes a ObjectNotFoundException with a custom <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The custom message.</param>
        public ObjectNotFoundException(string message)
            : this(message, (Exception)null)
        {
        }

        /// <summary>
        /// Initializes a ObjectNotFoundException with an inner exception which lead to generating this one.
        /// </summary>
        /// <param name="innerException">The exception which lead to generating this one.</param>
        public ObjectNotFoundException(Exception innerException)
            : this(null, innerException)
        {
        }

        /// <summary>
        /// Initializes a ObjectNotFoundException with a custom <paramref name="message"/> and
        /// an inner exception which lead to generating this one.
        /// </summary>
        /// <param name="message">The custom message.</param>
        /// <param name="innerException">The exception which lead to generating this one.</param>
        public ObjectNotFoundException(
            string message,
            Exception innerException)
            : base(string.IsNullOrWhiteSpace(message) ? "The specified object was not found." : message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectNotFoundException" /> class.
        /// </summary>
        /// <param name="objectIdentifier">The object unique identifier.</param>
        /// <param name="objectType">Type of the object.</param>
        public ObjectNotFoundException(
            string objectIdentifier,
            Type objectType)
            : this(objectIdentifier, objectType, null, null)
        {
        }

        /// <summary>
        /// Initializes a ObjectNotFoundException with object identifier and type.
        /// </summary>
        /// <param name="objectIdentifier">The unique object identifier.</param>
        /// <param name="objectType">The type of the object.</param>
        /// <param name="message">The custom message.</param>
        public ObjectNotFoundException(
            string objectIdentifier,
            Type objectType,
            string message)
            : this(objectIdentifier, objectType, message, null)
        {
        }

        /// <summary>
        /// Initializes a ObjectNotFoundException with object identifier, type, and
        /// an inner exception which lead to generating this one.
        /// </summary>
        /// <param name="objectIdentifier">The object identifier.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="innerException">The exception which lead to generating this one.</param>
        public ObjectNotFoundException(
            string objectIdentifier,
            Type objectType,
            Exception innerException)
            : this(objectIdentifier, objectType, null, innerException)
        {
        }

        /// <summary>
        /// Initializes a ObjectNotFoundException with object identifier, type, a custom <paramref name="message" /> and
        /// an inner exception which lead to generating this one.
        /// </summary>
        /// <param name="objectIdentifier">The object identifier.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="message">The custom message.</param>
        /// <param name="innerException">The exception which lead to generating this one.</param>
        public ObjectNotFoundException(
            string objectIdentifier,
            Type objectType,
            string message,
            Exception innerException)
            : base(objectIdentifier, objectType, string.IsNullOrWhiteSpace(message) ? "The specified object was not found." : message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectIdentifierNotUniqueException"/> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected ObjectNotFoundException(
            SerializationInfo info, 
            StreamingContext context)
            : base(info, context)
        {
            Contract.Requires<ArgumentNullException>(info != null, nameof(info));
        }
        #endregion
    }
}
