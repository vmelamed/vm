using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using vm.Aspects.Facilities;

namespace vm.Aspects.Wcf.Behaviors
{
    /// <summary>
    /// Class EnableCorsMessageInspector.
    /// </summary>
    /// <seealso cref="IDispatchMessageInspector" />
    /// <remarks>
    /// Based on: https://blogs.msdn.microsoft.com/carlosfigueira/2012/05/14/implementing-cors-support-in-wcf/
    /// </remarks>
    internal class EnableCorsMessageInspector : IDispatchMessageInspector
    {
        readonly IList<string> _corsEnabledOperationsNames;
        readonly string[] _allowedOrigins;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnableCorsMessageInspector" /> class.
        /// </summary>
        /// <param name="list">The list of operations to be inspected.</param>
        /// <param name="allowedOrigins">Explicit list of allowed origins. If the array is empty, all origins are allowed.</param>
        public EnableCorsMessageInspector(
            IEnumerable<OperationDescription> list,
            params string[] allowedOrigins)
        {
            Contract.Requires<ArgumentNullException>(list != null, nameof(list));

            _corsEnabledOperationsNames = list.Select(o => o.Name).ToList();
            _allowedOrigins             = allowedOrigins?.Any() == true ? allowedOrigins : null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnableCorsMessageInspector" /> class.
        /// </summary>
        /// <param name="list">The list of operations to be inspected.</param>
        /// <param name="allowedOrigins">Explicit list of comma, semicolon or space separated allowed origins. If the string is <see langword="null"/> or empty, all origins are allowed.</param>
        public EnableCorsMessageInspector(
            IEnumerable<OperationDescription> list,
            string allowedOrigins)
        {
            Contract.Requires<ArgumentNullException>(list != null, nameof(list));

            _corsEnabledOperationsNames = list.Select(o => o.Name).ToList();

            if (allowedOrigins.IsNullOrWhiteSpace())
                _allowedOrigins = null;
            else
                _allowedOrigins = allowedOrigins.Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Called after an inbound message has been received but before the message is dispatched
        /// to the intended operation.
        /// </summary>
        /// <param name="request">The request message.</param>
        /// <param name="channel">The incoming channel.</param>
        /// <param name="instanceContext">The current service instance.</param>
        /// <returns>
        /// The object used to correlate state.
        /// This object is passed back in the <see cref="IDispatchMessageInspector.BeforeSendReply"/> method.
        /// </returns>
        public object AfterReceiveRequest(
            ref Message request,
            IClientChannel channel,
            InstanceContext instanceContext)
        {
            var httpRequest = (HttpRequestMessageProperty)request.Properties[HttpRequestMessageProperty.Name];
            var origin = httpRequest?.Headers[Constants.Origin];

            if (origin == null)
                return null;

            var operationName = (string)request.Properties[WebHttpDispatchOperationSelector.HttpOperationNamePropertyName];

            if (operationName == null)
                return null;

            if (!_corsEnabledOperationsNames.Contains(operationName, StringComparer.OrdinalIgnoreCase))
            {
                Facility.LogWriter.LogError($"Failing CORS because the request operation {operationName} is not allowed.");
                return null;
            }

            if (_allowedOrigins!=null  &&  _allowedOrigins.Any()  &&  !_allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
            {
                Facility.LogWriter.LogError($"Failing CORS because the request origin {origin} is not explicitly allowed.");
                return null;
            }

            return origin;
        }

        /// <summary>
        /// Called after the operation has returned but before the reply message is sent.
        /// </summary>
        /// <param name="reply">The reply message. This value is null if the operation is one way.</param>
        /// <param name="correlationState">The correlation object returned from the <see cref="IDispatchMessageInspector.AfterReceiveRequest"/> method.</param>
        public void BeforeSendReply(
            ref Message reply,
            object correlationState)
        {
            string origin = correlationState as string;

            if (origin == null)
                return;

            var httpResponse = (HttpResponseMessageProperty)reply.Properties[HttpResponseMessageProperty.Name];

            if (httpResponse == null)
            {
                httpResponse = new HttpResponseMessageProperty();
                reply.Properties.Add(HttpResponseMessageProperty.Name, httpResponse);
            }

            var origins = httpResponse.Headers[Constants.AccessControlAllowOrigin];

            if (origins == null  ||  !origins.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                             .Contains(origin, StringComparer.OrdinalIgnoreCase))
                httpResponse.Headers.Add(Constants.AccessControlAllowOrigin, origin);
        }
    }
}