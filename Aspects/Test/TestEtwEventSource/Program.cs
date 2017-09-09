using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using vm.Aspects;
using vm.Aspects.Facilities;
using vm.Aspects.Facilities.LogWriters.Etw;

namespace TestEtwEventSource
{
    class Program
    {
        static void Main(string[] args)
        {
            TelemetryConfiguration.Active.InstrumentationKey = "1ea142b2-1d97-453e-a0f4-32b15523dd7d";

            var ai = new TelemetryClient();

            lock (DIContainer.Initialize())
            {
                var registrations = DIContainer.Root.GetRegistrationsSnapshot();

                DIContainer.Root
                        .UnsafeRegister(Facility.Registrar, registrations)
                        ;
            }


            const int timesToRepeat = 1;

            for (int i = 0; i < timesToRepeat; i++)
                try
                {
                    Facility.LogWriter.TraceInfo("Hello there!");

                    EtwLogEntryEventSource.Log.Trace(-1, "This is EtwLogEntryEventSource.Log.Trace");
                    EtwLogEntryEventSource.Log.WriteMessage("This is EtwLogEntryEventSource.Log.WriteMessage");

                    throw new InvalidOperationException("This is a test exception.");
                }
                catch (Exception x)
                {
                    Facility.LogWriter.ExceptionError(x);
                    Debug.WriteLine("", x.DumpString());
                    ai.TrackException(x);
                }

            ai.Flush();
            Task.Delay(1000).Wait();

            Console.Write("Press any key to finish...");
            Console.ReadKey(true);
            Console.WriteLine();
        }
    }
}
