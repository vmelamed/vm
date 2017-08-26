using System.Diagnostics.Tracing;
using vm.Aspects.Facilities.Diagnostics;

namespace vm.Aspects.Facilities.LogWriters.Etw
{
    public sealed partial class EtwLogEntryEventSource
    {
        const int VerboseMessageId                  = 100;
        const int InformationalMessageId            = 101;
        const int WarningMessageId                  = 102;
        const int ErrorMessageId                    = 103;
        const int CriticalMessageId                 = 104;

        const int VerboseTraceId                    = 110;
        const int InformationalTraceId              = 111;
        const int WarningTraceId                    = 112;
        const int ErrorTraceId                      = 113;
        const int CriticalTraceId                   = 114;

        const int VerboseLogEntryId                 = 120;
        const int InformationalLogEntryId           = 121;
        const int WarningLogEntryId                 = 122;
        const int ErrorLogEntryId                   = 123;
        const int CriticalLogEntryId                = 124;

        const int VerboseDumpObjectId               = 130;
        const int InformationalDumpObjectId         = 131;
        const int WarningDumpObjectId               = 132;
        const int ErrorDumpObjectId                 = 133;
        const int CriticalDumpObjectId              = 134;

        /// <summary>
        /// Lists the events keywords.
        /// </summary>
        public static class Keywords
        {
            /// <summary>
            /// The entry is from vm.Aspects.
            /// </summary>
            public const EventKeywords vmAspects = VmAspectsEventSource.Keywords.vmAspects;
            /// <summary>
            /// The entry is an Enterprise Library Logging Application Block <see cref="LogEntry"/>.
            /// </summary>
            public const EventKeywords LogEntry  = (EventKeywords)(0x10000L << 4);
            /// <summary>
            /// The entry is an Enterprise Library Logging Application Block.
            /// </summary>
            public const EventKeywords ELLab     = (EventKeywords)(0x10000L << 5);
            /// <summary>
            /// The entry is an exception.
            /// </summary>
            public const EventKeywords Exception = (EventKeywords)(0x10000L << 0);
            /// <summary>
            /// The entry is associated with a message.
            /// </summary>
            public const EventKeywords Message   = (EventKeywords)(0x10000L << 1);
            /// <summary>
            /// The entry is associated with a message.
            /// </summary>
            public const EventKeywords Dump      = (EventKeywords)(0x10000L << 2);
            /// <summary>
            /// The entry is a trace.
            /// </summary>
            public const EventKeywords Trace     = (EventKeywords)(0x10000L << 3);
        }
    }
}
