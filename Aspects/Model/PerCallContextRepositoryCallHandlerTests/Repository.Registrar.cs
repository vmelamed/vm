using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using vm.Aspects.Model.EFRepository;
using vm.Aspects.Model.EFRepository.HiLoIdentity;
using vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests.Migrations;
using vm.Aspects.Model.Repository;

namespace vm.Aspects.Model.PerCallContextRepositoryCallHandlerTests
{
    public partial class Repository
    {
        public static new ContainerRegistrar Registrar { get; } = new RepositoryRegistrar();

        private class RepositoryRegistrar : ContainerRegistrar
        {
            protected override void DoRegister(
                IUnityContainer container,
                IDictionary<RegistrationLookup, ContainerRegistration> registrations)
                => RegisterCommon(container, registrations)
                ;

            protected override void DoTestRegister(
                IUnityContainer container,
                IDictionary<RegistrationLookup, ContainerRegistration> registrations)
                => RegisterCommon(container, registrations)
                ;

            IUnityContainer RegisterCommon(
                IUnityContainer container,
                IDictionary<RegistrationLookup, ContainerRegistration> registrations)
            {
                Func<IRepository> generatorsRepositoryFactory = () => ServiceLocator.Current.GetInstance<IRepository>(HiLoStoreIdProvider.HiLoGeneratorsRepositoryResolveName);

                container
                    .UnsafeRegister(EFRepositoryBase.Registrar, registrations, true)

                    .RegisterTypeIfNot<IDatabaseInitializer<Repository>, MigrateDatabaseToLatestVersion<Repository, Configuration>>(registrations)
                    .RegisterTypeIfNot<IStoreIdProvider, HiLoStoreIdProvider>(registrations, new ContainerControlledLifetimeManager(), new InjectionConstructor(generatorsRepositoryFactory, false))

                    // SYNCHRONOUS repositories registration
                    // the repository used by the HiLo generator
                    .RegisterTypeIfNot<IRepository, Repository>(registrations, HiLoStoreIdProvider.HiLoGeneratorsRepositoryResolveName)
                    // the repository used by the services
                    .RegisterTypeIfNot<IRepository, Repository>(registrations, new PerCallContextLifetimeManager())
                    // a transient repository used by tests and anything else.
                    .RegisterTypeIfNot<IRepository, Repository>(registrations, "transient")

                    // ASYNCHRONOUS repositories registration
                    // the repository used by the HiLo generator
                    .RegisterTypeIfNot<IRepositoryAsync, Repository>(registrations, HiLoStoreIdProvider.HiLoGeneratorsRepositoryResolveName)
                    // the repository used by the services
                    .RegisterTypeIfNot<IRepositoryAsync, Repository>(registrations, new PerCallContextLifetimeManager())
                    // a transient repository used by tests and anything else.
                    .RegisterTypeIfNot<IRepositoryAsync, Repository>(registrations, "transient")
                    ;

                return container;
            }
        }
    }
}
