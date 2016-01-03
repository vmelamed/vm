using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using Microsoft.Practices.ServiceLocation;
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
            static void RegisterMetadata()
            {
                ClassMetadataRegistrar.RegisterMetadata()
                    .Register<SqlException, SqlExceptionDumpMetadata>()
                    .Register<SqlError, SqlErrorDumpMetadata>()
                    .Register<MetadataItem, MetadataItemDumpMetadata>()
                    .Register<UpdateException, UpdateExceptionDumpMetadata>()
                    .Register<DbUpdateException, DbUpdateExceptionDumpMetadata>()
                    .Register<ObjectStateEntry, ObjectStateEntryDumpMetadata>()
                    .Register<DbEntityEntry, DbEntityEntryDumpMetadata>()
                    .Register<DbEntityValidationResult, DbEntityValidationResultDumpMetadata>()
                    .Register<DbValidationError, DbValidationErrorDumpMetadata>()
                    .Register<ObjectContext>(new DumpAttribute { RecurseDump = ShouldDump.Skip })
                    .Register<MetadataProperty>(new DumpAttribute { RecurseDump = ShouldDump.Skip })
                    .Register<FieldMetadata>(new DumpAttribute { RecurseDump = ShouldDump.Skip })
                    .Register<EntityKey>(new DumpAttribute { RecurseDump = ShouldDump.Skip })
                    .Register<TypeUsage>(new DumpAttribute { RecurseDump = ShouldDump.Skip })
                    .Register<AssociationSet>(new DumpAttribute { RecurseDump = ShouldDump.Skip })
                    .Register<EntityRecordInfo>(new DumpAttribute { RecurseDump = ShouldDump.Skip })
                    .Register<ReferentialConstraint>(new DumpAttribute { RecurseDump = ShouldDump.Skip })
                    .Register<RelationshipManager>(new DumpAttribute { RecurseDump = ShouldDump.Skip })
                    ;
            }

            /// <summary>
            /// Does the actual work of the registration.
            /// The method is called from a synchronized context, i.e. does not need to be thread safe.
            /// </summary>
            /// <param name="container">The container where to register the defaults.</param>
            /// <param name="registrations">The registrations dictionary used for faster lookup of the existing registrations.</param>
            [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Unity will own it.")]
            protected override void DoRegister(
                IUnityContainer container,
                IDictionary<RegistrationLookup, ContainerRegistration> registrations)
            {
                RegisterMetadata();

                Facility
                    .Registrar
                    .UnsafeRegister(container, registrations)
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
                    //.RegisterTypeIfNot<IDatabaseInitializer<EFRepositoryBase-derived>, MigrateDatabaseToLatestVersion<EFRepositoryBase-derived, Configuration>>(new InjectionConstructor(true))
                    ;
            }

            [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Unity will own it.")]
            protected override void DoTestRegister(
                IUnityContainer container,
                IDictionary<RegistrationLookup, ContainerRegistration> registrations)
            {
                RegisterMetadata();

                Facility
                    .Registrar
                    .UnsafeRegister(container, registrations, true)
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
                    //.RegisterTypeIfNot<IDatabaseInitializer<EFRepositoryBase-derived>, MigrateDatabaseToLatestVersion<EFRepositoryBase-derived, Configuration>>(new InjectionConstructor(true))
                    ;

                var clock = ServiceLocator.Current.GetInstance<IClock>() as TestClock;

                if (clock != null)
                    clock.StartTime = DateTime.Parse("2016-01-01T00:00:00.0000000Z", CultureInfo.InvariantCulture);
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
