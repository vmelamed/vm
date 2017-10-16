using System;

namespace vm.Aspects.Wcf
{
    /// <summary>
    /// Class Constants defines some useful constant values for use in bindings, etc.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The WCF streaming buffer size - 64KB
        /// </summary>
        public const int StreamingMaxBufferSize             = 64 * 1024;

        /// <summary>
        /// The WCF streaming maximum message size (roughly the max. size of the transferred file) - limit it to 4GB for now.
        /// </summary>
        public const long StreamingMaxMessageSize           = 4L * 1024 * 1024 * 1024;

        /// <summary>
        /// The WCF streaming maximum message size (roughly the max. size of the transferred file) for basic HTTP binding - limit it to 64MB for now.
        /// This binding uses MTOM to transfer file and buffers the entire file in memory before sending or receiving it. Therefore it has significantly
        /// smaller MaxMessageSize default.
        /// </summary>
        public const long StreamingBasicHttpMaxMessageSize  = 64 * 1024 * 1024;

        /// <summary>
        /// Increased size of the message from 64K to 1M
        /// </summary>
        public const int DefaultReceivedMessageSize         = 64 * 1024;

        /// <summary>
        /// Increased size of the message from 64K to 1M
        /// </summary>
        public const int MaxReceivedMessage                 = 1024 * 1024;

#if DEBUG
        /// <summary>
        /// The default send timeout - when debugging we need more time while stepping through the service's code - make the default 15 min (should be enough for debugging)
        /// or read it from the config file (see below).
        /// </summary>
        public static readonly TimeSpan DefaultSendTimeout = new TimeSpan(0, 15, 0);
        /// <summary>
        /// The default receive timeout - when debugging we need more time while stepping through the service's code - make the default 30 min (should be enough for debugging)
        /// or read it from the config file (see below).
        /// </summary>
        public static readonly TimeSpan DefaultReceiveTimeout = new TimeSpan(0, 30, 0);
        /// <summary>
        /// The default transaction timeout - when debugging we need more time while stepping through the service's code - make the default 30 min (should be enough for debugging)
        /// or read it from the config file (see below).
        /// </summary>
        public const string DefaultTransactionTimeout = "00:30:00";
#else
        /// <summary>
        /// The default send timeout - 1 min
        /// or read it from the config file (see below).
        /// </summary>
        public static readonly TimeSpan DefaultSendTimeout = new TimeSpan(0, 1, 0);
        /// <summary>
        /// The default receive timeout - 10 min
        /// or read it from the config file (see below).
        /// </summary>
        public static readonly TimeSpan DefaultReceiveTimeout = new TimeSpan(0, 10, 0);
        /// <summary>
        /// The default transaction timeout - 10 min
        /// or read it from the config file (see below).
        /// </summary>
        public const string DefaultTransactionTimeout = "00:10:00";
#endif

        /// <summary>
        /// The name of the application setting representing the default debug mode WCF send timeout.
        /// </summary>
        public const string SendTimeoutAppSettingName = "WcfSendTimeout";

        /// <summary>
        /// The name of the application setting representing the default debug mode WCF receive timeout.
        /// </summary>
        public const string ReceiveTimeoutAppSettingName = "WcfReceiveTimeout";

        /// <summary>
        /// The name of the application setting representing the default debug mode WCF transaction timeout.
        /// </summary>
        public const string TransactionTimeoutAppSettingName = "WcfTransactionTimeout";

        /// <summary>
        /// Specifies that the URI is accessed through the NetMsmq scheme used by Windows Communication Foundation (WCF). 
        /// </summary>
        public const string UriSchemeNetMsmq = "net.msmq";

        internal const string RestfulSchemeSuffix   = ".rest";
        internal const string BasicHttpSchemeSuffix = ".basic";

        #region CORS related constants. Needed internally only.
        internal const string Origin                      = "Origin";
        internal const string AccessControlAllowOrigin    = "Access-Control-Allow-Origin";
        internal const string AccessControlRequestMethod  = "Access-Control-Request-Method";
        internal const string AccessControlRequestHeaders = "Access-Control-Request-Headers";
        internal const string AccessControlAllowMethods   = "Access-Control-Allow-Methods";
        internal const string AccessControlAllowHeaders   = "Access-Control-Allow-Headers";
        internal const string AccessControlMaxAge         = "Access-Control-Max-Age";
        internal const string PreflightSuffix             = "_preflight_";
        #endregion
    }
}
