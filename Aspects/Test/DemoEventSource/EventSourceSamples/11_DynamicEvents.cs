using Microsoft.Diagnostics.Tracing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EventSourceSamples    
{
    class DynamicEventSourceDemo
    {
        static TextWriter Out = AllSamples.Out;

        /// <summary>
        /// This is a trivial example that shows the use of the 'dynamic' (non contract)
        /// use of EventSource.   It is recommended that you use the contract based version
        /// if you know the event schema at compile time.  
        /// 
        /// To see the events use
        /// 
        /// PerfView /onlyProviders=*Samples-EventSourceDemos-Dynamic collect
        /// </summary>
        public static void Run()
        {
            Out.WriteLine("******************** DynamicEventSource  Demo ********************");
            Out.WriteLine("PerfView /onlyProviders=*Samples-EventSourceDemos-Dynamic collect");

            var mySource = new EventSource("Samples-EventSourceDemos-Dynamic");

            // Notice I can make events on the fly, but this is not as efficient as the
            // contract based approach.   
            mySource.Write("DynamicEventStart", new { id = 3, arg = "hello" });
            for (int i = 0; i < 10; i++)
                mySource.Write("DynamicLoop", new { iteration = i });
            mySource.Write("DynamicEventStop", new { id = 3, arg = "hello" });

            Out.WriteLine("Done.");
        }
    }
}
