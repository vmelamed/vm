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
    /// <seealso cref="EventSource" />
    /// <seealso cref="IEtwLogEntryHandler" />
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
        /// <param name="data">The trace data to emit.</param>
        [NonEvent]
        unsafe protected void WriteEvent(
            int id,
            LogEntry data)
        {
            if (!IsEnabled())
                return;

            var logEntryEventId    = data.EventId;
            var priority           = data.Priority;
            var categories         = string.Join(", ", data.Categories);
            var relatedActivityId  = data.ActivityId;
            var extendedProperties = DumpExtendedProperties(data.ExtendedProperties);

            fixed (char* pTitle = data.Title,
                         pSeverity = data.LoggedSeverity,
                         pCategories = categories,
                         pMessage = data.Message,
                         pErrorMessages = data.ErrorMessages,
                         pProcessName = data.ProcessName,
                         pAppDomainName = data.AppDomainName,
                         pExtendedProperties = extendedProperties)
            {
                const int eventDataSize = 8;

                var eventData = stackalloc EventData[eventDataSize];

                var i = 0;

                eventData[i].DataPointer = (IntPtr)(&logEntryEventId);
                eventData[i].Size = sizeof(int);
                i++;
                //eventData[i].DataPointer = (IntPtr)pTitle;
                //eventData[i].Size = SizeInBytes(data.Title);
                //i++;
                eventData[i].DataPointer = (IntPtr)pSeverity;
                eventData[i].Size = SizeInBytes(data.LoggedSeverity);
                i++;
                eventData[i].DataPointer = (IntPtr)(&priority);
                eventData[i].Size = sizeof(int);
                i++;
                //eventData[i].DataPointer = (IntPtr)pCategories;
                //eventData[i].Size = SizeInBytes(categories);
                //i++;
                eventData[i].DataPointer = (IntPtr)pMessage;
                eventData[i].Size = SizeInBytes(data.Message);
                i++;
                eventData[i].DataPointer = (IntPtr)(&relatedActivityId);
                eventData[i].Size = sizeof(Guid);
                i++;
                //eventData[i].DataPointer = (IntPtr)pErrorMessages;
                //eventData[i].Size = SizeInBytes(data.ErrorMessages);
                //i++;
                eventData[i].DataPointer = (IntPtr)pProcessName;
                eventData[i].Size = SizeInBytes(data.ProcessName);
                i++;
                eventData[i].DataPointer = (IntPtr)pAppDomainName;
                eventData[i].Size = SizeInBytes(data.AppDomainName);
                i++;
                eventData[i].DataPointer = (IntPtr)pExtendedProperties;
                eventData[i].Size = SizeInBytes(extendedProperties);
                i++;

                Debug.Assert(i == eventDataSize);

                WriteEventCore(id, eventDataSize, eventData);
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
