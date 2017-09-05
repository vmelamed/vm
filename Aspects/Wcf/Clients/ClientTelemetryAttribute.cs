using System;

namespace vm.Aspects.Wcf.Clients
{
    /// <summary>
    /// Instruments client-side WCF endpoints to generate DependencyTelemetry events on calls.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ClientTelemetryAttribute : Attribute
    {
    }
}
