using Microsoft.Diagnostics.Tracing;
using System;
using System.IO;
using System.Linq;

namespace EventSourceSamples
{
    /// <summary>
    /// An EventListener is the most basic 'sink' for EventSource events. All other sinks of 
    /// EventSource data can be thought of as 'built in' EventListeners. In any particular 
    /// AppDomain all the EventSources send messages to any EventListener in the same
    /// AppDomain that have subscribed to them (using the EnableEvents API.
    /// <para>
    /// You create a particular kind of EventListener by sub-classing the EventListener class
    /// Here we create an EventListener that 
    ///   . Enables all events on any EventSource-derived class created in the appDomain
    ///   . Sends all events raised by the event source classes created to the 'Out' textWriter 
    ///     (typically the Console).  
    /// </para>
    /// </summary>
    public class ConsoleEventListener : EventListener
    {
        static TextWriter _out = AllSamples.Out;

        /// <summary>
        /// Override this method to get a list of all the eventSources that exist.  
        /// </summary>
        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            // Because we want to turn on every EventSource, we subscribe to a callback that triggers
            // when new EventSources are created.  It is also fired when the EventListener is created
            // for all pre-existing EventSources.  Thus this callback get called once for every 
            // EventSource regardless of the order of EventSource and EventListener creation.  

            // For any EventSource we learn about, turn it on.   
            EnableEvents(eventSource, EventLevel.LogAlways, EventKeywords.All);
        }

        /// <summary>
        /// We override this method to get a callback on every event we subscribed to with EnableEvents
        /// </summary>
        /// <param name="eventData"></param>
        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            // report all event information
            _out.Write("  Event {0} ", eventData.EventName);

            // Don't display activity information, as that's not used in the demos
            // Out.Write(" (activity {0}{1}) ", ShortGuid(eventData.ActivityId), 
            //                                  eventData.RelatedActivityId != Guid.Empty ? "->" + ShortGuid(eventData.RelatedActivityId) : "");

            // Events can have formatting strings 'the Message property on the 'Event' attribute.  
            // If the event has a formatted message, print that, otherwise print out argument values.  
            if (eventData.Message != null)
                _out.WriteLine(eventData.Message, eventData.Payload?.ToArray());
            else
            {
                string[] sargs = eventData.Payload?.Select(o => o.ToString())?.ToArray();
                _out.WriteLine("({0}).", sargs != null ? string.Join(", ", sargs) : "");
            }
        }

        #region Private members

        private static string ShortGuid(Guid guid)
        { return guid.ToString().Substring(0, 8); }

        #endregion
    }
}
