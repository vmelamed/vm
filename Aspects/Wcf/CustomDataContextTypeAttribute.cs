using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using vm.Aspects.Wcf.Clients;

namespace vm.Aspects.Wcf
{
    /// <summary>
    /// Specifies the type of the custom context data that is expected in the 
    /// messages' headers of each call to the type that this attribute is applied to.
    /// In other words the attribute makes sense only when applied to service or operation contracts.
    /// (Use <see cref="ContextLightClient{T,S}"/> and <see cref="CustomDataContext{T}"/>.)
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
    [Serializable]
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Interface |
        AttributeTargets.Method,
        AllowMultiple = true,
        Inherited = true)]
    public sealed class CustomDataContextTypeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomDataContextTypeAttribute" /> class with
        /// the type of of the custom header that is (optionally) expected in the message headers.
        /// The attribute makes sense only when applied to service or operation contracts.
        /// </summary>
        /// <param name="customDataContextType">The type of the custom context data.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="customDataContextType"/> is <see langword="null"/></exception>
        public CustomDataContextTypeAttribute(Type customDataContextType)
        {
            Contract.Requires<ArgumentNullException>(customDataContextType != null, nameof(customDataContextType));

            CustomDataContextType = customDataContextType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomDataContextTypeAttribute" /> class with
        /// the type of of the custom header that is (optionally) expected in the message headers.
        /// The attribute makes sense only when applied to service or operation contracts.
        /// </summary>
        /// <param name="isRequired">if set to <see langword="true" /> specifies that the custom data header (the context) is required (the default).</param>
        /// <remarks>Usually this form of the constructor is used on method and the type of the header is inherited by the attribute on the parent class or structure.</remarks>
        /// <example>
        /// In the following example the interface IBusiness requires for each method to receive a custom data context (header) of type BusinessDataContext.
        /// The method Ping however overrides this requirement by attributing it with [CustomDataContextType(false)] and allowing to skip the header for this method.
        /// This can be useful if you would like to be able to 'ping' the service from a browser, postman, fiddler, etc.
        /// <![CDATA[
        /// [CustomDataContextType(typeof(BusinessDataContext))]
        /// public interface IBusiness
        /// {
        ///     [OperationContract]
        ///     [CustomDataContextType(false)]
        ///     string Ping(string pong);
        ///     
        ///     [OperationContract]
        ///     void Execute(string id);
        ///     
        ///     ...
        /// }
        /// ]]>
        /// </example>
        public CustomDataContextTypeAttribute(bool isRequired)
        {
            IsOptional = !isRequired;
        }

        /// <summary>
        /// Gets the type of the context data.
        /// </summary>
        /// <value>The type of the context data.</value>
        public Type CustomDataContextType { get; }

        /// <summary>
        /// Gets or sets a value indicating whether custom context header is required or optional.
        /// The default is false, i.e. required.
        /// </summary>
        public bool IsOptional { get; set; }
    }
}
