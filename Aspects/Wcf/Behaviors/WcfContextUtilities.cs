using System;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;

namespace vm.Aspects.Wcf.Behaviors
{
    /// <summary>
    /// Class WcfContexts implements <see cref="IWcfContextUtilities"/>.
    /// </summary>
    public class WcfContextUtilities : IWcfContextUtilities
    {
        /// <summary>
        /// Gets a value indicating whether the execution context is also a web operation context.
        /// </summary>
        public bool HasOperationContext => OperationContext.Current != null;

        /// <summary>
        /// Gets a value indicating whether the current code runs in a web operation context.
        /// </summary>
        public bool HasWebOperationContext => WebOperationContext.Current != null;

        /// <summary>
        /// Gets the action.
        /// </summary>
        /// <returns>The fault action.</returns>
        public string OperationAction
        {
            get
            {
                if (HasWebOperationContext)
                    return WebOperationContext
                                    .Current
                                    .IncomingRequest
                                    .UriTemplateMatch
                                    .Data
                                    .ToString();
                else
                if (HasOperationContext)
                    return OperationContext
                                    .Current
                                    .RequestContext
                                    .RequestMessage
                                    .Headers
                                    .Action;
                else
                    return null;
            }
        }

        /// <summary>
        /// Gets the method corresponding to the operation action.
        /// </summary>
        /// <returns>The method.</returns>
        public MethodInfo OperationMethod
        {
            get
            {
                var operationAction = OperationAction;
                var action = OperationContext
                                .Current
                                .EndpointDispatcher
                                .DispatchRuntime
                                .Operations
                                .FirstOrDefault(o => operationAction.Equals(o.Name, StringComparison.OrdinalIgnoreCase))
                                .Action;

                return OperationContext
                            .Current
                            .EndpointDispatcher
                            .DispatchRuntime
                            .Type
                            .GetMethods()
                            .FirstOrDefault(m => operationAction.Equals(m.Name, StringComparison.OrdinalIgnoreCase)  ||
                                                 action.Equals(m.GetCustomAttribute<OperationContractAttribute>()?.Action, StringComparison.OrdinalIgnoreCase));
            }
        }

        /// <summary>
        /// Gets the web HTTP behavior.
        /// </summary>
        /// <returns>The WebHttpBehavior.</returns>
        public WebHttpBehavior WebHttpBehavior => OperationContext
                                                    .Current
                                                    .Host
                                                    .Description
                                                    .Endpoints
                                                    .SelectMany(ep => ep.EndpointBehaviors)
                                                    .OfType<WebHttpBehavior>()
                                                    .FirstOrDefault();

        /// <summary>
        /// Gets the fault action either from the fault's type or if not specified, from the action in the current <see cref="OperationContext"/>.
        /// </summary>
        /// <param name="faultContractType">Type of the fault contract.</param>
        /// <returns>The fault action.</returns>
        public string GetFaultedAction(
            Type faultContractType)
        {
            var operationAction = OperationAction;

            // for unhandled exception use the operation action
            if (operationAction == null || faultContractType == null)
                return operationAction;

            return OperationContext
                        .Current
                        .EndpointDispatcher
                        .DispatchRuntime
                        .Operations
                        .FirstOrDefault(o => operationAction.Equals(o.Name, StringComparison.OrdinalIgnoreCase))
                        ?.FaultContractInfos
                        ?.FirstOrDefault(f => f.Detail == faultContractType)
                        ?.Action
                   ?? operationAction;
        }
    }
}
