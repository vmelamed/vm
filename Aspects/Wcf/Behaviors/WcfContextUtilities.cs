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
        public bool HasOperationContext => OperationContext.Current!=null  &&  !HasWebOperationContext;

        /// <summary>
        /// Gets a value indicating whether the current code runs in a web operation context.
        /// </summary>
        public bool HasWebOperationContext => WebOperationContext.Current!=null  &&  OperationContext.Current.EndpointDispatcher.ChannelDispatcher.BindingName.ToUpperInvariant().Contains(nameof(WebHttpBinding).ToUpperInvariant());

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
                                    ?.UriTemplateMatch
                                    ?.Data
                                    ?.ToString();
                else
                if (HasOperationContext)
                    return OperationContext
                                    .Current
                                    .RequestContext
                                    ?.RequestMessage
                                    ?.Headers
                                    ?.Action;
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

                if (operationAction == null)
                    return null;

                var action = OperationContext
                                .Current
                                .EndpointDispatcher
                                .DispatchRuntime
                                .Operations
                                .FirstOrDefault(o => operationAction.Equals(o.Name, StringComparison.OrdinalIgnoreCase))
                                ?.Action;

                if (action == null)
                    return null;

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
        /// Gets the attributes on the method implementing the current operation declared on both the interface and the service class.
        /// </summary>
        public Attribute[] OperationMethodAllAttributes
        {
            get
            {
                var operationAction = OperationAction;

                if (operationAction == null)
                    return null;

                var action = OperationContext
                                .Current
                                .EndpointDispatcher
                                .DispatchRuntime
                                .Operations
                                .FirstOrDefault(o => operationAction.Equals(o.Name, StringComparison.OrdinalIgnoreCase))
                                ?.Action
                                ;

                if (action == null)
                    return null;

                var serviceType = OperationContext
                                            .Current
                                            .EndpointDispatcher
                                            .DispatchRuntime
                                            .Type
                                            ;

                var serviceClassMethodInfos = serviceType
                                                .GetMethods()
                                                ;

                var mi = serviceClassMethodInfos
                            .FirstOrDefault(m => operationAction.Equals(m.Name, StringComparison.OrdinalIgnoreCase)  ||
                                                 action.Equals(m.GetCustomAttribute<OperationContractAttribute>()?.Action, StringComparison.OrdinalIgnoreCase))
                            ;

                if (mi != null)
                    return mi.GetCustomAttributes()
                             .ToArray()
                             ;

                if (serviceClassMethodInfos==null  ||  serviceClassMethodInfos.Length==0)
                    return null;    // there is something wrong here?

                // try to find the method on the interface
                mi = serviceType
                            .GetInterfaces()
                            .Where(i => i.GetCustomAttribute<ServiceContractAttribute>() != null)
                            .Select(i => i)
                            .SelectMany(i => i.GetMethods())
                            .FirstOrDefault(m => m.GetCustomAttribute<OperationContractAttribute>() != null  &&
                                                 operationAction.Equals(m.Name, StringComparison.OrdinalIgnoreCase)  ||
                                                 action.Equals(m.GetCustomAttribute<OperationContractAttribute>()?.Action, StringComparison.OrdinalIgnoreCase))
                            ;

                if (mi == null)
                    return null;

                // find the method again on the service
                var smi = serviceClassMethodInfos
                            .First(m => m.Name == mi.Name)
                            ;

                return mi.GetCustomAttributes()
                         .Union(smi
                         .GetCustomAttributes())
                         .ToArray()
                         ;
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
