using System;
using System.Configuration;
using System.Diagnostics.Contracts;

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
        static readonly TimeSpan defaultSendTimeout = new TimeSpan(0, 15, 0);
        /// <summary>
        /// The default receive timeout - when debugging we need more time while stepping through the service's code - make the default 30 min (should be enough for debugging)
        /// or read it from the config file (see below).
        /// </summary>
        static readonly TimeSpan defaultReceiveTimeout = new TimeSpan(0, 30, 0);
        /// <summary>
        /// The default transaction timeout - when debugging we need more time while stepping through the service's code - make the default 30 min (should be enough for debugging)
        /// or read it from the config file (see below).
        /// </summary>
        const string defaultTransactionTimeout = "00:30:00";
#else
        /// <summary>
        /// The default send timeout - 1 min
        /// or read it from the config file (see below).
        /// </summary>
        static readonly TimeSpan defaultSendTimeout = new TimeSpan(0, 1, 0);
        /// <summary>
        /// The default receive timeout - 10 min
        /// or read it from the config file (see below).
        /// </summary>
        static readonly TimeSpan defaultReceiveTimeout = new TimeSpan(0, 10, 0);
        /// <summary>
        /// The default transaction timeout - 10 min
        /// or read it from the config file (see below).
        /// </summary>
        static readonly string defaultTransactionTimeout = "00:10:00";
#endif

        /// <summary>
        /// The cached WCF debug send timeout
        /// </summary>
        static TimeSpan? _defaultSendTimeout;

        /// <summary>
        /// The cached WCF debug receive timeout
        /// </summary>
        static TimeSpan? _defaultReceiveTimeout;

        /// <summary>
        /// The cached WCF debug transaction timeout
        /// </summary>
        static string _defaultTransactionTimeout;

        /// <summary>
        /// The name of the application setting representing the default debug mode WCF send timeout.
        /// </summary>
        const string SendTimeoutAppSettingName = "WcfSendTimeout";

        /// <summary>
        /// The name of the application setting representing the default debug mode WCF receive timeout.
        /// </summary>
        const string ReceiveTimeoutAppSettingName = "WcfReceiveTimeout";

        /// <summary>
        /// The name of the application setting representing the default debug mode WCF transaction timeout.
        /// </summary>
        const string TransactionTimeoutAppSettingName = "WcfTransactionTimeout";

        /// <summary>
        /// Gets the default WCF send timeout. In debug mode by default the value is "00:15:00" - 15 min and in release mode is 1 min.
        /// The value can be overridden in the configuration file by specifying app. setting "WcfSendTimeout", e.g.
        /// <![CDATA[<add key="WcfSendTimeout" value="00:20:00">]]>. The format of the value is the same as for the 
        /// method <see cref="M:System.TimeSpan.TryParse(string, TimeSpan)"/>.
        /// </summary>
        public static TimeSpan DefaultSendTimeout
        {
            get
            {
                if (!_defaultSendTimeout.HasValue)
                {
                    var timeoutString = ConfigurationManager.AppSettings[SendTimeoutAppSettingName];
                    TimeSpan tmo;

                    if (!string.IsNullOrEmpty(timeoutString) &&
                        TimeSpan.TryParse(timeoutString, out tmo))
                        _defaultSendTimeout = tmo;
                    else
                        _defaultSendTimeout = defaultSendTimeout;
                }

                return _defaultSendTimeout.Value;
            }
        }

        /// <summary>
        /// Gets the default WCF receive timeout. In debug mode by default the value is "00:30:00" - 30 min and in release mode is 10 min.
        /// The value can be overridden in the configuration file by specifying app. setting "WcfReceiveTimeout", e.g.
        /// <![CDATA[<add key="WcfReceiveTimeout" value="00:20:00">]]>. The format of the value is the same as for the 
        /// method <see cref="M:System.TimeSpan.TryParse"/>.
        /// </summary>
        public static TimeSpan DefaultReceiveTimeout
        {
            get
            {
                if (!_defaultReceiveTimeout.HasValue)
                {
                    var timeoutString = ConfigurationManager.AppSettings[ReceiveTimeoutAppSettingName];
                    TimeSpan tmo;

                    if (!string.IsNullOrEmpty(timeoutString) &&
                        TimeSpan.TryParse(timeoutString, out tmo))
                        _defaultReceiveTimeout = tmo;
                    else
                        _defaultReceiveTimeout = defaultReceiveTimeout;
                }

                return _defaultReceiveTimeout.Value;
            }
        }

        /// <summary>
        /// Gets the default WCF transaction timeout. In debug mode by default the value is "00:30:00" - 30 min and in release mode is 10 min.
        /// The value can be overridden in the configuration file by specifying app. setting "WcfTransactionTimeout", e.g.
        /// <![CDATA[<add key="WcfTransactionTimeout" value="00:20:00">]]>. The format of the value is the same as for the 
        /// method <see cref="M:System.TimeSpan.TryParse"/>.
        /// </summary>
        public static string DefaultTransactionTimeout
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

                if (_defaultTransactionTimeout == null)
                {
                    var timeoutString = ConfigurationManager.AppSettings[TransactionTimeoutAppSettingName];
                    TimeSpan tmo;

                    if (!string.IsNullOrEmpty(timeoutString) &&
                        TimeSpan.TryParse(timeoutString, out tmo) &&
                        tmo >= new TimeSpan(0, 0, 30) &&  // make sure the transaction timeout is reasonable: between 30s and 1hr
                        tmo <= new TimeSpan(1, 0, 0))
                        _defaultTransactionTimeout = tmo.ToString();
                    else
                        _defaultTransactionTimeout = defaultTransactionTimeout;
                }

                return _defaultTransactionTimeout;
            }
        }

        #region CORS related constants. Needed internally only.
        internal const string Origin                      = "Origin";
        internal const string AccessControlAllowOrigin    = "Access-Control-Allow-Origin";
        internal const string AccessControlRequestMethod  = "Access-Control-Request-Method";
        internal const string AccessControlRequestHeaders = "Access-Control-Request-Headers";
        internal const string AccessControlAllowMethods   = "Access-Control-Allow-Methods";
        internal const string AccessControlAllowHeaders   = "Access-Control-Allow-Headers";
        internal const string PreflightSuffix             = "_preflight_";
        #endregion
    }
}
