using System;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Diagnostics;
using vm.Aspects.Diagnostics.ExternalMetadata;
using vm.Aspects.Facilities;
using vm.Aspects.Model.EFRepository.HiLoIdentity;
using vm.Aspects.Model.Metadata;
using vm.Aspects.Model.Repository;
using vm.Aspects.Model.Tests;

namespace vm.Aspects.Model.EFRepository.Tests
{
    /// <summary>
    /// Summary description for EFRepositoryTests
    /// </summary>
    [TestClass]
    public class EFRepositoryTests : IRepositoryTest<TestEFRepository>
    {
        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void ClassInitialize(TestContext testContext)
        {
            try
            {
                ClassMetadataRegistrar
                    .RegisterMetadata()
                    .Register<UpdateException, UpdateExceptionDumpMetadata>()
                    .Register<DbUpdateException, DbUpdateExceptionDumpMetadata>()
                    .Register<ObjectStateEntry, ObjectStateEntryDumpMetadata>()
                    ;

                DIContainer.Initialize();

                lock (DIContainer.Root)
                {
                    var registrations = DIContainer.Root.GetRegistrationsSnapshot();

                    DIContainer.Root
                               .UnsafeRegister(Facility.Registrar, registrations, true)
                               .UnsafeRegister(TestEFRepository.Registrar, registrations, true)
                               ;
                }
            }
            catch (Exception x)
            {
                testContext.WriteLine("{0}", x.DumpString());
                throw;
            }
        }

        // Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void ClassCleanup()
        //{
        //}
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        static IStoreIdProvider _storeIdProvider = new HiLoStoreIdProvider(() => new TestEFRepository("ModelTests"));

        public static IRepository CreateRepository()
        {
            return new TestEFRepository("ModelTests", _storeIdProvider);
        }

        protected override IRepository GetRepository()
        {
            return CreateRepository();
        }
    }
}
