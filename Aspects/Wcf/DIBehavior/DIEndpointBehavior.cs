using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace vm.Aspects.Wcf.DIBehavior
{
    /// <summary>
    /// Class DIEndpointBehavior. This class cannot be inherited. Endpoint behavior which supplies the instance provider 
    /// (or service object factory) that resolves the service instance from the DI container.
    /// </summary>
    public sealed class DIEndpointBehavior : IEndpointBehavior
    {
        readonly string _resolveName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DIEndpointBehavior"/> class.
        /// </summary>
        public DIEndpointBehavior()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DIEndpointBehavior"/> class.
        /// </summary>
        /// <param name="resolveName">Name of the resolve.</param>
        public DIEndpointBehavior(
            string resolveName)
        {
            _resolveName = resolveName;
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
                throw new ArgumentNullException("endpoint");
            if (endpointDispatcher==null)
                throw new ArgumentNullException("endpointDispatcher");
            if (endpointDispatcher.DispatchRuntime==null)
                throw new ArgumentException("The DispatchRuntime property cannot be null.", "endpointDispatcher");

            endpointDispatcher.DispatchRuntime.InstanceProvider = new DIInstanceProvider(endpoint.Contract.ContractType, _resolveName);
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
