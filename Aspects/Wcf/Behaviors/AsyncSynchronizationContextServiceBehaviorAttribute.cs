using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Threading;

namespace vm.Aspects.Wcf.Behaviors
{
    /// <summary>
    /// Installs <see cref="AsyncServiceSynchronizationContext"/> which carries the <see cref="OperationContext"/>
    /// and optionally the <see cref="WebOperationContext"/> accros threads.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Interface,
        AllowMultiple = false,
        Inherited = false)]
    public sealed class AsyncServiceSynchronizationContextBehaviorAttribute : Attribute, IContractBehavior
    {
        SynchronizationContext _synchronizationContext = new AsyncServiceSynchronizationContext();

        /// <summary>
        /// Adds the binding parameters.
        /// </summary>
        /// <param name="contractDescription">The contract description.</param>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="bindingParameters">The binding parameters.</param>
        public void AddBindingParameters(
            ContractDescription contractDescription,
            ServiceEndpoint endpoint,
            BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>
        /// Applies the client behavior.
        /// </summary>
        /// <param name="contractDescription">The contract description.</param>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="clientRuntime">The client runtime.</param>
        public void ApplyClientBehavior(
            ContractDescription contractDescription,
            ServiceEndpoint endpoint,
            ClientRuntime clientRuntime)
        {
        }

        /// <summary>
        /// Applies the dispatch behavior.
        /// </summary>
        /// <param name="contractDescription">The contract description.</param>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="dispatchRuntime">The dispatch runtime.</param>
        public void ApplyDispatchBehavior(
            ContractDescription contractDescription,
            ServiceEndpoint endpoint,
            DispatchRuntime dispatchRuntime)
        {
            dispatchRuntime.SynchronizationContext = _synchronizationContext;
        }

        /// <summary>
        /// Validates the specified contract description.
        /// </summary>
        /// <param name="contractDescription">The contract description.</param>
        /// <param name="endpoint">The endpoint.</param>
        public void Validate(
            ContractDescription contractDescription,
            ServiceEndpoint endpoint)
        {
        }
    }
}
