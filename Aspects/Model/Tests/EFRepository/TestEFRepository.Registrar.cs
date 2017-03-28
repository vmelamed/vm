using Microsoft.Practices.Unity;
using System.Collections.Generic;
using System.Data.Entity;
using vm.Aspects.Model.Repository;

namespace vm.Aspects.Model.EFRepository.Tests
{
    public partial class TestEFRepository
    {
        public const string ConnectionString = "ModelTests";

        private class TestEFRepositoryRegistrar : ContainerRegistrar
        {
            protected override void DoRegister(
                IUnityContainer container,
                IDictionary<RegistrationLookup, ContainerRegistration> registrations)
            {
                container
                    .Register(EFRepositoryBase.Registrar<TestEFRepository>(), true)
                    //.RegisterTypeIfNot<IDatabaseInitializer<TestEFRepository>, MigrateDatabaseToLatestVersion<TestEFRepository, Configuration>>(registrations, new InjectionConstructor(true))
                    .RegisterTypeIfNot<IDatabaseInitializer<TestEFRepository>, DropCreateDatabaseAlways<TestEFRepository>>(registrations)
                    .RegisterTypeIfNot<IStoreIdProvider, SqlStoreIdProvider>(registrations, new ContainerControlledLifetimeManager())
                    //.RegisterTypeIfNot<IStoreIdProvider, HiLoStoreIdProvider>(registrations, new ContainerControlledLifetimeManager())

                    // the repository used by the HiLo generator
                    //.RegisterTypeIfNot<IRepository, TestEFRepository>(registrations, HiLoStoreIdProvider.HiLoGeneratorsRepositoryResolveName, new InjectionConstructor(new InjectionParameter<string>(ConnectionString)))
                    // the repository used by the rest of the tests
                    .RegisterTypeIfNot<IRepository, TestEFRepository>(registrations, new InjectionConstructor(new InjectionParameter<string>(ConnectionString)))
                    ;
            }
        }

        public static ContainerRegistrar Registrar { get; } = new TestEFRepositoryRegistrar();
    }
}
