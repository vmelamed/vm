using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.ServiceModel;
using System.Threading.Tasks;
using vm.Aspects.Diagnostics;
using vm.Aspects.Facilities;

namespace vm.Aspects.Policies
{
    /// <summary>
    /// Class CallData encapsulates some audit data to be output about the current call.
    /// </summary>
    public class CallData
    {
        /// <summary>
        /// Gets or sets the call stack.
        /// </summary>
        public string CallStack { get; set; }

        /// <summary>
        /// Gets or sets the call timer that measures the actual call duration.
        /// </summary>
        public Stopwatch CallTimer { get; set; }

        /// <summary>
        /// Gets or sets the caller's principal identity name.
        /// </summary>
        public string IdentityName { get; set; }
    }

    /// <summary>
    /// Class CallTraceCallHandler is an aspect (policy) which when injected in an object will dump information about each call
    /// into the current <see cref="P:Facility.LogWriter" />.
    /// </summary>
    public class CallTraceCallHandler : BaseCallHandler<CallData>
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
            Contract.Requires<ArgumentNullException>(logWriter != null, nameof(logWriter));

            LogWriter = logWriter;
        }

        #region Overridables
        /// <summary>
        /// Prepares per call data specific to the handler - an instance of <see cref="CallData"/>.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>T.</returns>
        protected override CallData Prepare(
            IMethodInvocation input)
        {
            Contract.Ensures(Contract.Result<CallData>() != null);

            var callData = new CallData();

            return InitializeCallData(callData, input);
        }

        /// <summary>
        /// Initializes the call data.
        /// </summary>
        /// <param name="callData">The call data.</param>
        /// <param name="input">The input.</param>
        /// <returns>CallData.</returns>
        protected virtual CallData InitializeCallData(
            CallData callData,
            IMethodInvocation input)
        {
            Contract.Requires<ArgumentNullException>(callData != null, nameof(callData));
            Contract.Ensures(Contract.Result<CallData>() != null);

            if (IncludeCallStack)
                callData.CallStack = Environment.StackTrace;

            if (IncludeCallTime)
                callData.CallTimer = new Stopwatch();

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
            CallData callData)
        {
            Contract.Ensures(Contract.Result<IMethodReturn>() == null);

            if (!LogBeforeCall  ||  !LogWriter.IsLoggingEnabled())
                return null;

            var entry = NewLogEntry(StartCallCategory);

            if (!LogWriter.ShouldLog(entry))
                return null;

            Action logBeforeCall = () => Facility.ExceptionManager.Process(
                                                () => LogBeforeCallData(entry, input, callData),
                                                ExceptionPolicyProvider.LogAndSwallow);

            if (LogAsynchronously)
                Task.Run(logBeforeCall);
            else
                logBeforeCall();

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
            CallData callData)
        {
            if (LogWriter.IsLoggingEnabled() && LogAfterCall && IncludeCallTime)
                callData.CallTimer.Start();

            return base.DoInvoke(input, getNext, callData);
        }

        /// <summary>
        /// Posts the invoke.
        /// </summary>
        /// <param name="input">Object representing the inputs to the current call to the target.</param>
        /// <param name="callData">The additional audit data about the call.</param>
        /// <param name="methodReturn">Object representing the return value from the target.</param>
        /// <returns>Object representing the return value from the target.</returns>
        protected virtual IMethodReturn PostInvoke(
            IMethodInvocation input,
            CallData callData,
            IMethodReturn methodReturn)
        {
            Contract.Ensures(Contract.Result<IMethodReturn>() != null);

            // async methods are always dumped in ContinueWith
            if (methodReturn.ReturnValue is Task)
                return methodReturn;

            callData.CallTimer.Stop();

            if (!LogAfterCall || !LogWriter.IsLoggingEnabled())
                return methodReturn;

            Action<LogEntry> logAfterCall = e => Facility.ExceptionManager
                                                         .Process(
                                                            () => LogAfterCallData(e, input, callData, methodReturn),
                                                            ExceptionPolicyProvider.LogAndSwallow);

            var entry = NewLogEntry(EndCallCategory);

            if (!LogWriter.ShouldLog(entry))
                return methodReturn;

            if (LogAsynchronously)
                Task.Run(() => logAfterCall(entry));
            else
                logAfterCall(entry);

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
            CallData callData)
        {
            var result = await base.ContinueWith<TResult>(input, methodReturn, callData);

            callData.CallTimer.Stop();

            if (!LogAfterCall || !LogWriter.IsLoggingEnabled())
                return result;

            Action<LogEntry> logAfterCall = e => Facility.ExceptionManager
                                                         .Process(
                                                            () => LogAfterCallData(e, input, callData, methodReturn),
                                                            ExceptionPolicyProvider.LogAndSwallow);

            var entry = NewLogEntry(EndCallCategory);

            if (!LogWriter.ShouldLog(entry))
                return result;

            if (LogAsynchronously)
                Task.Run(() => logAfterCall(entry));
            else
                logAfterCall(entry);

            return result;
        }
        #endregion

        /// <summary>
        /// Creates a new log entry.
        /// </summary>
        /// <returns>LogEntry.</returns>
        LogEntry NewLogEntry(string category) =>
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
            CallData callData)
        {
            Contract.Requires<ArgumentNullException>(entry != null, nameof(entry));
            Contract.Requires<ArgumentNullException>(input != null, nameof(input));
            Contract.Requires<ArgumentNullException>(callData != null, nameof(callData));

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
            CallData callData,
            IMethodReturn ignore = null)
        {
            Contract.Requires<ArgumentNullException>(writer != null, nameof(writer));
            Contract.Requires<ArgumentNullException>(input != null, nameof(input));
            Contract.Requires<ArgumentNullException>(callData != null, nameof(callData));

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
        /// <param name="methodReturn">Object representing the return value from the target.</param>
        void LogAfterCallData(
            LogEntry entry,
            IMethodInvocation input,
            CallData callData,
            IMethodReturn methodReturn)
        {
            Contract.Requires<ArgumentNullException>(entry != null, nameof(entry));
            Contract.Requires<ArgumentNullException>(input != null, nameof(input));
            Contract.Requires<ArgumentNullException>(callData != null, nameof(callData));
            Contract.Requires<ArgumentNullException>(methodReturn != null, nameof(methodReturn));

            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                writer.Indent(2);

                DoDumpAfterCall(writer, input, callData, methodReturn);

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
        /// <param name="methodReturn">Object representing the return value from the target.</param>
        protected virtual void DoDumpAfterCall(
            TextWriter writer,
            IMethodInvocation input,
            CallData callData,
            IMethodReturn methodReturn)
        {
            Contract.Requires<ArgumentNullException>(writer       != null, nameof(writer));
            Contract.Requires<ArgumentNullException>(input        != null, nameof(input));
            Contract.Requires<ArgumentNullException>(callData     != null, nameof(callData));
            Contract.Requires<ArgumentNullException>(methodReturn != null, nameof(methodReturn));

            DumpMethod(writer, input);
            DumpParametersAfterCall(writer, input);
            DumpResult(writer, input, methodReturn);
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
        protected void DumpTime(
            TextWriter writer,
            CallData callData)
        {
            Contract.Requires<ArgumentNullException>(writer != null, nameof(writer));
            Contract.Requires<ArgumentNullException>(callData != null, nameof(callData));

            if (!IncludeCallTime  ||  callData.CallTimer == null)
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
            CallData callData)
        {
            Contract.Requires<ArgumentNullException>(writer != null, nameof(writer));
            Contract.Requires<ArgumentNullException>(callData != null, nameof(callData));

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
            Contract.Requires<ArgumentNullException>(writer != null, nameof(writer));
            Contract.Requires<ArgumentNullException>(input != null, nameof(input));

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
            Contract.Requires<ArgumentNullException>(writer != null, nameof(writer));
            Contract.Requires<ArgumentNullException>(input != null, nameof(input));

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
        protected void DumpParametersAfterCall(
            TextWriter writer,
            IMethodInvocation input)
        {
            Contract.Requires<ArgumentNullException>(writer != null, nameof(writer));
            Contract.Requires<ArgumentNullException>(input != null, nameof(input));

            if (!IncludeParameters)
                return;

            // if we already dumped the parameters before the call - dump only the out and ref parameters
            writer.Write("(");
            writer.Indent(2);

            // dump the parameters
            for (var i = 0; i<input.Inputs.Count; i++)
            {
                var pi = input.Inputs.GetParameterInfo(i);

                if (!LogBeforeCall || pi.IsOut || pi.ParameterType.IsByRef)
                    DumpOutputParameter(writer, pi, input.Inputs[i]);

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
            Contract.Requires<ArgumentNullException>(writer != null, nameof(writer));
            Contract.Requires<ArgumentNullException>(pi != null, nameof(pi));

            writer.WriteLine();
            writer.Write(
                "{0}{1} {2}{3}",
                pi.IsOut ? "out " : (pi.ParameterType.IsByRef ? "ref " : string.Empty),
                pi.ParameterType.Name,
                pi.Name,
                !pi.IsOut ? " = " : string.Empty);

            value.DumpText(writer, 5, null, pi.GetCustomAttribute<DumpAttribute>(true));
        }

        /// <summary>
        /// Dumps the output parameter.
        /// </summary>
        /// <param name="writer">The writer to dump the call information to.</param>
        /// <param name="pi">The reflection structure representing an output parameter.</param>
        /// <param name="value">The value of the output parameter.</param>
        protected static void DumpOutputParameter(
            TextWriter writer,
            ParameterInfo pi,
            object value)
        {
            Contract.Requires<ArgumentNullException>(writer != null, nameof(writer));
            Contract.Requires<ArgumentNullException>(pi != null, nameof(pi));

            if (pi.ParameterType == typeof(Task))
                return;

            writer.WriteLine();
            writer.Write(
                "{0}{1} {2} = ",
                pi.IsOut ? "out " : (pi.ParameterType.IsByRef ? "ref " : string.Empty),
                pi.ParameterType.Name,
                pi.Name);

            value.DumpText(writer, 2, null, pi.GetCustomAttribute<DumpAttribute>(true));
        }

        /// <summary>
        /// Dumps the stack.
        /// </summary>
        /// <param name="writer">The writer to dump the call information to.</param>
        /// <param name="callData">The additional audit data about the call.</param>
        protected void DumpStack(
            TextWriter writer,
            CallData callData)
        {
            Contract.Requires<ArgumentNullException>(writer != null, nameof(writer));
            Contract.Requires<ArgumentNullException>(callData != null, nameof(callData));

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
        /// <param name="methodReturn">Object representing the return value from the target.</param>
        protected void DumpResult(
            TextWriter writer,
            IMethodInvocation input,
            IMethodReturn methodReturn)
        {
            Contract.Requires<ArgumentNullException>(writer != null, nameof(writer));
            Contract.Requires<ArgumentNullException>(input != null, nameof(input));
            Contract.Requires<ArgumentNullException>(methodReturn != null, nameof(methodReturn));

            writer.WriteLine();
            if (IncludeException && methodReturn.Exception != null)
            {
                writer.Write("THROWS EXCEPTION: ");
                methodReturn.Exception.DumpText(writer, 2);
            }
            else
            if (IncludeReturnValue && methodReturn.ReturnValue!=null)
            {
                var methodInfo = input.MethodBase as MethodInfo;

                if (methodInfo == null || methodInfo.ReturnType == typeof(void)  ||  methodInfo.ReturnType == typeof(Task))
                    return;

                writer.Write("RETURN VALUE: ");
                methodReturn.ReturnValue.DumpText(
                    writer,
                    2,
                    null,
                    methodInfo.GetCustomAttribute<DumpAttribute>());
            }
        }
    }
}
