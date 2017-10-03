using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace vm.Aspects.Exceptions
{
    /// <summary>
    /// Class ObjectException represents a general problem with some business/domain object.
    /// </summary>
    [Serializable]
    public class ObjectException : BusinessException
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectException"/> class.
        /// </summary>
        public ObjectException()
            : this(null, (Exception)null)
        {
        }

        /// <summary>
        /// Initializes an ObjectException with a custom <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The custom message.</param>
        public ObjectException(string message)
            : this(message, (Exception)null)
        {
        }

        /// <summary>
        /// Initializes an ObjectException with an inner exception which lead to generating this one.
        /// </summary>
        /// <param name="innerException">The exception which lead to generating this one.</param>
        public ObjectException(Exception innerException)
            : this(null, innerException)
        {
        }

        /// <summary>
        /// Initializes an ObjectException with a custom <paramref name="message"/> and
        /// an inner exception which lead to generating this one.
        /// </summary>
        /// <param name="message">The custom message.</param>
        /// <param name="innerException">The exception which lead to generating this one.</param>
        public ObjectException(
            string message,
            Exception innerException)
            : base(string.IsNullOrWhiteSpace(message) ? "Failure with a business object." : message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectException" /> class with the unique identifier of the object and its type.
        /// </summary>
        /// <param name="objectIdentifier">The object unique identifier.</param>
        /// <param name="objectType">Type of the object.</param>
        public ObjectException(
            string objectIdentifier,
            Type objectType)
            : this(objectIdentifier, objectType, null, null)
        {
        }

        /// <summary>
        /// Initializes a ObjectException with a custom <paramref name="message" />, the unique identifier of the object and its type.
        /// </summary>
        /// <param name="objectIdentifier">The unique object identifier.</param>
        /// <param name="objectType">The type of the object.</param>
        /// <param name="message">The custom message.</param>
        public ObjectException(
            string objectIdentifier,
            Type objectType,
            string message)
            : this(objectIdentifier, objectType, message, null)
        {
        }

        /// <summary>
        /// Initializes a ObjectException with the unique identifier of the object, its type and
        /// an inner exception which lead to generating this one.
        /// </summary>
        /// <param name="objectIdentifier">The object identifier.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="innerException">The exception which lead to generating this one.</param>
        public ObjectException(
            string objectIdentifier,
            Type objectType,
            Exception innerException)
            : this(objectIdentifier, objectType, null, innerException)
        {
        }

        /// <summary>
        /// Initializes a ObjectException with a custom <paramref name="message" />, the unique identifier of the object, its type and
        /// an inner exception which lead to generating this one.
        /// </summary>
        /// <param name="objectIdentifier">The object identifier.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="message">The custom message.</param>
        /// <param name="innerException">The exception which lead to generating this one.</param>
        public ObjectException(
            string objectIdentifier,
            Type objectType,
            string message,
            Exception innerException)
            : this(message, innerException)
        {
            ObjectIdentifier = objectIdentifier;

            if (objectType != null)
                ObjectType = objectType.FullName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectException"/> class (part of the .NET object serialization pattern).
        /// </summary>
        /// <param name="info">The serialization information.</param>
        /// <param name="context">The streaming context.</param>
        protected ObjectException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            ObjectIdentifier = info.GetString("ObjectIdentifier");
            ObjectType       = info.GetString("ObjectType");
        }
        #endregion

        /// <summary>
        /// Gets the unique identifier of the object causing the problem.
        /// </summary>
        public string ObjectIdentifier { get; protected set; }

        /// <summary>
        /// Gets the string representation of the type of the object causing the problem.
        /// </summary>
        public string ObjectType { get; protected set; }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.
        /// </summary>
        /// <param name="info">
        /// The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.
        /// </param>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter" />
        /// </PermissionSet>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("ObjectIdentifier", ObjectIdentifier);
            info.AddValue("ObjectType", ObjectType);
        }

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
