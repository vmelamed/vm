using System.Diagnostics.Tracing;

namespace vm.Aspects.Facilities.Diagnostics
{
    public sealed partial class VmAspectsEventSource
    {
        #region WCF events
        /// <summary>
        /// Finished the the DI registrations needed for the service host.
        /// </summary>
        [NonEvent]
        public void ServiceHostFactoryRegisteredDefaults<TService>()
        {
            if (IsEnabled(EventLevel.Informational, Keywords.vmAspects))
                ServiceHostFactoryRegisteredDefaults(typeof(TService).FullName);
        }

        /// <summary>
        /// Created the the service host.
        /// </summary>
        [NonEvent]
        public void ServiceHostFactoryCreatedServiceHost<TService>()
        {
            if (IsEnabled(EventLevel.Informational, Keywords.vmAspects))
                ServiceHostFactoryCreatedServiceHost(typeof(TService).FullName);
        }

        /// <summary>
        /// Marks the beginning of the initialization of the service host.
        /// </summary>
        [NonEvent]
        public void InitializeServiceHostStart<TService>()
        {
            if (IsEnabled(EventLevel.Informational, Keywords.vmAspects))
                InitializeServiceHostStart(typeof(TService).FullName);
        }

        /// <summary>
        /// Marks the end of the initialization of the service host.
        /// </summary>
        [NonEvent]
        public void InitializeServiceHostStop<TService>()
        {
            if (IsEnabled(EventLevel.Informational, Keywords.vmAspects))
                InitializeServiceHostStop(typeof(TService).FullName);
        }

        /// <summary>
        /// Finished the the DI registrations needed for the service host.
        /// </summary>
        /// <param name="ServiceTypeName">The name of the service type.</param>
        [Event(ServiceHostFactoryRegisteredDefaultsId, Level = EventLevel.Informational, Keywords = Keywords.vmAspects | Keywords.Wcf)]
        void ServiceHostFactoryRegisteredDefaults(
            string ServiceTypeName)
        {
            if (IsEnabled())
                WriteEvent(ServiceHostFactoryRegisteredDefaultsId, ServiceTypeName);
        }

        /// <summary>
        /// Created the the service host.
        /// </summary>
        /// <param name="ServiceTypeName">Name of the service type.</param>
        [Event(ServiceHostFactoryCreatedServiceHostId, Level = EventLevel.Informational, Keywords = Keywords.vmAspects | Keywords.Wcf)]
        void ServiceHostFactoryCreatedServiceHost(
            string ServiceTypeName)
        {
            if (IsEnabled())
                WriteEvent(ServiceHostFactoryCreatedServiceHostId, ServiceTypeName);
        }

        /// <summary>
        /// Marks the beginning of the initialization of the service host.
        /// </summary>
        /// <param name="ServiceTypeName">Name of the service type.</param>
        [Event(InitializeServiceHostStartId, Level = EventLevel.Informational, Keywords = Keywords.vmAspects | Keywords.Wcf)]
        void InitializeServiceHostStart(
            string ServiceTypeName)
        {
            if (IsEnabled())
                WriteEvent(InitializeServiceHostStartId, ServiceTypeName);
        }

        /// <summary>
        /// Marks the end of the initialization of the service host.
        /// </summary>
        /// <param name="ServiceTypeName">Name of the service type.</param>
        [Event(InitializeServiceHostStopId, Level = EventLevel.Informational, Keywords = Keywords.vmAspects | Keywords.Wcf)]
        void InitializeServiceHostStop(
            string ServiceTypeName)
        {
            if (IsEnabled())
                WriteEvent(InitializeServiceHostStopId, ServiceTypeName);
        }

        /// <summary>
        /// Writes an event that CORS enabled for an endpoint.
        /// </summary>
        /// <param name="Address">The address.</param>
        /// <param name="Binding">The binding.</param>
        /// <param name="Contract">The contract.</param>
        /// <param name="Allowed">The allowed.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Contract")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Binding")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Allowed")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Address")]
        [Event(EnabledCorsForId, Level = EventLevel.Warning, Keywords = Keywords.vmAspects | Keywords.Wcf | Keywords.Cors, Message = "CORS enabled for {0}/{1}/{2} for URL-s: {3}")]
        public void EnabledCors(
            string Address,
            string Binding,
            string Contract,
            string Allowed)
        {
            if (IsEnabled())
                WriteEvent(EnabledCorsForId, Address, Binding, Contract, Allowed);
        }

        /// <summary>
        /// The CORS origin is not allowed.
        /// </summary>
        /// <param name="Origin">The origin.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Origin")]
        [Event(CorsOriginNotAllowedId, Level = EventLevel.Warning, Keywords = Keywords.vmAspects | Keywords.Wcf | Keywords.Cors, Message = "CORS origin {0} not allowed.")]
        public void CorsOriginNotAllowed(
            string Origin)
        {
            if (IsEnabled())
                WriteEvent(CorsOriginNotAllowedId, Origin);
        }

        /// <remarks/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Operation")]
        [Event(CorsOperationNotAllowedId, Level = EventLevel.Warning, Keywords = Keywords.vmAspects | Keywords.Wcf | Keywords.Cors, Message = "CORS operation {0} not allowed.")]
        public void CorsOperationNotAllowed(
           string Operation)
        {
            if (IsEnabled())
                WriteEvent(CorsOperationNotAllowedId, Operation);
        }
        #endregion
    }
}
