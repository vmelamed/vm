using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.ServiceModel;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Unity.InterceptionExtension;
using vm.Aspects.Diagnostics;
using vm.Aspects.Facilities;

namespace vm.Aspects.Policies
{
    /// <summary>
    /// Class CallTraceCallHandler is an aspect (policy) which when injected in an object will dump information about each call
    /// into the current <see cref="P:Facility.LogWriter" />.
    /// </summary>
    public class CallTraceCallHandler : BaseCallHandler<CallTraceData>
    {
        #region Properties
        /// <summary>
        /// Gets or sets a value indicating whether to log an event before the calls. Default: false.
        /// </summary>
        public bool LogBeforeCall { get; set; }

        /// <summary>
        /// Gets or sets the message prefix of the event before the calls. Default: null.
        /// </summary>
        public string LogBeforeMessagePrefix { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to log an after the calls. Default: true.
        /// </summary>
        public bool LogAfterCall { get; set; } = true;

        /// <summary>
        /// Gets or sets the message prefix of the event after the calls. Default: null.
        /// </summary>
        public string LogAfterMessagePrefix { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to log asynchronously.
        /// </summary>
        public bool LogAsynchronously { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to include the identity of the principal caller. Default: true
        /// </summary>
        /// <value><see langword="true"/> to include the principal; otherwise, <see langword="false"/>.</value>
        public bool IncludePrincipal { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to include a dump of the parameters. Default: true.
        /// </summary>
        public bool IncludeParameters { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to include the call stack. Default: false.
        /// </summary>
        public bool IncludeCallStack { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include the call time in the logged events after the calls. Default: true.
        /// </summary>
        public bool IncludeCallTime { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to include a dump of the return values in the logged events after the calls. Default: true.
        /// </summary>
        public bool IncludeReturnValue { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to include a dump of the exceptions thrown from the calls. Default: true. 
        /// </summary>
        public bool IncludeException { get; set; } = true;

        /// <summary>
        /// Gets or sets the category to which the start call log events will be sent. Default: &quot;Start Call Trace&quot;.
        /// </summary>
        public string StartCallCategory { get; set; } = LogWriterFacades.StartCallTrace;

        /// <summary>
        /// Gets or sets the category to which the end log events will be sent. Default: &quot;Event Call Trace&quot;.
        /// </summary>
        public string EndCallCategory { get; set; } = LogWriterFacades.EndCallTrace;

        /// <summary>
        /// Gets or sets the priority of the logged call trace events. Default: -1
        /// </summary>
        public int Priority { get; set; } = -1;

        /// <summary>
        /// Gets or sets the severity of the logged call trace events. Default: TraceEventType.Information
        /// </summary>
        public TraceEventType Severity { get; set; } = TraceEventType.Information;

        /// <summary>
        /// Gets or sets the logged call trace events' event id. Default: 0
        /// </summary>
        public int EventId { get; set; }

        /// <summary>
        /// Gets or sets the title of the logged call trace events. Default: null.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the log writer.
        /// </summary>
        /// <value>The log writer.</value>
        LogWriter LogWriter { get; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="CallTraceCallHandler"/> class.
        /// </summary>
        /// <param name="logWriter">The log writer.</param>
        /// <exception cref="System.ArgumentNullException">logWriter</exception>
        public CallTraceCallHandler(
            LogWriter logWriter)
        {
            if (logWriter == null)
                throw new ArgumentNullException(nameof(logWriter));

            LogWriter = logWriter;
        }

        #region Overridables
        /// <summary>
        /// Prepares per call data specific to the handler - an instance of <see cref="CallTraceData"/>.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>T.</returns>
        protected override CallTraceData Prepare(
            IMethodInvocation input) => InitializeCallData(new CallTraceData(), input);

        /// <summary>
        /// Initializes the call data.
        /// </summary>
        /// <param name="callData">The call data.</param>
        /// <param name="input">The input.</param>
        /// <returns>CallData.</returns>
        protected virtual CallTraceData InitializeCallData(
            CallTraceData callData,
            IMethodInvocation input)
        {
            if (callData == null)
                throw new ArgumentNullException(nameof(callData));

            if (IncludeCallStack)
                callData.CallStack = Environment.StackTrace;

            if (IncludePrincipal)
            {
                if (ServiceSecurityContext.Current != null &&
                    ServiceSecurityContext.Current.PrimaryIdentity != null)
                    callData.IdentityName = ServiceSecurityContext.Current.PrimaryIdentity.Name;
                else
                    callData.IdentityName = WindowsIdentity.GetCurrent().Name;
            }

            return callData;
        }

        /// <summary>
        /// Performs the necessary actions (to dump data) before invoking the next handler in the pipeline.
        /// </summary>
        /// <param name="input">Object representing the inputs to the current call to the target.</param>
        /// <param name="callData">The additional audit data about the call.</param>
        /// <returns>Represents the return value from the target.</returns>
        protected override IMethodReturn PreInvoke(
            IMethodInvocation input,
            CallTraceData callData)
        {
            if (!LogBeforeCall  ||  !LogWriter.IsLoggingEnabled())
                return null;

            var entry = CreateLogEntry(StartCallCategory);

            if (LogWriter.ShouldLog(entry))
            {
                Action logBeforeCall = () => Facility
                                                .ExceptionManager
                                                .Process(
                                                    () => LogBeforeCallData(entry, input, callData),
                                                    ExceptionPolicyProvider.LogAndSwallowPolicyName);

                if (LogAsynchronously)
                    Task.Run(logBeforeCall);
                else
                    logBeforeCall();
            }

            return null;
        }

        /// <summary>
        /// Invokes the next handler in the pipeline. Optionally may register the call duration.
        /// </summary>
        /// <param name="input">Object representing the inputs to the current call to the target.</param>
        /// <param name="getNext">Delegate to execute to get the next delegate in the handler chain.</param>
        /// <param name="callData">The additional audit data about the call.</param>
        /// <returns>Object representing the return value from the target.</returns>
        protected override IMethodReturn DoInvoke(
            IMethodInvocation input,
            GetNextHandlerDelegate getNext,
            CallTraceData callData)
        {
            var takeTime = LogAfterCall && IncludeCallTime  &&  LogWriter.IsLoggingEnabled();

            if (takeTime)
            {
                callData.CallTimer = new Stopwatch();
                callData.CallTimer.Start();
            }

            var methodReturn = base.DoInvoke(input, getNext, callData);

            if (takeTime)
                callData.CallTimer.Stop();

            return methodReturn;
        }

        /// <summary>
        /// Process the output from the call so far and optionally modify the output.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="methodReturn">The method return.</param>
        /// <param name="callData">The per-call data.</param>
        /// <returns>IMethodReturn.</returns>
        protected override IMethodReturn PostInvoke(
            IMethodInvocation input,
            IMethodReturn methodReturn,
            CallTraceData callData)
        {
            // async methods are always dumped in ContinueWith
            if (methodReturn.IsAsyncCall())
                return methodReturn;

            callData.ReturnValue  = methodReturn.ReturnValue;
            callData.OutputValues = methodReturn.Outputs;
            callData.Exception    = methodReturn.Exception;

            LogPostInvoke(input, callData);

            return methodReturn;
        }

        /// <summary>
        /// Gives the aspect a chance to do some final work after the main task is truly complete.
        /// The overriding implementations should begin by calling the base class' implementation first.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="input">The input.</param>
        /// <param name="methodReturn">The method return.</param>
        /// <param name="callData">The call data.</param>
        /// <returns>Task{TResult}.</returns>
        protected override async Task<TResult> ContinueWith<TResult>(
            IMethodInvocation input,
            IMethodReturn methodReturn,
            CallTraceData callData)
        {
            TResult result = default(TResult);

            try
            {
                result               = await base.ContinueWith<TResult>(input, methodReturn, callData);
                callData.ReturnValue = result;
            }
            catch (Exception x)
            {
                callData.Exception = x;
            }

            LogPostInvoke(input, callData);

            if (callData.Exception != null)
                throw callData.Exception;

            return result;
        }
        #endregion

        /// <summary>
        /// Does the actual post-invoke logging.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="callData">The call data.</param>
        protected virtual void LogPostInvoke(
            IMethodInvocation input,
            CallTraceData callData)
        {
            if (!LogAfterCall  ||  !LogWriter.IsLoggingEnabled())
                return;

            var entry = CreateLogEntry(EndCallCategory);

            if (LogWriter.ShouldLog(entry))
            {
                Action logAfterCall = () => Facility
                                                .ExceptionManager
                                                .Process(
                                                    () => LogAfterCallData(entry, input, callData),
                                                    ExceptionPolicyProvider.LogAndSwallowPolicyName);

                if (LogAsynchronously)
                    Task.Run(() => logAfterCall());
                else
                    logAfterCall();
            }
        }

        /// <summary>
        /// Creates a new log entry.
        /// </summary>
        /// <returns>LogEntry.</returns>
        LogEntry CreateLogEntry(string category) =>
            new LogEntry
            {
                Categories = new[] { category },
                Severity   = Severity,
                EventId    = EventId,
                Priority   = Priority,
                Title      = Title,
            };

        /// <summary>
        /// Logs the before call data.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="input">Object representing the inputs to the current call to the target.</param>
        /// <param name="callData">The additional audit data about the call.</param>
        void LogBeforeCallData(
            LogEntry entry,
            IMethodInvocation input,
            CallTraceData callData)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (callData == null)
                throw new ArgumentNullException(nameof(callData));

            // build the call message:
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                writer.Indent(2);

                DoDumpBeforeCall(writer, input, callData);

                writer.Unindent(2);
                writer.WriteLine();

                // get the message
                entry.Message = writer.GetStringBuilder().ToString();
            }

            // log the event entry
            LogWriter.Write(entry);
        }

        /// <summary>
        /// Dumps the data that needs to be dumped before the call.
        /// </summary>
        /// <param name="writer">The writer to dump the call information to.</param>
        /// <param name="input">Object representing the inputs to the current call to the target.</param>
        /// <param name="callData">The additional audit data about the call.</param>
        /// <param name="ignore">not used.</param>
        protected virtual void DoDumpBeforeCall(
            TextWriter writer,
            IMethodInvocation input,
            CallTraceData callData,
            IMethodReturn ignore = null)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (callData == null)
                throw new ArgumentNullException(nameof(callData));

            DumpMethod(writer, input);
            DumpParameters(writer, input);
            DumpPrincipal(writer, callData);
            DumpStack(writer, callData);
        }

        /// <summary>
        /// Logs the after call data.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="input">Object representing the inputs to the current call to the target.</param>
        /// <param name="callData">The additional audit data about the call.</param>
        void LogAfterCallData(
            LogEntry entry,
            IMethodInvocation input,
            CallTraceData callData)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (callData == null)
                throw new ArgumentNullException(nameof(callData));

            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                writer.Indent(2);

                DoDumpAfterCall(writer, input, callData);

                writer.Unindent(2);
                writer.WriteLine();

                entry.Message = writer.GetStringBuilder().ToString();
            }

            LogWriter.Write(entry);
        }

        /// <summary>
        /// Dumps the data that needs to be dumped after the call.
        /// </summary>
        /// <param name="writer">The writer to dump the call information to.</param>
        /// <param name="input">Object representing the inputs to the current call to the target.</param>
        /// <param name="callData">The additional audit data about the call.</param>
        protected virtual void DoDumpAfterCall(
            TextWriter writer,
            IMethodInvocation input,
            CallTraceData callData)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (callData == null)
                throw new ArgumentNullException(nameof(callData));

            DumpMethod(writer, input);
            DumpParametersAfterCall(writer, input, callData);
            DumpResult(writer, input, callData);
            DumpTime(writer, callData);
            if (!LogBeforeCall)
            {
                // these will not change on the way out - dump them only if they are not dumped already
                DumpPrincipal(writer, callData);
                DumpStack(writer, callData);
            }
        }

        /// <summary>
        /// Dumps the time.
        /// </summary>
        /// <param name="writer">The writer to dump the call information to.</param>
        /// <param name="callData">The additional audit data about the call.</param>
        protected static void DumpTime(
            TextWriter writer,
            CallTraceData callData)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            if (callData == null)
                throw new ArgumentNullException(nameof(callData));

            if (callData.CallTimer == null)
                return;

            writer.WriteLine();
            writer.Write($@"Call duration: {callData.CallTimer.Elapsed:d\.hh\.mm\.ss\.fffffff}");
        }

        /// <summary>
        /// Dumps the principal.
        /// </summary>
        /// <param name="writer">The writer to dump the call information to.</param>
        /// <param name="callData">The additional audit data about the call.</param>
        protected void DumpPrincipal(
            TextWriter writer,
            CallTraceData callData)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            if (callData == null)
                throw new ArgumentNullException(nameof(callData));

            if (!IncludePrincipal)
                return;

            writer.WriteLine();
            writer.Write($"Caller Identity: {callData.IdentityName}");
        }

        /// <summary>
        /// Dumps the method.
        /// </summary>
        /// <param name="writer">The writer to dump the call information to.</param>
        /// <param name="input">Object representing the inputs to the current call to the target.</param>
        protected static void DumpMethod(
            TextWriter writer,
            IMethodInvocation input)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            // dump the method on a single line in a simple format
            writer.WriteLine();
            writer.Write($"{input.Target.GetType().Name}.{input.MethodBase.Name}");
        }

        /// <summary>
        /// Dumps the parameters.
        /// </summary>
        /// <param name="writer">The writer to dump the call information to.</param>
        /// <param name="input">Object representing the inputs to the current call to the target.</param>
        protected void DumpParameters(
            TextWriter writer,
            IMethodInvocation input)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (!IncludeParameters)
                return;

            writer.Write("(");
            writer.Indent(2);
            for (var i = 0; i<input.Inputs.Count; i++)
            {
                // dump the parameter
                DumpParameter(writer, input.Inputs.GetParameterInfo(i), input.Inputs[i]);
                if (i != input.Inputs.Count-1)
                    writer.Write(",");
            }
            writer.Write(");");

            writer.Unindent(2);
        }

        /// <summary>
        /// Dumps the parameters after call.
        /// </summary>
        /// <param name="writer">The writer to dump the call information to.</param>
        /// <param name="input">Object representing the inputs to the current call to the target.</param>
        /// <param name="callData">The call data.</param>
        protected void DumpParametersAfterCall(
            TextWriter writer,
            IMethodInvocation input,
            CallTraceData callData)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (callData == null)
                throw new ArgumentNullException(nameof(callData));

            if (!IncludeParameters)
                return;

            // if we already dumped the parameters before the call - dump only the out and ref parameters
            writer.Write("(");
            writer.Indent(2);

            var outValueIndex = 0;

            // dump the parameters
            for (var i = 0; i<input.Inputs.Count; i++)
            {
                var pi = input.Inputs.GetParameterInfo(i);
                var hasOutValue = pi.IsOut || pi.ParameterType.IsByRef;

                if (!LogBeforeCall || hasOutValue)
                {
                    var outValue = callData.OutputValues != null  &&
                                   outValueIndex < callData.OutputValues.Count
                                        ? callData.OutputValues[outValueIndex++]
                                        : null;

                    DumpOutputParameter(writer, pi, input.Inputs[i], hasOutValue, outValue);
                }

                if (i != input.Inputs.Count-1)
                    writer.Write(",");
            }

            writer.Write(");");
            writer.Unindent(2);
        }

        /// <summary>
        /// Dumps the parameter.
        /// </summary>
        /// <param name="writer">The writer to dump the call information to.</param>
        /// <param name="pi">The reflection structure representing the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        protected static void DumpParameter(
            TextWriter writer,
            ParameterInfo pi,
            object value)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            if (pi == null)
                throw new ArgumentNullException(nameof(pi));

            writer.WriteLine();
            writer.Write(
                "{0}{1} {2}{3}",
                pi.IsOut
                    ? "out "
                    : (pi.ParameterType.IsByRef ? "ref " : string.Empty),
                pi.ParameterType.Name,
                pi.Name,
                !pi.IsOut ? " = " : string.Empty);

            if (!pi.IsOut)  // don't dump out parameters!
                value.DumpText(writer, 5, null, pi.GetCustomAttribute<DumpAttribute>(true));
        }

        /// <summary>
        /// Dumps the output parameter.
        /// </summary>
        /// <param name="writer">The writer to dump the call information to.</param>
        /// <param name="pi">The reflection structure representing an output parameter.</param>
        /// <param name="inValue">The value of the parameter on input.</param>
        /// <param name="hasOutValue">The parameter has output value.</param>
        /// <param name="outValue">The output value of the ref/output parameter.</param>
        protected static void DumpOutputParameter(
            TextWriter writer,
            ParameterInfo pi,
            object inValue,
            bool hasOutValue,
            object outValue = null)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            if (pi == null)
                throw new ArgumentNullException(nameof(pi));

            writer.WriteLine();
            writer.Write(
                "{0}{1} {2} = ",
                pi.IsOut ? "out "
                         : (pi.ParameterType.IsByRef ? "ref " : string.Empty),
                pi.ParameterType.Name,
                pi.Name,
                !pi.IsOut ? " = " : string.Empty);

            var dumpAttribute = pi.GetCustomAttribute<DumpAttribute>(true);

            if (!pi.IsOut)  // don't dump out parameters!
                inValue.DumpText(writer, 5, null, dumpAttribute);
            if (hasOutValue)
            {
                writer.Write(" -> ");
                outValue.DumpText(writer, 5, null, dumpAttribute);
            }
        }

        /// <summary>
        /// Dumps the stack.
        /// </summary>
        /// <param name="writer">The writer to dump the call information to.</param>
        /// <param name="callData">The additional audit data about the call.</param>
        protected void DumpStack(
            TextWriter writer,
            CallTraceData callData)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            if (callData == null)
                throw new ArgumentNullException(nameof(callData));

            if (!IncludeCallStack)
                return;

            writer.WriteLine();
            writer.Write("Call stack:");

            writer.Indent(2);

            writer.WriteLine();
            using (var reader = new StringReader(callData.CallStack))
            {
                // skip the first line
                var line = reader.ReadLine();

                while (line != null)
                {
                    line = reader.ReadLine();
                    if (line != null)
                        writer.WriteLine(line);
                }
            }

            writer.Unindent(2);
        }

        /// <summary>
        /// Dumps the result.
        /// </summary>
        /// <param name="writer">The writer to dump the call information to.</param>
        /// <param name="input">Object representing the inputs to the current call to the target.</param>
        /// <param name="callData">The call data.</param>
        protected void DumpResult(
            TextWriter writer,
            IMethodInvocation input,
            CallTraceData callData)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (callData == null)
                throw new ArgumentNullException(nameof(callData));

            writer.WriteLine();
            if (IncludeException && callData.Exception != null)
            {
                writer.Write("THROWS EXCEPTION: ");
                callData.Exception.DumpText(writer, 2);
            }
            else
            if (IncludeReturnValue  &&  callData.ReturnValue!=null)
            {
                var methodInfo = input.MethodBase as MethodInfo;

                if (methodInfo == null || methodInfo.ReturnType == typeof(void)  ||  methodInfo.ReturnType == typeof(Task))
                    return;

                writer.Write("RETURN VALUE: ");
                callData.ReturnValue.DumpText(
                    writer,
                    2,
                    null,
                    methodInfo.GetCustomAttribute<DumpAttribute>());
            }
        }
    }
}
