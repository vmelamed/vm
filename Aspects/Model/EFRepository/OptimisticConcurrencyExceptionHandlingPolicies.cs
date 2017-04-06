using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.Logging;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using vm.Aspects.Exceptions;
using vm.Aspects.Facilities;

namespace vm.Aspects.Model.EFRepository
{
    /// <summary>
    /// Optimistic concurrency exception handling policies registrar.
    /// </summary>
    /// <seealso cref="IExceptionPolicyProvider" />
    public partial class OptimisticConcurrencyExceptionHandlingPolicies : IExceptionPolicyProvider
    {
        /// <summary>
        /// The DI registration name of the policy provider
        /// </summary>
        public const string RegistrationName = nameof(OptimisticConcurrencyExceptionHandlingPolicies);

        /// <summary>
        /// The no exception processing whatsoever.
        /// </summary>
        public const string NonePolicyName = nameof(OptimisticConcurrencyStrategy.None);
        /// <summary>
        /// The store wins policy name.
        /// </summary>
        public const string StoreWinsPolicyName = nameof(OptimisticConcurrencyStrategy.StoreWins);
        /// <summary>
        /// The client wins policy name. The <see cref="DbUpdateConcurrencyException"/>-s are swallowed, 
        /// some of the DB related exceptions are wrapped in <see cref="RepeatableOperationException"/> and 
        /// the rest of the exceptions are rethrown.
        /// </summary>
        public const string ClientWinsPolicyName = nameof(OptimisticConcurrencyStrategy.ClientWins);

        /// <summary>
        /// The title of the logged exceptions.
        /// </summary>
        public const string LogExceptionTitle = "vm.Aspects.Model.EFRepository";

        #region IExceptionPolicyEntries Members
        /// <summary>
        /// Gets a dictionary of exception policy names and respective lists of policy entries.
        /// </summary>
        public IDictionary<string, IEnumerable<ExceptionPolicyEntry>> ExceptionPolicyEntries
            => new SortedList<string, IEnumerable<ExceptionPolicyEntry>>
            {
                [NonePolicyName]       = NoneExceptionPolicyEntries(),
                [StoreWinsPolicyName]  = StoreWinsExceptionPolicyEntries(),
                [ClientWinsPolicyName] = ClientWinsExceptionPolicyEntries(),
            };
        #endregion

        static ExceptionPolicyEntry[] NoneExceptionPolicyEntries(
            string logExceptionTitle = LogExceptionTitle)
        {
            int eventId = 3500;

            return new ExceptionPolicyEntry[]
            {
                // basically log warning and rethrow
                new ExceptionPolicyEntry(
                        typeof(Exception),
                        PostHandlingAction.NotifyRethrow,
                        new IExceptionHandler[]
                        {
                            new LoggingExceptionHandler(
                                    LogWriterFacades.Exception,
                                    eventId++,
                                    TraceEventType.Information, // the exceptions are logged as Information so that they are easy to suppress.
                                    logExceptionTitle,          // They should be handled properly by the higher on the stack callers.
                                    1,
                                    typeof(DumpExceptionFormatter),
                                    Facility.LogWriter),
                        })
            };
        }

        static ExceptionPolicyEntry[] ClientWinsExceptionPolicyEntries(
            string logExceptionTitle = LogExceptionTitle)
        {
            int eventId = 3600;

            return new ExceptionPolicyEntry[]
            {
                new ExceptionPolicyEntry(
                        typeof(DbUpdateConcurrencyException),
                        PostHandlingAction.None,
                        new IExceptionHandler[]
                        {
                            new LoggingExceptionHandler(
                                    LogWriterFacades.Exception,
                                    eventId++,
                                    TraceEventType.Information, // the DbUpdateConcurrencyException-s are logged as Information so that they are easy to suppress.
                                    logExceptionTitle,          // the exception is handled locally in the repository by repeating the operation and enforcing the client values
                                    2,
                                    typeof(DumpExceptionFormatter),
                                    Facility.LogWriter),

                            // do not even call the handler for DbUpdateConcurrencyException
                        }),

                new ExceptionPolicyEntry(
                        typeof(Exception),
                        PostHandlingAction.ThrowNewException,
                        new IExceptionHandler[]
                        {
                            new LoggingExceptionHandler(
                                    LogWriterFacades.Exception,
                                    eventId++,
                                    TraceEventType.Warning,     // all other exceptions are logged as warnings, some of them will be wrapped and thrown as RepeatableOperationException the rest will be rethrown
                                    logExceptionTitle,          // callers higher on the stack should handle them properly
                                    2,
                                    typeof(DumpExceptionFormatter),
                                    Facility.LogWriter),

                            new EFRepositoryExceptionHandler(
                                    OptimisticConcurrencyStrategy.ClientWins),
                        })
            };
        }

        static ExceptionPolicyEntry[] StoreWinsExceptionPolicyEntries(
            string logExceptionTitle = LogExceptionTitle)
        {
            int eventId = 3700;

            return new ExceptionPolicyEntry[]
            {
                new ExceptionPolicyEntry(
                        typeof(Exception),
                        PostHandlingAction.ThrowNewException,
                        new IExceptionHandler[]
                        {
                            new LoggingExceptionHandler(
                                    LogWriterFacades.Exception,
                                    eventId++,
                                    TraceEventType.Warning,     // all exceptions are logged as warnings, some of them will be wrapped and thrown as RepeatableOperationException the rest will be rethrown
                                    logExceptionTitle,          // callers higher on the stack should handle them properly
                                    2,
                                    typeof(DumpExceptionFormatter),
                                    Facility.LogWriter),

                            new EFRepositoryExceptionHandler(
                                    OptimisticConcurrencyStrategy.StoreWins),
                        })
            };
        }

        /// <summary>
        /// Gets the registrar of the policies.
        /// </summary>
        public static ContainerRegistrar Registrar { get; } = new EFRepositoryExceptionHandlerRegistrar();

        /// <summary>
        /// Class ExceptionHandlingPoliciesRegistrar. Registers the two exception handling policies.
        /// </summary>
        class EFRepositoryExceptionHandlerRegistrar : ContainerRegistrar
        {
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
                            RegistrationName,
                            new OptimisticConcurrencyExceptionHandlingPolicies());
            }
        }
    }
}
