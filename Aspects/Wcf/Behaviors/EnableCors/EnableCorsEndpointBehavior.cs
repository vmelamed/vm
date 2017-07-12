using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace vm.Aspects.Wcf.Behaviors
{
    /// <summary>
    /// Behavior which enables CORS for endpoints with <see cref="WebHttpBinding"/>.
    /// </summary>
    /// <seealso cref="IOperationBehavior" />
    /// <remarks>
    /// Based on: https://blogs.msdn.microsoft.com/carlosfigueira/2012/05/14/implementing-cors-support-in-wcf/
    /// </remarks>
    public class EnableCorsEndpointBehavior : IEndpointBehavior
    {
        readonly string[] _allowedOrigins;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnableCorsEndpointBehavior"/> class.
        /// </summary>
        /// <param name="allowedOrigins">The explicitly allowed origins. If empty - all origins are allowed.</param>
        public EnableCorsEndpointBehavior(
            params string[] allowedOrigins)
        {
            _allowedOrigins = allowedOrigins;
        }

        /// <summary>
        /// Adds the binding parameters.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="bindingParameters">The binding parameters.</param>
        public void AddBindingParameters(
            ServiceEndpoint endpoint,
            BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>
        /// Applies the client behavior.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="clientRuntime">The client runtime.</param>
        public void ApplyClientBehavior(
            ServiceEndpoint endpoint,
            ClientRuntime clientRuntime)
        {
        }

        /// <summary>
        /// Applies the dispatch behavior.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="endpointDispatcher">The endpoint dispatcher.</param>
        public void ApplyDispatchBehavior(
            ServiceEndpoint endpoint,
            EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher
                .DispatchRuntime
                .MessageInspectors
                .Add(new EnableCorsMessageInspector(endpoint.Contract.Operations, _allowedOrigins));
        }

        /// <summary>
        /// Validates the specified endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        public void Validate(ServiceEndpoint endpoint)
        {
        }
    }
}
