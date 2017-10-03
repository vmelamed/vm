using System;
using System.Runtime.Serialization;

namespace vm.Aspects.Exceptions
{
    /// <summary>
    /// Class RepeatableOperationException represents a transient exceptional situation, 
    /// where the current operation can be attempted later with the same parameters.
    /// </summary>
    [Serializable]
    public class RepeatableOperationException : BusinessException
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessException"/> class.
        /// </summary>
        public RepeatableOperationException()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a BusinessException with a custom <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The custom message.</param>
        public RepeatableOperationException(string message)
            : this(message, null)
        {
        }

        /// <summary>
        /// Initializes a BusinessException with a custom <paramref name="message"/> and
        /// an inner exception which lead to generating this one.
        /// </summary>
        /// <param name="message">The custom message.</param>
        /// <param name="innerException">The exception which lead to generating this one.</param>
        public RepeatableOperationException(string message, Exception innerException)
            : base(string.IsNullOrWhiteSpace(message) ? "The operation was not successful, however it may succeed later. See inner exception for details." : message, innerException)
        {
        }

        /// <summary>
        /// Initializes a BusinessException with an inner exception which lead to generating this one.
        /// </summary>
        /// <param name="innerException">The exception which lead to generating this one.</param>
        public RepeatableOperationException(Exception innerException)
            : this(null, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RepeatableOperationException" /> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected RepeatableOperationException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
        }
        #endregion
    }
}
