using System;
using System.Diagnostics;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace vm.Aspects.Facilities
{
    /// <summary>
    /// Interface IEtwLogEntryHandler can be implemented by custom ETW event sources that wish to handle
    /// Enterprise Library logging application block events.
    /// </summary>
    public interface IEtwLogEntryHandler
    {
        /// <summary>
        /// Writes the log entry event.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="logEntry">An EL-LAB log entry object.</param>
        void WriteLogEntry(
            int eventId,
            TraceEventCache eventCache,
            LogEntry logEntry);

        /// <summary>
        /// Writes the log entry event.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="text">An arbitrary object that will be dumped and logged.</param>
        void WriteLogText(
            int eventId,
            TraceEventCache eventCache,
            string text);

        /// <summary>
        /// Writes the log entry event.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="data">An arbitrary object that will be dumped and logged.</param>
        void WriteLogData(
            int eventId,
            TraceEventCache eventCache,
            object data);

        #region Logging exceptions
        /// <summary>
        /// Writes an exception information entry.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="exception">The exception to be logged.</param>
        void WriteExceptionVerboseEntry(
            int eventId,
            TraceEventCache eventCache,
            Exception exception);

        /// <summary>
        /// Writes an exception information entry.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="exception">The exception to be logged.</param>
        void WriteExceptionInfoEntry(
            int eventId,
            TraceEventCache eventCache,
            Exception exception);

        /// <summary>
        /// Writes an exception information entry.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="exception">The exception to be logged.</param>
        void WriteExceptionWarningEntry(
            int eventId,
            TraceEventCache eventCache,
            Exception exception);

        /// <summary>
        /// Writes an exception information entry.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="exception">The exception to be logged.</param>
        void WriteExceptionErrorEntry(
            int eventId,
            TraceEventCache eventCache,
            Exception exception);

        /// <summary>
        /// Writes an exception information entry.
        /// </summary>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="exception">The exception to be logged.</param>
        void WriteExceptionCriticalEntry(
            int eventId,
            TraceEventCache eventCache,
            Exception exception);
        #endregion

        #region Trace messages
        /// <summary>
        /// Writes a trace message information entry.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="traceMessage">The trace message to be logged.</param>
        void WriteTraceVerboseEntry(
            int eventId,
            TraceEventCache eventCache,
            string traceMessage);

        /// <summary>
        /// Writes a trace message information entry.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="traceMessage">The trace message to be logged.</param>
        void WriteTraceInfoEntry(
            int eventId,
            TraceEventCache eventCache,
            string traceMessage);

        /// <summary>
        /// Writes a trace message information entry.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="traceMessage">The trace message to be logged.</param>
        void WriteTraceWarningEntry(
            int eventId,
            TraceEventCache eventCache,
            string traceMessage);

        /// <summary>
        /// Writes a trace message information entry.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="traceMessage">The trace message to be logged.</param>
        void WriteTraceErrorEntry(
            int eventId,
            TraceEventCache eventCache,
            string traceMessage);

        /// <summary>
        /// Writes a trace message information entry.
        /// </summary>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="traceMessage">The trace message to be logged.</param>
        void WriteTraceCriticalEntry(
            int eventId,
            TraceEventCache eventCache,
            string traceMessage);
        #endregion

        #region Call start call tracing messages
        /// <summary>
        /// Writes a start call tracing message information entry.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack start call tracing information.</param>
        /// <param name="callTrace">The start call tracing message to be logged.</param>
        void WriteStartCallTraceVerboseEntry(
            int eventId,
            TraceEventCache eventCache,
            string callTrace);

        /// <summary>
        /// Writes a start call tracing message information entry.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack start call tracing information.</param>
        /// <param name="callTrace">The start call tracing message to be logged.</param>
        void WriteStartCallTraceInfoEntry(
            int eventId,
            TraceEventCache eventCache,
            string callTrace);

        /// <summary>
        /// Writes a start call tracing message information entry.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack start call tracing information.</param>
        /// <param name="callTrace">The start call tracing message to be logged.</param>
        void WriteStartCallTraceWarningEntry(
            int eventId,
            TraceEventCache eventCache,
            string callTrace);

        /// <summary>
        /// Writes a start call tracing message information entry.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack start call tracing information.</param>
        /// <param name="callTrace">The start call tracing message to be logged.</param>
        void WriteStartCallTraceErrorEntry(
            int eventId,
            TraceEventCache eventCache,
            string callTrace);

        /// <summary>
        /// Writes a start call tracing message information entry.
        /// </summary>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack start call tracing information.</param>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="callTrace">The start call tracing message to be logged.</param>
        void WriteStartCallTraceCriticalEntry(
            int eventId,
            TraceEventCache eventCache,
            string callTrace);
        #endregion

        #region Call end call tracing messages
        /// <summary>
        /// Writes a end call tracing message information entry.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack end call tracing information.</param>
        /// <param name="callTrace">The end call tracing message to be logged.</param>
        void WriteEndCallTraceVerboseEntry(
            int eventId,
            TraceEventCache eventCache,
            string callTrace);

        /// <summary>
        /// Writes a end call tracing message information entry.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack end call tracing information.</param>
        /// <param name="callTrace">The end call tracing message to be logged.</param>
        void WriteEndCallTraceInfoEntry(
            int eventId,
            TraceEventCache eventCache,
            string callTrace);

        /// <summary>
        /// Writes a end call tracing message information entry.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack end call tracing information.</param>
        /// <param name="callTrace">The end call tracing message to be logged.</param>
        void WriteEndCallTraceWarningEntry(
            int eventId,
            TraceEventCache eventCache,
            string callTrace);

        /// <summary>
        /// Writes a end call tracing message information entry.
        /// </summary>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack end call tracing information.</param>
        /// <param name="callTrace">The end call tracing message to be logged.</param>
        void WriteEndCallTraceErrorEntry(
            int eventId,
            TraceEventCache eventCache,
            string callTrace);

        /// <summary>
        /// Writes a end call tracing message information entry.
        /// </summary>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack end call tracing information.</param>
        /// <param name="eventId">EL LAB event identifier.</param>
        /// <param name="callTrace">The end call tracing message to be logged.</param>
        void WriteEndCallTraceCriticalEntry(
            int eventId,
            TraceEventCache eventCache,
            string callTrace);
        #endregion
    }
}
