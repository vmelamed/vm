using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace vm.Aspects.Facilities
{
    /// <summary>
    /// Base class for event sources that would log event entry objects from the Enterprise Library logging application block.
    /// </summary>
    /// <seealso cref="System.Diagnostics.Tracing.EventSource" />
    /// <seealso cref="vm.Aspects.Facilities.IEtwLogEntryHandler" />
    public abstract class EtwLogEntryBaseEventSource : EventSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EtwLogEntryBaseEventSource"/> class.
        /// </summary>
        protected EtwLogEntryBaseEventSource()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EtwLogEntryBaseEventSource"/> class.
        /// </summary>
        /// <param name="throwOnEventWriteErrors">true to throw an exception when an error occurs in the underlying Windows code; otherwise, false.</param>
        protected EtwLogEntryBaseEventSource(
            bool throwOnEventWriteErrors)
            : base(throwOnEventWriteErrors)
        {
        }

        /// <summary>
        /// Writes the event.
        /// </summary>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="eventId">A numeric identifier for the EL-LAB event.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="data">The trace data to emit.</param>
        protected unsafe void WriteLogEntryEvent(
            int id,
            int eventId,
            TraceEventCache eventCache,
            object data)
        {
            if (!IsEnabled())
                return;

            WriteLogEntryEvent(id, eventId, eventCache, data.DumpString());
        }

        /// <summary>
        /// Writes the event.
        /// </summary>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="eventId">A numeric identifier for the EL-LAB event.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="data">The trace data to emit.</param>
        protected unsafe void WriteLogEntryEvent(
            int id,
            int eventId,
            TraceEventCache eventCache,
            string data)
        {
            if (!IsEnabled())
                return;

            const int numArguments = 4;

            fixed (char* pStack = eventCache?.Callstack,
                         pData = data)
            {
                var eventData = stackalloc EventData[numArguments];
                var i = 0;

                eventData[i++] = new EventData { DataPointer = (IntPtr)(&id), Size = sizeof(int) };
                eventData[i++] = new EventData { DataPointer = (IntPtr)(&eventId), Size = sizeof(int) };
                eventData[i++] = new EventData { DataPointer = (IntPtr)pData, Size = SizeInBytes(data) };
                eventData[i++] = new EventData { DataPointer = (IntPtr)pStack, Size = SizeInBytes(eventCache.Callstack) };

                WriteEventCore(eventId, numArguments, eventData);
            }
        }

        /// <summary>
        /// Writes the event.
        /// </summary>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="eventCache">A <see cref="TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="data">The trace data to emit.</param>
        protected unsafe void WriteLogEntryEvent(
            int id,
            TraceEventCache eventCache,
            LogEntry data)
        {
            if (!IsEnabled())
                return;

            var categories         = string.Join(", ", data.Categories);
            var priority           = data.Priority;
            var relatedActivity    = data.RelatedActivityId!=null ? data.RelatedActivityId.Value.ToString() : null;
            var extendedProperties = DumpExtendedProperties(data.ExtendedProperties);
            var logEntryEventId    = data.EventId;

            fixed (char* pStack = eventCache.Callstack,
                         pActivityId = data.ActivityIdString,
                         pAppDomainName = data.AppDomainName,
                         pCategories = categories,
                         pTitle = data.Title,
                         pErrorMessages = data.ErrorMessages,
                         pSeverity = data.LoggedSeverity,
                         pMessage = data.Message,
                         pProcessName = data.ProcessName,
                         pRelatedActivity = relatedActivity,
                         pExtendedProperties = extendedProperties)
            {
                const int MaxEventEntries = 32;

                var eventData = stackalloc EventData[MaxEventEntries];

                for (var j = 0; j < MaxEventEntries; j++)
                    eventData = null;

                var i = 0;

                eventData[i++] = new EventData { DataPointer = (IntPtr)(&logEntryEventId), Size = sizeof(int) };
                eventData[i++] = new EventData { DataPointer = (IntPtr)pTitle, Size = SizeInBytes(data.Title) };
                eventData[i++] = new EventData { DataPointer = (IntPtr)pSeverity, Size = SizeInBytes(data.LoggedSeverity) };
                eventData[i++] = new EventData { DataPointer = (IntPtr)(&priority), Size = sizeof(int) };
                eventData[i++] = new EventData { DataPointer = (IntPtr)pCategories, Size = SizeInBytes(categories) };
                eventData[i++] = new EventData { DataPointer = (IntPtr)pMessage, Size = SizeInBytes(data.Message) };
                eventData[i++] = new EventData { DataPointer = (IntPtr)pActivityId, Size = SizeInBytes(data.ActivityIdString) };
                eventData[i++] = new EventData { DataPointer = (IntPtr)pErrorMessages, Size = SizeInBytes(data.ErrorMessages) };
                eventData[i++] = new EventData { DataPointer = (IntPtr)pProcessName, Size = SizeInBytes(data.ProcessName) };
                eventData[i++] = new EventData { DataPointer = (IntPtr)pRelatedActivity, Size = SizeInBytes(relatedActivity) };
                eventData[i++] = new EventData { DataPointer = (IntPtr)pAppDomainName, Size = SizeInBytes(data.AppDomainName) };
                eventData[i++] = new EventData { DataPointer = (IntPtr)pExtendedProperties, Size = SizeInBytes(extendedProperties) };
                eventData[i++] = new EventData { DataPointer = (IntPtr)pStack, Size = SizeInBytes(eventCache.Callstack) };

                WriteEventCore(id, i, eventData);
            }
        }

        /// <summary>
        /// Dumps the extended properties.
        /// </summary>
        /// <param name="extendedProperties">The extended properties.</param>
        /// <returns>System.String.</returns>
        /// <exception cref="NotImplementedException"></exception>
        protected static string DumpExtendedProperties(
            IDictionary<string, object> extendedProperties)
        {
            if (extendedProperties == null  ||  extendedProperties.Count == 0)
                return null;

            using (var writer = new StringWriter())
            {
                foreach (var kv in extendedProperties)
                    writer.WriteLine("{0,-24} = {1}", kv.Key, kv.Value.DumpString(1));

                return writer.GetStringBuilder().ToString();
            }
        }

        /// <summary>
        /// Computes the sizes the in bytes of a string.
        /// </summary>
        /// <param name="text">The string.</param>
        /// <returns>System.Int32.</returns>
        protected static int SizeInBytes(string text)
            => text == null ? 0 : (text.Length + 1) * sizeof(char);

    }
}
