using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;
using System.ServiceModel.Channels;
using vm.Aspects.Diagnostics;
using vm.Aspects.Policies;

namespace vm.Aspects.Wcf.ServicePolicies
{
    /// <summary>
    /// Class ServiceCallData extends <see cref="T:CallData"/> with caller's address and the content of the custom data context (if present in the operation context).
    /// </summary>
    public class ServiceCallData : CallData
    {
        /// <summary>
        /// Gets or sets the caller address.
        /// </summary>
        public string CallerAddress { get; set; }
        /// <summary>
        /// Gets or sets the custom context.
        /// </summary>
        public object CustomContext { get; set; }
    }

    /// <summary>
    /// Class ServiceCallTraceCallHandler extends <see cref="CallTraceCallHandler"/> with WCF service specific information.
    /// </summary>
    public class ServiceCallTraceCallHandler : CallTraceCallHandler
    {
        #region Properties
        /// <summary>
        /// Gets or sets a value indicating whether to include the caller's address. Default: true.
        /// </summary>
        public bool IncludeCallerAddress { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to include the custom context if any. Default: true.
        /// </summary>
        public bool IncludeCustomContext { get; set; } = true;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="CallTraceCallHandler" /> class.
        /// </summary>
        /// <param name="logWriter">The log writer.</param>
        public ServiceCallTraceCallHandler(
            LogWriter logWriter)
            : base(logWriter)
        {
            Contract.Requires<ArgumentNullException>(logWriter != null, nameof(logWriter));
        }

        /// <summary>
        /// Prepares per call data specific to the handler - an instance of <see cref="T:vm.Aspects.Policies.CallData" />.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>T.</returns>
        protected override CallData Prepare(
            IMethodInvocation input)
        {
            Contract.Ensures(Contract.Result<CallData>() != null);

            var serviceCallData = new ServiceCallData();

            return InitializeCallData(serviceCallData, input);
        }

        /// <summary>
        /// Initializes the call data.
        /// </summary>
        /// <param name="callData">The call data.</param>
        /// <param name="input">The input.</param>
        /// <returns>CallData.</returns>
        protected override CallData InitializeCallData(
            CallData callData,
            IMethodInvocation input)
        {
            var serviceCallData = (ServiceCallData)callData;

            base.InitializeCallData(serviceCallData, input);

            if (IncludeCustomContext)
            {
                var customContextTypeAttribute = input.MethodBase
                                                      .DeclaringType
                                                      .GetCustomAttribute<CustomDataContextTypeAttribute>(true);

                if (customContextTypeAttribute != null)
                {
                    var contextType = typeof(CustomDataContext<>).MakeGenericType(customContextTypeAttribute.CustomDataContextType);
                    var context = contextType.GetProperty("Current").GetValue(null, null);

                    if (context != null)
                        serviceCallData.CustomContext = contextType.GetProperty("Value").GetValue(context, null);
                }
            }

            object property;

            if (IncludeCallerAddress  &&
                OperationContext.Current?.IncomingMessageProperties != null &&
                OperationContext.Current.IncomingMessageProperties.TryGetValue(RemoteEndpointMessageProperty.Name, out property))
            {
                var endpointMessageProperty = property as RemoteEndpointMessageProperty;

                if (endpointMessageProperty != null)
                {
                    serviceCallData.CallerAddress = $"{endpointMessageProperty.Address}:{endpointMessageProperty.Port}";
                    // just in case someone needs it up the stack:
                    CallContext.LogicalSetData("callerIpAddress", endpointMessageProperty.Address);
                }
            }

            return callData;
        }

        /// <summary>
        /// Dumps the data that needs to be dumped before the call.
        /// </summary>
        /// <param name="writer">The writer to dump the call information to.</param>
        /// <param name="input">Object representing the inputs to the current call to the target.</param>
        /// <param name="callData">The additional audit data about the call.</param>
        /// <param name="ignore">not used.</param>
        protected override void DoDumpBeforeCall(
            TextWriter writer,
            IMethodInvocation input,
            CallData callData,
            IMethodReturn ignore = null)
        {
            DumpCallerAddress(writer, callData);
            DumpCustomContext(writer, callData);

            base.DoDumpBeforeCall(writer, input, callData, ignore);
        }

        /// <summary>
        /// Dumps the data that needs to be dumped after the call.
        /// </summary>
        /// <param name="writer">The writer to dump the call information to.</param>
        /// <param name="input">Object representing the inputs to the current call to the target.</param>
        /// <param name="callData">The additional audit data about the call.</param>
        /// <param name="methodReturn">Object representing the return value from the target.</param>
        protected override void DoDumpAfterCall(
            TextWriter writer,
            IMethodInvocation input,
            CallData callData,
            IMethodReturn methodReturn)
        {
            if (!LogBeforeCall)
            {
                DumpCallerAddress(writer, callData);
                DumpCustomContext(writer, callData);
            }
            base.DoDumpAfterCall(writer, input, callData, methodReturn);
        }

        void DumpCallerAddress(
            TextWriter writer,
            CallData callData)
        {
            Contract.Requires<ArgumentNullException>(writer != null, nameof(writer));
            Contract.Requires<ArgumentNullException>(callData != null, nameof(callData));

            var wcfCallData = callData as ServiceCallData;

            if (wcfCallData == null)
                throw new ArgumentException("callData must be of type ServiceCallData");

            if (!IncludeCallerAddress || string.IsNullOrWhiteSpace(wcfCallData.CallerAddress))
                return;

            writer.WriteLine();
            writer.Write($"Caller Address: {wcfCallData.CallerAddress}");
        }

        void DumpCustomContext(
            TextWriter writer,
            CallData callData)
        {
            Contract.Requires<ArgumentNullException>(writer != null, nameof(writer));
            Contract.Requires<ArgumentNullException>(callData != null, nameof(callData));

            if (!IncludeCustomContext)
                return;

            var wcfCallData = callData as ServiceCallData;

            if (wcfCallData == null)
                throw new ArgumentException("callData must be of type ServiceCallData", nameof(callData));

            var contextValue = wcfCallData.CustomContext;

            writer.Indent(2);
            writer.WriteLine();
            writer.Write("Custom context: ");
            contextValue.DumpText(writer, 4);
            writer.Unindent(2);
        }
    }
}
