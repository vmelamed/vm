using System;
using System.Diagnostics;
using vm.Aspects;
using vm.Aspects.Facilities;

namespace TestEtwEventSource
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                DIContainer
                    .Initialize();

                lock (DIContainer.Root)
                {
                    var registrations = DIContainer.Root.GetRegistrationsSnapshot();

                    DIContainer.Root
                            .UnsafeRegister(Facility.Registrar, registrations)
                            ;
                }

                Facility.LogWriter.TraceInfo("Hello there!", new object[] { });
            }
            catch (Exception x)
            {
                Debug.WriteLine("", x.DumpString());
            }
        }
    }
}
