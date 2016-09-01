using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.ServiceModel;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.WCF;
using Microsoft.Practices.Unity;
using vm.Aspects.Facilities;

namespace vm.Aspects.Wcf.Tests
{
    /// <summary>
    /// Class WcfExceptionHandlingPolicies. Defines a registrar and implements <see cref="T:IExceptionPolicyProvider"/> which add a number of mappings of exceptions to faults 
    /// which will be used by the WCF exception shielding mechanism. 
    /// </summary>
    public partial class MockExceptionHandlingPolicies : IExceptionPolicyProvider
    {
        static IDictionary<string, IEnumerable<ExceptionPolicyEntry>> _exceptionPolicyEntries
            => new SortedList<string, IEnumerable<ExceptionPolicyEntry>>
            {
                [ExceptionShielding.DefaultExceptionPolicy]  = WcfExceptionShieldingPolicyEntries(),
                ["CustomPolicy"]             = CustomPolicyEntries(),
                ["UnhandledLoggedException"] = UnhandledLoggedExceptionPolicyEntries(),
                ["HandledLoggedException"]   = HandledLoggedExceptionPolicyEntries(),
                ["FaultException"]           = FaultExceptionPolicyEntries(),
            };

        #region IExceptionPolicyEntries Members
        /// <summary>
        /// Gets a dictionary of exception policy names and respective lists of policy entries.
        /// </summary>
        public IDictionary<string, IEnumerable<ExceptionPolicyEntry>> ExceptionPolicyEntries
            => _exceptionPolicyEntries;
        #endregion

        private class MockExceptionHandlingPoliciesRegistrar : ContainerRegistrar
        {
            protected override void DoRegister(
                IUnityContainer container,
                IDictionary<RegistrationLookup, ContainerRegistration> registrations)
            {
                container
                    .RegisterInstanceIfNot<IExceptionPolicyProvider>(registrations, "vm.Aspects.Tests", new MockExceptionHandlingPolicies())
                    ;
            }
        }

        static readonly MockExceptionHandlingPoliciesRegistrar _registrar = new MockExceptionHandlingPoliciesRegistrar();

        /// <summary>
        /// Gets the WCF exception handling policies registrar.
        /// </summary>
        public static ContainerRegistrar Registrar => _registrar;


        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Refers to many exceptions that can be thrown.")]
        static List<ExceptionPolicyEntry> WcfExceptionShieldingPolicyEntries()
            => new List<ExceptionPolicyEntry>
            {
                new ExceptionPolicyEntry(
                        typeof(ArithmeticException),
                        PostHandlingAction.ThrowNewException,
                        new IExceptionHandler[]
                        {
                            new MockFaultContractExceptionHandler()
                        }),
                new ExceptionPolicyEntry(
                        typeof(ArgumentNullException),
                        PostHandlingAction.ThrowNewException,
                        new IExceptionHandler[]
                        {
                            new FaultContractExceptionHandler(typeof(MockFaultContract), new NameValueCollection { ["Id"] = "Guid" })
                        })
            };

        static List<ExceptionPolicyEntry> CustomPolicyEntries()
            => new List<ExceptionPolicyEntry>
            {
                new ExceptionPolicyEntry(
                        typeof(ArgumentException),
                        PostHandlingAction.ThrowNewException,
                        new IExceptionHandler[]
                        {
                            new MockFaultContractExceptionHandler(),
                        })
            };

        static List<ExceptionPolicyEntry> UnhandledLoggedExceptionPolicyEntries()
            => new List<ExceptionPolicyEntry>
            {
                new ExceptionPolicyEntry(
                        typeof(ArithmeticException),
                        PostHandlingAction.ThrowNewException,
                        new IExceptionHandler[]
                        {
                            new MockUnhandledLoggingExceptionHandler(new NameValueCollection { ["Id"] = "Guid" }),
                            new WrapHandler("{handlingInstanceId}", typeof(Exception))
                        })
            };

        static List<ExceptionPolicyEntry> HandledLoggedExceptionPolicyEntries()
            => new List<ExceptionPolicyEntry>
            {
                new ExceptionPolicyEntry(
                        typeof(ArithmeticException),
                        PostHandlingAction.ThrowNewException,
                        new IExceptionHandler[]
                        {
                            new MockHandledLoggingExceptionHandler(new NameValueCollection { ["Id"] = "Guid" }),
                            new FaultContractExceptionHandler(typeof(MockFaultContract), new NameValueCollection { ["Id"] = "Guid" })
                        })
            };

        static List<ExceptionPolicyEntry> FaultExceptionPolicyEntries()
            => new List<ExceptionPolicyEntry>
            {
                new ExceptionPolicyEntry(
                        typeof(FaultException),
                        PostHandlingAction.ThrowNewException,
                        new IExceptionHandler[]
                        {
                        })
            };
    }
}
