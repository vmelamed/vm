using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Tracing;

namespace vm.Aspects.Facilities.Diagnostics
{
    /// <summary>
    /// Class VmAspectsBaseEventSource implements primitives for writing to <see cref="VmAspectsEventSource"/>.
    /// </summary>
    /// <seealso cref="System.Diagnostics.Tracing.EventSource" />
    public abstract class VmAspectsBaseEventSource : EventSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VmAspectsBaseEventSource"/> class.
        /// </summary>
        protected VmAspectsBaseEventSource()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VmAspectsBaseEventSource"/> class.
        /// </summary>
        /// <param name="throwOnEventWriteErrors">true to throw an exception when an error occurs in the underlying Windows code; otherwise, false.</param>
        protected VmAspectsBaseEventSource(
            bool throwOnEventWriteErrors)
            : base(throwOnEventWriteErrors)
        {
        }

        // The length of the end of line sequence.
        static readonly int _newLineSize = Environment.NewLine.Length * sizeof(char);

        /// <summary>
        /// Optionally strips the leading CR-LF of two text fields of the event and writes them to the log.
        /// </summary>
        /// <param name="eventId">The ETW event identifier.</param>
        /// <param name="text1">The first text.</param>
        /// <param name="text2">The second text.</param>
        [NonEvent]
        protected unsafe void WriteDumpEvent(
            int eventId,
            string text1,
            string text2)
        {
            if (!IsEnabled())
                return;

            fixed (char*
                        pText1 = text1,
                        pText2 = text2)
            {
                var eventData = stackalloc EventData[2];

                AssignStringStripLeadingEndOfLine(&eventData[0], pText1, text1);
                AssignStringStripLeadingEndOfLine(&eventData[1], pText2, text2);

                WriteEventCore(eventId, 2, eventData);
            }
        }

        /// <summary>
        /// Assigns the string to an <see cref="T:EventData"/> item while stripping the leading end of line if present.
        /// </summary>
        /// <param name="eventData">The event data item to assign to.</param>
        /// <param name="pText">The p text.</param>
        /// <param name="text">The text.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [NonEvent]
        unsafe void AssignStringStripLeadingEndOfLine(
            EventData* eventData,
            char* pText,
            string text)
        {
            if (text == null)
            {
                eventData->DataPointer = IntPtr.Zero;
                eventData->Size        = 0;
                return;
            }

            eventData->DataPointer = (IntPtr)pText;
            eventData->Size        = (text.Length+1) * sizeof(char);

            if (text.StartsWith(Environment.NewLine, StringComparison.OrdinalIgnoreCase))
            {
                eventData->DataPointer += _newLineSize;
                eventData->Size        -= _newLineSize;
            }
        }

        /// <summary>
        /// Assigns the string.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        /// <param name="pText">The p text.</param>
        /// <param name="text">The text.</param>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [NonEvent]
        unsafe void AssignString(
            EventData* eventData,
            char* pText,
            string text)
        {
            if (text == null)
            {
                eventData->DataPointer = IntPtr.Zero;
                eventData->Size        = 0;
                return;
            }

            eventData->DataPointer = (IntPtr)pText;
            eventData->Size        = (text.Length+1) * sizeof(char);
        }

        /// <summary>
        /// Assigns the int value to an <see cref="T:EventData"/> item.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        /// <param name="pValue">The p value.</param>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [NonEvent]
        unsafe void AssignIntValue(
            EventData* eventData,
            int* pValue)
        {
            eventData->DataPointer = (IntPtr)pValue;
            eventData->Size        = sizeof(int);
        }
    }
}
