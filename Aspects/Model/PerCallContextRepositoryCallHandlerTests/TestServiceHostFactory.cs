using Microsoft.Practices.Unity;
using System.Collections.Generic;
using System.ServiceModel;
using vm.Aspects.Wcf.Services;

namespace vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests
{
    public class TestServiceHostFactory : MessagingPatternInitializedServiceHostFactory<ITestService, TestService, ServiceInitializer>
    {
        protected override IUnityContainer DoRegisterDefaults(
            IUnityContainer container,
            IDictionary<RegistrationLookup, ContainerRegistration> registrations) => base.DoRegisterDefaults(container, registrations)
                                                                                         .UnsafeRegister(TestService.Registrar, registrations)
                                                                                         ;

        /// <summary>
        /// Gives opportunity to the service host factory to add programmatically endpoints before configuring them all.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <returns>ServiceHost.</returns>
        protected override ServiceHost AddEndpoints(
            ServiceHost host)
        {
            //host.AddServiceEndpoint(
            //        typeof(IService),
            //        new NetNamedPipeBinding(),
            //        "");

            //host.AddServiceEndpoint(
            //        typeof(IService),
            //        new WSHttpBinding(WebHttpSecurityMode.Transport),
            //        "");

            host.AddServiceEndpoint(
                    typeof(ITestService),
                    new NetTcpBinding(),
                    "TestService.svc");

            return host;
        }
    }
}
