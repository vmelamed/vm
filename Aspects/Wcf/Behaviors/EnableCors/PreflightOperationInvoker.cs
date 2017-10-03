using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using vm.Aspects.Facilities;
using vm.Aspects.Facilities.Diagnostics;

namespace vm.Aspects.Wcf.Behaviors
{
    class PreflightOperationInvoker : IOperationInvoker
    {
        readonly string _replyAction;
        readonly List<string> _allowedHttpMethods;
        readonly string[] _allowedOrigins;
        readonly int _maxAge;

        public PreflightOperationInvoker(
            string replyAction,
            IEnumerable<string> allowedHttpMethods,
            string[] allowedOrigins = null,
            int maxAge = 600)
        {
            if (allowedHttpMethods == null)
                throw new ArgumentNullException(nameof(allowedHttpMethods));

            _replyAction        = replyAction;
            _allowedHttpMethods = allowedHttpMethods.ToList();
            _allowedOrigins     = allowedOrigins;
            _maxAge             = maxAge;
        }

        public object[] AllocateInputs() => new object[1];

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public object Invoke(
            object instance,
            object[] inputs,
            out object[] outputs)
        {
            outputs = null;

            // get the request headers
            var input          = (Message)inputs[0];

            var httpRequest    = (HttpRequestMessageProperty)input.Properties[HttpRequestMessageProperty.Name];
            var origin         = httpRequest.Headers[Constants.Origin];
            var requestMethod  = httpRequest.Headers[Constants.AccessControlRequestMethod];
            var requestHeaders = httpRequest.Headers[Constants.AccessControlRequestHeaders];

            // build the appropriate HTTP response
            var httpResponse = new HttpResponseMessageProperty
            {
                SuppressEntityBody = true,
                StatusCode         = HttpStatusCode.OK,
            };

            // is it allowed origin
            if (_allowedOrigins!=null  &&  _allowedOrigins.Any()  &&  !_allowedOrigins.Contains(origin))
            {
                // build the appropriate negative HTTP response
                httpResponse.StatusCode = HttpStatusCode.NotAcceptable;

                // build the reply message
                var replyNo = Message.CreateMessage(MessageVersion.None, _replyAction);

                replyNo.Properties.Add(HttpResponseMessageProperty.Name, httpResponse);
                VmAspectsEventSource.Log.CorsOriginNotAllowed(origin);
                Facility.LogWriter.LogError($"Failing CORS because the request origin {origin} is not explicitly allowed.");

                return replyNo;
            }

            if (origin != null)
            {
                var origins = httpResponse.Headers.Get(Constants.AccessControlAllowOrigin);

                if (origins == null  ||
                    !origins.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                            .Contains(origin, StringComparer.OrdinalIgnoreCase))
                    httpResponse.Headers.Add(Constants.AccessControlAllowOrigin, origin);
            }

            if (requestMethod != null &&
                _allowedHttpMethods.Contains(requestMethod))
                httpResponse.Headers.Add(Constants.AccessControlAllowMethods, string.Join(",", _allowedHttpMethods));

            if (requestHeaders != null)
                httpResponse.Headers.Add(Constants.AccessControlAllowHeaders, requestHeaders);

            if (_maxAge > 0)
                httpResponse.Headers.Add(Constants.AccessControlMaxAge, _maxAge.ToString());

            // build the reply message
            var reply = Message.CreateMessage(MessageVersion.None, _replyAction);

            reply.Properties.Add(HttpResponseMessageProperty.Name, httpResponse);

            return reply;
        }

        public bool IsSynchronous => true;

        public IAsyncResult InvokeBegin(
            object instance,
            object[] inputs,
            AsyncCallback callback, object state)
        {
            throw new NotSupportedException("Only synchronous invocations are allowed.");
        }

        public object InvokeEnd(
            object instance,
            out object[] outputs,
            IAsyncResult result)
        {
            throw new NotSupportedException("Only synchronous invocations are allowed.");
        }
    }
}
