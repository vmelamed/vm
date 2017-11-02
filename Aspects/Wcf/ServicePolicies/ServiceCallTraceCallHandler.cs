using System;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.Unity.InterceptionExtension;
using vm.Aspects.Policies;

namespace vm.Aspects.Wcf.ServicePolicies
{
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
            if (logWriter == null)
                throw new ArgumentNullException(nameof(logWriter));
        }

        /// <summary>
        /// Prepares per call data specific to the handler - an instance of <see cref="T:vm.Aspects.Policies.CallData" />.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>T.</returns>
        protected override CallTraceData Prepare(
            IMethodInvocation input)
            => InitializeCallData(new ServiceCallTraceData(), input);

        /// <summary>
        /// Initializes the call data.
        /// </summary>
        /// <param name="callData">The call data.</param>
        /// <param name="input">The input.</param>
        /// <returns>CallData.</returns>
        protected override CallTraceData InitializeCallData(
            CallTraceData callData,
            IMethodInvocation input)
        {
            base.InitializeCallData(callData, input);

            var serviceCallData = (ServiceCallTraceData)callData;

            if (IncludeCustomContext)
            {
                var customContextTypeAttribute = input.MethodBase
                                                      .GetMethodCustomAttribute<CustomDataContextTypeAttribute>();

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
            CallTraceData callData,
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
        protected override void DoBeginDumpAfterCall(
            TextWriter writer,
            IMethodInvocation input,
            CallTraceData callData)
        {
            if (!LogBeforeCall)
            {
                DumpCallerAddress(writer, callData);
                DumpCustomContext(writer, callData);
            }
            base.DoBeginDumpAfterCall(writer, input, callData);
        }

        void DumpCallerAddress(
            TextWriter writer,
            CallTraceData callData)
        {
            var wcfCallData = (ServiceCallTraceData)callData;

            if (!IncludeCallerAddress || string.IsNullOrWhiteSpace(wcfCallData.CallerAddress))
                return;

            writer.WriteLine();
            writer.Write($"Caller Address: {wcfCallData.CallerAddress}");
        }

        void DumpCustomContext(
            TextWriter writer,
            CallTraceData callData)
        {
            if (!IncludeCustomContext)
                return;

            var wcfCallData  = (ServiceCallTraceData)callData;
            var contextValue = wcfCallData.CustomContext;

            writer.WriteLine();
            writer.Write("Custom context: ");
            contextValue.DumpText(writer, 2);
        }
    }
}
