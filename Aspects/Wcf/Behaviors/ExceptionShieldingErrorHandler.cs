using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Net;
using System.Net.Mime;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Web.Services.Protocols;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.WCF;
using vm.Aspects.Facilities;
using vm.Aspects.Wcf.Bindings;
using vm.Aspects.Wcf.FaultContracts;

namespace vm.Aspects.Wcf.Behaviors
{
    /// <summary>
    /// The error handler class that implements the exception shielding logic.
    /// Adds setting of the HttpStatusCode from the fault exceptions.
    /// </summary>
    public class ExceptionShieldingErrorHandler : IErrorHandler
    {
        IWcfContextUtilities _wcfContext;

        #region Constructors and Fields

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExceptionShieldingErrorHandler" /> class with
        /// the <see cref="ExceptionShielding.DefaultExceptionPolicy" /> value.
        /// </summary>
        /// <param name="wcfContext">The WCF contexts behavior.</param>
        public ExceptionShieldingErrorHandler(
            IWcfContextUtilities wcfContext)
            : this(wcfContext, ExceptionShielding.DefaultExceptionPolicy)
        {
            Contract.Requires<ArgumentNullException>(wcfContext != null, nameof(wcfContext));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExceptionShieldingErrorHandler"/> class.
        /// </summary>
        /// <param name="wcfContext">The WCF contexts behavior.</param>
        /// <param name="exceptionPolicyName">Name of the exception policy.</param>
        public ExceptionShieldingErrorHandler(
            IWcfContextUtilities wcfContext,
            string exceptionPolicyName)
        {
            Contract.Requires<ArgumentNullException>(wcfContext != null, nameof(wcfContext));
            Contract.Requires<ArgumentNullException>(exceptionPolicyName!=null, nameof(exceptionPolicyName));
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(exceptionPolicyName), "The argument "+nameof(exceptionPolicyName)+" cannot be empty or consist of whitespace characters only.");

            _wcfContext         = wcfContext;
            ExceptionPolicyName = exceptionPolicyName;
        }

        /// <summary>
        /// Gets or sets the name of the exception policy.
        /// </summary>
        /// <value>The name of the exception policy.</value>
        public string ExceptionPolicyName { get; }

        #endregion

        #region IErrorHandler Members
        /// <summary>
        /// Enables the creation of a custom <see cref="FaultException{T}"/> that is returned from an exception in the course of a service method.
        /// </summary>
        /// <param name="error">The <see cref="Exception"/> object thrown in the course of the service operation.</param>
        /// <param name="version">The SOAP version of the message.</param>
        /// <param name="fault">The <see cref="Message"/> object that is returned to the client, or service, in the duplex case.</param>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "exceptionToThrow is evaluated two times.")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "As designed. Core feature of the handler.")]
        public void ProvideFault(
            Exception error,
            MessageVersion version,
            ref Message fault)
        {
            // thrown by the code and will be handled properly by WCF behaviors downstream in any context.
            if (error is WebFaultException)
                return;

            // thrown by the code and will be handled properly by WCF behaviors downstream in SOAP context.
            if (!_wcfContext.HasWebOperationContext  &&  _wcfContext.HasOperationContext  &&  error is FaultException)
                return;

            // Create a default fault in case it is null
            if (fault == null)
                fault = Message.CreateMessage(version ?? MessageVersion.Default, "");

            try
            {
                Exception exceptionToThrow;

                // Execute the exception handlers
                if (Facility.ExceptionManager.HandleException(error, ExceptionPolicyName, out exceptionToThrow))
                {
                    if (exceptionToThrow == null)
                        exceptionToThrow = error;

                    var faultContractWrapper = exceptionToThrow as FaultContractWrapperException;

                    if (faultContractWrapper != null)
                        HandleFaultWrapper(faultContractWrapper, ref fault);
                    else
                    {
                        var faultException = exceptionToThrow as FaultException;

                        if (faultException != null)
                            HandleFaultException(faultException, ref fault);
                        else
                            // unhandled exception - shield it.
                            ProcessUnhandledException(exceptionToThrow, ref fault);
                    }

                    return;
                }

                // The exception is not handled by the specified exception policy - try the default policy in a similar fashion:
                if (!ExceptionPolicyName.Equals(ExceptionShielding.DefaultExceptionPolicy, StringComparison.OrdinalIgnoreCase)  &&
                    Facility.ExceptionManager.HandleException(error, ExceptionShielding.DefaultExceptionPolicy, out exceptionToThrow))
                {
                    if (exceptionToThrow == null)
                        exceptionToThrow = error;

                    var wrapper = exceptionToThrow as FaultContractWrapperException;

                    if (wrapper != null)
                        HandleFaultWrapper(wrapper, ref fault);
                    else
                    {
                        var faultException = exceptionToThrow as FaultException;

                        if (faultException != null)
                            HandleFaultException(faultException, ref fault);
                        else
                            // this is an unhandled exception so treat it as server and shield it.
                            ProcessUnhandledException(exceptionToThrow, ref fault);
                    }

                    return;
                }

                // unhandled exception - shield it.
                ProcessUnhandledException(error, ref fault);
            }
            catch (Exception unhandledException)
            {
                // unhandled exception - shield it.
                ProcessUnhandledException(unhandledException, ref fault);
            }
        }

        /// <summary>
        /// Enables error-related processing and returns a value that indicates whether the
        /// dispatcher aborts the session and the instance context in certain cases.
        /// </summary>
        /// <param name="error">
        /// The exception thrown during processing.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if WCF should not abort the session (if there is one) and instance context if the instance context is not <see cref="InstanceContextMode.Single"/>;
        /// otherwise, <see langword="false"/>. The default is false.
        /// </returns>
        public bool HandleError(Exception error)
        {
            // Since we did all the exception handling and shielding in the ProvideFault method, we should return true.
            return true;
        }

        #endregion

        #region Internal Implementation

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "As designed. Core feature of the handler.")]
        void HandleFaultWrapper(
            FaultContractWrapperException faultContractWrapper,
            ref Message fault)
        {
            try
            {
                var action = _wcfContext.GetFaultedAction(faultContractWrapper.FaultContract.GetType()) ?? fault.Headers.Action;

                if (!_wcfContext.HasWebOperationContext)
                {
                    // in a SOAP only context, just build a new fault message out of the wrapped fault data and return
                    fault = Message.CreateMessage(
                                        fault.Version,
                                        ((FaultException)Activator.CreateInstance(
                                                                    typeof(FaultException<>)
                                                                        .MakeGenericType(faultContractWrapper.FaultContract.GetType()),
                                                                    faultContractWrapper.FaultContract,
                                                                    faultContractWrapper.Message))
                                                    .CreateMessageFault(),
                                        action);
                }
                else
                {
                    var faultDetails = faultContractWrapper.FaultContract as Fault;

                    // we need to build a JSON or XML message out of the wrapped fault and put it in the web response:
                    BuildHttpResponseMessage(
                        faultDetails,
                        action,
                        faultDetails?.HttpStatusCode ?? HttpStatusCode.InternalServerError,
                        ref fault);
                }
            }
            catch (Exception unhandledException)
            {
                // There was an error during MessageFault build process, so treat it as an Unhandled Exception
                // log the exception and send an unhandled server exception
                var handlingInstanceId = LogServerException(unhandledException);

                HandleFault(unhandledException, ref fault, handlingInstanceId, null);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "As designed. Core feature of the handler.")]
        void HandleFaultException(
            FaultException faultException,
            ref Message fault)
        {
            // In a SOAP scenario simply return - the code threw (Web)FaultException and WCF downstream will handle it properly either way.
            // Also in a WebHttp scenario: if the code threw WebFaultException - return - WCF downstream will handle it properly.
            if (!_wcfContext.HasWebOperationContext  ||
                faultException is WebFaultException)
                return;

            try
            {
                // This is the case of a FaultException in a Web context.
                var faultExceptionType = faultException.GetType();

                if (faultExceptionType.IsGenericType)
                {
                    // it is FaultException<T> - extract the detail and build the message
                    var faultDetail = faultExceptionType
                                            .GetProperty("Detail")
                                            .GetGetMethod()
                                            .Invoke(faultException, null);

                    BuildHttpResponseMessage(
                        faultDetail,
                        _wcfContext.GetFaultedAction(faultDetail.GetType()) ?? fault.Headers.Action,
                        (faultDetail as Fault)?.HttpStatusCode ?? HttpStatusCode.InternalServerError,
                        ref fault);
                }
                else
                    BuildHttpResponseMessage(
                        faultException.Message,
                        fault.Headers.Action,
                        HttpStatusCode.InternalServerError,
                        ref fault);
            }
            catch (Exception unhandledException)
            {
                // There was an error during MessageFault build process, so treat it as an Unhandled Exception
                // log the exception and send an unhandled server exception
                var handlingInstanceId = LogServerException(unhandledException);

                HandleFault(unhandledException, ref fault, handlingInstanceId, null);
            }
        }

        void ProcessUnhandledException(
            Exception unhandledException,
            ref Message fault)
        {
            // if the current error is a FaultException process as above.
            var faultException = unhandledException as FaultException;

            if (faultException != null)
            {
                HandleFaultException(faultException, ref fault);
                return;
            }

            Guid handlingInstanceId = GetHandlingInstanceId(unhandledException, Guid.Empty);

            if (handlingInstanceId == Guid.Empty)
                // log if we don't get handling instance ID
                handlingInstanceId = LogServerException(unhandledException);

            HandleFault(unhandledException, ref fault, handlingInstanceId);
        }

        void HandleFault(
            Exception error,
            ref Message fault,
            Guid handlingInstanceId,
            FaultContractWrapperException faultContractWrapper = null)
        {
            if (_wcfContext.HasWebOperationContext)
                BuildHttpResponseMessage(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "An error has occurred while consuming this service. Please contact your administrator for more information. Error ID: {0}",
                        GetHandlingInstanceId(error, handlingInstanceId)),
                    fault.Headers.Action,
                    HttpStatusCode.InternalServerError,
                    ref fault);
            else
                fault = Message.CreateMessage(
                                    fault.Version,
                                    new FaultException(
                                            new FaultReason(
                                                new FaultReasonText(
                                                        string.Format(
                                                            CultureInfo.InvariantCulture,
                                                            "An error has occurred while consuming this service. Please contact your administrator for more information. Error ID: {0}",
                                                            GetHandlingInstanceId(error, handlingInstanceId)),
                                                        CultureInfo.InvariantCulture)),
                                            FaultCode.CreateReceiverFaultCode(
                                                    SoapException.ServerFaultCode.Name,
                                                    SoapException.ServerFaultCode.Namespace))
                                        .CreateMessageFault(),
                                    _wcfContext.GetFaultedAction(faultContractWrapper?.FaultContract?.GetType()) ?? fault.Headers.Action);
        }

        static Guid GetHandlingInstanceId(Exception exception)
        {
            return GetHandlingInstanceId(exception, Guid.NewGuid());
        }

        static Guid GetHandlingInstanceId(
            Exception exception,
            Guid optionalHandlingInstanceId)
        {
            var result = optionalHandlingInstanceId;
            var match  = RegularExpression.Guid.Match(exception.Message);

            if (match.Success)
                result = new Guid(match.Value);

            return result;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "As designed. Exception is logged.")]
        static Guid LogServerException(
            Exception exception)
        {
            // try to get the handling instance from the exception message or get a new one.
            Guid handlingInstanceId = GetHandlingInstanceId(exception);

            // Log exception info to configured log object.
            try
            {
                if (Facility.LogWriter.IsLoggingEnabled())
                    Facility.LogWriter.Write(exception, new Dictionary<string, object> { ["HandlingInstance ID:"] = handlingInstanceId });
                else
                    Trace.TraceError("Unhandled error occurred while consuming this service. Error ID: {0}", handlingInstanceId, exception.DumpString(1));
            }
            catch (Exception x)
            {
                Trace.TraceError("Unhandled error occurred while logging the original exception. Error ID: {0}", handlingInstanceId, x.ToString());
            }

            return handlingInstanceId;
        }

        WebContentFormat GetWebContentFormat()
        {
            if (WebOperationContext.Current == null)
                return WebContentFormat.Default;

            var typeMapper = new WebContentTypeMapperDefaultJson();
            var format = WebContentFormat.Default;
            ContentType contentType = null;

            // 1. The media types in the request message’s Accept header.
            var acceptHeader = WebOperationContext.Current.IncomingRequest.Headers[HttpRequestHeader.Accept];

            if (!string.IsNullOrWhiteSpace(acceptHeader))
            {
                contentType = new ContentType(acceptHeader);
                format = typeMapper.GetMessageFormatForContentType(contentType.MediaType);

                if (format != WebContentFormat.Default)
                    return format;
            }

            // 2. The content-type of the request message.
            var requestContentType = WebOperationContext.Current.IncomingRequest.Headers[HttpRequestHeader.ContentType];

            if (!string.IsNullOrWhiteSpace(requestContentType))
            {
                contentType = new ContentType(requestContentType);
                format = typeMapper.GetMessageFormatForContentType(contentType.MediaType);

                if (format != WebContentFormat.Default)
                    return format;
            }

            // 3. The default format setting in the operation.
            var operation = _wcfContext.OperationMethod;

            if (operation != null)
            {
                var webGet = operation.GetCustomAttribute<WebGetAttribute>();

                if (webGet != null  &&  webGet.IsResponseFormatSetExplicitly)
                    return webGet.ResponseFormat == WebMessageFormat.Json ? WebContentFormat.Json : WebContentFormat.Xml;

                var webInvoke = operation.GetCustomAttribute<WebInvokeAttribute>();

                if (webInvoke != null  &&  webInvoke.IsResponseFormatSetExplicitly)
                    return webInvoke.ResponseFormat == WebMessageFormat.Json ? WebContentFormat.Json : WebContentFormat.Xml;
            }

            // 4. The default format setting in the WebHttpBehavior.
            var webBehavior = _wcfContext.WebHttpBehavior;

            if (webBehavior != null)
                return webBehavior.DefaultOutgoingResponseFormat == WebMessageFormat.Json ? WebContentFormat.Json : WebContentFormat.Xml;

            return WebContentFormat.Default;
        }

        void BuildHttpResponseMessage(
            object faultDetails,
            string action,
            HttpStatusCode httpStatusCode,
            ref Message fault)
        {
            var responseMessageProperty               = new HttpResponseMessageProperty();

            responseMessageProperty.StatusCode        = httpStatusCode;
            responseMessageProperty.StatusDescription = Fault.GetHttpStatusDescription(responseMessageProperty.StatusCode);

            var webFormat    = GetWebContentFormat();

            if (webFormat == WebContentFormat.Json)
            {
                // set the status code, description and content type in the response header:
                responseMessageProperty.Headers[HttpResponseHeader.ContentType] = "application/json";

                // build a new fault message. In the body use a JSON serializer to serialize the fault details.
                fault = Message.CreateMessage(
                                        fault.Version,
                                        action,
                                        faultDetails,
                                        new DataContractJsonSerializer(faultDetails.GetType()));

                fault.Properties.Add(HttpResponseMessageProperty.Name, responseMessageProperty);
                fault.Properties.Add(WebBodyFormatMessageProperty.Name, new WebBodyFormatMessageProperty(webFormat));
                return;
            }

            if (webFormat == WebContentFormat.Xml)
            {

                // set the status code, description and content type in the response header:
                responseMessageProperty.Headers[HttpResponseHeader.ContentType] = "application/xml";

                // build a new fault message. In the body use a data contract (XML) serializer to serialize the fault details.
                fault = Message.CreateMessage(
                                        fault.Version,
                                        action,
                                        faultDetails,
                                        new DataContractSerializer(faultDetails.GetType()));

                fault.Properties.Add(HttpResponseMessageProperty.Name, responseMessageProperty);
                fault.Properties.Add(WebBodyFormatMessageProperty.Name, new WebBodyFormatMessageProperty(webFormat));
                return;
            }
            else
            {
                // just dump the message as text
                responseMessageProperty.Headers[HttpResponseHeader.ContentType] = "text/plain";

                fault = Message.CreateMessage(
                                        fault.Version,
                                        action,
                                        faultDetails.DumpString());

                fault.Properties.Add(HttpResponseMessageProperty.Name, responseMessageProperty);
                fault.Properties.Add(WebBodyFormatMessageProperty.Name, new WebBodyFormatMessageProperty(WebContentFormat.Raw));
            }
        }
        #endregion
    }
}
