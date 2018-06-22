using System.Collections.Generic;
using System.ServiceModel;

using Unity;
using Unity.Registration;

using vm.Aspects.Wcf.Services;

namespace vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests
{
    public class TestServiceTasksHostFactory : MessagingPatternInitializedServiceHostFactory<ITestServiceTasks, TestServiceTasks, ServiceInitializer>
    {
        protected override IUnityContainer DoRegisterDefaults(
            IUnityContainer container,
            IDictionary<RegistrationLookup, IContainerRegistration> registrations) => base.DoRegisterDefaults(container, registrations)
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
                    typeof(ITestServiceTasks),
                    new NetTcpBinding(),
                    "TestServiceTasks.svc");

            return host;
        }
    }
}
