using System.Diagnostics.Tracing;

namespace vm.Aspects.Facilities.Diagnostics
{
    public sealed partial class VmAspectsEventSource
    {
        const int VerboseExceptionId                                = 1;
        const int InformationalExceptionId                          = 2;
        const int WarningExceptionId                                = 3;
        const int ErrorExceptionId                                  = 4;
        const int CriticalExceptionId                               = 5;
        const int AlwaysExceptionId                                 = 6;

        const int VerboseTraceId                                    = 11;
        const int InformationalTraceId                              = 12;
        const int WarningTraceId                                    = 13;
        const int ErrorTraceId                                      = 14;
        const int CriticalTraceId                                   = 15;
        const int AlwaysTraceId                                     = 16;

        const int VerboseDumpId                                     = 21;
        const int InformationalDumpId                               = 22;
        const int WarningDumpId                                     = 23;
        const int ErrorDumpId                                       = 24;
        const int CriticalDumpId                                    = 25;
        const int AlwaysDumpId                                      = 26;

        const int RetryStartId                                      = 31;
        const int RetryStopId                                       = 32;
        const int RetryingId                                        = 33;
        const int RetryFailedId                                     = 34;

        const int EFRepositoryInitializationStartId                 = 35;
        const int EFRepositoryInitializationStopId                  = 36;
        const int EFRepositoryInitializationFailedId                = 37;

        const int EFRepositoryCommitChangesId                       = 38;

        const int EFRepositoryMappingViewsCacheId                   = 39;

        const int UnitOfWorkStartId                                 = 40;
        const int UnitOfWorkStopId                                  = 41;
        const int UnitOfWorkFailedId                                = 42;

        const int RegistrarRegisteredId                             = 50;

        const int CallHandlerFailsId                                = 51;

        const int ServiceHostFactoryRegisteredDefaultsId            = 52;
        const int ServiceHostFactoryCreatedServiceHostId            = 53;
        const int InitializeServiceHostStartId                      = 54;
        const int InitializeServiceHostStopId                       = 55;

        const int EnabledCorsForId                                  = 60;
        const int CorsOperationNotAllowedId                         = 61;
        const int CorsOriginNotAllowedId                            = 62;

        /// <summary>
        /// Lists the events keywords.
        /// </summary>
        public static class Keywords
        {
            /// <summary>
            /// The entry is from vm.Aspects.
            /// </summary>
            public const EventKeywords vmAspects    = (EventKeywords)0x0040000000000L;
            /// <summary>
            /// The entry is an exception.
            /// </summary>
            public const EventKeywords Exception    = (EventKeywords)(0x10000L << 0);
            /// <summary>
            /// The entry is a trace message.
            /// </summary>
            public const EventKeywords Trace        = (EventKeywords)(0x10000L << 1);
            /// <summary>
            /// The entry is associated with an object dump.
            /// </summary>
            public const EventKeywords Dump         = (EventKeywords)(0x10000L << 2);
            /// <summary>
            /// The entry is associated with the dependency injection container.
            /// </summary>
            public const EventKeywords DI           = (EventKeywords)(0x10000L << 3);
            /// <summary>
            /// The entry is associated with unit of work.
            /// </summary>
            public const EventKeywords Uow          = (EventKeywords)(0x10000L << 4);
            /// <summary>
            /// The entry is associated with WCF services/host/factories.
            /// </summary>
            public const EventKeywords Wcf          = (EventKeywords)(0x10000L << 5);
            /// <summary>
            /// The entry is associated with aspect (call handler).
            /// </summary>
            public const EventKeywords Aspect       = (EventKeywords)(0x10000L << 6);
            /// <summary>
            /// The entry is associated with aspect (call handler).
            /// </summary>
            public const EventKeywords EFRepository = (EventKeywords)(0x10000L << 7);
            /// <summary>
            /// The entry is associated with aspect (call handler).
            /// </summary>
            public const EventKeywords Retry        = (EventKeywords)(0x10000L << 8);
            /// <summary>
            /// The entry is associated with aspect (call handler).
            /// </summary>
            public const EventKeywords Cors        = (EventKeywords)(0x10000L << 9);
        }
    }
}
