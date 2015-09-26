using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
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
using Microsoft.Practices.Unity;
using vm.Aspects.Exceptions;
using vm.Aspects.Facilities;
using vm.Aspects.Wcf.FaultContracts;

namespace vm.Aspects.Wcf.ServicePolicies
{
    /// <summary>
    /// Class WcfExceptionHandlingPolicies. Defines a registrar and implements <see cref="T:IExceptionPolicyProvider"/> which add a number of mappings of exceptions to faults 
    /// which will be used by the WCF exception shielding mechanism. 
    /// </summary>
    public class ServiceExceptionHandlingPolicies : IExceptionPolicyProvider
    {
        private class WcfExceptionHandlingPoliciesRegistrar : ContainerRegistrar
        {
            protected override void DoRegister(
                IUnityContainer container,
                IDictionary<RegistrationLookup, ContainerRegistration> registrations)
            {
                container
                    .RegisterInstanceIfNot<IExceptionPolicyProvider>(
                            registrations,
                            "vm.Aspects.Wcf",
                            new ServiceExceptionHandlingPolicies());
            }
        }

        static readonly WcfExceptionHandlingPoliciesRegistrar _registrar = new WcfExceptionHandlingPoliciesRegistrar();

        /// <summary>
        /// Gets the WCF exception handling policies registrar.
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
        {
            get
            {
                return new SortedList<string, IEnumerable<ExceptionPolicyEntry>>
                {
                    { WcfExceptionShielding, ServiceExceptionHandlingPolicies.WcfExceptionShieldingPolicyEntries() }
                };
            }
        }
        #endregion

        static NameValueCollection _faultMappings = new NameValueCollection
                                                    {
                                                        { "HandlingInstanceId", "{Guid}" },
                                                    };

        /// <summary>
        /// Creates an exception policy entry that logs the exception and throws a new fault exception created out of the original exception.
        /// </summary>
        /// <param name="exceptionType">Type of the exception.</param>
        /// <param name="faultType">Type of the fault.</param>
        /// <param name="eventId">The event identifier.</param>
        /// <returns>An <see cref="ExceptionPolicyEntry"/> instance.</returns>
        static ExceptionPolicyEntry GetThrowFaultExceptionPolicyEntry(
            Type exceptionType,
            Type faultType,
            int eventId)
        {
            return new ExceptionPolicyEntry(
                            exceptionType,
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
                                        faultType,
                                        _faultMappings),
                           });
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification="Refers to many exceptions that can be thrown.")]
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
                GetThrowFaultExceptionPolicyEntry(typeof(Exception), typeof(Fault), eventId++),

                GetThrowFaultExceptionPolicyEntry(typeof(ArgumentException), typeof(ArgumentFault), eventId++),
                GetThrowFaultExceptionPolicyEntry(typeof(ArgumentNullException), typeof(ArgumentNullFault), eventId++),
                GetThrowFaultExceptionPolicyEntry(typeof(ArgumentValidationException), typeof(ArgumentValidationFault), eventId++),
                GetThrowFaultExceptionPolicyEntry(typeof(ValidationException), typeof(ValidationResultsFault), eventId++),
                GetThrowFaultExceptionPolicyEntry(typeof(InvalidOperationException), typeof(InvalidOperationFault), eventId++),
                GetThrowFaultExceptionPolicyEntry(typeof(NotImplementedException), typeof(NotImplementedFault), eventId++),
                GetThrowFaultExceptionPolicyEntry(typeof(DataException), typeof(DataFault), eventId++),
                GetThrowFaultExceptionPolicyEntry(typeof(DbException), typeof(DataFault), eventId++),
                GetThrowFaultExceptionPolicyEntry(typeof(IOException), typeof(IOFault), eventId++),
                GetThrowFaultExceptionPolicyEntry(typeof(DirectoryNotFoundException), typeof(DirectoryNotFoundFault), eventId++),
                GetThrowFaultExceptionPolicyEntry(typeof(PathTooLongException), typeof(PathTooLongFault), eventId++),
                GetThrowFaultExceptionPolicyEntry(typeof(FileNotFoundException), typeof(FileNotFoundFault), eventId++),
                GetThrowFaultExceptionPolicyEntry(typeof(ObjectException), typeof(ObjectFault), eventId++),
                GetThrowFaultExceptionPolicyEntry(typeof(ObjectNotFoundException), typeof(ObjectNotFoundFault), eventId++),
                GetThrowFaultExceptionPolicyEntry(typeof(ObjectIdentifierNotUniqueException), typeof(ObjectIdentifierNotUniqueFault), eventId++),
                GetThrowFaultExceptionPolicyEntry(typeof(BusinessException), typeof(BusinessFault), eventId++),
                GetThrowFaultExceptionPolicyEntry(typeof(UnauthorizedAccessException), typeof(UnauthorizedAccessFault), eventId++),
                GetThrowFaultExceptionPolicyEntry(typeof(FormatException), typeof(FormatFault), eventId++),
                GetThrowFaultExceptionPolicyEntry(typeof(SerializationException), typeof(SerializationFault), eventId++),
                GetThrowFaultExceptionPolicyEntry(typeof(XmlException), typeof(XmlFault), eventId++),
                GetThrowFaultExceptionPolicyEntry(typeof(AggregateException), typeof(AggregateFault), eventId++),
                
                // to keep the event ID-s consistent, only append to the list here:
            };
        }
    }
}
