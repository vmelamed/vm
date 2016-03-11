using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="EnableCorsMessageInspector"/> class.
        /// </summary>
        /// <param name="list">The list of operations to be inspected.</param>
        public EnableCorsMessageInspector(
            List<OperationDescription> list)
        {
            Contract.Requires<ArgumentNullException>(list != null, nameof(list));

            _corsEnabledOperationsNames = list.Select(o => o.Name).ToList();
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
            var httpProp = (HttpRequestMessageProperty)request.Properties[HttpRequestMessageProperty.Name];

            if (httpProp == null)
                return null;

            object operationName;

            request.Properties.TryGetValue(WebHttpDispatchOperationSelector.HttpOperationNamePropertyName, out operationName);

            if (operationName != null &&
                _corsEnabledOperationsNames.Contains((string)operationName))
                return httpProp.Headers[Constants.Origin];

            return null;
        }

        /// <summary>
        /// Called after the operation has returned but before the reply message is sent.
        /// </summary>
        /// <param name="reply">The reply message. This value is null if the operation is one way.</param>
        /// <param name="correlationState">
        /// The correlation object returned from the <see cref="IDispatchMessageInspector.AfterReceiveRequest"/> method.
        /// </param>
        public void BeforeSendReply(
            ref Message reply,
            object correlationState)
        {
            string origin = correlationState as string;

            if (origin == null)
                return;

            HttpResponseMessageProperty httpProp = null;

            if (reply.Properties.ContainsKey(HttpResponseMessageProperty.Name))
                httpProp = (HttpResponseMessageProperty)reply.Properties[HttpResponseMessageProperty.Name];
            else
            {
                httpProp = new HttpResponseMessageProperty();
                reply.Properties.Add(HttpResponseMessageProperty.Name, httpProp);
            }

            var origins = httpProp.Headers.Get(Constants.AccessControlAllowOrigin);

            if (origins == null  ||
                !origins.Split(new char[] { ',', ' '}, StringSplitOptions.RemoveEmptyEntries)
                        .Contains(origin, StringComparer.OrdinalIgnoreCase))          
                httpProp.Headers.Add(Constants.AccessControlAllowOrigin, origin);
        }
    }
}