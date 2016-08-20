using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace vm.Aspects.Facilities
{
    /// <summary>
    /// ETW event source that takes Event Library's logging application block's log event entries.
    /// </summary>
    /// <seealso cref="vm.Aspects.Facilities.EtwLogEntryBaseEventSource" />
    [EventSource(Name = "vm-Aspects-LogEntryEventSource")]
    public sealed class EtwLogEntryEventSource : EtwLogEntryBaseEventSource, IEtwLogEntryHandler
    {
        /// <summary>
        /// The log singleton instance.
        /// </summary>
        public static EtwLogEntryEventSource Log = new EtwLogEntryEventSource();

        const int WriteLogEntryId                    = 1;
        const int WriteLogTextId                     = 2;
        const int WriteLogDataId                     = 3;
        const int WriteExceptionVerboseEntryId       = 4;
        const int WriteExceptionInfoEntryId          = 5;
        const int WriteExceptionWarningEntryId       = 6;
        const int WriteExceptionErrorEntryId         = 7;
        const int WriteExceptionCriticalEntryId      = 8;
        const int WriteTraceVerboseEntryId           = 9;
        const int WriteTraceInfoEntryId              = 10;
        const int WriteTraceWarningEntryId           = 11;
        const int WriteTraceErrorEntryId             = 12;
        const int WriteTraceCriticalEntryId          = 13;
        const int WriteStartCallTraceVerboseEntryId  = 14;
        const int WriteStartCallTraceInfoEntryId     = 15;
        const int WriteStartCallTraceWarningEntryId  = 16;
        const int WriteStartCallTraceErrorEntryId    = 17;
        const int WriteStartCallTraceCriticalEntryId = 18;
        const int WriteEndCallTraceVerboseEntryId    = 19;
        const int WriteEndCallTraceInfoEntryId       = 20;
        const int WriteEndCallTraceWarningEntryId    = 21;
        const int WriteEndCallTraceErrorEntryId      = 22;
        const int WriteEndCallTraceCriticalEntryId   = 23;

        /// <summary>
        /// Writes the log entry event.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="logEntry">An EL-LAB log entry object.</param>
        [Event(WriteLogEntryId)]
        void IEtwLogEntryHandler.WriteLogEntry(
            int eventId,
            TraceEventCache eventCache,
            LogEntry logEntry)
        {
            WriteLogEntryEvent(WriteLogDataId, eventId, eventCache, logEntry);
        }

        /// <summary>
        /// Writes the log entry event.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="text">An arbitrary object that will be dumped and logged.</param>
        [Event(WriteLogTextId)]
        void IEtwLogEntryHandler.WriteLogText(
            int eventId,
            TraceEventCache eventCache,
            string text)
        {
            WriteLogEntryEvent(WriteLogDataId, eventId, eventCache, text);
        }

        /// <summary>
        /// Writes the log entry event.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="data">An arbitrary object that will be dumped and logged.</param>
        [Event(WriteLogDataId)]
        void IEtwLogEntryHandler.WriteLogData(
            int eventId,
            TraceEventCache eventCache,
            object data)
        {
            WriteLogEntryEvent(WriteLogDataId, eventId, eventCache, data);
        }

        #region Logging exceptions
        /// <summary>
        /// Writes an exception information entry.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="exception">The exception to be logged.</param>
        [Event(WriteExceptionVerboseEntryId, Level = EventLevel.Verbose, Opcode = EventOpcode.Info, Version = 1)]
        void IEtwLogEntryHandler.WriteExceptionVerboseEntry(
            int eventId,
            TraceEventCache eventCache,
            Exception exception)
        {
            WriteLogEntryEvent(WriteExceptionVerboseEntryId, eventId, eventCache, exception);
        }

        /// <summary>
        /// Writes an exception information entry.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="exception">The exception to be logged.</param>
        [Event(WriteExceptionInfoEntryId, Level = EventLevel.Informational, Opcode = EventOpcode.Info, Version = 1)]
        void IEtwLogEntryHandler.WriteExceptionInfoEntry(
            int eventId,
            TraceEventCache eventCache,
            Exception exception)
        {
            WriteLogEntryEvent(WriteExceptionInfoEntryId, eventId, eventCache, exception);
        }

        /// <summary>
        /// Writes an exception information entry.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="exception">The exception to be logged.</param>
        [Event(WriteExceptionWarningEntryId, Level = EventLevel.Warning, Opcode = EventOpcode.Info, Version = 1)]
        void IEtwLogEntryHandler.WriteExceptionWarningEntry(
            int eventId,
            TraceEventCache eventCache,
            Exception exception)
        {
            WriteLogEntryEvent(WriteExceptionWarningEntryId, eventId, eventCache, exception);
        }

        /// <summary>
        /// Writes an exception information entry.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="exception">The exception to be logged.</param>
        [Event(WriteExceptionErrorEntryId, Level = EventLevel.Error, Opcode = EventOpcode.Info, Version = 1)]
        void IEtwLogEntryHandler.WriteExceptionErrorEntry(
            int eventId,
            TraceEventCache eventCache,
            Exception exception)
        {
            WriteLogEntryEvent(WriteExceptionErrorEntryId, eventId, eventCache, exception);
        }

        /// <summary>
        /// Writes an exception information entry.
        /// </summary>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="exception">The exception to be logged.</param>
        [Event(WriteExceptionCriticalEntryId, Level = EventLevel.Critical, Opcode = EventOpcode.Info, Version = 1)]
        void IEtwLogEntryHandler.WriteExceptionCriticalEntry(
            int eventId,
            TraceEventCache eventCache,
            Exception exception)
        { 
            WriteLogEntryEvent(WriteExceptionCriticalEntryId, eventId, eventCache, exception);
        }
        #endregion

        #region Trace messages
        /// <summary>
        /// Writes a trace message information entry.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="traceMessage">The trace message to be logged.</param>
        [Event(WriteTraceVerboseEntryId, Level = EventLevel.Verbose, Opcode = EventOpcode.Info, Version = 1)]
        void IEtwLogEntryHandler.WriteTraceVerboseEntry(
            int eventId,
            TraceEventCache eventCache,
            string traceMessage)
        {
            WriteLogEntryEvent(WriteTraceVerboseEntryId, eventId, eventCache, traceMessage);
        }

        /// <summary>
        /// Writes a trace message information entry.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="traceMessage">The trace message to be logged.</param>
        [Event(WriteTraceInfoEntryId, Level = EventLevel.Informational, Opcode = EventOpcode.Info, Version = 1)]
        void IEtwLogEntryHandler.WriteTraceInfoEntry(
            int eventId,
            TraceEventCache eventCache,
            string traceMessage)
        {
            WriteLogEntryEvent(WriteTraceInfoEntryId, eventId, eventCache, traceMessage);
        }

        /// <summary>
        /// Writes a trace message information entry.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="traceMessage">The trace message to be logged.</param>
        [Event(WriteTraceWarningEntryId, Level = EventLevel.Warning, Opcode = EventOpcode.Info, Version = 1)]
        void IEtwLogEntryHandler.WriteTraceWarningEntry(
            int eventId,
            TraceEventCache eventCache,
            string traceMessage)
        {
            WriteLogEntryEvent(WriteTraceWarningEntryId, eventId, eventCache, traceMessage);
        }

        /// <summary>
        /// Writes a trace message information entry.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="traceMessage">The trace message to be logged.</param>
        [Event(WriteTraceErrorEntryId, Level = EventLevel.Error, Opcode = EventOpcode.Info, Version = 1)]
        void IEtwLogEntryHandler.WriteTraceErrorEntry(
            int eventId,
            TraceEventCache eventCache,
            string traceMessage)
        {
            WriteLogEntryEvent(WriteTraceErrorEntryId, eventId, eventCache, traceMessage);
        }

        /// <summary>
        /// Writes a trace message information entry.
        /// </summary>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="traceMessage">The trace message to be logged.</param>
        [Event(WriteTraceCriticalEntryId, Level = EventLevel.Critical, Opcode = EventOpcode.Info, Version = 1)]
        void IEtwLogEntryHandler.WriteTraceCriticalEntry(
            int eventId,
            TraceEventCache eventCache,
            string traceMessage)
        {
            WriteLogEntryEvent(WriteTraceCriticalEntryId, eventId, eventCache, traceMessage);
        }
        #endregion

        #region Call start call tracing messages
        /// <summary>
        /// Writes a start call tracing message information entry.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack start call tracing information.</param>
        /// <param name="callTrace">The start call tracing message to be logged.</param>
        [Event(WriteStartCallTraceVerboseEntryId, Level = EventLevel.Verbose, Opcode = EventOpcode.Info, Version = 1)]
        void IEtwLogEntryHandler.WriteStartCallTraceVerboseEntry(
            int eventId,
            TraceEventCache eventCache,
            string callTrace)
        {
            WriteLogEntryEvent(WriteStartCallTraceVerboseEntryId, eventId, eventCache, callTrace);
        }

        /// <summary>
        /// Writes a start call tracing message information entry.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack start call tracing information.</param>
        /// <param name="callTrace">The start call tracing message to be logged.</param>
        [Event(WriteStartCallTraceInfoEntryId, Level = EventLevel.Informational, Opcode = EventOpcode.Info, Version = 1)]
        void IEtwLogEntryHandler.WriteStartCallTraceInfoEntry(
            int eventId,
            TraceEventCache eventCache,
            string callTrace)
        {
            WriteLogEntryEvent(WriteStartCallTraceInfoEntryId, eventId, eventCache, callTrace);
        }

        /// <summary>
        /// Writes a start call tracing message information entry.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack start call tracing information.</param>
        /// <param name="callTrace">The start call tracing message to be logged.</param>
        [Event(WriteStartCallTraceWarningEntryId, Level = EventLevel.Warning, Opcode = EventOpcode.Info, Version = 1)]
        void IEtwLogEntryHandler.WriteStartCallTraceWarningEntry(
            int eventId,
            TraceEventCache eventCache,
            string callTrace)
        {
            WriteLogEntryEvent(WriteStartCallTraceWarningEntryId, eventId, eventCache, callTrace);
        }

        /// <summary>
        /// Writes a start call tracing message information entry.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack start call tracing information.</param>
        /// <param name="callTrace">The start call tracing message to be logged.</param>
        [Event(WriteStartCallTraceErrorEntryId, Level = EventLevel.Error, Opcode = EventOpcode.Info, Version = 1)]
        void IEtwLogEntryHandler.WriteStartCallTraceErrorEntry(
            int eventId,
            TraceEventCache eventCache,
            string callTrace)
        {
            WriteLogEntryEvent(WriteStartCallTraceErrorEntryId, eventId, eventCache, callTrace);
        }

        /// <summary>
        /// Writes a start call tracing message information entry.
        /// </summary>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack start call tracing information.</param>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="callTrace">The start call tracing message to be logged.</param>
        [Event(WriteStartCallTraceCriticalEntryId, Level = EventLevel.Critical, Opcode = EventOpcode.Info, Version = 1)]
        void IEtwLogEntryHandler.WriteStartCallTraceCriticalEntry(
            int eventId,
            TraceEventCache eventCache,
            string callTrace)
        {
            WriteLogEntryEvent(WriteStartCallTraceCriticalEntryId, eventId, eventCache, callTrace);
        }
        #endregion

        #region Call end call tracing messages
        /// <summary>
        /// Writes a end call tracing message information entry.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack end call tracing information.</param>
        /// <param name="callTrace">The end call tracing message to be logged.</param>
        [Event(WriteEndCallTraceVerboseEntryId, Level = EventLevel.Verbose, Opcode = EventOpcode.Info, Version = 1)]
        void IEtwLogEntryHandler.WriteEndCallTraceVerboseEntry(
            int eventId,
            TraceEventCache eventCache,
            string callTrace)
        {
            WriteLogEntryEvent(WriteEndCallTraceVerboseEntryId, eventId, eventCache, callTrace);
        }

        /// <summary>
        /// Writes a end call tracing message information entry.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack end call tracing information.</param>
        /// <param name="callTrace">The end call tracing message to be logged.</param>
        [Event(WriteEndCallTraceInfoEntryId, Level = EventLevel.Informational, Opcode = EventOpcode.Info, Version = 1)]
        void IEtwLogEntryHandler.WriteEndCallTraceInfoEntry(
            int eventId,
            TraceEventCache eventCache,
            string callTrace)
        {
            WriteLogEntryEvent(WriteEndCallTraceInfoEntryId, eventId, eventCache, callTrace);
        }

        /// <summary>
        /// Writes a end call tracing message information entry.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack end call tracing information.</param>
        /// <param name="callTrace">The end call tracing message to be logged.</param>
        [Event(WriteEndCallTraceWarningEntryId, Level = EventLevel.Warning, Opcode = EventOpcode.Info, Version = 1)]
        void IEtwLogEntryHandler.WriteEndCallTraceWarningEntry(
            int eventId,
            TraceEventCache eventCache,
            string callTrace)
        {
            WriteLogEntryEvent(WriteEndCallTraceWarningEntryId, eventId, eventCache, callTrace);
        }

        /// <summary>
        /// Writes a end call tracing message information entry.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack end call tracing information.</param>
        /// <param name="callTrace">The end call tracing message to be logged.</param>
        [Event(WriteEndCallTraceErrorEntryId, Level = EventLevel.Error, Opcode = EventOpcode.Info, Version = 1)]
        void IEtwLogEntryHandler.WriteEndCallTraceErrorEntry(
            int eventId,
            TraceEventCache eventCache,
            string callTrace)
        {
            WriteLogEntryEvent(WriteEndCallTraceErrorEntryId, eventId, eventCache, callTrace);
        }

        /// <summary>
        /// Writes a end call tracing message information entry.
        /// </summary>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack end call tracing information.</param>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="callTrace">The end call tracing message to be logged.</param>
        [Event(WriteEndCallTraceCriticalEntryId, Level = EventLevel.Critical, Opcode = EventOpcode.Info, Version = 1)]
        void IEtwLogEntryHandler.WriteEndCallTraceCriticalEntry(
            int eventId,
            TraceEventCache eventCache,
            string callTrace)
        {
            WriteLogEntryEvent(WriteEndCallTraceCriticalEntryId, eventId, eventCache, callTrace);
        }
        #endregion
    }
}
