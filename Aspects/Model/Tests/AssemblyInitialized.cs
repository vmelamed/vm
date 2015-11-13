using System;
using System.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.PolicyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Diagnostics;
using vm.Aspects.Diagnostics.ExternalMetadata;
using vm.Aspects.Facilities;
using vm.Aspects.Model.EFRepository.Tests;

namespace vm.Aspects.Model.Tests
{
    [TestClass]
    public class AssemblyInitialized
    {
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            try
            {
                ClassMetadataRegistrar.RegisterMetadata()
                    .Register<ArgumentValidationException, ArgumentValidationExceptionDumpMetadata>()
                    .Register<ValidationResult, ValidationResultDumpMetadata>()
                    .Register<ValidationResults, ValidationResultsDumpMetadata>()
                    .Register<ConfigurationErrorsException, ConfigurationErrorsExceptionDumpMetadata>()
                    ;

                DIContainer
                    .Initialize()
                    .Register(Facility.Registrar, true)
                    .Register(TestEFRepository.Registrar, true)
                    .RegisterInstanceIfNot<IMoneyDefaults>(new MoneyDefaults());
            }
            catch (Exception x)
            {
                context.WriteLine("{0}", x.DumpString());
                throw;
            }
        }
    }
}
