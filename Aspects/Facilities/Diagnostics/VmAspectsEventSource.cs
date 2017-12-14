using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Tracing;

namespace vm.Aspects.Facilities.Diagnostics
{
    /// <summary>
    /// Class EtwEventSource. This class cannot be inherited.
    /// </summary>
    /// <seealso cref="EventSource" />
    [EventSource(Name = "vm-Aspects-EventSource")]
    public sealed partial class VmAspectsEventSource : VmAspectsBaseEventSource
    {
        /// <summary>
        /// The log singleton instance.
        /// </summary>
        public static VmAspectsEventSource Log { get; }

        static readonly IReadOnlyDictionary<EventLevel, Action<string, string, string>> _exceptions;
        static readonly IReadOnlyDictionary<EventLevel, Action<string>> _traces;
        static readonly IReadOnlyDictionary<EventLevel, Action<string, string>> _dumps;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Log must be initialized before the other initializations take place.")]
        static VmAspectsEventSource()
        {
            // make sure _log is initialized before _xyz
            Log = new VmAspectsEventSource();

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

            if (!_traces.TryGetValue(eventLevel, out var writeMessage))
                writeMessage = Log.InformationalTrace;

            writeMessage(text);
        }

        #region Write trace to ETW
        [Event(VerboseTraceId, Level = EventLevel.Verbose, Keywords = Keywords.vmAspects | Keywords.Trace, Message = "{0}")]
        void VerboseTrace(
            string Text)
        {
            if (IsEnabled())
                WriteEvent(VerboseTraceId, Text);
        }

        [Event(InformationalTraceId, Level = EventLevel.Informational, Keywords = Keywords.vmAspects | Keywords.Trace, Message = "{0}")]
        void InformationalTrace(
            string Text)
        {
            if (IsEnabled())
                WriteEvent(InformationalTraceId, Text);
        }

        [Event(WarningTraceId, Level = EventLevel.Warning, Keywords = Keywords.vmAspects | Keywords.Trace, Message = "{0}")]
        void WarningTrace(
            string Text)
        {
            if (IsEnabled())
                WriteEvent(WarningTraceId, Text);
        }

        [Event(ErrorTraceId, Level = EventLevel.Error, Keywords = Keywords.vmAspects | Keywords.Trace, Message = "{0}")]
        void ErrorTrace(
            string Text)
        {
            if (IsEnabled())
                WriteEvent(ErrorTraceId, Text);
        }

        [Event(CriticalTraceId, Level = EventLevel.Critical, Keywords = Keywords.vmAspects | Keywords.Trace, Message = "{0}")]
        void CriticalTrace(
            string Text)
        {
            if (IsEnabled())
                WriteEvent(CriticalTraceId, Text);
        }

        [Event(AlwaysTraceId, Level = EventLevel.Critical, Keywords = Keywords.vmAspects | Keywords.Trace, Message = "{0}")]
        void AlwaysTrace(
            string Text)
        {
            if (IsEnabled())
                WriteEvent(AlwaysTraceId, Text);
        }
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

            if (!_dumps.TryGetValue(eventLevel, out var dump))
                dump = Log.VerboseDump;

            dump(description, reference.DumpString());
        }

        #region Write text and dumped object to ETW
        [Event(VerboseDumpId, Level = EventLevel.Verbose, Keywords = Keywords.vmAspects | Keywords.Dump, Message = "{0}")]
        void VerboseDump(
            string Description,
            string Dump)
        {
            if (IsEnabled())
                WriteEvent(VerboseDumpId, Description, Dump);
        }

        [Event(InformationalDumpId, Level = EventLevel.Informational, Keywords = Keywords.vmAspects | Keywords.Dump, Message = "{0}")]
        void InformationalDump(
            string Description,
            string Dump)
        {
            if (IsEnabled())
                WriteEvent(InformationalDumpId, Description, Dump);
        }

        [Event(WarningDumpId, Level = EventLevel.Warning, Keywords = Keywords.vmAspects | Keywords.Dump, Message = "{0}")]
        void WarningDump(
            string Description,
            string Dump)
        {
            if (IsEnabled())
                WriteEvent(WarningDumpId, Description, Dump);
        }

        [Event(ErrorDumpId, Level = EventLevel.Error, Keywords = Keywords.vmAspects | Keywords.Dump, Message = "{0}")]
        void ErrorDump(
            string Description,
            string Dump)
        {
            if (IsEnabled())
                WriteEvent(ErrorDumpId, Description, Dump);
        }

        [Event(CriticalDumpId, Level = EventLevel.Critical, Keywords = Keywords.vmAspects | Keywords.Dump, Message = "{0}")]
        void CriticalDump(
            string Description,
            string Dump)
        {
            if (IsEnabled())
                WriteEvent(CriticalDumpId, Description, Dump);
        }

        [Event(AlwaysDumpId, Level = EventLevel.Critical, Keywords = Keywords.vmAspects | Keywords.Dump, Message = "{0}")]
        void AlwaysDump(
            string Description,
            string Dump)
        {
            if (IsEnabled())
                WriteEvent(AlwaysDumpId, Description, Dump);
        }
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
            if (ex == null)
                throw new ArgumentNullException(nameof(ex));

            if (!IsEnabled(eventLevel, Keywords.vmAspects | Keywords.Exception))
                return;

            if (!_exceptions.TryGetValue(eventLevel, out var writeException))
                writeException = Log.ErrorException;

            Debug.Assert(writeException != null);

            writeException(ex.GetType().FullName, ex.Message, ex.DumpString());
        }

        #region Exceptions
        [Event(VerboseExceptionId, Level = EventLevel.Verbose, Keywords = Keywords.vmAspects | Keywords.Exception, Message = "{0}")]
        void VerboseException(
            string Type,
            string Message,
            string Dump)
        {
            if (IsEnabled())
                WriteEvent(VerboseExceptionId, Type, Message, Dump);
        }

        [Event(InformationalExceptionId, Level = EventLevel.Informational, Keywords = Keywords.vmAspects | Keywords.Exception, Message = "{0}")]
        void InformationalException(
            string Type,
            string Message,
            string Dump)
        {
            if (IsEnabled())
                WriteEvent(InformationalExceptionId, Type, Message, Dump);
        }

        [Event(WarningExceptionId, Level = EventLevel.Warning, Keywords = Keywords.vmAspects | Keywords.Exception, Message = "{0}")]
        void WarningException(
            string Type,
            string Message,
            string Dump)
        {
            if (IsEnabled())
                WriteEvent(WarningExceptionId, Type, Message, Dump);
        }

        [Event(ErrorExceptionId, Level = EventLevel.Error, Keywords = Keywords.vmAspects | Keywords.Exception, Message = "{0}")]
        void ErrorException(
            string Type,
            string Message,
            string Dump)
        {
            if (IsEnabled())
                WriteEvent(ErrorExceptionId, Type, Message, Dump);
        }

        [Event(CriticalExceptionId, Level = EventLevel.Critical, Keywords = Keywords.vmAspects | Keywords.Exception, Message = "{0}")]
        void CriticalException(
            string Type,
            string Message,
            string Dump)
        {
            if (IsEnabled())
                WriteEvent(CriticalExceptionId, Type, Message, Dump);
        }

        [Event(AlwaysExceptionId, Level = EventLevel.LogAlways, Keywords = Keywords.vmAspects | Keywords.Exception, Message = "{0}")]
        void AlwaysException(
            string Type,
            string Message,
            string Dump)
        {
            if (IsEnabled())
                WriteEvent(AlwaysExceptionId, Type, Message, Dump);
        }
        #endregion

        #region Registrar registered in the DI
        /// <summary>
        /// Writes an event to ETW when the specified registrar was registered in <see cref="DIContainer"/>.
        /// </summary>
        /// <param name="registrar">The registrar instance.</param>
        [NonEvent]
        public void RegistrarRegistered(
            ContainerRegistrar registrar)
        {
            if (IsEnabled(EventLevel.Verbose, Keywords.vmAspects | Keywords.DI))
                RegistrarRegistered(registrar.GetType().FullName);
        }

        /// <summary>
        /// Writes an event to ETW when the specified registrar was registered in <see cref="DIContainer"/>.
        /// </summary>
        /// <param name="Type">The type.</param>
        [Event(RegistrarRegisteredId, Level = EventLevel.Verbose, Keywords = Keywords.vmAspects | Keywords.DI, Message = "Registrar {0} registered.")]
        void RegistrarRegistered(
            string Type)
        {
            if (IsEnabled())
                WriteEvent(RegistrarRegisteredId, Type);
        }
        #endregion

#pragma warning disable CS3001 // Argument type is not CLS-compliant
        #region Call handler causes exception
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
#pragma warning restore CS3001 // Argument type is not CLS-compliant

        /// <summary>
        /// Writes an event to ETW when a call handler fails a call before or after calling the next aspect in the pipeline.
        /// </summary>
        /// <param name="TargetType">Type of the target.</param>
        /// <param name="MethodName">Name of the method.</param>
        /// <param name="ExceptionDump">The exception dump.</param>
        [Event(CallHandlerFailsId, Level = EventLevel.Informational, Keywords = Keywords.vmAspects | Keywords.Aspect, Message = "{0} returns exception in the call to {1}.{2}")]
        void CallHandlerFails(
            string TargetType,
            string MethodName,
            string ExceptionDump)
        {
            if (IsEnabled())
                WriteEvent(CallHandlerFailsId, TargetType, MethodName, ExceptionDump);
        }
        #endregion
    }
}
