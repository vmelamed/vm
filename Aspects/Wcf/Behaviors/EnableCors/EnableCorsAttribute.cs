using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace vm.Aspects.Wcf.Behaviors
{
    /// <summary>
    /// Enable CORS for endpoints with <see cref="WebHttpBinding"/>.
    /// </summary>
    /// <seealso cref="Attribute" />
    /// <seealso cref="IOperationBehavior" />
    /// <remarks>
    /// Based on: https://blogs.msdn.microsoft.com/carlosfigueira/2012/05/14/implementing-cors-support-in-wcf/
    /// </remarks>
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Interface |
        AttributeTargets.Method,
        AllowMultiple = false,
        Inherited = false)]
    public sealed class EnableCorsAttribute : Attribute, IOperationBehavior, IContractBehavior
    {
        #region IOperationBehavior
        /// <remarks/>
        public void AddBindingParameters(
            OperationDescription operationDescription,
            BindingParameterCollection bindingParameters)
        {
        }

        /// <remarks/>
        public void ApplyClientBehavior(
            OperationDescription operationDescription,
            ClientOperation clientOperation)
        {
        }

        /// <remarks/>
        public void ApplyDispatchBehavior(
            OperationDescription operationDescription,
            DispatchOperation dispatchOperation)
        {
        }

        /// <remarks/>
        public void Validate(
            OperationDescription operationDescription)
        {
        }
        #endregion

        #region IContractBehavior
        /// <remarks/>
        public void AddBindingParameters(
            ContractDescription contractDescription,
            ServiceEndpoint endpoint,
            BindingParameterCollection bindingParameters)
        {
        }

        /// <remarks/>
        public void ApplyClientBehavior(
            ContractDescription contractDescription,
            ServiceEndpoint endpoint,
            ClientRuntime clientRuntime)
        {
        }

        /// <remarks/>
        public void ApplyDispatchBehavior(
            ContractDescription contractDescription,
            ServiceEndpoint endpoint,
            DispatchRuntime dispatchRuntime)
        {
        }

        /// <remarks/>
        public void Validate(
            ContractDescription contractDescription,
            ServiceEndpoint endpoint)
        {
        }
        #endregion
    }
}
