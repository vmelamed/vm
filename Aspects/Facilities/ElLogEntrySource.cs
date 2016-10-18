using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Logging.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging.Formatters;
using Microsoft.Practices.EnterpriseLibrary.Logging.TraceListeners;

namespace vm.Aspects.Facilities
{
    /// <summary>
    /// Class EtwTraceListener...
    /// </summary>
    /// <seealso cref="System.Diagnostics.TraceListener" />
    [ConfigurationElementType(typeof(CustomTraceListenerData))]
    public class EtwTraceListener : CustomTraceListener
    {
        const string EventName = "EL Log event";

        readonly EventSource _etwSource;
        readonly IEtwLogEntryHandler _etwLogEntryHandler;
        readonly IDictionary<PropertyInfo, int> _logEntryToEtwWriteIndex;
        readonly int _parametersCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="EtwTraceListener" /> class.
        /// </summary>
        /// <param name="etwSource">The etw source.</param>
        /// <param name="logEntryToEtwWriteIndex">Maps the <see cref="LogEntry"/> properties to index in the <see langword="params"/> array of objects parameter of the <see cref="EventSource.WriteEvent(int, object[])"/>.</param>
        /// <param name="formatter">The formatter.</param>
        public EtwTraceListener(
            EventSource etwSource,
            IDictionary<PropertyInfo, int> logEntryToEtwWriteIndex = null,
            ILogFormatter formatter = null)
        {
            Contract.Requires<ArgumentNullException>(etwSource != null, nameof(etwSource));
            Contract.Requires<ArgumentException>(logEntryToEtwWriteIndex == null || logEntryToEtwWriteIndex.Count==logEntryToEtwWriteIndex.Select(kv => kv.Value).Max(), "The maximum index mapped in the argument "+nameof(logEntryToEtwWriteIndex)+" is different from the number of entries in it.");

            if (logEntryToEtwWriteIndex != null  &&  logEntryToEtwWriteIndex.Count != 0)
            {
                bool[] taken = new bool[logEntryToEtwWriteIndex.Count];

                for (int i = 0; i < taken.Length; i++)
                    taken[i] = false;

                foreach (var kv in logEntryToEtwWriteIndex)
                {
                    if (kv.Key.DeclaringType != typeof(LogEntry)  &&
                        kv.Key.DeclaringType != typeof(TraceEventCache))
                        throw new ArgumentException("The "+nameof(PropertyInfo)+" keys must refer to properties from the "+nameof(LogEntry)+" or "+nameof(TraceEventCache)+" classes only.", nameof(logEntryToEtwWriteIndex));

                    if (kv.Value < 0  ||  kv.Value >= logEntryToEtwWriteIndex.Count)
                        throw new ArgumentException($"Parameter position {kv.Value} specified in the parameter "+nameof(logEntryToEtwWriteIndex)+" is greater than the number of parameters in the dictionary.", nameof(logEntryToEtwWriteIndex));
                    if (taken[kv.Value])
                        throw new ArgumentException($"Duplicate parameter position {kv.Value} specified in the parameter "+nameof(logEntryToEtwWriteIndex), nameof(logEntryToEtwWriteIndex));

                    taken[kv.Value] = true;
                }

                _logEntryToEtwWriteIndex = logEntryToEtwWriteIndex;
                _parametersCount         = logEntryToEtwWriteIndex.Count;
            }

            Formatter                  = formatter;
            _etwSource                 = etwSource;
            _etwLogEntryHandler        = etwSource as IEtwLogEntryHandler;
        }

        class TraceProperties : IEquatable<TraceProperties>
        {
            public TraceProperties(
                TraceEventType traceEventType,
                Type dataType,
                string source)
            {
                Contract.Requires<ArgumentNullException>(dataType != null, nameof(dataType));

                TraceEventType = traceEventType;

                if (dataType != typeof(LogEntry)  &&
                    dataType != typeof(string))
                    DataType = typeof(object);
                else
                    DataType = dataType;

                if (source != LogWriterFacades.StartCallTrace  &&
                    source != LogWriterFacades.EndCallTrace)
                    Source = null;
                else
                    Source = source;
            }

            public TraceEventType TraceEventType { get; }
            public Type DataType { get; }
            public string Source { get; set; }

            #region Identity rules implementation.
            public virtual bool Equals(TraceProperties other)
            {
                if (ReferenceEquals(other, null))
                    return false;
                if (ReferenceEquals(this, other))
                    return true;
                if (GetType() != other.GetType())
                    return false;

                return TraceEventType == other.TraceEventType  &&
                       DataType       == other.DataType  &&
                       Source         == other.Source;
            }

            public override bool Equals(object obj) => Equals(obj as TraceProperties);

            public override int GetHashCode()
            {
                var hashCode = Constants.HashInitializer;

                hashCode = Constants.HashMultiplier * hashCode + TraceEventType.GetHashCode();
                hashCode = Constants.HashMultiplier * hashCode + DataType.GetHashCode();
                hashCode = Constants.HashMultiplier * hashCode + Source.GetHashCode();

                return hashCode;
            }

            public static bool operator ==(TraceProperties left, TraceProperties right) => ReferenceEquals(left, null)
                                                                                    ? ReferenceEquals(right, null)
                                                                                    : left.Equals(right);

            public static bool operator !=(TraceProperties left, TraceProperties right) => !(left==right);
            #endregion
        }

        static readonly IReadOnlyDictionary<TraceProperties, Action<IEtwLogEntryHandler, int, TraceEventCache, object>> _etwWriters = new ReadOnlyDictionary<TraceProperties, Action<IEtwLogEntryHandler, int, TraceEventCache, object>>(
            new Dictionary<TraceProperties, Action<IEtwLogEntryHandler, int, TraceEventCache, object>>
            {
                [new TraceProperties(TraceEventType.Verbose,     typeof(Exception), null)]                           = (i,id,c,o) => i.WriteExceptionVerboseEntry (id, c, (Exception)o),
                [new TraceProperties(TraceEventType.Information, typeof(Exception), null)]                           = (i,id,c,o) => i.WriteExceptionInfoEntry    (id, c, (Exception)o),
                [new TraceProperties(TraceEventType.Warning,     typeof(Exception), null)]                           = (i,id,c,o) => i.WriteExceptionWarningEntry (id, c, (Exception)o),
                [new TraceProperties(TraceEventType.Error,       typeof(Exception), null)]                           = (i,id,c,o) => i.WriteExceptionErrorEntry   (id, c, (Exception)o),
                [new TraceProperties(TraceEventType.Critical,    typeof(Exception), null)]                           = (i,id,c,o) => i.WriteExceptionCriticalEntry(id, c, (Exception)o),

                [new TraceProperties(TraceEventType.Verbose,     typeof(string), null)]                              = (i,id,c,o) => i.WriteTraceVerboseEntry (id, c, (string)o),
                [new TraceProperties(TraceEventType.Information, typeof(string), null)]                              = (i,id,c,o) => i.WriteTraceInfoEntry    (id, c, (string)o),
                [new TraceProperties(TraceEventType.Warning,     typeof(string), null)]                              = (i,id,c,o) => i.WriteTraceWarningEntry (id, c, (string)o),
                [new TraceProperties(TraceEventType.Error,       typeof(string), null)]                              = (i,id,c,o) => i.WriteTraceErrorEntry   (id, c, (string)o),
                [new TraceProperties(TraceEventType.Critical,    typeof(string), null)]                              = (i,id,c,o) => i.WriteTraceCriticalEntry(id, c, (string)o),

                [new TraceProperties(TraceEventType.Verbose,     typeof(object), null)]                              = (i,id,c,o) => i.WriteTraceVerboseEntry (id, c, o.DumpString()),
                [new TraceProperties(TraceEventType.Information, typeof(object), null)]                              = (i,id,c,o) => i.WriteTraceInfoEntry    (id, c, o.DumpString()),
                [new TraceProperties(TraceEventType.Warning,     typeof(object), null)]                              = (i,id,c,o) => i.WriteTraceWarningEntry (id, c, o.DumpString()),
                [new TraceProperties(TraceEventType.Error,       typeof(object), null)]                              = (i,id,c,o) => i.WriteTraceErrorEntry   (id, c, o.DumpString()),
                [new TraceProperties(TraceEventType.Critical,    typeof(object), null)]                              = (i,id,c,o) => i.WriteTraceCriticalEntry(id, c, o.DumpString()),

                [new TraceProperties(TraceEventType.Verbose,     typeof(string), LogWriterFacades.StartCallTrace)]   = (i,id,c,o) => i.WriteStartCallTraceVerboseEntry (id, c, (string)o),
                [new TraceProperties(TraceEventType.Information, typeof(string), LogWriterFacades.StartCallTrace)]   = (i,id,c,o) => i.WriteStartCallTraceInfoEntry    (id, c, (string)o),
                [new TraceProperties(TraceEventType.Warning,     typeof(string), LogWriterFacades.StartCallTrace)]   = (i,id,c,o) => i.WriteStartCallTraceWarningEntry (id, c, (string)o),
                [new TraceProperties(TraceEventType.Error,       typeof(string), LogWriterFacades.StartCallTrace)]   = (i,id,c,o) => i.WriteStartCallTraceErrorEntry   (id, c, (string)o),
                [new TraceProperties(TraceEventType.Critical,    typeof(string), LogWriterFacades.StartCallTrace)]   = (i,id,c,o) => i.WriteStartCallTraceCriticalEntry(id, c, (string)o),

                [new TraceProperties(TraceEventType.Verbose,     typeof(string), LogWriterFacades.EndCallTrace)]     = (i,id,c,o) => i.WriteEndCallTraceVerboseEntry (id, c, (string)o),
                [new TraceProperties(TraceEventType.Information, typeof(string), LogWriterFacades.EndCallTrace)]     = (i,id,c,o) => i.WriteEndCallTraceInfoEntry    (id, c, (string)o),
                [new TraceProperties(TraceEventType.Warning,     typeof(string), LogWriterFacades.EndCallTrace)]     = (i,id,c,o) => i.WriteEndCallTraceWarningEntry (id, c, (string)o),
                [new TraceProperties(TraceEventType.Error,       typeof(string), LogWriterFacades.EndCallTrace)]     = (i,id,c,o) => i.WriteEndCallTraceErrorEntry   (id, c, (string)o),
                [new TraceProperties(TraceEventType.Critical,    typeof(string), LogWriterFacades.EndCallTrace)]     = (i,id,c,o) => i.WriteEndCallTraceCriticalEntry(id, c, (string)o),

            });

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
            if (eventCache == null)
                throw new ArgumentNullException(nameof(eventCache));
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (Filter != null  &&  !Filter.ShouldTrace(eventCache, source, eventType, id, null, null, data, null))
                return;

            var logEntry = data as LogEntry;

            if (_etwLogEntryHandler != null)
            {
                if (logEntry != null)
                    _etwLogEntryHandler.WriteLogEntry(id, eventCache, logEntry);
                else
                    _etwLogEntryHandler.WriteLogData(id, eventCache, data);

                return;
            }

            if (logEntry == null)
            {
                TraceNonLogEntry(eventCache, eventType, data);
                return;
            }

            // if there is no mapping table - dump the log entry
            if (_logEntryToEtwWriteIndex == null)
            {
                if (Formatter != null)
                    TraceNonLogEntry(eventCache, eventType, Formatter.Format(logEntry));
                else
                    TraceNonLogEntry(eventCache, eventType, logEntry);
                return;
            }

            // map the entry's properties to parameters of the Write method
            object[] parameters = new object[_parametersCount];

            for (var i = 0; i<_parametersCount; i++)
                parameters[i] = 0;

            foreach (var kv in _logEntryToEtwWriteIndex)
                if (kv.Key.DeclaringType == typeof(LogEntry))
                    parameters[kv.Value] = kv.Key.GetValue(logEntry);
                else
                    parameters[kv.Value] = kv.Key.GetValue(eventCache);

#if DOTNET461
            _etwSource.Write(
                EventName,
                GetEventSourceOptions(eventType),
                parameters);
#endif
        }

        static void TraceNonLogEntry(
            TraceEventCache eventCache,
            TraceEventType eventType,
            object data)
        {
            if (eventCache == null)
                throw new ArgumentNullException(nameof(eventCache));
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var stringData = data as string;

            if (stringData == null)
                stringData = data.DumpString();

#if DOTNET461
            _etwSource.Write(
                EventName,
                GetEventSourceOptions(eventType),
                stringData);
#endif
        }

        static readonly IReadOnlyDictionary<TraceEventType, EventLevel> _traceEventType2eventLevel = new ReadOnlyDictionary<TraceEventType, EventLevel>(
            new SortedDictionary<TraceEventType, EventLevel>
            {
                [TraceEventType.Verbose]     = EventLevel.Verbose,
                [TraceEventType.Information] = EventLevel.Informational,
                [TraceEventType.Warning]     = EventLevel.Warning,
                [TraceEventType.Error]       = EventLevel.Error,
                [TraceEventType.Critical]    = EventLevel.Critical,

                [TraceEventType.Start]       = EventLevel.Informational,
                [TraceEventType.Suspend]     = EventLevel.Informational,
                [TraceEventType.Resume]      = EventLevel.Informational,
                [TraceEventType.Stop]        = EventLevel.Informational,
            });

        static readonly IReadOnlyDictionary<TraceEventType, EventOpcode> _traceEventType2eventOpcode = new ReadOnlyDictionary<TraceEventType, EventOpcode>(
            new SortedDictionary<TraceEventType, EventOpcode>
            {
                [TraceEventType.Verbose]     = EventOpcode.Info,
                [TraceEventType.Information] = EventOpcode.Info,
                [TraceEventType.Warning]     = EventOpcode.Info,
                [TraceEventType.Error]       = EventOpcode.Info,
                [TraceEventType.Critical]    = EventOpcode.Info,

                [TraceEventType.Start]       = EventOpcode.Start,
                [TraceEventType.Suspend]     = EventOpcode.Suspend,
                [TraceEventType.Resume]      = EventOpcode.Resume,
                [TraceEventType.Stop]        = EventOpcode.Stop,
            });

#if DOTNET461
        static EventSourceOptions GetEventSourceOptions(
            TraceEventType eventType)
            => new EventSourceOptions
            {
                ActivityOptions = EventActivityOptions.None,
                Level           = _traceEventType2eventLevel[eventType],
                Opcode          = _traceEventType2eventOpcode[eventType],
            };
#endif

        /// <summary>
        /// When overridden in a derived class, writes the specified message to the listener you create in the derived class.
        /// </summary>
        /// <param name="message">A message to write.</param>
        public override void Write(string message)
        {
#if DOTNET461
            if (string.IsNullOrEmpty(message))
                return;

            _etwSource.Write(EventName, message);
#endif
        }

        /// <summary>
        /// When overridden in a derived class, writes a message to the listener you create in the derived class, followed by a line terminator.
        /// </summary>
        /// <param name="message">A message to write.</param>
        public override void WriteLine(string message)
        {
            Write(message);
        }
    }
}
