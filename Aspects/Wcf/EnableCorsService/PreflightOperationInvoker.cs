using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace vm.Aspects.Wcf.EnableCorsService
{
    /// <remarks>
    /// Based on: https://blogs.msdn.microsoft.com/carlosfigueira/2012/05/14/implementing-cors-support-in-wcf/
    /// </remarks>
    class PreflightOperationInvoker : IOperationInvoker
    {
        string _replyAction;
        List<string> _allowedHttpMethods;

        public PreflightOperationInvoker(
            string replyAction,
            List<string> allowedHttpMethods)
        {
            _replyAction        = replyAction;
            _allowedHttpMethods = allowedHttpMethods;
        }

        public object[] AllocateInputs() => new object[1];

        public object Invoke(
            object instance,
            object[] inputs,
            out object[] outputs)
        {
            outputs = null;

            return HandlePreflight((Message)inputs[0]);
        }

        public IAsyncResult InvokeBegin(
            object instance,
            object[] inputs,
            AsyncCallback callback,
            object state)
        {
            throw new NotSupportedException("Only synchronous invocation are supported.");
        }

        public object InvokeEnd(
            object instance,
            out object[] outputs,
            IAsyncResult result)
        {
            throw new NotSupportedException("Only synchronous invocation are supported.");
        }

        public bool IsSynchronous => true;

        Message HandlePreflight(Message input)
        {
            var httpRequest    = (HttpRequestMessageProperty)input.Properties[HttpRequestMessageProperty.Name];
            var origin         = httpRequest.Headers[Constants.Origin];
            var requestMethod  = httpRequest.Headers[Constants.AccessControlRequestMethod];
            var requestHeaders = httpRequest.Headers[Constants.AccessControlRequestHeaders];

            var reply        = Message.CreateMessage(MessageVersion.None, _replyAction);
            var httpResponse = new HttpResponseMessageProperty();

            reply.Properties.Add(HttpResponseMessageProperty.Name, httpResponse);

            httpResponse.SuppressEntityBody = true;
            httpResponse.StatusCode         = HttpStatusCode.OK;

            if (origin != null)
                httpResponse.Headers.Add(Constants.AccessControlAllowOrigin, origin);

            if (requestMethod != null && _allowedHttpMethods.Contains(requestMethod))
                httpResponse.Headers.Add(Constants.AccessControlAllowMethods, string.Join(",", _allowedHttpMethods));

            if (requestHeaders != null)
                httpResponse.Headers.Add(Constants.AccessControlAllowHeaders, requestHeaders);

            return reply;
        }
    }
}