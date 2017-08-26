using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Logging.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging.TraceListeners;

namespace vm.Aspects.Facilities.LogWriters.Etw
{
    /// <summary>
    /// Class EtwTraceListener...
    /// </summary>
    /// <seealso cref="TraceListener" />
    [ConfigurationElementType(typeof(CustomTraceListenerData))]
    public class EtwTraceListener : CustomTraceListener
    {
        /// <summary>
        /// Writes trace information, a data object and event information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the <see cref="TraceEventType" /> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the EL-LAB event.</param>
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

            if (logEntry != null)
            {
                EtwLogEntryEventSource.Log.WriteLogEntry(logEntry);
                return;
            }

            var stringData = data as string;

            if (stringData != null)
            {
                EtwLogEntryEventSource.Log.Trace(id, stringData, source, eventType);
                return;
            }

            if (data != null)
                EtwLogEntryEventSource.Log.Trace(id, data.DumpString(), source, eventType);
            else
                EtwLogEntryEventSource.Log.Trace(id, string.Empty, source, eventType);
        }

        /// <summary>
        /// When overridden in a derived class, writes the specified message to the listener you create in the derived class.
        /// </summary>
        /// <param name="message">A message to write.</param>
        public override void Write(string message)
        {
            EtwLogEntryEventSource.Log.WriteMessage(message);
        }

        /// <summary>
        /// When overridden in a derived class, writes a message to the listener you create in the derived class, followed by a line terminator.
        /// </summary>
        /// <param name="message">A message to write.</param>
        public override void WriteLine(string message)
        {
            Write(message);
        }

        /// <summary>
        /// Writes trace information, an array of data objects and event information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType" /> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="data">An array of objects to emit as data.</param>
        public override void TraceData(
            TraceEventCache eventCache,
            string source,
            TraceEventType eventType,
            int id,
            params object[] data)
        {
            if (Filter != null  &&  !Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, data))
                return;

            if (data == null  ||  !data.Any())
            {
                EtwLogEntryEventSource.Log.Trace(id, string.Empty, source, eventType);
                return;
            }

            using (var writer = new StringWriter())
            {
                foreach (var d in data)
                {
                    var s = d as string;

                    if (s != null)
                        writer.WriteLine(s);
                    else
                        d.DumpText(writer);
                }

                EtwLogEntryEventSource.Log.Trace(id, writer.ToString(), source, eventType);
            }
        }

        /// <summary>
        /// Writes trace and event information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType" /> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        public override void TraceEvent(
            TraceEventCache eventCache,
            string source,
            TraceEventType eventType,
            int id)
        {
            TraceData(eventCache, source, eventType, id, null);
        }

        /// <summary>
        /// Writes trace information, a formatted array of objects and event information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType" /> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="format">A format string that contains zero or more format items, which correspond to objects in the <paramref name="args" /> array.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        public override void TraceEvent(
            TraceEventCache eventCache,
            string source,
            TraceEventType eventType,
            int id,
            string format,
            params object[] args)
        {
            if (Filter != null  &&  !Filter.ShouldTrace(eventCache, source, eventType, id, format, args, null, null))
                return;

            if (args!=null  &&  args.Any())
                TraceData(eventCache, source, eventType, id, string.Format(CultureInfo.InvariantCulture, format, args));
            else
                TraceData(eventCache, source, eventType, id, string.Empty);
        }

        /// <summary>
        /// Writes trace information, a message, and event information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">A <see cref="T:System.Diagnostics.TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType" /> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="message">A message to write.</param>
        public override void TraceEvent(
            TraceEventCache eventCache,
            string source,
            TraceEventType eventType,
            int id,
            string message)
        {
            TraceData(eventCache, source, eventType, id, message);
        }
    }
}
