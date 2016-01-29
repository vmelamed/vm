//===============================================================================
// The code below is modified Enterprise Library code.
//=============================================================================== 
// Microsoft patterns & practices Enterprise Library
// Exception Handling Application Block
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Web.Services.Protocols;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.WCF;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using vm.Aspects.Facilities;
using vm.Aspects.Wcf.FaultContracts;

namespace vm.Aspects.Wcf.Behaviors
{
    /// <summary>
    /// The error handler class that implements the exception shielding logic.
    /// Adds setting of the HttpStatusCode from the fault exceptions.
    /// </summary>
    public class ExceptionShieldingErrorHandler : IErrorHandler
    {
        #region Constructors and Fields

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExceptionShieldingErrorHandler"/> class with
        /// the <see cref="ExceptionShielding.DefaultExceptionPolicy"/> value.
        /// </summary>
        public ExceptionShieldingErrorHandler()
            : this(ExceptionShielding.DefaultExceptionPolicy)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExceptionShieldingErrorHandler"/> class.
        /// </summary>
        /// <param name="exceptionPolicyName">Name of the exception policy.</param>
        public ExceptionShieldingErrorHandler(
            string exceptionPolicyName)
        {
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
        /// Provides the fault.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <param name="version">The version.</param>
        /// <param name="fault">The fault.</param>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "exceptionToThrow is evaluated two times.")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "As designed. Core feature of the block.")]
        public void ProvideFault(
            Exception error,
            MessageVersion version,
            ref Message fault)
        {
            if (error is FaultException)
                return;

            // Will create a default Message in case it is null
            EnsureMessage(ref fault, version);

            try
            {
                Exception exceptionToThrow;

                // Execute the EHAB policy pipeline
                if (Facility.ExceptionManager.HandleException(error, ExceptionPolicyName, out exceptionToThrow))
                {
                    var faultContractWrapper = exceptionToThrow as FaultContractWrapperException;

                    if (faultContractWrapper != null)
                        HandleFaultWrapper(faultContractWrapper, ref fault);
                    else
                    {
                        var faultException = exceptionToThrow as FaultException;

                        if (faultException != null)
                            HandleFault(faultException, ref fault);
                        else
                            // this is an unhandled exception so treat it as server and shield it.
                            ProcessUnhandledException(exceptionToThrow, ref fault);
                    }

                    return;
                }

                // If we get to this line, then this exception is not
                // defined in the specified policy so treat it as unhandled if not in the default policy
                // run first the default exception policy
                if (!ExceptionPolicyName.Equals(ExceptionShielding.DefaultExceptionPolicy, StringComparison.OrdinalIgnoreCase)  &&
                    Facility.ExceptionManager.HandleException(error, ExceptionShielding.DefaultExceptionPolicy, out exceptionToThrow))
                {
                    var wrapper = exceptionToThrow as FaultContractWrapperException;

                    if (wrapper != null)
                        HandleFaultWrapper(wrapper, ref fault);
                    else
                    {
                        var faultException = exceptionToThrow as FaultException;

                        if (faultException != null)
                            HandleFault(faultException, ref fault);
                        else
                            // this is an unhandled exception so treat it as server and shield it.
                            ProcessUnhandledException(exceptionToThrow, ref fault);
                    }

                    return;
                }

                // this is an unhandled exception so treat it as server and shield it.
                ProcessUnhandledException(error, ref fault);
            }
            catch (Exception unhandledException)
            {
                // this is an unhandled exception so treat it as server and shield it.
                ProcessUnhandledException(unhandledException, ref fault);
            }
        }

        /// <summary>
        /// Handles the error.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        public bool HandleError(Exception error)
        {
            // Since we did all the exception handling and shielding in ProvideFault method, we just return true.
            return true;
        }

        #endregion

        #region Internal Implementation

        static void ProcessUnhandledException(
            Exception unhandledException,
            ref Message fault)
        {
            // if the current error is not already a FaultException
            // process and return, otherwise, just return the current FaultException.
            var faultException = unhandledException as FaultException;

            if (faultException != null)
            {
                HandleFault(faultException, ref fault);
                return;
            }

            // Log only if we don't get any handling instance ID in the exception message.
            // in the configuration file. (see exception handlers for logging)
            Guid handlingInstanceId = GetHandlingInstanceId(unhandledException, Guid.Empty);

            if (handlingInstanceId == Guid.Empty)
                handlingInstanceId = LogServerException(unhandledException);

            HandleFault(unhandledException, ref fault, handlingInstanceId, null);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "As designed. Core feature of the block.")]
        static void HandleFaultWrapper(
            FaultContractWrapperException faultContractWrapper,
            ref Message fault)
        {
            try
            {
                var faultDetails   = faultContractWrapper.FaultContract as Fault;
                var isSerializable = faultContractWrapper.FaultContract.GetType().IsSerializable;
                var action         = GetFaultAction(faultContractWrapper) ?? fault.Headers.Action;

                if (WebOperationContext.Current == null)
                {
                    var messageFault = BuildMessageFault(faultContractWrapper);

                    fault = Message.CreateMessage(fault.Version, messageFault, action);
                    return;
                }

                fault = Message.CreateMessage(fault.Version, action, faultDetails, new DataContractJsonSerializer(faultContractWrapper.FaultContract.GetType()));

                var webBodyFormat = new WebBodyFormatMessageProperty(WebContentFormat.Json);

                fault.Properties.Add(WebBodyFormatMessageProperty.Name, webBodyFormat);

                var responseMessageProperty = new HttpResponseMessageProperty
                {
                    StatusCode        = faultDetails?.HttpStatusCode ?? HttpStatusCode.BadRequest,
                    StatusDescription = "",
                };

                responseMessageProperty.Headers[HttpResponseHeader.ContentType] = "application/json";
                fault.Properties.Add(HttpResponseMessageProperty.Name, responseMessageProperty);
            }
            catch (Exception unhandledException)
            {
                // There was an error during MessageFault build process, so treat it as an Unhandled Exception
                // log the exception and send an unhandled server exception
                var handlingInstanceId = LogServerException(unhandledException);

                HandleFault(unhandledException, ref fault, handlingInstanceId, null);
            }
        }

        static void HandleFault(
            FaultException faultException,
            ref Message fault)
        {
            if (WebOperationContext.Current == null)
                return;

            throw new NotImplementedException();
        }

        static void HandleFault(
            Exception error,
            ref Message fault,
            Guid handlingInstanceId,
            FaultContractWrapperException faultContractWrapper)
        {
            if (WebOperationContext.Current == null)
            {
                var messageFault = BuildMessageFault(error, handlingInstanceId);

                fault = Message.CreateMessage(fault.Version, messageFault, GetFaultAction(faultContractWrapper) ?? fault.Headers.Action);
                return;
            }
        }

        static string GetFaultAction(
            FaultContractWrapperException faultContractWrapper)
        {
            if (OperationContext.Current == null) // we are running outside of a host
                return null;

            string operationAction = OperationContext
                                        .Current
                                        .RequestContext
                                        .RequestMessage
                                        .Headers
                                        .Action;

            // for unhandled exception use the operation action
            if (faultContractWrapper == null)
                return operationAction;

            var faultContractType = faultContractWrapper.FaultContract.GetType();

            operationAction = OperationContext
                                        .Current
                                        .EndpointDispatcher
                                        .DispatchRuntime
                                        .Operations
                                        .FirstOrDefault(o => o.Action.Equals(operationAction, StringComparison.OrdinalIgnoreCase))
                                        ?.FaultContractInfos
                                        .FirstOrDefault(f => f.Detail == faultContractType)
                                        ?.Action;

            return operationAction;
        }

        /// <summary>
        /// Build the shielded MessageFault.
        /// </summary>
        /// <param name="serverException"></param>
        /// <param name="handlingInstanceId"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.ExceptionUtility.FormatExceptionMessage(System.String,System.Guid)")]
        static MessageFault BuildMessageFault(
            Exception serverException,
            Guid handlingInstanceId)
        {
            string exceptionMessage = ExceptionUtility.FormatExceptionMessage(
                                                            "An error has occurred while consuming this service. Please contact your administrator for more information. Error ID: {handlingInstanceID}",
                                                            GetHandlingInstanceId(serverException, handlingInstanceId));

            FaultException faultException = new FaultException(
                    new FaultReason(new FaultReasonText(exceptionMessage, CultureInfo.CurrentCulture)),
                    FaultCode.CreateReceiverFaultCode(SoapException.ServerFaultCode.Name, SoapException.ServerFaultCode.Namespace));

            return faultException.CreateMessageFault();
        }

        /// <summary>
        /// Build the unshielded MessageFault.
        /// </summary>
        /// <param name="faultContractWrapper"></param>
        /// <returns></returns>
        static MessageFault BuildMessageFault(
            FaultContractWrapperException faultContractWrapper)
        {
            Type faultExceptionType = typeof(FaultException<>);
            Type constructedFaultExceptionType = faultExceptionType.MakeGenericType(faultContractWrapper.FaultContract.GetType());

            //Encapsulate the FaultContract in the FaultException
            FaultException faultException =
                (FaultException)Activator.CreateInstance(
                    constructedFaultExceptionType,
                    faultContractWrapper.FaultContract,
                    new FaultReason(new FaultReasonText(faultContractWrapper.Message, CultureInfo.CurrentCulture)),
                    FaultCode.CreateSenderFaultCode(SoapException.ClientFaultCode.Name, SoapException.ClientFaultCode.Namespace));

            return faultException.CreateMessageFault();
        }

        static void EnsureMessage(
            ref Message message,
            MessageVersion defaultVersion)
        {
            if (message == null)
                message = Message.CreateMessage(defaultVersion ?? MessageVersion.Default, ""); // ExceptionShielding.FaultAction);
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

            if (RegularExpression.Guid.IsMatch(exception.Message))
                result = new Guid(exception.Message);

            return result;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "As designed. Exception is logged.")]
        static Guid LogServerException(
            Exception exception)
        {
            // try to get the handling instance from the exception message or get a new one.
            Guid handlingInstanceId = GetHandlingInstanceId(exception);

            // Log exception info to configured log object.
            bool logged = false;
            try
            {
                if (Logger.IsLoggingEnabled())
                {
                    Logger.Write(exception, new Dictionary<string, object> { ["HandlingInstance ID:"] = handlingInstanceId });
                    logged = true;
                }
            }
            catch (Exception e)
            {
                // if we can't log, then trace the exception information
                Trace.TraceError("Unhandled error occurred while logging the original exception. Error ID: {0}", handlingInstanceId, e);
            }
            finally
            {
                if (!logged)
                    // if we can't log, then trace the exception information
                    Trace.TraceError("Unhandled error occurred while consuming this service. Error ID: {0}", handlingInstanceId, exception);
            }

            return handlingInstanceId;
        }

        static void SetHttpStatusCode(
            FaultContractWrapperException faultContractWrapper)
        {
            if (WebOperationContext.Current == null)
                return;

            var aspectsFault = faultContractWrapper?.FaultContract as Fault;

            WebOperationContext.Current.OutgoingResponse.StatusCode = aspectsFault != null
                                                                            ? aspectsFault.HttpStatusCode
                                                                            : HttpStatusCode.InternalServerError;
        }
        #endregion
    }
}
