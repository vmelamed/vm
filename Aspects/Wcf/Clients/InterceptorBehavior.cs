using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace vm.Aspects.Wcf.Clients
{
    sealed class InterceptorBehavior : IEndpointBehavior, IClientMessageInspector
    {
        readonly ICallIntercept _interceptor;

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptorBehavior"/> class.
        /// </summary>
        /// <param name="interceptor">The interceptor that will be leveraging the behavior.</param>
        internal InterceptorBehavior(
            ICallIntercept interceptor)
        {
            if (interceptor == null)
                throw new ArgumentNullException(nameof(interceptor));

            _interceptor = interceptor;
        }

        #region IClientMessageInspector Members
        /// <summary>
        /// Enables inspection or modification of a message before a request message is sent to a service.
        /// </summary>
        /// <param name="request">The message to be sent to the service.</param>
        /// <param name="channel">The WCF client object channel.</param>
        /// <returns>The object that is returned as the <c>correlationState</c> argument of the
        /// <see cref="M:System.ServiceModel.Dispatcher.IClientMessageInspector.AfterReceiveReply(System.ServiceModel.Channels.Message@,System.Object)" /> method.
        /// This is null if no correlation state is used.The best practice is to make this a <see cref="T:System.Guid" /> to ensure that no two
        /// <c>correlationState</c> objects are the same.</returns>
        object IClientMessageInspector.BeforeSendRequest(
            ref Message request,
            IClientChannel channel)
        {
            _interceptor.PreInvoke(ref request);

            return null;
        }

        /// <summary>
        /// Enables inspection or modification of a message after a reply message is received but prior to passing it back to the client application.
        /// </summary>
        /// <param name="reply">The message to be transformed into types and handed back to the client application.</param>
        /// <param name="correlationState">Correlation state data.</param>
        void IClientMessageInspector.AfterReceiveReply(
            ref Message reply,
            object correlationState)
        {
            _interceptor.PostInvoke(ref reply);
        }
        #endregion

        #region IEndpointBehavior Members
        /// <summary>
        /// Implements a modification or extension of the client across an endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint that is to be customized.</param>
        /// <param name="clientRuntime">The client runtime to be customized.</param>
        /// <exception cref="System.ArgumentNullException">clientRuntime</exception>
        void IEndpointBehavior.ApplyClientBehavior(
            ServiceEndpoint endpoint,
            ClientRuntime clientRuntime)
        {
            if (clientRuntime == null)
                throw new ArgumentNullException(nameof(clientRuntime));

            clientRuntime.MessageInspectors.Add(this);
        }

        /// <summary>
        /// Implements a modification or extension of the service across an endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint that exposes the contract.</param>
        /// <param name="endpointDispatcher">The endpoint dispatcher to be modified or extended.</param>
        void IEndpointBehavior.ApplyDispatchBehavior(
            ServiceEndpoint endpoint,
            EndpointDispatcher endpointDispatcher)
        {
        }

        /// <summary>
        /// Implement to confirm that the endpoint meets some intended criteria.
        /// </summary>
        /// <param name="endpoint">The endpoint to validate.</param>
        void IEndpointBehavior.Validate(
            ServiceEndpoint endpoint)
        {
        }

        /// <summary>
        /// Implement to pass data at runtime to bindings to support custom behavior.
        /// </summary>
        /// <param name="endpoint">The endpoint to modify.</param>
        /// <param name="bindingParameters">The objects that binding elements require to support the behavior.</param>
        void IEndpointBehavior.AddBindingParameters(
            ServiceEndpoint endpoint,
            BindingParameterCollection bindingParameters)
        {
        }
        #endregion
    }
}
