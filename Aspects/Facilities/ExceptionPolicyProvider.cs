using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.Logging;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;

namespace vm.Aspects.Facilities
{
    /// <summary>
    /// Class ExceptionPolicyProvider. Contains a registrar which registers two popular exception handling policies - "Log and Swallow" and "Log and Rethrow".
    /// Implements <see cref="T:vm.Aspects.Facilities.IExceptionPolicyEntries"/> which gives the sets of policies and corresponding entries.
    /// When the exception manager is instantiated it will get all registered instances of the interface and will merge the registered entries from the same policy.
    /// </summary>
    public class ExceptionPolicyProvider : IExceptionPolicyProvider
    {
        /// <summary>
        /// The registrar of the policies.
        /// </summary>
        static readonly ExceptionHandlingPoliciesRegistrar _registrar = new ExceptionHandlingPoliciesRegistrar();

        /// <summary>
        /// Gets the registrar of the policies.
        /// </summary>
        public static ContainerRegistrar Registrar
        {
            get
            {
                Contract.Ensures(Contract.Result<ContainerRegistrar>() != null);

                return _registrar;
            }
        }

        /// <summary>
        /// The registration name for the facilities policy provider by the facilities.
        /// </summary>
        public const string RegistrationName = "vm.Aspects.Facilities";
        /// <summary>
        /// The name of the log and swallow policy.
        /// </summary>
        public const string LogAndSwallow = "Log and Swallow";
        /// <summary>
        /// The name of the log and swallow policy.
        /// </summary>
        public const string LogAndRethrow = "Log and Rethrow";

        /// <summary>
        /// Class ExceptionHandlingPoliciesRegistrar. Registers the two exception handling policies.
        /// </summary>
        private class ExceptionHandlingPoliciesRegistrar : ContainerRegistrar
        {
            public override void Reset(
                IUnityContainer container = null)
            {
                ExceptionPolicy.Reset();
                base.Reset(container);
            }

            /// <summary>
            /// Does the actual work of the registration.
            /// The method is not thread safe.
            /// </summary>
            /// <param name="container">The container where to register the defaults.</param>
            /// <param name="registrations">The registrations dictionary used for faster lookup of the existing registrations.</param>
            protected override void DoRegister(
                IUnityContainer container,
                IDictionary<RegistrationLookup, ContainerRegistration> registrations)
            {
                container
                    .RegisterInstanceIfNot<IExceptionPolicyProvider>(
                            registrations,
                            "vm.Aspects.Facilities",
                            new ExceptionPolicyProvider());
            }
        }

        #region IExceptionPolicyEntries Members

        /// <summary>
        /// Gets a dictionary of exception policy names and respective lists of policy entries.
        /// </summary>
        public IDictionary<string, IEnumerable<ExceptionPolicyEntry>> ExceptionPolicyEntries
        {
            get
            {
                return new SortedList<string, IEnumerable<ExceptionPolicyEntry>>
                {
                    {
                        LogAndSwallow,
                        new List<ExceptionPolicyEntry>
                        {
                            new ExceptionPolicyEntry(
                                typeof(Exception),
                                PostHandlingAction.None,
                                new IExceptionHandler[]
                                {
                                    new LoggingExceptionHandler(
                                            LogWriterFacades.Exception,
                                            1000,
                                            TraceEventType.Error,
                                            "vm.Aspects.Facilities",
                                            1,
                                            typeof(DumpExceptionFormatter),
                                            Facility.LogWriter),
                                }),
                        }
                    },
                    {
                        LogAndRethrow,
                        new List<ExceptionPolicyEntry>
                        {
                            new ExceptionPolicyEntry(
                                typeof(Exception),
                                PostHandlingAction.NotifyRethrow,
                                new IExceptionHandler[]
                                {
                                    new LoggingExceptionHandler(
                                            LogWriterFacades.Exception,
                                            2000,
                                            TraceEventType.Error,
                                            "vm.Aspects.Facilities",
                                            1,
                                            typeof(DumpExceptionFormatter),
                                            Facility.LogWriter),
                                }),
                        }
                    },
                };
            }
        }

        #endregion

        /// <summary>
        /// Resolves all registered exception entries associated with exception policies, 
        /// merges them and creates an exception manager based on all registered policies.
        /// </summary>
        /// <returns>ExceptionManager object.</returns>
        public static ExceptionManager CreateExceptionManager()
        {
            Contract.Ensures(Contract.Result<ExceptionManager>() != null);

            // gather all policies and merge the ones with the same names
            var policyEntries = new Dictionary<string, List<ExceptionPolicyEntry>>();
            List<ExceptionPolicyEntry> list;

            foreach (var e in ServiceLocator.Current.GetAllInstances<IExceptionPolicyProvider>())
                foreach (var d in e.ExceptionPolicyEntries)
                    if (!policyEntries.TryGetValue(d.Key, out list))
                        // add a new policy
                        policyEntries[d.Key] = d.Value.ToList();
                    else
                        // add the entries to the existing policy
                        list.AddRange(d.Value);

            // create the exception manager
            var exceptionManager = new ExceptionManager(
                                            policyEntries.Select(
                // create a policy definition out of the name and the merged entries
                                                pe => new ExceptionPolicyDefinition(pe.Key, pe.Value)));

            // set the manager in the ExceptionPolicy class
            ExceptionPolicy.SetExceptionManager(exceptionManager, false);

            return exceptionManager;
        }
    }
}
