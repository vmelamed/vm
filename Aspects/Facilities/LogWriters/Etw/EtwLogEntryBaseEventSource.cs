using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace vm.Aspects.Facilities.LogWriters.Etw
{
    /// <summary>
    /// Base class for event sources that would log event entry objects from the Enterprise Library logging application block.
    /// </summary>
    /// <seealso cref="EventSource" />
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
        /// Writes the <see cref="LogEntry" /> fields.
        /// </summary>
        /// <param name="eventId">The ETW event identifier.</param>
        /// <param name="elEventId">The EL LAB event identifier.</param>
        /// <param name="priority">The priority.</param>
        /// <param name="severity">The severity.</param>
        /// <param name="message">The message.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <param name="categories">The categories.</param>
        /// <param name="extendedProperties">The extended properties.</param>
        [NonEvent]
        unsafe protected void WriteEvent(
            int eventId,
            int elEventId,
            int priority,
            TraceEventType severity,
            string message,
            string errorMessages,
            string categories,
            string extendedProperties)
        {
            if (!IsEnabled())
                return;

            // remove the leading CR-LF
            int messageStart = 0;

            if (message.StartsWith(Environment.NewLine))
                messageStart = Environment.NewLine.Length;

            var intSeverity = (int)severity;
            var i = 0;

            // make sure that the EventDataLength constant is always equal to the number of logged fields below
            const int EventDataLength = 7;

            fixed (char*
                        pMessage = message,
                        pErrorMessages = errorMessages,
                        pCategories = categories,
                        pExtendedProperties = extendedProperties)
            {
                var eventData = stackalloc EventData[EventDataLength];

                eventData[i].DataPointer = (IntPtr)(&elEventId);
                eventData[i].Size        = sizeof(int);
                i++;
                eventData[i].DataPointer = (IntPtr)(&priority);
                eventData[i].Size        = sizeof(int);
                i++;
                eventData[i].DataPointer = (IntPtr)(&intSeverity);
                eventData[i].Size        = sizeof(int);
                i++;
                eventData[i].DataPointer = (IntPtr)(&pMessage[messageStart]);
                eventData[i].Size        = SizeInBytes(message);
                i++;
                eventData[i].DataPointer = (IntPtr)pErrorMessages;
                eventData[i].Size        = SizeInBytes(errorMessages);
                i++;
                eventData[i].DataPointer = (IntPtr)pCategories;
                eventData[i].Size        = SizeInBytes(categories);
                i++;
                eventData[i].DataPointer = (IntPtr)pExtendedProperties;
                eventData[i].Size        = SizeInBytes(extendedProperties);
                i++;

                WriteEventCore(eventId, i, eventData);
            }

            // make sure that the EventDataLength constant is always equal to the number of logged fields below
            Debug.Assert(i == EventDataLength);
        }

        /// <summary>
        /// Writes the event.
        /// </summary>
        /// <param name="eventId">The ETW event identifier.</param>
        /// <param name="elEventId">The EL LAB event identifier.</param>
        /// <param name="text">The text.</param>
        /// <param name="source">The source.</param>
        [NonEvent]
        unsafe protected void WriteEvent(
            int eventId,
            int elEventId,
            string text,
            string source)
        {
            if (!IsEnabled())
                return;

            fixed (char*
                    pText = text,
                    pSource = source)
            {
                // make sure that the EventDataLength constant is always equal to the number of logged fields below
                const int EventDataLength = 3;

                var eventData = stackalloc EventData[EventDataLength];
                var i = 0;

                eventData[i].DataPointer = (IntPtr)(&elEventId);
                eventData[i].Size        = sizeof(int);
                i++;
                eventData[i].DataPointer = (IntPtr)pText;
                eventData[i].Size        = SizeInBytes(text);
                i++;
                eventData[i].DataPointer = (IntPtr)pSource;
                eventData[i].Size        = SizeInBytes(source);
                i++;

                // make sure that the EventDataLength constant is always equal to the number of logged fields below
                Debug.Assert(i == EventDataLength);

                WriteEventCore(eventId, i, eventData);
            }
        }

        /// <summary>
        /// Computes the size of a string in bytes.
        /// </summary>
        /// <param name="text">The string.</param>
        /// <returns>System.Int32.</returns>
        protected static int SizeInBytes(string text)
        {
            if (text == null)
                return 0;

            var size = (text.Length+1)*sizeof(char);

            if (text.StartsWith(Environment.NewLine))
                return size - Environment.NewLine.Length*sizeof(char);
            else
                return size;
        }
    }
}
