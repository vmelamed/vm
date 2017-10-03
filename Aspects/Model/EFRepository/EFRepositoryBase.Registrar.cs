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
using System.Globalization;
using Microsoft.Practices.ServiceLocation;
using Unity;
using vm.Aspects.Diagnostics;
using vm.Aspects.Diagnostics.ExternalMetadata;
using vm.Aspects.Facilities;
using vm.Aspects.Model.EFRepository.HiLoIdentity;
using vm.Aspects.Model.Metadata;
using vm.Aspects.Model.Repository;

namespace vm.Aspects.Model.EFRepository
{
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Yes, it is a big class and yet it's not a God class")]
    public abstract partial class EFRepositoryBase
    {
        /// <summary>
        /// Gets the registrar for the base class.
        /// </summary>
        /// <typeparam name="T">The type of the actual repository derived from <see cref="EFRepositoryBase"/>.</typeparam>
        /// <returns>A <see cref="ContainerRegistrar"/> derived repository instance.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Unity will own it.")]
        public static ContainerRegistrar Registrar<T>() where T : EFRepositoryBase
        {
            
            return new EFRepositoryBaseRegistrar<T>();
        }

        class EFRepositoryBaseRegistrar<T> : ContainerRegistrar where T : EFRepositoryBase
        {
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
                if (container == null)
                    throw new ArgumentNullException(nameof(container));
                if (registrations == null)
                    throw new ArgumentNullException(nameof(registrations));

                DoRegisterCommon(container, registrations, false);
            }

            [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Unity will own it.")]
            protected override void DoTestRegister(
                IUnityContainer container,
                IDictionary<RegistrationLookup, ContainerRegistration> registrations)
            {
                if (container == null)
                    throw new ArgumentNullException(nameof(container));
                if (registrations == null)
                    throw new ArgumentNullException(nameof(registrations));

                DoRegisterCommon(container, registrations, true);

                var clock = ServiceLocator.Current.GetInstance<IClock>() as TestClock;

                if (clock != null)
                    clock.StartTime = DateTime.Parse("2016-01-01T00:00:00.0000000Z", CultureInfo.InvariantCulture);
            }

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

            [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
            static void DoRegisterCommon(
                IUnityContainer container,
                IDictionary<RegistrationLookup, ContainerRegistration> registrations,
                bool isTest)
            {
                RegisterMetadata();

                container
                    // make sure the facilities are registered
                    .UnsafeRegister(Facility.Registrar, registrations, isTest)

                    // register the RDBMS bridge
                    .RegisterTypeIfNot<IOrmSpecifics, EFSpecifics>(registrations, new ContainerControlledLifetimeManager())

                    // register the default HiLo generator:
                    .RegisterTypeIfNot<IStoreIdProvider, HiLoStoreIdProvider>(registrations, new ContainerControlledLifetimeManager())

                    // from time to time this HiLo generator needs a factory-method that will return a fresh transient repository. The default method resolves the repository from the container, registered below.
                    .RegisterInstanceIfNot<Func<IRepository>>(registrations, HiLoStoreIdProvider.HiLoGeneratorsRepositoryResolveName, () => DIContainer.Root.Resolve<IRepository>(HiLoStoreIdProvider.HiLoGeneratorsRepositoryResolveName))
                    .RegisterTypeIfNot(registrations, typeof(IRepository), typeof(T), HiLoStoreIdProvider.HiLoGeneratorsRepositoryResolveName, new TransientLifetimeManager())

                    // In the descendant classes, if you enable migrations, register a database initializer:
                    //.RegisterTypeIfNot<IDatabaseInitializer<T>, MigrateDatabaseToLatestVersion<T, Configuration>>(new InjectionConstructor(true))

                    .UnsafeRegister(OptimisticConcurrencyExceptionHandlingPolicies.Registrar, registrations, isTest)
                    ;
            }
        }
    }
}
