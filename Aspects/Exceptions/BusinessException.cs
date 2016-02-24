using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.Common.Properties;

namespace vm.Aspects.Exceptions
{
    /// <summary>
    /// Class BusinessException represents a general violation of some business rule or problem with some business logic.
    /// </summary>
    [Serializable]
    public class BusinessException : Exception
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:BusinessException"/> class.
        /// </summary>
        public BusinessException()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a <see cref="T:BusinessException"/> with a custom <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The custom message.</param>
        public BusinessException(string message)
            : this(message, null)
        {
        }

        /// <summary>
        /// Initializes a <see cref="T:BusinessException"/> with an inner exception which lead to generating this one.
        /// </summary>
        /// <param name="innerException">The exception which lead to generating this one.</param>
        public BusinessException(Exception innerException)
            : this(null, innerException)
        {
        }

        /// <summary>
        /// Initializes a <see cref="T:BusinessException"/> with a custom <paramref name="message"/> and
        /// an inner exception which lead to generating this one.
        /// </summary>
        /// <param name="message">The custom message.</param>
        /// <param name="innerException">The exception which lead to generating this one.</param>
        public BusinessException(
            string message,
            Exception innerException)
            : base(string.IsNullOrWhiteSpace(message) ? "Violation of business logic or rule." : message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Exception" /> class with serialized data.
        /// </summary>
        /// <param name="info">
        /// The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.
        /// </param>
        protected BusinessException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
        ///   </PermissionSet>
        public override string ToString() => this.DumpString();
    }
}
