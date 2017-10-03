using System.Collections.Generic;
using System.ServiceModel;
using Microsoft.ApplicationInsights.Wcf;
using Unity.InterceptionExtension;
using vm.Aspects.Facilities;
using vm.Aspects.Wcf.Behaviors;

namespace vm.Aspects.Wcf.TestServer
{
    [DIBehavior]
    [Tag("ServicePolicy")]
    [ServiceTelemetry]
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerCall,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class RequestResponseService : IRequestResponse
    {
        public ICollection<string> GetStrings(int numberOfStrings)
        {
            var strings = new List<string>(numberOfStrings);

            for (var i = 0; i<numberOfStrings; i++)
                strings.Add(Facility.GuidGenerator.NewGuid().ToString("N"));

            return strings;
        }
    }
}
