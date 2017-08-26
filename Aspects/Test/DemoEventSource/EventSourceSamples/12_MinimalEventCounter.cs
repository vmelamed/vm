using System.IO;
using Microsoft.Diagnostics.Tracing;
using System.Threading;

//
// This demonstrates a minimal usage scenario for event counters
//
// * MinimalEventCounterSource specifies:
//     * an explicit name for the ETW provider it defines (in the EventSourceAttribute)
//     * the singleton instance exposed to the user code: MinimalEventCounterSource.Log
//     * one non-decorated ETW event method: MinimalEventCounterSource.Request()
//
// * MinimalEventCounterDemo 
//     * fires ETW events by calling MinimalEventCounterSource.Log.Load()
//
namespace EventSourceSamples
{
    // Give your event sources a descriptive name using the EventSourceAttribute, otherwise the name of the class is used. 
    [EventSource(Name = "Samples-EventCounterDemos-Minimal")]
    public sealed class MinimalEventCounterSource : EventSource
    {
        // define the singleton instance of the event source
        public static MinimalEventCounterSource Log = new MinimalEventCounterSource();
        private EventCounter requestCounter;

        private MinimalEventCounterSource()
        {
            this.requestCounter = new EventCounter("request", this);
        }

        /// <summary>
        /// Call this method to indicate that a request for a URL was made which took a particular amount of time
        public void Request(string url, float elapsedMSec)
        {
            WriteEvent(1, url, elapsedMSec);                // This logs it to the event stream if events are on.    

            // Notes:
            //   1. Each event counter object instance represents a metric that we want to know its statistics.
            //      In this case, we wanted to know the statistics about the request time
            //   2. Each WriteMetric call represents a single measure on that metric.
            this.requestCounter.WriteMetric(elapsedMSec);   // This adds it to the EventCounter called 'Request' if EventCounters are on
        }
    }

    public class MinimalEventCounterDemo
    {
        static TextWriter Out = AllSamples.Out;

        /// <summary>
        /// This is a simplest demo of EventCounters.  Because AllSamples.Run created a Console
        /// listener, these messages go to the console.   You can also use an ETW controller like
        /// the 'PerfView tool' (bing PerfView for download) to turn on the events e.g. 
        ///
        /// To turn on both events and counters:
        ///     PerfView /onlyProviders=*Samples-EventCounterDemos-Minimal:EventCounterIntervalSec=1 collect
        /// 
        /// To turn on only counters:
        ///     PerfView /onlyProviders=*Samples-EventCounterDemos-Minimal:*:Critical:EventCounterIntervalSec=1 collect
        /// </summary>
        public static void Run()
        {
            Out.WriteLine("******************** MinimalEventCounterSource Demo ********************");
            Out.WriteLine("Sending 15 Request events over 4 seconds from the Samples-EventCounterDemos-Minimal source.");

            // Generate some data, 
            for (int i = 0; i < 5; i++)
            {
                MinimalEventCounterSource.Log.Request("http://Someplace", 100 + i);
                MinimalEventCounterSource.Log.Request("http://Someplace", 23 + i);
                MinimalEventCounterSource.Log.Request("http://Someplace", 32 + i);
                Thread.Sleep(1000);
            }

            Out.WriteLine("Done.");

            //
            // At this point, the PerfView should have got a few messages about the request statistics.
            // You should see something like this:
            // count               : 3
            // mean                : ~= 51 (increasing 1 at a time)
            // StandardDerivation  : ~= 32.3776 
            // min                 : starting with 23 and then increasing 1 at a time
            // max                 : starting with 100 and then increasing 1 at a time
            // IntervalSec : ~= 1
            //
            // These statistics are pretty obvious. 
            //
        }
    }
}
