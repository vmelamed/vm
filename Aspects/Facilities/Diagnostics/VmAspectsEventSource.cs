using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Diagnostics.Tracing;
using Microsoft.Practices.Unity.InterceptionExtension;
using vm.Aspects.Threading;

namespace vm.Aspects.Facilities.Diagnostics
{
    /// <summary>
    /// Class EtwEventSource. This class cannot be inherited.
    /// </summary>
    /// <seealso cref="EventSource" />
    [EventSource(Name = "vm-Aspects-EventSource")]
    public sealed partial class VmAspectsEventSource : EventSource
    {
        /// <summary>
        /// The log singleton instance.
        /// </summary>
        public static VmAspectsEventSource Log { get; }

        static readonly IReadOnlyDictionary<EventLevel, Action<string, string, string>> _exceptions;
        static readonly IReadOnlyDictionary<EventLevel, Action<string>> _traces;
        static readonly IReadOnlyDictionary<EventLevel, Action<string, string>> _dumps;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "We need to guarantees the order of initialization of Log first.")]
        static VmAspectsEventSource()
        {
            // make sure _log is initialized before _xyz
            Log = new VmAspectsEventSource();

            _exceptions = new ReadOnlyDictionary<EventLevel, Action<string, string, string>>(
                new Dictionary<EventLevel, Action<string, string, string>>
                {
                    [EventLevel.Verbose]       = Log.VerboseException,
                    [EventLevel.Warning]       = Log.WarningException,
                    [EventLevel.Error]         = Log.ErrorException,
                    [EventLevel.Critical]      = Log.CriticalException,
                    [EventLevel.Informational] = Log.InformationalException,
                    [EventLevel.LogAlways]     = Log.AlwaysException,
                });

            _traces = new ReadOnlyDictionary<EventLevel, Action<string>>(
                new Dictionary<EventLevel, Action<string>>
                {
                    [EventLevel.Verbose]       = Log.VerboseTrace,
                    [EventLevel.Warning]       = Log.WarningTrace,
                    [EventLevel.Error]         = Log.ErrorTrace,
                    [EventLevel.Critical]      = Log.CriticalTrace,
                    [EventLevel.Informational] = Log.InformationalTrace,
                    [EventLevel.LogAlways]     = Log.AlwaysTrace,
                });

            _dumps = new ReadOnlyDictionary<EventLevel, Action<string, string>>(
                new Dictionary<EventLevel, Action<string, string>>
                {
                    [EventLevel.Verbose]       = Log.VerboseDump,
                    [EventLevel.Warning]       = Log.WarningDump,
                    [EventLevel.Error]         = Log.ErrorDump,
                    [EventLevel.Critical]      = Log.CriticalDump,
                    [EventLevel.Informational] = Log.InformationalDump,
                    [EventLevel.LogAlways]     = Log.AlwaysDump,
                });
        }

        /// <summary>
        /// Outputs a trace to ETW.
        /// </summary>
        /// <param name="eventLevel">Level of the event.</param>
        /// <param name="text">The text of the trace.</param>
        [NonEvent]
        public void Trace(
            string text,
            EventLevel eventLevel = EventLevel.Informational)
        {
            if (!IsEnabled(eventLevel, Keywords.vmAspects | Keywords.Trace))
                return;

            Action<string> writeMessage;

            if (!_traces.TryGetValue(eventLevel, out writeMessage))
                writeMessage = Log.InformationalTrace;

            writeMessage(text);
        }

        #region Write trace to ETW
        [Event(VerboseTraceId, Level = EventLevel.Verbose, Keywords = Keywords.vmAspects | Keywords.Trace | Keywords.Trace, Message = "{0}")]
        void VerboseTrace(
            string Text)
            => WriteEvent(VerboseTraceId, Text);

        [Event(InformationalTraceId, Level = EventLevel.Informational, Keywords = Keywords.vmAspects | Keywords.Trace | Keywords.Trace, Message = "{0}")]
        void InformationalTrace(
            string Text)
            => WriteEvent(InformationalTraceId, Text);

        [Event(WarningTraceId, Level = EventLevel.Warning, Keywords = Keywords.vmAspects | Keywords.Trace | Keywords.Trace, Message = "{0}")]
        void WarningTrace(
            string Text)
            => WriteEvent(WarningTraceId, Text);

        [Event(ErrorTraceId, Level = EventLevel.Error, Keywords = Keywords.vmAspects | Keywords.Trace | Keywords.Trace, Message = "{0}")]
        void ErrorTrace(
            string Text)
            => WriteEvent(ErrorTraceId, Text);

        [Event(CriticalTraceId, Level = EventLevel.Critical, Keywords = Keywords.vmAspects | Keywords.Trace | Keywords.Trace, Message = "{0}")]
        void CriticalTrace(
            string Text)
            => WriteEvent(CriticalTraceId, Text);

        [Event(AlwaysTraceId, Level = EventLevel.Critical, Keywords = Keywords.vmAspects | Keywords.Trace | Keywords.Trace, Message = "{0}")]
        void AlwaysTrace(
            string Text)
            => WriteEvent(AlwaysTraceId, Text);
        #endregion

        /// <summary>
        /// Dump an object to ETW.
        /// </summary>
        /// <param name="description">Short text describing of the object dump.</param>
        /// <param name="reference">The reference to the object to be dumped.</param>
        /// <param name="eventLevel">Level of the event.</param>
        [NonEvent]
        public void Dump(
            string description,
            object reference,
            EventLevel eventLevel = EventLevel.Verbose)
        {
            if (!IsEnabled(eventLevel, Keywords.vmAspects | Keywords.Dump))
                return;

            Action<string, string> dump;

            if (!_dumps.TryGetValue(eventLevel, out dump))
                dump = Log.VerboseDump;

            dump(description, reference.DumpString());
        }

        #region Write text and dumped object to ETW
        [Event(VerboseDumpId, Level = EventLevel.Verbose, Keywords = Keywords.vmAspects | Keywords.Dump | Keywords.Dump, Message = "{0}")]
        void VerboseDump(
            string Description,
            string Dump)
            => WriteEvent(VerboseDumpId, Description, Dump);

        [Event(InformationalDumpId, Level = EventLevel.Informational, Keywords = Keywords.vmAspects | Keywords.Dump | Keywords.Dump, Message = "{0}")]
        void InformationalDump(
            string Description,
            string Dump)
            => WriteEvent(InformationalDumpId, Description, Dump);

        [Event(WarningDumpId, Level = EventLevel.Warning, Keywords = Keywords.vmAspects | Keywords.Dump | Keywords.Dump, Message = "{0}")]
        void WarningDump(
            string Description,
            string Dump)
            => WriteEvent(WarningDumpId, Description, Dump);

        [Event(ErrorDumpId, Level = EventLevel.Error, Keywords = Keywords.vmAspects | Keywords.Dump | Keywords.Dump, Message = "{0}")]
        void ErrorDump(
            string Description,
            string Dump)
            => WriteEvent(ErrorDumpId, Description, Dump);

        [Event(CriticalDumpId, Level = EventLevel.Critical, Keywords = Keywords.vmAspects | Keywords.Dump | Keywords.Dump, Message = "{0}")]
        void CriticalDump(
            string Description,
            string Dump)
            => WriteEvent(CriticalDumpId, Description, Dump);

        [Event(AlwaysDumpId, Level = EventLevel.Critical, Keywords = Keywords.vmAspects | Keywords.Dump | Keywords.Dump, Message = "{0}")]
        void AlwaysDump(
            string Description,
            string Dump)
            => WriteEvent(AlwaysDumpId, Description, Dump);
        #endregion

        /// <summary>
        /// Writes an exception to the ETW.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="eventLevel">The event level.</param>
        [NonEvent]
        public void Exception(
            Exception ex,
            EventLevel eventLevel = EventLevel.Error)
        {
            Contract.Requires<ArgumentNullException>(ex != null, nameof(ex));

            if (!IsEnabled(eventLevel, Keywords.vmAspects | Keywords.Exception))
                return;

            Action<string, string, string> writeException;

            if (!_exceptions.TryGetValue(eventLevel, out writeException))
                writeException = Log.ErrorException;

            writeException(ex.GetType().FullName, ex.Message, ex.DumpString());
        }

        #region Exceptions
        [Event(VerboseExceptionId, Level = EventLevel.Verbose, Message = "{0}", Keywords = Keywords.vmAspects | Keywords.Exception)]
        void VerboseException(
            string Type,
            string Message,
            string Dump)
            => WriteEvent(VerboseExceptionId, Type, Message, Dump);

        [Event(InformationalExceptionId, Level = EventLevel.Informational, Message = "{0}", Keywords = Keywords.vmAspects | Keywords.Exception)]
        void InformationalException(
            string Type,
            string Message,
            string Dump)
            => WriteEvent(InformationalExceptionId, Type, Message, Dump);

        [Event(WarningExceptionId, Level = EventLevel.Warning, Message = "{0}", Keywords = Keywords.vmAspects | Keywords.Exception)]
        void WarningException(
            string Type,
            string Message,
            string Dump)
            => WriteEvent(WarningExceptionId, Type, Message, Dump);

        [Event(ErrorExceptionId, Level = EventLevel.Error, Message = "{0}", Keywords = Keywords.vmAspects | Keywords.Exception)]
        void ErrorException(
            string Type,
            string Message,
            string Dump)
            => WriteEvent(ErrorExceptionId, Type, Message, Dump);

        [Event(CriticalExceptionId, Level = EventLevel.Critical, Message = "{0}", Keywords = Keywords.vmAspects | Keywords.Exception)]
        void CriticalException(
            string Type,
            string Message,
            string Dump)
            => WriteEvent(CriticalExceptionId, Type, Message, Dump);

        [Event(AlwaysExceptionId, Level = EventLevel.LogAlways, Message = "{0}", Keywords = Keywords.vmAspects | Keywords.Exception)]
        void AlwaysException(
            string Type,
            string Message,
            string Dump)
            => WriteEvent(AlwaysExceptionId, Type, Message, Dump);
        #endregion

        /// <summary>
        /// Writes an event to ETW when the specified registrar was registered in <see cref="DIContainer"/>.
        /// </summary>
        /// <param name="registrar">The registrar instance.</param>
        [NonEvent]
        public void Registered(
            ContainerRegistrar registrar)
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.vmAspects | Keywords.DI))
                Registered(registrar.GetType().FullName);
        }

        /// <summary>
        /// Writes an event to ETW when the specified registrar was registered in <see cref="DIContainer"/>.
        /// </summary>
        /// <param name="Type">The type.</param>
        [Event(RegisteredId, Level = EventLevel.Verbose, Message = "Registered {0}.", Keywords = Keywords.vmAspects | Keywords.DI)]
        void Registered(
            string Type)
        {
            if (IsEnabled())
                WriteEvent(RegisteredId, Type);
        }

        /// <summary>
        /// Writes an event to ETW when a <see cref="Retry{T}"/> retries an operation.
        /// </summary>
        /// <param name="retry">The retry-er.</param>
        /// <param name="delay">The delay.</param>
        public void Retrying<T>(
            Retry<T> retry,
            int delay)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.vmAspects))
                Retrying(retry.GetType().FullName, delay);
        }

        /// <summary>
        /// Writes an event to ETW when a <see cref="RetryTasks{T}"/> retries an operation.
        /// </summary>
        /// <param name="retry">The retry-er.</param>
        /// <param name="delay">The delay.</param>
        public void Retrying<T>(
            RetryTasks<T> retry,
            int delay)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.vmAspects))
                Retrying(retry.GetType().FullName, delay);
        }

        /// <summary>
        /// Writes an event to ETW when a <see cref="Retry{T}"/> retries an operation.
        /// </summary>
        /// <param name="retryType">Type of the retry.</param>
        /// <param name="delay">The delay.</param>
        [Event(RetryingId, Level = EventLevel.Informational, Message = "Retrying {0} after {1}msec delay.", Keywords = Keywords.vmAspects)]
        void Retrying(
            string retryType,
            int delay)
        {
            if (IsEnabled())
                WriteEvent(RetryingId, retryType, delay);
        }

        /// <summary>
        /// Writes an event to ETW when a <see cref="Retry{T}" /> retries an operation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="retry">The retry-er.</param>
        /// <param name="retries">Number of retries.</param>
        /// <param name="maxRetries">The maximum retries.</param>
        public void RetryFailed<T>(
            Retry<T> retry,
            int retries,
            int maxRetries)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.vmAspects))
                RetryFailed(retry.GetType().FullName, retries, maxRetries);
        }

        /// <summary>
        /// Writes an event to ETW when a <see cref="RetryTasks{T}" /> retries an operation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="retry">The retry-er.</param>
        /// <param name="retries">The delay.</param>
        /// <param name="maxRetries">The maximum retries.</param>
        public void RetryFailed<T>(
            RetryTasks<T> retry,
            int retries,
            int maxRetries)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.vmAspects))
                RetryFailed(retry.GetType().FullName, retries, maxRetries);
        }

        /// <summary>
        /// Writes an event to ETW when a <see cref="Retry{T}" /> retries an operation.
        /// </summary>
        /// <param name="retryType">Type of the retry.</param>
        /// <param name="retries">Number of retries.</param>
        /// <param name="maxRetries">The maximum retries.</param>
        [Event(RetryFailedId, Level = EventLevel.Informational, Message = "{0} failed after {1}/{2} retries.", Keywords = Keywords.vmAspects)]
        void RetryFailed(
            string retryType,
            int retries,
            int maxRetries)
        {
            if (IsEnabled())
                WriteEvent(RetryingId, retryType, retries, maxRetries);
        }

        /// <summary>
        /// Writes an event to ETW when a call handler fails a call before or after calling the next aspect in the pipeline.
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="exceptionDump">The exception dump.</param>
        [Event(CallHandlerFailsId, Level = EventLevel.Informational, Message = "{0} returns exception in the call to {1}.{2}", Keywords = Keywords.vmAspects)]
        void CallHandlerFails(
            string targetType,
            string methodName,
            string exceptionDump)
        {
            if (IsEnabled())
                WriteEvent(CallHandlerFailsId, targetType, methodName, exceptionDump);
        }

        /// <summary>
        ///  Writes an event to ETW when a call handler fails a call before or after calling the next aspect in the pipeline.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="exception">The exception.</param>
        [NonEvent]
        public void CallHandlerFails(
            IMethodInvocation input,
            Exception exception)
        {
            if (IsEnabled(EventLevel.Informational, Keywords.vmAspects))
                CallHandlerFails(
                    input.Target.GetType().Name,
                    input.MethodBase.Name,
                    exception.DumpString());
        }
    }
}
