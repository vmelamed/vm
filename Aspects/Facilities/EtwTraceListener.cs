using System;
using System.Diagnostics;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Logging.Configuration;

namespace vm.Aspects.Facilities
{
    /// <summary>
    /// Class EtwTraceListener...
    /// </summary>
    /// <seealso cref="System.Diagnostics.TraceListener" />
    [ConfigurationElementType(typeof(CustomTraceListenerData))]
    public class EtwTraceListener : TraceListener
    {
        /// <summary>
        /// Writes trace information, a data object and event information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the <see cref="TraceEventType" /> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="data">The trace data to emit.</param>
        public override void TraceData(
            TraceEventCache eventCache,
            string source,
            TraceEventType eventType,
            int id,
            object data)
        {
            if (Filter != null  &&  !Filter.ShouldTrace(eventCache, source, eventType, id, null, null, data, null))
                return;

            var logEntry = data as LogEntry;

            if (logEntry == null)
                TraceNonLogEntry(eventCache, source, eventType, id, data);


        }

        void TraceNonLogEntry(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, writes the specified message to the listener you create in the derived class.
        /// </summary>
        /// <param name="message">A message to write.</param>
        public override void Write(string message)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, writes a message to the listener you create in the derived class, followed by a line terminator.
        /// </summary>
        /// <param name="message">A message to write.</param>
        public override void WriteLine(string message)
        {
            throw new NotImplementedException();
        }
    }
}
