using Microsoft.Practices.Unity;
using System.Collections.Generic;
using System.Data.Entity;
using vm.Aspects.Model.EFRepository;
using vm.Aspects.Model.Repository;

namespace vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests
{
    public partial class TestRepository
    {
        public static ContainerRegistrar Registrar { get; } = new TestRepositoryRegistrar();

        private class TestRepositoryRegistrar : ContainerRegistrar
        {
            protected override void DoRegister(
                IUnityContainer container,
                IDictionary<RegistrationLookup, ContainerRegistration> registrations)
                => RegisterCommon(container, registrations, false);

            protected override void DoTestRegister(
                IUnityContainer container,
                IDictionary<RegistrationLookup, ContainerRegistration> registrations)
                => RegisterCommon(container, registrations, true);

            IUnityContainer RegisterCommon(
                IUnityContainer container,
                IDictionary<RegistrationLookup, ContainerRegistration> registrations,
                bool isTest)
            {
                container
                    //.RegisterTypeIfNot<IDatabaseInitializer<TestRepository>, MigrateDatabaseToLatestVersion<TestRepository, Configuration>>(registrations)
                    .RegisterTypeIfNot<IDatabaseInitializer<TestRepository>, DropCreateDatabaseAlways<TestRepository>>(registrations)

                    // SYNCHRONOUS repositories registration
                    // the repository used by the services
                    .RegisterTypeIfNot<IRepository, TestRepository>(registrations, new HierarchicalLifetimeManager())
                    // a transient repository used by tests and anything else.
                    .RegisterTypeIfNot<IRepository, TestRepository>(registrations, "transient")

                    .UnsafeRegister(EFRepositoryBase.Registrar<TestRepository>(), registrations, isTest)
                    ;

                return container;
            }
        }
    }
}
