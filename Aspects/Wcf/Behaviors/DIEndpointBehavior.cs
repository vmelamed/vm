using Microsoft.Practices.Unity;
using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace vm.Aspects.Wcf.Behaviors
{
    /// <summary>
    /// Class DIEndpointBehavior. This class cannot be inherited. Endpoint behavior which supplies the instance provider 
    /// (or service object factory) that resolves the service instance from the DI container.
    /// </summary>
    public sealed class DIEndpointBehavior : IEndpointBehavior
    {
        readonly string _resolveName;
        readonly bool _useRootContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DIEndpointBehavior" /> class.
        /// </summary>
        /// <param name="resolveName">Name of the resolve.</param>
        /// <param name="useRootContainer">
        /// If set to <see langword="false"/> (the default) the instance provider will create a child container off of the root container and 
        /// the service and all of its dependencies will be resolved from that child container. Upon service instance release, the container, 
        /// the service and all dependencies with <see cref="HierarchicalLifetimeManager"/> will be disposed.
        /// If set to <see langword="true"/> the instance provider will resolve the service instance from the root container, 
        /// upon release the service will be disposed and it is responsible for the disposal of its dependencies.
        /// </param>
        public DIEndpointBehavior(
            string resolveName = null,
            bool useRootContainer = false)
        {
            _resolveName      = resolveName;
            _useRootContainer = useRootContainer;
        }

        #region IEndpointBehavior Members

        /// <summary>
        /// Implement to pass data at runtime to bindings to support custom behavior.
        /// </summary>
        /// <param name="endpoint">The endpoint to modify.</param>
        /// <param name="bindingParameters">The objects that binding elements require to support the behavior.</param>
        public void AddBindingParameters(
            ServiceEndpoint endpoint,
            BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>
        /// Implements a modification or extension of the client across an endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint that is to be customized.</param>
        /// <param name="clientRuntime">The client runtime to be customized.</param>
        public void ApplyClientBehavior(
            ServiceEndpoint endpoint,
            ClientRuntime clientRuntime)
        {
        }

        /// <summary>
        /// Implements a modification or extension of the service across an endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint that exposes the contract.</param>
        /// <param name="endpointDispatcher">The endpoint dispatcher to be modified or extended.</param>
        /// <exception cref="System.ArgumentNullException">
        /// endpoint
        /// or
        /// endpointDispatcher
        /// </exception>
        /// <exception cref="System.ArgumentException">The DispatchRuntime property cannot be null.;endpointDispatcher</exception>
        public void ApplyDispatchBehavior(
            ServiceEndpoint endpoint,
            EndpointDispatcher endpointDispatcher)
        {
            if (endpoint==null)
                throw new ArgumentNullException(nameof(endpoint));
            if (endpointDispatcher==null)
                throw new ArgumentNullException(nameof(endpointDispatcher));
            if (endpointDispatcher.DispatchRuntime==null)
                throw new ArgumentException("The DispatchRuntime property cannot be null.", nameof(endpointDispatcher));

            endpointDispatcher.DispatchRuntime.InstanceProvider = new DIInstanceProvider(endpoint.Contract.ContractType, _resolveName, _useRootContainer);
        }

        /// <summary>
        /// Implement to confirm that the endpoint meets some intended criteria.
        /// </summary>
        /// <param name="endpoint">The endpoint to validate.</param>
        public void Validate(ServiceEndpoint endpoint)
        {
        }

        #endregion
    }
}
