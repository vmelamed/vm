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
using System.ServiceModel;
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
        /// The name of the exception handling policy.
        /// </summary>
        public const string WcfExceptionShielding = "WCF Exception Shielding";
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
                [WcfExceptionShielding] = WcfExceptionShieldingPolicyEntries(),
            };
        #endregion

        /// <summary>
        /// Creates an exception policy entry that logs the exception and throws a new fault exception created out of the original exception.
        /// </summary>
        /// <typeparam name="TException">The type of the exception.</typeparam>
        /// <typeparam name="TFault">The type of the fault contract.</typeparam>
        /// <param name="eventId">The event identifier.</param>
        /// <returns>An <see cref="ExceptionPolicyEntry" /> instance.</returns>
        public static ExceptionPolicyEntry GetThrowFaultExceptionPolicyEntry<TException, TFault>(int eventId)
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
                                        LogExceptionTitle,
                                        1,
                                        typeof(DumpExceptionFormatter),
                                        Facility.LogWriter),

                                new FaultContractExceptionHandler(
                                        typeof(TFault),
                                        new NameValueCollection {["Id"] = "{Guid}"}),
                           });
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Refers to many exceptions that can be thrown.")]
        static List<ExceptionPolicyEntry> WcfExceptionShieldingPolicyEntries()
        {
            int eventId = 3000;

            return new List<ExceptionPolicyEntry>
            {
                // pass-through the fault exceptions
                new ExceptionPolicyEntry(
                            typeof(FaultException),
                            PostHandlingAction.NotifyRethrow,
                            new IExceptionHandler[]
                            {
                                new LoggingExceptionHandler(
                                        "Fault",
                                        eventId++,
                                        TraceEventType.Error,
                                        LogExceptionTitle,
                                        1,
                                        typeof(DumpExceptionFormatter),
                                        Facility.LogWriter),
                           }),

                // The catch all policy:
                GetThrowFaultExceptionPolicyEntry<Exception, Fault>(eventId++),

                GetThrowFaultExceptionPolicyEntry<ArgumentException, ArgumentFault>(eventId++),
                GetThrowFaultExceptionPolicyEntry<ArgumentNullException, ArgumentNullFault>(eventId++),
                GetThrowFaultExceptionPolicyEntry<ArgumentValidationException, ArgumentValidationFault>(eventId++),
                GetThrowFaultExceptionPolicyEntry<InvalidOperationException, InvalidOperationFault>(eventId++),
                GetThrowFaultExceptionPolicyEntry<NotImplementedException, NotImplementedFault>(eventId++),
                GetThrowFaultExceptionPolicyEntry<DataException, DataFault>(eventId++),
                GetThrowFaultExceptionPolicyEntry<DbException, DataFault>(eventId++),
                GetThrowFaultExceptionPolicyEntry<IOException, IOFault>(eventId++),
                GetThrowFaultExceptionPolicyEntry<DirectoryNotFoundException, DirectoryNotFoundFault>(eventId++),
                GetThrowFaultExceptionPolicyEntry<PathTooLongException, PathTooLongFault>(eventId++),
                GetThrowFaultExceptionPolicyEntry<FileNotFoundException, FileNotFoundFault>(eventId++),
                GetThrowFaultExceptionPolicyEntry<ObjectException, ObjectFault>(eventId++),
                GetThrowFaultExceptionPolicyEntry<ObjectNotFoundException, ObjectNotFoundFault>(eventId++),
                GetThrowFaultExceptionPolicyEntry<ObjectIdentifierNotUniqueException, ObjectIdentifierNotUniqueFault>(eventId++),
                GetThrowFaultExceptionPolicyEntry<BusinessException, BusinessFault>(eventId++),
                GetThrowFaultExceptionPolicyEntry<UnauthorizedAccessException, UnauthorizedAccessFault>(eventId++),
                GetThrowFaultExceptionPolicyEntry<FormatException, FormatFault>(eventId++),
                GetThrowFaultExceptionPolicyEntry<SerializationException, SerializationFault>(eventId++),
                GetThrowFaultExceptionPolicyEntry<XmlException, XmlFault>(eventId++),
                GetThrowFaultExceptionPolicyEntry<AggregateException, AggregateFault>(eventId++),
                
                // to keep the event ID-s consistent, only append to the list above
            };
        }
    }
}
