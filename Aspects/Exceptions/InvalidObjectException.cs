using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using vm.Aspects.Diagnostics;

namespace vm.Aspects.Exceptions
{
    /// <summary>
    /// Exception thrown as a result of a failed object validation. 
    /// To find out all reasons for the exception examine the property <see cref="ValidationResults"/>.
    /// </summary>
    [Serializable]
    public sealed class InvalidObjectException : BusinessException
    {
        #region Properties
        /// <summary>
        /// Gets or sets a <see cref="ValidationResults"/> object, describing all validation problems.
        /// </summary>
        public ValidationResults ValidationResults { get; private set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidObjectException"/> class.
        /// </summary>
        public InvalidObjectException()
            : this(null, null, null)
        {
        }

        /// <summary>
        /// Initializes a ValidationException with a custom <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The custom message.</param>
        public InvalidObjectException(string message)
            : this(null, message, null)
        {
        }

        /// <summary>
        /// Initializes a ValidationException with a custom <paramref name="message"/> and an inner exception which lead to generating this one.
        /// </summary>
        /// <param name="message">The custom message.</param>
        /// <param name="innerException">The exception which lead to generating this one.</param>
        public InvalidObjectException(
            string message,
            Exception innerException)
            : this(null, message, innerException)
        {
        }

        /// <summary>
        /// Initializes a ValidationException with a <see cref="ValidationResults"/> object, a custom <paramref name="message"/> and
        /// an inner exception which lead to generating this one.
        /// </summary>
        /// <param name="validationResults"><see cref="ValidationResults"/> object, describing all validation problems.</param>
        /// <param name="message">The custom message.</param>
        /// <param name="innerException">The exception which lead to generating this one.</param>
        public InvalidObjectException(
            ValidationResults validationResults,
            string message = null,
            Exception innerException = null)
            : base(string.IsNullOrWhiteSpace(message) ? "Validation failed." : message, innerException)
        {
            Debug.Assert(validationResults==null || !validationResults.IsValid);

            ValidationResults = validationResults ?? new ValidationResults();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Exception" /> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        private InvalidObjectException(
            SerializationInfo info, 
            StreamingContext context)
            : base(info, context)
        {
            Contract.Requires<ArgumentNullException>(info != null, nameof(info));

            ValidationResults = (ValidationResults)info.GetValue("ValidationResults", typeof(ValidationResults));
        }
        #endregion

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter" />
        ///   </PermissionSet>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(
            SerializationInfo info, 
            StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ValidationResults", ValidationResults, typeof(ValidationResults));
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*"/>
        ///   </PermissionSet>
        public override string ToString()
        {
            return this.DumpString();
        }
    }
}
