using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
        /// Gets the registrar of the policies.
        /// </summary>
        public static ContainerRegistrar Registrar { get; } = new ExceptionHandlingPoliciesRegistrar();

        /// <summary>
        /// The registration name for the facilities policy provider by the facilities.
        /// </summary>
        public const string RegistrationName = "vm.Aspects.Facilities";
        /// <summary>
        /// The name of the log and swallow policy.
        /// </summary>
        public const string LogAndSwallowPolicyName = "Log and Swallow";
        /// <summary>
        /// The name of the log and swallow policy.
        /// </summary>
        public const string LogAndRethrowPolicyName = "Log and Rethrow";

        /// <summary>
        /// Creates a logging exception handler.
        /// </summary>
        /// <param name="exceptionTitle">The exception title.</param>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="priority">The priority.</param>
        /// <returns>LoggingExceptionHandler.</returns>
        public static LoggingExceptionHandler CreateLoggingExceptionHandler(
            string exceptionTitle,
            int eventId,
            TraceEventType eventType = TraceEventType.Error,
            int priority = 1)
            => new LoggingExceptionHandler(
                                        LogWriterFacades.Exception,
                                        eventId++,
                                        eventType,
                                        exceptionTitle,
                                        priority,
                                        typeof(DumpExceptionFormatter),
                                        Facility.LogWriter);

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
                if (container == null)
                    throw new ArgumentNullException(nameof(container));
                if (registrations == null)
                    throw new ArgumentNullException(nameof(registrations));

                container
                    .RegisterInstanceIfNot<IExceptionPolicyProvider>(
                            registrations,
                            RegistrationName,
                            new ExceptionPolicyProvider());
            }
        }

        #region IExceptionPolicyEntries Members

        /// <summary>
        /// Gets a dictionary of exception policy names and respective lists of policy entries.
        /// </summary>
        public IDictionary<string, IEnumerable<ExceptionPolicyEntry>> ExceptionPolicyEntries =>
            new SortedList<string, IEnumerable<ExceptionPolicyEntry>>
            {
                [LogAndSwallowPolicyName] = new List<ExceptionPolicyEntry>
                                            {
                                                new ExceptionPolicyEntry(
                                                    typeof(Exception),
                                                    PostHandlingAction.None,
                                                    new IExceptionHandler[]
                                                    {
                                                        CreateLoggingExceptionHandler(
                                                                RegistrationName,
                                                                1000),
                                                    }),
                                            },

                [LogAndRethrowPolicyName] = new List<ExceptionPolicyEntry>
                                            {
                                                new ExceptionPolicyEntry(
                                                    typeof(Exception),
                                                    PostHandlingAction.NotifyRethrow,
                                                    new IExceptionHandler[]
                                                    {
                                                        CreateLoggingExceptionHandler(
                                                                RegistrationName,
                                                                1100),
                                                    }),
                                            },
            };
        #endregion

        /// <summary>
        /// Resolves all registered exception entries associated with exception policies, 
        /// merges them and creates an exception manager based on all registered policies.
        /// </summary>
        /// <returns>ExceptionManager object.</returns>
        public static ExceptionManager CreateExceptionManager()
        {
            // gather all policies and merge the ones with the same names
            var policyEntries = new Dictionary<string, List<ExceptionPolicyEntry>>();

            // merge all policy definitions
            foreach (var provider in ServiceLocator.Current.GetAllInstances<IExceptionPolicyProvider>())
                foreach (var policyDefinition in provider.ExceptionPolicyEntries)
                    if (!policyEntries.TryGetValue(policyDefinition.Key, out var list))
                        // add a new policy
                        policyEntries[policyDefinition.Key] = policyDefinition.Value.ToList();
                    else
                    {
                        var duplicates = list.Intersect(policyDefinition.Value).ToList();

                        if (duplicates.Any())
                        {
                            var message = new StringBuilder(
                                                $"One of the exception policy providers brings duplicate exception entries to policy {policyDefinition.Key}:\n{string.Join("\n  ", duplicates.Select(x => x.ExceptionType.FullName))}");

                            throw new InvalidOperationException(message.ToString());
                        }

                        // add the entries to the existing policy
                        list.AddRange(policyDefinition.Value);
                    }

            // create the exception manager
            var exceptionManager = new ExceptionManager(
                                            policyEntries
                                                .Select(
                                                    // create a policy definition out of the name and the merged entries
                                                    pe => new ExceptionPolicyDefinition(pe.Key, pe.Value)));

            // set the manager in the ExceptionPolicy class
            ExceptionPolicy.SetExceptionManager(exceptionManager, false);

            return exceptionManager;
        }
    }
}
