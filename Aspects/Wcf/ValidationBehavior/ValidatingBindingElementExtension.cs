using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.Diagnostics.Contracts;

namespace vm.Aspects.Wcf.ValidationBehavior
{
    /// <summary>
    /// Represents a configuration element that specifies a message validation.
    /// </summary>
    public class ValidatingBindingElementExtension : BindingElementExtensionElement
    {
        /// <summary>
        /// When overridden in a derived class, gets the <see cref="T:System.Type"></see> object that represents the custom binding element.
        /// </summary>
        /// <value></value>
        /// <returns>A <see cref="T:System.Type"></see> object that represents the custom binding type.</returns>
        public override Type BindingElementType
        {
            get 
            {
                Contract.Ensures(Contract.Result<Type>() != null);

                return typeof(ValidatingBindingElement); 
            }
        }

        /// <summary>
        /// When overridden in a derived class, returns a custom binding element object.
        /// </summary>
        /// <returns>
        /// A custom <see cref="T:System.ServiceModel.Channels.BindingElement"></see> object.
        /// </returns>
        protected override BindingElement CreateBindingElement()
        {
            Contract.Ensures(Contract.Result<BindingElement>() != null);

            return new ValidatingBindingElement();
        }
    }
}
