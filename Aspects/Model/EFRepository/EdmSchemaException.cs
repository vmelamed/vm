using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace vm.Aspects.Model.EFRepository
{
    /// <summary>
    /// Class EdmSchemaException lists the schema errors when attempted to load the mapping views.
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable]
    public class EdmSchemaException : Exception
    {
        const string ErrorsCount = nameof(ErrorsCount);
        const string Error       = nameof(Error);

        readonly string[] _errors;

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="EdmSchemaException"/> class.
        /// </summary>
        public EdmSchemaException()
            : this(null, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmSchemaException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public EdmSchemaException(
            string message)
            : this(message, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmSchemaException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public EdmSchemaException(
            string message,
            Exception innerException)
            : this(message, null, innerException)
        {
        }

        /// <summary>
        /// Initializes a <see cref="EdmSchemaException" /> with a custom <paramref name="message" />,
        /// a collection of EDM errors and an inner exception.
        /// </summary>
        /// <param name="message">The custom message.</param>
        /// <param name="errors">The EDM schema errors.</param>
        /// <param name="innerException">The exception which lead to generating this one.</param>
        public EdmSchemaException(
            string message,
            IEnumerable<EdmSchemaError> errors,
            Exception innerException = null)
            : base(message.IsNullOrWhiteSpace() ? "EDM schema errors." : message, innerException)
        {
            if (errors != null)
                _errors = errors.Select(e => e.ToString()).ToArray();
            else
                _errors = new string[0];
        }

        /// <summary>
        /// Initializes a <see cref="EdmSchemaException" /> with a collection of EDM errors and
        /// inner exception.
        /// </summary>
        /// <param name="errors">The EDM schema errors.</param>
        /// <param name="innerException">The exception which lead to generating this one.</param>
        public EdmSchemaException(
            IEnumerable<EdmSchemaError> errors,
            Exception innerException = null)
            : this(null, errors, innerException)
        {
            Contract.Requires<ArgumentNullException>(errors != null, nameof(errors));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Exception" /> class with serialized data.
        /// </summary>
        /// <param name="info">
        /// The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="StreamingContext" /> that contains contextual information about the source or destination.
        /// </param>
        protected EdmSchemaException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
            _errors = DeserializeErrors(info, context).ToArray();
        }

        IEnumerable<string> DeserializeErrors(
            SerializationInfo info,
            StreamingContext context)
        {
            var count = info.GetInt32(ErrorsCount);

            for (var i = 0; i<count; i++)
                yield return info.GetString(Error+i);
        }
        #endregion

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="SerializationInfo" /> with information about the exception.
        /// </summary>
        /// <param name="info">
        /// The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="StreamingContext" /> that contains contextual information about the source or destination.
        /// </param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(
            SerializationInfo info,
            StreamingContext context)
        {
            base.GetObjectData(info, context);

            var count = _errors.Count();

            info.AddValue(ErrorsCount, count);
            for (var i = 0; i<count; i++)
                info.AddValue(Error+i, _errors.ElementAt(i));
        }

    }
}
