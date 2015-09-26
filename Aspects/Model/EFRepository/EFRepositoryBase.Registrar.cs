using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using Microsoft.Practices.Unity;
using vm.Aspects.Diagnostics;
using vm.Aspects.Diagnostics.ExternalMetadata;
using vm.Aspects.Facilities;
using vm.Aspects.Model.Metadata;
using vm.Aspects.Model.Repository;

namespace vm.Aspects.Model.EFRepository
{
    public abstract partial class EFRepositoryBase
    {
        class EFRepositoryBaseRegistrar : ContainerRegistrar
        {
            /// <summary>
            /// Does the actual work of the registration.
            /// The method is called from a synchronized context, i.e. does not need to be thread safe.
            /// </summary>
            /// <param name="container">The container where to register the defaults.</param>
            /// <param name="registrations">The registrations dictionary used for faster lookup of the existing registrations.</param>
            [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification="Unity will own it.")]
            protected override void DoRegister(
                IUnityContainer container,
                IDictionary<RegistrationLookup, ContainerRegistration> registrations)
            {
                ClassMetadataRegistrar.RegisterMetadata()
                    .Register<ObjectStateEntry, ObjectStateEntryDumpMetadata>()
                    .Register<SqlException, SqlExceptionDumpMetadata>()
                    .Register<SqlError, SqlErrorDumpMetadata>()
                    .Register<MetadataItem, MetadataItemDumpMetadata>()
                    .Register<UpdateException, vm.Aspects.Model.Metadata.UpdateExceptionDumpMetadata>()
                    .Register<DbEntityValidationResult, DbEntityValidationResultDumpMetadata>()
                    .Register<DbValidationError, DbValidationErrorDumpMetadata>()
                    ;

                Facility.Registrar.UnsafeRegister(container, registrations)
                    .RegisterTypeIfNot<IOrmSpecifics, EFSpecifics>(registrations, new ContainerControlledLifetimeManager())
                    //
                    //      In the derived repositories:
                    //
                    //              If you use HiLoStoreIdProvider, do not forget to register a transient repository for its generators:
                    //.RegisterTypeIfNot<IStoreIdProvider, HiLoStoreIdProvider>(registrations, new ContainerControlledLifetimeManager())
                    //.RegisterTypeIfNot<IRepository, MyEFRepository>(registrations, HiLoStoreIdProvider.HiLoGeneratorsRepositoryResolveName)
                    //
                    //              If you enable migrations, register this database initializer:
                    //
                    //.RegisterTypeIfNot<IDatabaseInitializer<EFRepositoryBase>, MigrateDatabaseToLatestVersion<EFRepositoryBase, Configuration>>(new InjectionConstructor(true))
                    ;
            }

            /// <summary>
            /// The inheriting types should override this method if they need to register different configuration for unit testing purposes.
            /// The default implementation calls <see cref="M:ContainerRegistrar.DoRegister" />.
            /// The method is called from a synchronized context, i.e. does not need to be thread safe.
            /// </summary>
            /// <param name="container">The container where to register the defaults.</param>
            /// <param name="registrations">The registrations dictionary used for faster lookup of the existing registrations.</param>
            [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification="Unity will own it.")]
            protected override void DoTestRegister(
                IUnityContainer container,
                IDictionary<RegistrationLookup, ContainerRegistration> registrations)
            {
                ClassMetadataRegistrar.RegisterMetadata()
                    .Register<ObjectStateEntry, ObjectStateEntryDumpMetadata>()
                    .Register<SqlException, SqlExceptionDumpMetadata>()
                    .Register<SqlError, SqlErrorDumpMetadata>()
                    .Register<MetadataItem, MetadataItemDumpMetadata>()
                    .Register<UpdateException, vm.Aspects.Model.Metadata.UpdateExceptionDumpMetadata>()
                    .Register<DbEntityValidationResult, DbEntityValidationResultDumpMetadata>()
                    .Register<DbValidationError, DbValidationErrorDumpMetadata>()
                    ;

                Facility.Registrar.UnsafeRegister(container, registrations, true)
                    .RegisterTypeIfNot<IOrmSpecifics, EFSpecifics>(registrations, new ContainerControlledLifetimeManager())
                    //
                    //      In the derived repositories:
                    //
                    //              If you use HiLoStoreIdProvider, do not forget to register a transient repository for its generators:
                    //.RegisterTypeIfNot<IStoreIdProvider, HiLoStoreIdProvider>(registrations, new ContainerControlledLifetimeManager())
                    //.RegisterTypeIfNot<IRepository, MyEFRepository>(registrations, HiLoStoreIdProvider.HiLoGeneratorsRepositoryResolveName)
                    //
                    //              If you enable migrations, register this database initializer:
                    //
                    //.RegisterTypeIfNot<IDatabaseInitializer<EFRepositoryBase>, MigrateDatabaseToLatestVersion<EFRepositoryBase, Configuration>>(new InjectionConstructor(true))
                    ;
            }
        }

        static readonly ContainerRegistrar _registrar = new EFRepositoryBaseRegistrar();

        /// <summary>
        /// Gets the registrar for the base class.
        /// </summary>
        protected static ContainerRegistrar Registrar
        {
            get
            {
                Contract.Ensures(Contract.Result<ContainerRegistrar>() != null);

                return _registrar;
            }
        }
    }
}
