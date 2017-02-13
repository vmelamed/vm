using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.Logging;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.WCF;
using Microsoft.Practices.EnterpriseLibrary.Validation.PolicyInjection;
using vm.Aspects.Exceptions;
using vm.Aspects.Facilities;
using vm.Aspects.Wcf.FaultContracts;

namespace vm.Aspects.Wcf.ServicePolicies
{
    /// <summary>
    /// Class WcfExceptionHandlingPolicies. Defines a registrar and implements <see cref="T:IExceptionPolicyProvider"/> which add a number of mappings of exceptions to faults 
    /// which will be used by the WCF exception shielding mechanism. 
    /// </summary>
    public partial class ServiceExceptionHandlingPolicies : IExceptionPolicyProvider
    {
        /// <summary>
        /// The title of the logged exceptions.
        /// </summary>
        public const string LogExceptionTitle = "vm.Aspects.Wcf";

        #region IExceptionPolicyEntries Members

        /// <summary>
        /// Gets a dictionary of exception policy names and respective lists of policy entries.
        /// </summary>
        public IDictionary<string, IEnumerable<ExceptionPolicyEntry>> ExceptionPolicyEntries
            => new SortedList<string, IEnumerable<ExceptionPolicyEntry>>
            {
                [ExceptionShielding.DefaultExceptionPolicy] = WcfExceptionShieldingPolicyEntries(),
            };
        #endregion

        /// <summary>
        /// Creates an exception policy entry that logs the exception and throws a new fault exception created out of the original exception.
        /// </summary>
        /// <typeparam name="TException">The type of the exception.</typeparam>
        /// <typeparam name="TFault">The type of the fault contract.</typeparam>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="logExceptionTitle">The log exception title.</param>
        /// <returns>An <see cref="ExceptionPolicyEntry" /> instance.</returns>
        public static ExceptionPolicyEntry GetThrowFaultExceptionPolicyEntry<TException, TFault>(
            int eventId,
            string logExceptionTitle = LogExceptionTitle)
        {
            Contract.Ensures(Contract.Result<ExceptionPolicyEntry>() != null);

            return new ExceptionPolicyEntry(
                            typeof(TException),
                            PostHandlingAction.ThrowNewException,
                            new IExceptionHandler[]
                            {
                                new LoggingExceptionHandler(
                                        LogWriterFacades.Exception,
                                        eventId,
                                        TraceEventType.Error,
                                        logExceptionTitle,
                                        1,
                                        typeof(DumpExceptionFormatter),
                                        Facility.LogWriter),

                                new FaultContractExceptionHandler(
                                        typeof(TFault),
                                        new NameValueCollection
                                        {
                                            ["Id"]                 = "{Guid}",
                                            ["HandlingInstanceId"] = "{Guid}",
                                        }),
                           });
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Refers to many exceptions that can be thrown.")]
        static List<ExceptionPolicyEntry> WcfExceptionShieldingPolicyEntries(
            string logExceptionTitle = LogExceptionTitle)
        {
            int eventId = 3000;

            return new List<ExceptionPolicyEntry>
            {
                // The catch all policy:
                GetThrowFaultExceptionPolicyEntry<Exception, Fault>(eventId++, logExceptionTitle),

                GetThrowFaultExceptionPolicyEntry<ArgumentException, ArgumentFault>(eventId++, logExceptionTitle),
                GetThrowFaultExceptionPolicyEntry<ArgumentNullException, ArgumentNullFault>(eventId++, logExceptionTitle),
                GetThrowFaultExceptionPolicyEntry<ArgumentValidationException, ArgumentValidationFault>(eventId++, logExceptionTitle),
                GetThrowFaultExceptionPolicyEntry<InvalidOperationException, InvalidOperationFault>(eventId++, logExceptionTitle),
                GetThrowFaultExceptionPolicyEntry<NotImplementedException, NotImplementedFault>(eventId++, logExceptionTitle),
                GetThrowFaultExceptionPolicyEntry<DataException, DataFault>(eventId++, logExceptionTitle),
                GetThrowFaultExceptionPolicyEntry<DbException, DataFault>(eventId++, logExceptionTitle),
                GetThrowFaultExceptionPolicyEntry<IOException, IOFault>(eventId++, logExceptionTitle),
                GetThrowFaultExceptionPolicyEntry<DirectoryNotFoundException, DirectoryNotFoundFault>(eventId++, logExceptionTitle),
                GetThrowFaultExceptionPolicyEntry<PathTooLongException, PathTooLongFault>(eventId++, logExceptionTitle),
                GetThrowFaultExceptionPolicyEntry<FileNotFoundException, FileNotFoundFault>(eventId++, logExceptionTitle),
                GetThrowFaultExceptionPolicyEntry<ObjectException, ObjectFault>(eventId++, logExceptionTitle),
                GetThrowFaultExceptionPolicyEntry<ObjectNotFoundException, ObjectNotFoundFault>(eventId++, logExceptionTitle),
                GetThrowFaultExceptionPolicyEntry<ObjectIdentifierNotUniqueException, ObjectIdentifierNotUniqueFault>(eventId++, logExceptionTitle),
                GetThrowFaultExceptionPolicyEntry<BusinessException, BusinessFault>(eventId++, logExceptionTitle),
                GetThrowFaultExceptionPolicyEntry<UnauthorizedAccessException, UnauthorizedAccessFault>(eventId++, logExceptionTitle),
                GetThrowFaultExceptionPolicyEntry<FormatException, FormatFault>(eventId++, logExceptionTitle),
                GetThrowFaultExceptionPolicyEntry<SerializationException, SerializationFault>(eventId++, logExceptionTitle),
                GetThrowFaultExceptionPolicyEntry<XmlException, XmlFault>(eventId++, logExceptionTitle),
                GetThrowFaultExceptionPolicyEntry<AggregateException, AggregateFault>(eventId++, logExceptionTitle),
                GetThrowFaultExceptionPolicyEntry<RepeatableOperationException, RepeatableOperationFault>(eventId++, logExceptionTitle),
                
                // to keep the event ID-s consistent, only append to the list above
            };
        }
    }
}
