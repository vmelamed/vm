using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
            string severity,
            string message,
            string errorMessages,
            string categories,
            string extendedProperties)
        {
            if (!IsEnabled())
                return;

            fixed (char*
                        pSeverity = severity,
                        pMessage = message,
                        pErrorMessages = errorMessages,
                        pCategories = categories,
                        pExtendedProperties = extendedProperties)
            {
                const int EventDataLength = 7;
                var eventData = stackalloc EventData[EventDataLength];
                var i = 0;

                AssignIntValue(&eventData[i++], &elEventId);
                AssignIntValue(&eventData[i++], &priority);
                AssignString(&eventData[i++], pSeverity, severity);
                AssignString(&eventData[i++], pMessage, message, true);
                AssignString(&eventData[i++], pErrorMessages, errorMessages);
                AssignString(&eventData[i++], pCategories, categories);
                AssignString(&eventData[i++], pExtendedProperties, extendedProperties);

                // make sure that the EventDataLength constant is always equal to the number of logged fields
                Debug.Assert(i == EventDataLength);

                WriteEventCore(eventId, i, eventData);
            }
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
                const int EventDataLength = 3;
                var eventData = stackalloc EventData[EventDataLength];
                var i = 0;

                AssignIntValue(&eventData[i++], &elEventId);
                AssignString(&eventData[i++], pText, text);
                AssignString(&eventData[i++], pSource, source);

                // make sure that the EventDataLength constant is always equal to the number of logged fields
                Debug.Assert(i == EventDataLength);

                WriteEventCore(eventId, i, eventData);
            }
        }

        // The length of the end of line sequence.
        static int NewLineSize = Environment.NewLine.Length * sizeof(char);

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        unsafe void AssignString(
            EventData* eventData,
            char* pText,
            string text,
            bool stripLeadingNewLine = false)
        {
            if (text.IsNullOrWhiteSpace())
                fixed (char* pEmpty = string.Empty)
                {
                    eventData->DataPointer = (IntPtr)pEmpty;
                    eventData->Size        = sizeof(char);
                    return;
                }

            eventData->DataPointer = (IntPtr)pText;
            eventData->Size        = (text.Length+1) * sizeof(char);

            if (stripLeadingNewLine  &&
                text.StartsWith(Environment.NewLine, StringComparison.OrdinalIgnoreCase))
            {
                eventData->DataPointer += NewLineSize;
                eventData->Size        -= NewLineSize;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        unsafe void AssignIntValue(
            EventData* eventData,
            int* value)
        {
            eventData->DataPointer = (IntPtr)value;
            eventData->Size        = sizeof(int);
        }
    }
}
