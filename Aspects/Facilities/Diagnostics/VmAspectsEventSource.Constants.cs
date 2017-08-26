using System.Diagnostics.Tracing;

namespace vm.Aspects.Facilities.Diagnostics
{
    public sealed partial class VmAspectsEventSource
    {
        const int VerboseExceptionId        = 1;
        const int InformationalExceptionId  = 2;
        const int WarningExceptionId        = 3;
        const int ErrorExceptionId          = 4;
        const int CriticalExceptionId       = 5;
        const int AlwaysExceptionId         = 6;

        const int VerboseTraceId            = 11;
        const int InformationalTraceId      = 12;
        const int WarningTraceId            = 13;
        const int ErrorTraceId              = 14;
        const int CriticalTraceId           = 15;
        const int AlwaysTraceId             = 16;

        const int VerboseDumpId             = 21;
        const int InformationalDumpId       = 22;
        const int WarningDumpId             = 23;
        const int ErrorDumpId               = 24;
        const int CriticalDumpId            = 25;
        const int AlwaysDumpId              = 26;

        const int RegisteredId              = 30;
        const int RetryingId                = 31;
        const int RetryFailedId             = 32;
        const int CallHandlerFailsId        = 33;

        /// <summary>
        /// Lists the events keywords.
        /// </summary>
        public static class Keywords
        {
            /// <summary>
            /// The entry is from vm.Aspects.
            /// </summary>
            public const EventKeywords vmAspects = (EventKeywords)0x0040000000000L;
            /// <summary>
            /// The entry is an exception.
            /// </summary>
            public const EventKeywords Exception = (EventKeywords)(0x10000L << 0);
            /// <summary>
            /// The entry is a trace message.
            /// </summary>
            public const EventKeywords Trace     = (EventKeywords)(0x10000L << 1);
            /// <summary>
            /// The entry is associated with an object dump.
            /// </summary>
            public const EventKeywords Dump      = (EventKeywords)(0x10000L << 3);
            /// <summary>
            /// The entry is associated with the dependency injection container.
            /// </summary>
            public const EventKeywords DI        = (EventKeywords)(0x10000L << 4);
        }
    }
}
