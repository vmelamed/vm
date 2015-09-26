using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace vm.Aspects.Wcf
{
    /// <summary>
    /// Class CustomContextData represents custom (developer defined) context data that is included in the headers of every message.
    /// Based on Juval Lowe's code from [Programming WCF]
    /// </summary>
    /// <typeparam name="T">
    /// The type of the context. Must be either marked with <see cref="T:System.Runtime.Serialization.DataContractAttribute"/> or it must be serializable type.
    /// The size of the type plus the size of the other headers are limited by the property <c>MaxBufferSize</c>.
    /// </typeparam>
    [DataContract(Namespace="vm.Aspects.Wcf")]
    public class CustomDataContext<T>
    {
        static string _name;
        static string _namespace;
        static object _syncInitialize = new object();

        /// <summary>
        /// Gets the name of the custom context. The namespace and the name uniquely identify the header.
        /// </summary>
        public string Name
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);

                if (_name == null)
                    Initialize();

                return _name;
            }
        }

        /// <summary>
        /// Gets the namespace of the custom context. The namespace and the name uniquely identify the header.
        /// </summary>
        public string Namespace
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);

                if (_namespace == null)
                    Initialize();

                return _namespace;
            }
        }

        /// <summary>
        /// Initializes the name and namespace of the custom context.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if the type of the operation context class is not marked with a <see cref="T:System.Runtime.Serialization.DataContractAttribute"/> nor it is a serializable type.
        /// </exception>
        static void Initialize()
        {
            // already initialized?
            if (_name != null)
                return;

            var dataContractAttribute = typeof(T).GetCustomAttribute<DataContractAttribute>();

            // if the type is not serializable -throw exception about it
            if (dataContractAttribute == null && !typeof(T).IsSerializable)
                throw new InvalidOperationException("The type of the operation context class must be marked with a data contract attribute or must be serializable.");

            lock (_syncInitialize)
            {
                if (_name != null)
                    return;

                // if the namespace and the name of the context are not specified explicitly in the attribute,
                // assume the namespace and the name of the context type.
                _name = dataContractAttribute!=null &&
                        !string.IsNullOrWhiteSpace(dataContractAttribute.Name) 
                                ? dataContractAttribute.Name 
                                : typeof(T).Name;

                _namespace = dataContractAttribute!=null &&
                             !string.IsNullOrWhiteSpace(dataContractAttribute.Namespace) 
                                ? dataContractAttribute.Namespace 
                                : typeof(T).Namespace;
            }
        }

        /// <summary>
        /// Gets the value of the context.
        /// </summary>
        [DataMember]
        [ObjectValidator]
        public T Value { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:vm.Aspects.Wcf.CustomContext{T}"/> class with a <typeparamref name="T"/> value.
        /// </summary>
        /// <param name="value">The value.</param>
        public CustomDataContext(T value)
        {
            Initialize();
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:vm.Aspects.Wcf.CustomContext{T}"/> class with the default value of <typeparamref name="T"/>.
        /// </summary>
        public CustomDataContext()
            : this(default(T))
        {
            Initialize();
        }

        /// <summary>
        /// Gets or sets the current custom data context from the header of the current message.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Thrown by the setter if the value is <see langword="null" />.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if
        /// <list type="bullet">
        /// <item>Could not obtain the current operation context. It must be invoked from within a WCF service or OperationContextScope.</item>
        /// <item>A header with this namespace and name already exists in the message.</item>
        /// </list></exception>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification="It's OK here.")]
        public static CustomDataContext<T> Current
        {
            // get the custom header from the incoming message which is in the current operation context (called by the services)
            get
            {
                // make sure the header is initialized.
                Initialize();

                if (OperationContext.Current == null)
                    return null;

                // find the header by namespace and name
                var index = OperationContext.Current.IncomingMessageHeaders.FindHeader(_name, _namespace);

                if (index == -1)
                    return null;
                else
                    return OperationContext.Current.IncomingMessageHeaders.GetHeader<CustomDataContext<T>>(index);
            }

            set
            {
                // put the custom header into the outgoing message which is in the current operation context (called by the clients)
                Contract.Requires<ArgumentNullException>(value != null, nameof(value));
                Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(value.Name));

                // make sure the header is initialized.
                Initialize();

                var opContext = OperationContext.Current;

                if (opContext == null)
                    throw new InvalidOperationException("The current thread does not have operation context.");

                //make sure that there are no multiple CustomContextData<T> objects.
                var index = opContext.OutgoingMessageHeaders.FindHeader(_name, _namespace);

                if (index != -1)
                    throw new InvalidOperationException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "A header {1}/{0} already exists in the message.",
                            _name,
                            _namespace));

                value.AddToHeaders(opContext.OutgoingMessageHeaders);
            }
        }

        /// <summary>
        /// Adds the value of the context to the current outgoing message headers.
        /// </summary>
        /// <param name="headers">The headers collection.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="headers"/> is <see langword="null"/></exception>
        public void AddToHeaders(
            MessageHeaders headers)
        {
            Contract.Requires<ArgumentNullException>(headers != null, nameof(headers));
            Contract.Requires<InvalidOperationException>(!string.IsNullOrWhiteSpace(Name), "Property Name");

            headers.Add(
                new MessageHeader<CustomDataContext<T>>(this)
                        .GetUntypedHeader(Name, Namespace));
        }
    }
}
