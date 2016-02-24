using System.Collections.Generic;
using System.Data.Entity;
using Microsoft.Practices.Unity;
using vm.Aspects.Model.EFRepository.HiLoIdentity;
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
                EFRepositoryBase.Registrar.UnsafeRegister(container, registrations, true)
                    //.RegisterTypeIfNot<IDatabaseInitializer<TestEFRepository>, MigrateDatabaseToLatestVersion<TestEFRepository, Configuration>>(registrations, new InjectionConstructor(true))
                    .RegisterTypeIfNot<IDatabaseInitializer<TestEFRepository>, DropCreateDatabaseAlways<TestEFRepository>>(registrations)
                    .RegisterTypeIfNot<IStoreIdProvider, SqlStoreIdProvider>(registrations, new ContainerControlledLifetimeManager())
                    //.RegisterTypeIfNot<IStoreIdProvider, HiLoStoreIdProvider>(registrations, new ContainerControlledLifetimeManager())

                    // the repo used by the HiLo generator
                    .RegisterTypeIfNot<IRepository, TestEFRepository>(registrations, HiLoStoreIdProvider.HiLoGeneratorsRepositoryResolveName, new InjectionConstructor(new InjectionParameter<string>(ConnectionString)))
                    // the repo used by the rest of the tests
                    .RegisterTypeIfNot<IRepository, TestEFRepository>(registrations, new InjectionConstructor(new InjectionParameter<string>(ConnectionString)))
                    ;
            }
        }

        static readonly ContainerRegistrar _registrar = new TestEFRepositoryRegistrar();

        public static new ContainerRegistrar Registrar => _registrar;
    }
}
