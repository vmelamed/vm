using Microsoft.Practices.EnterpriseLibrary.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;

namespace vm.Aspects.Facilities.LogWriters.Etw
{
    /// <summary>
    /// ETW event source that takes Event Library's logging application block's log event entries.
    /// </summary>
    /// <seealso cref="EtwLogEntryBaseEventSource" />
    [EventSource(Name = "vm-Aspects-LogEntryEventSource")]
    public sealed partial class EtwLogEntryEventSource : EtwLogEntryBaseEventSource
    {
        /// <summary>
        /// The log singleton instance.
        /// </summary>
        public static EtwLogEntryEventSource Log { get; }

        static readonly IReadOnlyDictionary<TraceEventType, Action<int, int, string, string, string, string, string>> _writeLogEntry;
        static readonly IReadOnlyDictionary<TraceEventType, Action<string>> _writeMessage;
        static readonly IReadOnlyDictionary<TraceEventType, Action<string>> _dumpObject;
        static readonly IReadOnlyDictionary<TraceEventType, Action<int, string, string>> _trace;

        static readonly IReadOnlyDictionary<TraceEventType, EventLevel> _traceEventType2eventLevel =
            new ReadOnlyDictionary<TraceEventType, EventLevel>(
                new SortedDictionary<TraceEventType, EventLevel>
                {
                    [TraceEventType.Verbose]     = EventLevel.Verbose,
                    [TraceEventType.Warning]     = EventLevel.Warning,
                    [TraceEventType.Error]       = EventLevel.Error,
                    [TraceEventType.Critical]    = EventLevel.Critical,
                    [TraceEventType.Information] = EventLevel.Informational,
                    [TraceEventType.Start]       = EventLevel.Informational,
                    [TraceEventType.Suspend]     = EventLevel.Informational,
                    [TraceEventType.Resume]      = EventLevel.Informational,
                    [TraceEventType.Stop]        = EventLevel.Informational,
                    [TraceEventType.Transfer]    = EventLevel.Informational,
                });

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Log must be initialized before the other initializations take place.")]
        static EtwLogEntryEventSource()
        {
            // make sure Log is created before _writeXyz
            Log = new EtwLogEntryEventSource();

            _writeMessage = new ReadOnlyDictionary<TraceEventType, Action<string>>(
                new Dictionary<TraceEventType, Action<string>>
                {
                    [TraceEventType.Verbose]     = Log.VerboseMessage,
                    [TraceEventType.Warning]     = Log.WarningMessage,
                    [TraceEventType.Error]       = Log.ErrorMessage,
                    [TraceEventType.Critical]    = Log.CriticalMessage,
                    [TraceEventType.Information] = Log.InformationalMessage,
                    [TraceEventType.Start]       = Log.InformationalMessage,
                    [TraceEventType.Suspend]     = Log.InformationalMessage,
                    [TraceEventType.Resume]      = Log.InformationalMessage,
                    [TraceEventType.Stop]        = Log.InformationalMessage,
                    [TraceEventType.Transfer]    = Log.InformationalMessage,
                });

            _dumpObject = new ReadOnlyDictionary<TraceEventType, Action<string>>(
                new Dictionary<TraceEventType, Action<string>>
                {
                    [TraceEventType.Verbose]     = Log.VerboseDumpObject,
                    [TraceEventType.Warning]     = Log.WarningDumpObject,
                    [TraceEventType.Error]       = Log.ErrorDumpObject,
                    [TraceEventType.Critical]    = Log.CriticalDumpObject,
                    [TraceEventType.Information] = Log.InformationalDumpObject,
                    [TraceEventType.Start]       = Log.InformationalDumpObject,
                    [TraceEventType.Suspend]     = Log.InformationalDumpObject,
                    [TraceEventType.Resume]      = Log.InformationalDumpObject,
                    [TraceEventType.Stop]        = Log.InformationalDumpObject,
                    [TraceEventType.Transfer]    = Log.InformationalDumpObject,
                });

            _trace = new ReadOnlyDictionary<TraceEventType, Action<int, string, string>>(
                new Dictionary<TraceEventType, Action<int, string, string>>
                {
                    [TraceEventType.Verbose]     = Log.VerboseTrace,
                    [TraceEventType.Warning]     = Log.WarningTrace,
                    [TraceEventType.Error]       = Log.ErrorTrace,
                    [TraceEventType.Critical]    = Log.CriticalTrace,
                    [TraceEventType.Information] = Log.InformationalTrace,
                    [TraceEventType.Start]       = Log.InformationalTrace,
                    [TraceEventType.Suspend]     = Log.InformationalTrace,
                    [TraceEventType.Resume]      = Log.InformationalTrace,
                    [TraceEventType.Stop]        = Log.InformationalTrace,
                    [TraceEventType.Transfer]    = Log.InformationalTrace,
                });

            _writeLogEntry = new ReadOnlyDictionary<TraceEventType, Action<int, int, string, string, string, string, string>>(
                new Dictionary<TraceEventType, Action<int, int, string, string, string, string, string>>
                {
                    [TraceEventType.Verbose]     = Log.VerboseLogEntry,
                    [TraceEventType.Warning]     = Log.WarningLogEntry,
                    [TraceEventType.Error]       = Log.ErrorLogEntry,
                    [TraceEventType.Critical]    = Log.CriticalLogEntry,
                    [TraceEventType.Information] = Log.InformationalLogEntry,
                    [TraceEventType.Start]       = Log.InformationalLogEntry,
                    [TraceEventType.Suspend]     = Log.InformationalLogEntry,
                    [TraceEventType.Resume]      = Log.InformationalLogEntry,
                    [TraceEventType.Stop]        = Log.InformationalLogEntry,
                    [TraceEventType.Transfer]    = Log.InformationalLogEntry,
                });
        }

        bool IsEnabled(
            TraceEventType eventType,
            EventKeywords keyWords)
            => IsEnabled(_traceEventType2eventLevel[eventType], keyWords);


        /// <summary>
        /// Dispatches the message to the appropriate message event writer.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="message">The message.</param>
        [NonEvent]
        public void WriteMessage(
            string message,
            TraceEventType eventType = TraceEventType.Information)
        {
            if (!IsEnabled(eventType, Keywords.ELLab | Keywords.Message))
                return;

            if (!_writeMessage.TryGetValue(eventType, out var writeMessage))
                writeMessage = Log.InformationalMessage;

            writeMessage(message);
        }

        /// <summary>
        /// Dispatches a dump of the object to the appropriate dumped object event writer.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="data">The object.</param>
        [NonEvent]
        public void DumpObject(
            object data,
            TraceEventType eventType = TraceEventType.Verbose)
        {
            if (!IsEnabled(eventType, Keywords.ELLab | Keywords.Dump))
                return;

            var dataDump = data.DumpString();

            if (!_dumpObject.TryGetValue(eventType, out var dumpObject))
                dumpObject = Log.InformationalDumpObject;

            dumpObject(dataDump);
        }

        /// <summary>
        /// Dispatches the trace text to the appropriate trace event writer.
        /// </summary>
        /// <param name="id">The EL LAB identifier.</param>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="source">The source.</param>
        /// <param name="text">The text.</param>
        [NonEvent]
        public void Trace(
            int id,
            string text,
            string source = null,
            TraceEventType eventType = TraceEventType.Verbose)
        {
            if (!IsEnabled(eventType, Keywords.ELLab | Keywords.Trace))
                return;

            if (!_trace.TryGetValue(eventType, out var trace))
                trace = Log.InformationalTrace;

            trace(id, source, text);
        }

        /// <summary>
        /// Dispatches the log entry to the appropriate log entry event writer.
        /// </summary>
        /// <param name="logEntry">The log entry.</param>
        [NonEvent]
        public void WriteLogEntry(
            LogEntry logEntry)
        {
            if (logEntry == null)
                throw new ArgumentNullException(nameof(logEntry));

            if (!IsEnabled(logEntry.Severity, Keywords.ELLab | Keywords.LogEntry))
                return;

            var severity = logEntry.Severity.ToString();
            var message = logEntry.Message;
            var messages = logEntry.ErrorMessages;
            var categories = DumpCategories(logEntry.Categories);
            var extendedProperties = DumpExtendedProperties(logEntry.ExtendedProperties);

            if (!_writeLogEntry.TryGetValue(logEntry.Severity, out var writeLogEntry))
                writeLogEntry = Log.InformationalLogEntry;

            writeLogEntry(
                    logEntry.EventId,
                    logEntry.Priority,
                    severity,
                    message,
                    messages,
                    categories,
                    extendedProperties);
        }

        static string DumpCategories(
            ICollection<string> categories)
        {
            if (categories == null  ||  !categories.Any())
                return string.Empty;

            return string.Join(", ", categories);
        }

        static string DumpExtendedProperties(
            IDictionary<string, object> extendedProperties)
        {
            if (extendedProperties == null  ||  !extendedProperties.Any())
                return null;

            using (var writer = new StringWriter())
            {
                foreach (var kv in extendedProperties)
                    writer.WriteLine("{0,-24} = {1}", kv.Key, kv.Value.DumpString(1));

                return writer.GetStringBuilder().ToString();
            }
        }

        // ======================================================
        // Events:
        // ======================================================

        #region Write LogEntry events to ETW
        /// <summary>
        /// Writes a verbose log entry event.
        /// </summary>
        /// <param name="EventId">The EL-LAB event identifier.</param>
        /// <param name="Priority">The EL-LAB event priority.</param>
        /// <param name="Severity">The EL-LAB event severity.</param>
        /// <param name="Message">The EL-LAB event message.</param>
        /// <param name="ErrorMessages">The EL-LAB event error messages.</param>
        /// <param name="Categories">The EL-LAB event categories.</param>
        /// <param name="Extended">The EL-LAB event extended properties.</param>
        [Event(VerboseLogEntryId, Level = EventLevel.Verbose, Keywords = Keywords.vmAspects | Keywords.ELLab | Keywords.LogEntry, Message = "{5} {2}: {3}")]
        void VerboseLogEntry(
            int EventId,
            int Priority,
            string Severity,
            string Message,
            string ErrorMessages,
            string Categories,
            string Extended)
        {
            if (IsEnabled())
                WriteEvent(
                    VerboseLogEntryId,
                    EventId,
                    Priority,
                    Severity,
                    Message,
                    ErrorMessages,
                    Categories,
                    Extended);
        }

        /// <summary>
        /// Writes an informational log entry event.
        /// </summary>
        /// <param name="EventId">The EL-LAB event identifier.</param>
        /// <param name="Priority">The EL-LAB event priority.</param>
        /// <param name="Severity">The EL-LAB event severity.</param>
        /// <param name="Message">The EL-LAB event message.</param>
        /// <param name="ErrorMessages">The EL-LAB event error messages.</param>
        /// <param name="Categories">The EL-LAB event categories.</param>
        /// <param name="Extended">The EL-LAB event extended properties.</param>
        [Event(InformationalLogEntryId, Level = EventLevel.Informational, Keywords = Keywords.vmAspects | Keywords.ELLab | Keywords.LogEntry, Message = "{5} {2}: {3}")]
        void InformationalLogEntry(
            int EventId,
            int Priority,
            string Severity,
            string Message,
            string ErrorMessages,
            string Categories,
            string Extended)
        {
            if (IsEnabled())
                WriteEvent(
                    InformationalLogEntryId,
                    EventId,
                    Priority,
                    Severity,
                    Message,
                    ErrorMessages,
                    Categories,
                    Extended);
        }

        /// <summary>
        /// Writes a warning log entry event.
        /// </summary>
        /// <param name="EventId">The EL-LAB event identifier.</param>
        /// <param name="Priority">The EL-LAB event priority.</param>
        /// <param name="Severity">The EL-LAB event severity.</param>
        /// <param name="Message">The EL-LAB event message.</param>
        /// <param name="ErrorMessages">The EL-LAB event error messages.</param>
        /// <param name="Categories">The EL-LAB event categories.</param>
        /// <param name="Extended">The EL-LAB event extended properties.</param>
        [Event(WarningLogEntryId, Level = EventLevel.Warning, Keywords = Keywords.vmAspects | Keywords.ELLab | Keywords.LogEntry, Message = "{5} {2}: {3}")]
        void WarningLogEntry(
            int EventId,
            int Priority,
            string Severity,
            string Message,
            string ErrorMessages,
            string Categories,
            string Extended)
        {
            if (IsEnabled())
                WriteEvent(
                    WarningLogEntryId,
                    EventId,
                    Priority,
                    Severity,
                    Message,
                    ErrorMessages,
                    Categories,
                    Extended);
        }

        /// <summary>
        /// Writes an error log entry event.
        /// </summary>
        /// <param name="EventId">The EL-LAB event identifier.</param>
        /// <param name="Priority">The EL-LAB event priority.</param>
        /// <param name="Severity">The EL-LAB event severity.</param>
        /// <param name="Message">The EL-LAB event message.</param>
        /// <param name="ErrorMessages">The EL-LAB event error messages.</param>
        /// <param name="Categories">The EL-LAB event categories.</param>
        /// <param name="Extended">The EL-LAB event extended properties.</param>
        [Event(ErrorLogEntryId, Level = EventLevel.Error, Keywords = Keywords.vmAspects | Keywords.ELLab | Keywords.LogEntry, Message = "{5} {2}: {3}")]
        void ErrorLogEntry(
            int EventId,
            int Priority,
            string Severity,
            string Message,
            string ErrorMessages,
            string Categories,
            string Extended)
        {
            if (IsEnabled())
                WriteEvent(
                    ErrorLogEntryId,
                    EventId,
                    Priority,
                    Severity,
                    Message,
                    ErrorMessages,
                    Categories,
                    Extended);
        }

        /// <summary>
        /// Writes a critical log entry event.
        /// </summary>
        /// <param name="EventId">The EL-LAB event identifier.</param>
        /// <param name="Priority">The EL-LAB event priority.</param>
        /// <param name="Severity">The EL-LAB event severity.</param>
        /// <param name="Message">The EL-LAB event message.</param>
        /// <param name="ErrorMessages">The EL-LAB event error messages.</param>
        /// <param name="Categories">The EL-LAB event categories.</param>
        /// <param name="Extended">The EL-LAB event extended properties.</param>
        [Event(CriticalLogEntryId, Level = EventLevel.Critical, Keywords = Keywords.vmAspects | Keywords.ELLab | Keywords.LogEntry, Message = "{5} {2}: {3}")]
        void CriticalLogEntry(
            int EventId,
            int Priority,
            string Severity,
            string Message,
            string ErrorMessages,
            string Categories,
            string Extended)
        {
            if (IsEnabled())
                WriteEvent(
                    CriticalLogEntryId,
                    EventId,
                    Priority,
                    Severity,
                    Message,
                    ErrorMessages,
                    Categories,
                    Extended);
        }
        #endregion

        #region Write text message to ETW
        /// <summary>
        /// Writes the log entry event.
        /// </summary>
        /// <param name="Text">An arbitrary object that will be dumped and logged.</param>
        [Event(VerboseMessageId, Level = EventLevel.Verbose, Keywords = Keywords.vmAspects | Keywords.ELLab | Keywords.Message, Message = "{0}")]
        void VerboseMessage(
            string Text)
        {
            if (IsEnabled())
                WriteEvent(VerboseMessageId, Text);
        }

        /// <summary>
        /// Writes the log entry event.
        /// </summary>
        /// <param name="Text">An arbitrary object that will be dumped and logged.</param>
        [Event(InformationalMessageId, Level = EventLevel.Informational, Keywords = Keywords.vmAspects | Keywords.ELLab | Keywords.Message, Message = "{0}")]
        void InformationalMessage(
            string Text)
        {
            if (IsEnabled())
                WriteEvent(InformationalMessageId, Text);
        }

        /// <summary>
        /// Writes the log entry event.
        /// </summary>
        /// <param name="Text">An arbitrary object that will be dumped and logged.</param>
        [Event(WarningMessageId, Level = EventLevel.Warning, Keywords = Keywords.vmAspects | Keywords.ELLab | Keywords.Message, Message = "{0}")]
        void WarningMessage(
            string Text)
        {
            if (IsEnabled())
                WriteEvent(WarningMessageId, Text);
        }

        /// <summary>
        /// Writes the log entry event.
        /// </summary>
        /// <param name="Text">An arbitrary object that will be dumped and logged.</param>
        [Event(ErrorMessageId, Level = EventLevel.Error, Keywords = Keywords.vmAspects | Keywords.ELLab | Keywords.Message, Message = "{0}")]
        void ErrorMessage(
            string Text)
        {
            if (IsEnabled())
                WriteEvent(ErrorMessageId, Text);
        }

        /// <summary>
        /// Writes the log entry event.
        /// </summary>
        /// <param name="Text">An arbitrary object that will be dumped and logged.</param>
        [Event(CriticalMessageId, Level = EventLevel.Critical, Keywords = Keywords.vmAspects | Keywords.ELLab | Keywords.Message, Message = "{0}")]
        void CriticalMessage(
            string Text)
        {
            if (IsEnabled())
                WriteEvent(CriticalMessageId, Text);
        }
        #endregion

        #region Dump an arbitrary object to ETW
        /// <summary>
        /// Writes the log entry event.
        /// </summary>
        /// <param name="DataDump">An arbitrary object that will be dumped and logged.</param>
        [Event(VerboseDumpObjectId, Level = EventLevel.Verbose, Keywords = Keywords.vmAspects | Keywords.ELLab | Keywords.Dump)]
        void VerboseDumpObject(
            string DataDump)
        {
            if (IsEnabled())
                WriteEvent(VerboseDumpObjectId, DataDump);
        }

        /// <summary>
        /// Writes the log entry event.
        /// </summary>
        /// <param name="DataDump">An arbitrary object that will be dumped and logged.</param>
        [Event(InformationalDumpObjectId, Level = EventLevel.Informational, Keywords = Keywords.vmAspects | Keywords.ELLab | Keywords.Dump)]
        void InformationalDumpObject(
            string DataDump)
        {
            if (IsEnabled())
                WriteEvent(InformationalDumpObjectId, DataDump);
        }

        /// <summary>
        /// Writes the log entry event.
        /// </summary>
        /// <param name="DataDump">An arbitrary object that will be dumped and logged.</param>
        [Event(WarningDumpObjectId, Level = EventLevel.Warning, Keywords = Keywords.vmAspects | Keywords.ELLab | Keywords.Dump)]
        void WarningDumpObject(
            string DataDump)
        {
            if (IsEnabled())
                WriteEvent(WarningDumpObjectId, DataDump);
        }

        /// <summary>
        /// Writes the log entry event.
        /// </summary>
        /// <param name="DataDump">An arbitrary object that will be dumped and logged.</param>
        [Event(ErrorDumpObjectId, Level = EventLevel.Error, Keywords = Keywords.vmAspects | Keywords.ELLab | Keywords.Dump)]
        void ErrorDumpObject(
            string DataDump)
        {
            if (IsEnabled())
                WriteEvent(ErrorDumpObjectId, DataDump);
        }

        /// <summary>
        /// Writes the log entry event.
        /// </summary>
        /// <param name="DataDump">An arbitrary object that will be dumped and logged.</param>
        [Event(CriticalDumpObjectId, Level = EventLevel.Critical, Keywords = Keywords.vmAspects | Keywords.ELLab | Keywords.Dump)]
        void CriticalDumpObject(
            string DataDump)
        {
            if (IsEnabled())
                WriteEvent(CriticalDumpObjectId, DataDump);
        }
        #endregion

        #region Trace stuff to ETW
        /// <summary>
        /// Writes the log entry event.
        /// </summary>
        /// <param name="ID">The EL LAB identifier.</param>
        /// <param name="Source">The source.</param>
        /// <param name="Text">An arbitrary trace text.</param>
        [Event(VerboseTraceId, Level = EventLevel.Verbose, Keywords = Keywords.vmAspects | Keywords.ELLab | Keywords.Trace, Message = "{2}: {1}")]
        void VerboseTrace(
            int ID,
            string Text,
            string Source = null)
        {
            if (IsEnabled())
                WriteEvent(VerboseTraceId, ID, Text, Source);
        }

        /// <summary>
        /// Writes the log entry event.
        /// </summary>
        /// <param name="ID">The EL LAB identifier.</param>
        /// <param name="Source">The source.</param>
        /// <param name="Text">An arbitrary trace text.</param>
        [Event(InformationalTraceId, Level = EventLevel.Informational, Keywords = Keywords.vmAspects | Keywords.ELLab | Keywords.Trace, Message = "{2}: {1}")]
        void InformationalTrace(
            int ID,
            string Text,
            string Source = null)
        {
            if (IsEnabled())
                WriteEvent(InformationalTraceId, ID, Text, Source);
        }

        /// <summary>
        /// Writes the log entry event.
        /// </summary>
        /// <param name="ID">The EL LAB identifier.</param>
        /// <param name="Source">The source.</param>
        /// <param name="Text">An arbitrary trace text.</param>
        [Event(WarningTraceId, Level = EventLevel.Warning, Keywords = Keywords.vmAspects | Keywords.ELLab | Keywords.Trace, Message = "{2}: {1}")]
        void WarningTrace(
            int ID,
            string Text,
            string Source = null)
        {
            if (IsEnabled())
                WriteEvent(WarningTraceId, ID, Text, Source);
        }

        /// <summary>
        /// Writes the log entry event.
        /// </summary>
        /// <param name="ID">The EL LAB identifier.</param>
        /// <param name="Source">The source.</param>
        /// <param name="Text">An arbitrary trace text.</param>
        [Event(ErrorTraceId, Level = EventLevel.Error, Keywords = Keywords.vmAspects | Keywords.ELLab | Keywords.Trace, Message = "{2}: {1}")]
        void ErrorTrace(
            int ID,
            string Text,
            string Source = null)
        {
            if (IsEnabled())
                WriteEvent(ErrorTraceId, ID, Text, Source);
        }

        /// <summary>
        /// Writes the log entry event.
        /// </summary>
        /// <param name="ID">The EL LAB identifier.</param>
        /// <param name="Source">The source.</param>
        /// <param name="Text">An arbitrary trace text.</param>
        [Event(CriticalTraceId, Level = EventLevel.Critical, Keywords = Keywords.vmAspects | Keywords.ELLab | Keywords.Trace, Message = "{2}: {1}")]
        void CriticalTrace(
            int ID,
            string Text,
            string Source = null)
        {
            if (IsEnabled())
                WriteEvent(CriticalTraceId, ID, Text, Source);
        }
        #endregion
    }
}
