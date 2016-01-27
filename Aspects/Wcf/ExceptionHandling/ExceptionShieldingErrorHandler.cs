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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Web.Services.Protocols;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.WCF;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using vm.Aspects.Wcf.FaultContracts;

namespace vm.Aspects.Wcf.ExceptionHandling
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
        /// Handles the error.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        public bool HandleError(Exception error)
        {
            // Typical use of this method:
            // Implement the HandleError method to ensure error-related behaviors 
            // (error logging, assuring a fail fast, shutting down the application, and so on). 
            // If any implementation of HandleError returns true, subsequent implementations are
            // not called. If there are no implementations or no implementation returns true, it 
            // is processed according to the ServiceBehaviorAttribute.IncludeExceptionDetailInFaults 
            // property value.

            // Since we did all the exception handling and shielding in ProvideFault method,
            // we just return true.
            return true;
        }

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

            // Will create a default Message in case is null
            EnsureMessage(ref fault, version);

            try
            {
                Exception exceptionToThrow;

                // Execute the EHAB policy pipeline
                if (ExceptionPolicy.HandleException(error, ExceptionPolicyName, out exceptionToThrow))
                {
                    var wrapper = exceptionToThrow as FaultContractWrapperException;

                    if (wrapper != null)
                        HandleFault(wrapper, ref fault);
                    else
                    if (exceptionToThrow is FaultException)
                        return;
                    else
                        // this is an unhandled exception so treat it as server and shield it.
                        ProcessUnhandledException(exceptionToThrow, ref fault);

                    return;
                }

                // If we get to this line, then this exception is not
                // defined in the specified policy so treat it as unhandled if not in the default policy
                // run first the default exception policy
                if (!ExceptionPolicyName.Equals(ExceptionShielding.DefaultExceptionPolicy, StringComparison.OrdinalIgnoreCase)  &&
                    ExceptionPolicy.HandleException(error, ExceptionShielding.DefaultExceptionPolicy, out exceptionToThrow))
                {
                    var wrapper = exceptionToThrow as FaultContractWrapperException;

                    if (wrapper != null)
                        HandleFault(wrapper, ref fault);
                    else
                    if (exceptionToThrow is FaultException)
                        return;
                    else
                        // this is an unhandled exception so treat it as server and shield it.
                        ProcessUnhandledException(exceptionToThrow, ref fault);

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

        #endregion

        #region Internal Implementation

        static void ProcessUnhandledException(
            Exception unhandledException,
            ref Message fault)
        {
            // if the current error is not already a FaultException
            // process and return, otherwise, just return the current FaultException.
            if (!(unhandledException is FaultException))
            {
                // Log only if we don't get any handling instance ID in the exception message.
                // in the configuration file. (see exception handlers for logging)
                Guid handlingInstanceId = GetHandlingInstanceId(unhandledException, Guid.Empty);

                if (handlingInstanceId == Guid.Empty)
                    handlingInstanceId = LogServerException(unhandledException);

                HandleFault(unhandledException, ref fault, handlingInstanceId, null);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "As designed. Core feature of the block.")]
        static void HandleFault(
            FaultContractWrapperException faultContractWrapper,
            ref Message fault)
        {
            try
            {
                var messageFault = BuildMessageFault(faultContractWrapper);

                SetHttpStatusCode(faultContractWrapper);

                fault = Message.CreateMessage(fault.Version, messageFault, GetFaultAction(faultContractWrapper) ?? fault.Headers.Action);
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
            Exception error,
            ref Message fault,
            Guid handlingInstanceId,
            FaultContractWrapperException faultContractWrapper)
        {
            var messageFault = BuildMessageFault(error, handlingInstanceId);

            SetHttpStatusCode(faultContractWrapper);

            fault = Message.CreateMessage(fault.Version, messageFault, GetFaultAction(faultContractWrapper) ?? fault.Headers.Action);
        }

        static string GetFaultAction(
            FaultContractWrapperException faultContractWrapper)
        {
            if (OperationContext.Current == null) // we are running outside a host
                return null;

            string operationAction = OperationContext.Current.RequestContext.RequestMessage.Headers.Action;

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
        static MessageFault BuildMessageFault(FaultContractWrapperException faultContractWrapper)
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
