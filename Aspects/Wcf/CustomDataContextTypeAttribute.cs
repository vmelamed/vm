using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using vm.Aspects.Wcf.Clients;
using vm.Aspects.Wcf;

namespace vm.Aspects.Wcf
{
    /// <summary>
    /// Specifies the type of the custom context data that is expected in the 
    /// messages' headers of each call to the type that this attribute is applied to.
    /// In other words the attribute makes sense only when applied to service or operation contracts.
    /// (Use <see cref="ContextLightClient{T,S}"/> and <see cref="CustomDataContext{T}"/>.)
    /// </summary>
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
