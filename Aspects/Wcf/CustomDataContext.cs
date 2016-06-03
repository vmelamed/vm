using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Text;
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
    [DataContract(Namespace = "vm.Aspects.Wcf")]
    public class CustomDataContext<T>
    {
        static string _name;
        static string _namespace;
        static string _webHeaderName;
        static object _syncInitialize = new object();

        /// <summary>
        /// Gets the name of the custom context. The namespace and the name uniquely identify the header.
        /// </summary>
        public string Name
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                Contract.Ensures(Contract.Result<string>().Length > 0);
                Contract.Ensures(Contract.Result<string>().Any(c => !char.IsWhiteSpace(c)));

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
        /// Gets the name of the web header.
        /// </summary>
        public string WebHeaderName
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);

                if (_webHeaderName == null)
                    Initialize();

                return _webHeaderName;
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

                _webHeaderName = string.IsNullOrWhiteSpace(_namespace)
                                    ? _name
                                    : string.Format(CultureInfo.InvariantCulture, "{0}-{1}", _namespace, _name);
            }
        }

        /// <summary>
        /// Gets the value of the context.
        /// </summary>
        [DataMember(Name = "value")]
        [ObjectValidator]
        public T Value { get; set; }

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
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "It's OK here.")]
        public static CustomDataContext<T> Current
        {
            // get the custom header from the incoming message which is in the current operation context (called by the services)
            get
            {
                // make sure the header is initialized.
                Initialize();

                if (OperationContext.Current == null)
                    return null;

                WebOperationContext _webContext =  WebOperationContext.Current ??
                                                        CallContext.LogicalGetData(nameof(WebOperationContext)) as WebOperationContext;
                // in case AsyncServiceSynchronizationContext is installed

                // find the header by namespace and name
                if (_webContext != null)
                {
                    var serialized = _webContext.IncomingRequest.Headers.Get(_webHeaderName);

                    if (string.IsNullOrWhiteSpace(serialized))
                        return null;

                    using (var stream = new MemoryStream(Encoding.Unicode.GetBytes(serialized)))
                    {
                        var context = new CustomDataContext<T>((T)GetJsonSerializer().ReadObject(stream));

                        var onSerializedAttribute = typeof(T)
                                                        .GetMethods(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                                                        .FirstOrDefault(mi => mi.GetCustomAttribute<OnSerializedAttribute>() != null);

                        onSerializedAttribute?.Invoke(context.Value, new object[] { new StreamingContext() });
                        return context;
                    }
                }
                else
                {
                    var index = OperationContext.Current.IncomingMessageHeaders.FindHeader(_name, _namespace);

                    if (index != -1)
                        return OperationContext.Current.IncomingMessageHeaders.GetHeader<CustomDataContext<T>>(index);
                    else
                        return null;
                }
            }

            set
            {
                // put the custom header into the outgoing message which is in the current operation context (called by the clients)
                Contract.Requires<ArgumentNullException>(value != null, nameof(value));
                Contract.Requires<ArgumentException>(value.Name.Length > 0, "The argument "+nameof(value.Name)+" cannot be empty or consist of whitespace characters only.");
                Contract.Requires<ArgumentException>(value.Name.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(value.Name)+" cannot be empty or consist of whitespace characters only.");
                Contract.Requires<InvalidOperationException>(OperationContext.Current != null, "The current thread does not have operation context.");

                // make sure the header is initialized.
                Initialize();

                //make sure that there are no multiple CustomContextData<T> objects.
                if (WebOperationContext.Current == null)
                {
                    if (OperationContext.Current.OutgoingMessageHeaders.FindHeader(_name, _namespace) > -1)
                        throw new InvalidOperationException(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "A header {1}/{0} already exists in the message.",
                                _name,
                                _namespace));

                    value.AddToHeaders(OperationContext.Current.OutgoingMessageHeaders);
                }
                else
                {
                    if (WebOperationContext.Current.OutgoingRequest.Headers.Get(_webHeaderName) != null)
                        throw new InvalidOperationException(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "A header {0} already exists in the message.",
                                _webHeaderName));

                    value.AddToHeaders(WebOperationContext.Current.OutgoingRequest.Headers);
                }
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

            headers.Add(
                new MessageHeader<CustomDataContext<T>>(this)
                        .GetUntypedHeader(Name, Namespace));
        }

        /// <summary>
        /// Adds the value of the context to the current outgoing web message headers.
        /// </summary>
        /// <param name="headers">The headers collection.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="headers"/> is <see langword="null"/></exception>
        public void AddToHeaders(
            WebHeaderCollection headers)
        {
            Contract.Requires<ArgumentNullException>(headers != null, nameof(headers));

            using (var stream = new MemoryStream())
            {
                var serializer = GetJsonSerializer();

                serializer.WriteObject(stream, Value);
                headers.Add(_webHeaderName, Encoding.Default.GetString(stream.ToArray()));
            }
        }

        static DataContractJsonSerializer GetJsonSerializer() => new DataContractJsonSerializer(
                                                                        typeof(T),
                                                                        new DataContractJsonSerializerSettings
                                                                        {
                                                                            DateTimeFormat            = new DateTimeFormat("o", CultureInfo.InvariantCulture),
                                                                            EmitTypeInformation       = EmitTypeInformation.Never,
                                                                            UseSimpleDictionaryFormat = true,
                                                                        });
    }
}
