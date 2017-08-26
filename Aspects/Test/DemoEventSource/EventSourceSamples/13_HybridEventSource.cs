using Microsoft.Diagnostics.Tracing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EventSourceSamples
{
    class HybridEventSourceDemo
    {
        static TextWriter Out = AllSamples.Out;

        /// <summary>
        /// In this example we create an EventSource that has both statically defined events
        /// (StaticEvent and LoopCount), but also uses Write() to create dynamically defined
        /// events.  
        /// </summary>
        [EventSource(Name = "Samples-EventSourceDemos-Hybrid")]
        sealed class MySource : EventSource
        {
            public void StaticEvent(string arg) { WriteEvent(1, arg); }
            public void LoopCount(int index) { WriteEvent(2, index); }
            // TODO this is legal but currently EventRegister rejects it.  
            // public void ComplextStaticEvent(string arg, int[] arrayArg) { WriteEvent(3, arg, arrayArg); }

            public static MySource Logger = new MySource();

            // Because we want to listen using ETW, and we use complex payloads we need to
            // specify that we are using the ETW self-describing format.  Dynamic events 
            // only work with the self-describing format.   
            private MySource() : base(EventSourceSettings.EtwSelfDescribingEventFormat) { }
        }

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
            Out.WriteLine("PerfView /onlyProviders=*Samples-EventSourceDemos-Hybrid collect");

            // Notice I can make events on the fly, but this is not as efficient as the
            // contract based approach.   
            MySource.Logger.StaticEvent("Sending a statically defined event");

            for (int i = 0; i < 10; i++)
                MySource.Logger.LoopCount(i);

            // We can log complex payloads statically 
            // TODO reenable. MySource.Logger.ComplextStaticEvent("A Complex Event Arg", new int[] { 3, 4, 5 });

            // But we can also define new events 'on the fly' using Write<T>
            MySource.Logger.Write("ComplexEventWith_Write<T>", new
            {
                myID = "hi",
                time = DateTime.Now,                      // DateTime works
                point = new { x = 3, y = 5 },           // So do sub-structures
                intvalues = new[] { 3, 4, 5, 6 },      // Or arrays 
                arg1 = "hello",
                arg2 = 5,
                arg3 = false // and strings and primitive types
            });

            Out.WriteLine("Done.");
        }
    }
}
