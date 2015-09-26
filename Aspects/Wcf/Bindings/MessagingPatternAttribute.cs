using System;
using System.Diagnostics.Contracts;

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
        /// I.e. <see cref="T:WebHttpBinding"/> over <seealso cref="T:WSHttpBinding"/> or <see cref="T:BasicHttpBinding"/>. Otherwise, <seealso cref="T:WSHttpBinding"/>
        /// will be used over HTTPS protocol and <see cref="T:BasicHttpBinding"/> for HTTP.
        /// </param>
        public MessagingPatternAttribute(
            string name,
            bool restful = false)
        {
            Contract.Requires<ArgumentNullException>(name != null, nameof(name));
            Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(name), "The argument \"name\" cannot be null, empty or consist of whitespace characters only.");

            Name    = name;
            Restful = restful;
        }

        /// <summary>
        /// Gets the name of the messaging pattern.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets a value indicating that when the messages are transmitted over HTTP protocol, a REST-ful style of messaging is preferred.
        /// I.e. <see cref="T:WebHttpBinding"/> over <seealso cref="T:WSHttpBinding"/>.
        /// </summary>
        public bool Restful { get; private set; }
    }
}
