using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.ServiceModel;

namespace vm.Aspects.Wcf.Bindings
{
    /// <summary>
    /// Class MessagingPatternAttribute defines the messaging pattern for an interface, service or client.
    /// </summary>
    [Serializable]
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Interface,
        AllowMultiple = false,
        Inherited = true)]
    public sealed class MessagingPatternAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingPatternAttribute" /> class.
        /// </summary>
        /// <param name="name">The name of the pattern.</param>
        /// <param name="restful">
        /// If set to <see langword="true" />  when the messages are transmitted over HTTP or HTTPS protocols, a REST-ful style of messaging is preferred.
        /// I.e. <see cref="WebHttpBinding"/> over <seealso cref="WSHttpBinding"/> or <see cref="BasicHttpBinding"/>. Otherwise, <seealso cref="WSHttpBinding"/>
        /// will be used over HTTPS protocol and <see cref="BasicHttpBinding"/> for HTTP.
        /// </param>
        public MessagingPatternAttribute(
            string name,
            bool restful = false)
        {
            Contract.Requires<ArgumentException>(name != null  &&  name.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(name)+" cannot be null, empty string or consist of whitespace characters only.");

            Name    = name;
            Restful = restful;
        }

        /// <summary>
        /// Gets the name of the messaging pattern.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a value indicating that when the messages are transmitted over HTTP protocol, a REST-ful style of messaging is preferred.
        /// I.e. <see cref="WebHttpBinding"/> over <seealso cref="WSHttpBinding"/>.
        /// </summary>
        public bool Restful { get; }

        /// <summary>
        /// Gets a value indicating that whether the messages transmitted over HTTP protocol, a BasicHTTP style of messaging is preferred.
        /// I.e. <see cref="BasicHttpBinding"/> over <seealso cref="WSHttpBinding"/>.
        /// </summary>
        public bool BasicHttp { get; set; }
    }
}
