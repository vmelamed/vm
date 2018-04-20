using System;
using System.Collections.Generic;
using System.Data.Entity;

using Unity;
using Unity.Injection;
using Unity.Lifetime;
using Unity.Registration;

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
                IDictionary<RegistrationLookup, IContainerRegistration> registrations)
            {
                if (container == null)
                    throw new ArgumentNullException(nameof(container));
                if (registrations == null)
                    throw new ArgumentNullException(nameof(registrations));

                RegisterCommon(container, registrations, false);
            }

            protected override void DoTestRegister(
                IUnityContainer container,
                IDictionary<RegistrationLookup, IContainerRegistration> registrations)
            {
                if (container == null)
                    throw new ArgumentNullException(nameof(container));
                if (registrations == null)
                    throw new ArgumentNullException(nameof(registrations));

                RegisterCommon(container, registrations, true);
            }

            IUnityContainer RegisterCommon(
                IUnityContainer container,
                IDictionary<RegistrationLookup, IContainerRegistration> registrations,
                bool isTest)
            {
                container
                    //.RegisterTypeIfNot<IDatabaseInitializer<TestRepository>, MigrateDatabaseToLatestVersion<TestRepository, Configuration>>(registrations)
                    .RegisterTypeIfNot<IDatabaseInitializer<TestRepository>, DropCreateDatabaseAlways<TestRepository>>(registrations)

                    .UnsafeRegister(EFRepositoryBase.Registrar<TestRepository>(), registrations, isTest)

                    // repositories registration
                    // the repository used by the services
                    .RegisterTypeIfNot<IRepository, TestRepository>(
                                        registrations,
                                        new HierarchicalLifetimeManager(),
                                        new InjectionProperty(
                                                nameof(IRepository.OptimisticConcurrencyStrategy),
                                                Program.OptimisticConcurrencyStrategy))

                    // a transient repository used by tests and anything else.
                    .RegisterTypeIfNot<IRepository, TestRepository>(
                                        registrations,
                                        "transient",
                                        new InjectionProperty(
                                                nameof(IRepository.OptimisticConcurrencyStrategy),
                                                Program.OptimisticConcurrencyStrategy))
                    ;

                return container;
            }
        }
    }
}
