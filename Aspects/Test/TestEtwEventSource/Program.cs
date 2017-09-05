using System;
using System.Diagnostics;
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
            lock (DIContainer.Initialize())
            {
                var registrations = DIContainer.Root.GetRegistrationsSnapshot();

                DIContainer.Root
                        .UnsafeRegister(Facility.Registrar, registrations)
                        ;
            }

            TelemetryConfiguration.Active.InstrumentationKey = "f6e10d93-dfae-4112-a0b8-3e3436ec9a85";

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
                }

            Console.Write("Press any key to finish...");
            Console.ReadKey(true);
            Console.WriteLine();
        }
    }
}
