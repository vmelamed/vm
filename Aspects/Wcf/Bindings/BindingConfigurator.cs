using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.MsmqIntegration;
using System.Threading;
using vm.Aspects.Facilities;
using vm.Aspects.Threading;

namespace vm.Aspects.Wcf.Bindings
{
    /// <summary>
    /// Class BindingConfigurator.
    /// Configures the passed bindings according to the communication pattern specific for the implementing instance.
    /// </summary>
    public abstract partial class BindingConfigurator
    {
        /// <summary>
        /// The default send timeout as defined in WCF.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification="It is read-only.")]
        protected readonly TimeSpan DefaultSendTimeout = new TimeSpan(0, 1, 0);

        /// <summary>
        /// The default receive timeout as defined in WCF.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification="It is read-only.")]
        protected readonly TimeSpan DefaultReceiveTimeout = new TimeSpan(0, 10, 0);

        /// <summary>
        /// The configuration provider to read the timeouts from.
        /// </summary>
        readonly Lazy<IConfigurationProvider> _config;

        /// <summary>
        /// Gets the app/web configuration.
        /// </summary>
        public IConfigurationProvider Config => _config.Value;

        static ReaderWriterLockSlim _syncConfigurators = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        static IDictionary<Type, Func<BindingConfigurator, Binding, Binding>> _configurators = new Dictionary<Type, Func<BindingConfigurator, Binding, Binding>>
        {
            [typeof(BasicHttpBinding)]              = (c,b) => c.Configure((BasicHttpBinding)b),
            [typeof(BasicHttpsBinding)]             = (c,b) => c.Configure((BasicHttpsBinding)b),
            [typeof(CustomBinding)]                 = (c,b) => c.Configure((CustomBinding)b),
            [typeof(MsmqIntegrationBinding)]        = (c,b) => c.Configure((MsmqIntegrationBinding)b),
            [typeof(NetHttpBinding)]                = (c,b) => c.Configure((NetHttpBinding)b),
            [typeof(NetHttpsBinding)]               = (c,b) => c.Configure((NetHttpsBinding)b),
            [typeof(NetMsmqBinding)]                = (c,b) => c.Configure((NetMsmqBinding)b),
            [typeof(NetNamedPipeBinding)]           = (c,b) => c.Configure((NetNamedPipeBinding)b),
            [typeof(NetTcpBinding)]                 = (c,b) => c.Configure((NetTcpBinding)b),
            [typeof(WebHttpBinding)]                = (c,b) => c.Configure((WebHttpBinding)b),
            [typeof(WS2007FederationHttpBinding)]   = (c,b) => c.Configure((WS2007FederationHttpBinding)b),
            [typeof(WSDualHttpBinding)]             = (c,b) => c.Configure((WSDualHttpBinding)b),
            [typeof(WSFederationHttpBinding)]       = (c,b) => c.Configure((WSFederationHttpBinding)b),
            [typeof(WSHttpBinding)]                 = (c,b) => c.Configure((WSHttpBinding)b),
        };

        /// <summary>
        /// The synchronization object for the statically cached timeouts
        /// </summary>
        static object _syncTimeouts = new object();

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
        /// Gets the human readable messaging pattern identifier.
        /// </summary>
        public abstract string MessagingPattern { get; }

        /// <summary>
        /// Gets the default WCF send timeout. In debug mode by default the value is "00:15:00" - 15 min and in release mode is 1 min.
        /// The value can be overridden in the configuration file by specifying app. setting "WcfSendTimeout", e.g.
        /// <![CDATA[<add key="WcfSendTimeout" value="00:20:00">]]>. The format of the value is the same as for the 
        /// method <see cref="M:System.TimeSpan.TryParse(string, TimeSpan)"/>.
        /// </summary>
        public TimeSpan SendTimeout
        {
            get
            {
                if (!_defaultSendTimeout.HasValue)
                {
                    var timeoutString = Config[Constants.SendTimeoutAppSettingName];
                    TimeSpan tmo;

                    lock (_syncTimeouts)
                        if (!timeoutString.IsNullOrEmpty() &&
                            TimeSpan.TryParse(timeoutString, out tmo))
                            _defaultSendTimeout = tmo;
                        else
                            _defaultSendTimeout = Constants.DefaultSendTimeout;
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
        public TimeSpan ReceiveTimeout
        {
            get
            {
                if (!_defaultReceiveTimeout.HasValue)
                {
                    var timeoutString = Config[Constants.ReceiveTimeoutAppSettingName];
                    TimeSpan tmo;

                    lock (_syncTimeouts)
                        if (!timeoutString.IsNullOrEmpty() &&
                        TimeSpan.TryParse(timeoutString, out tmo))
                            _defaultReceiveTimeout = tmo;
                        else
                            _defaultReceiveTimeout = Constants.DefaultReceiveTimeout;
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
        public string TransactionTimeout
        {
            get
            {
                if (_defaultTransactionTimeout == null)
                {
                    var timeoutString = Config[Constants.TransactionTimeoutAppSettingName];
                    TimeSpan tmo;

                    lock (_syncTimeouts)
                        if (!timeoutString.IsNullOrEmpty() &&
                            TimeSpan.TryParse(timeoutString, out tmo) &&
                            tmo >= new TimeSpan(0, 0, 30) &&  // make sure the transaction timeout is reasonable: between 30s and 1hr
                            tmo <= new TimeSpan(1, 0, 0))
                            _defaultTransactionTimeout = tmo.ToString();
                        else
                            _defaultTransactionTimeout = Constants.DefaultTransactionTimeout;
                }

                return _defaultTransactionTimeout;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingConfigurator"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        protected BindingConfigurator(
            Lazy<IConfigurationProvider> config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            _config = config;
        }

        /// <summary>
        /// Adds a configurator for binding.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void AddConfigurator<T>(Func<BindingConfigurator, Binding, Binding> configure)
            where T : Binding
        {
            using (_syncConfigurators.WriterLock())
                _configurators[typeof(T)] = configure;
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>The configured binding.</returns>
        public Binding Configure(
            Binding binding)
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            Func<BindingConfigurator, Binding, Binding> configure;

            using (_syncConfigurators.ReaderLock())
                if (!_configurators.TryGetValue(binding.GetType(), out configure))
                    throw new NotSupportedException(
                                $"Configuration of {binding.GetType().Name} binding is not supported.");

            return configure(this, binding);
        }

        /// <summary>
        /// Provides the default configuration of <paramref name="binding"/>.
        /// Here it only gives the binding a name in the format &lt;binding type name&gt;_&lt;messaging pattern&gt; and
        /// increases the timeouts in debug mode.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>The binding.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="binding"/> is <see langword="null"/>.
        /// </exception>
        protected virtual Binding ConfigureDefault(
            Binding binding)
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            if (string.IsNullOrWhiteSpace(binding.Name))
                binding.Name = $"{binding.GetType().Name}_{MessagingPattern}";

            // in DEBUG mode increase the timeouts, so that one can debug without timing out.
            binding.SendTimeout    = SendTimeout;
            binding.ReceiveTimeout = ReceiveTimeout;

            // the default implementation is to not change the binding
            return binding;
        }

        /// <summary>
        /// Throws exception because the binding is not compatible with the current messaging pattern.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="binding"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="System.NotSupportedException">Always thrown.</exception>
        protected virtual void IncompatibleBinding(
            Binding binding)
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            throw new NotSupportedException(
                        $"A {binding.GetType().Name} binding is incompatible with messaging pattern {MessagingPattern}.");
        }

        /// <summary>
        /// Throws exception because the binding is not compatible with the current messaging pattern.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="binding"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="System.NotSupportedException">Always thrown.</exception>
        protected virtual void NotImplementedBinding(
            Binding binding)
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            throw new NotImplementedException(
                        $"A {binding.GetType().Name} binding is not implemented yet for messaging pattern {MessagingPattern}.");
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        public virtual Binding Configure(
            CustomBinding binding)
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            IncompatibleBinding(binding);
            return binding;
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        public virtual Binding Configure(
            BasicHttpBinding binding)
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            return ConfigureDefault(binding);
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        public virtual Binding Configure(
            BasicHttpsBinding binding)
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            return ConfigureDefault(binding);
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        public virtual Binding Configure(
            NetHttpBinding binding)
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            return ConfigureDefault(binding);
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        public virtual Binding Configure(
            NetHttpsBinding binding)
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            return ConfigureDefault(binding);
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        public virtual Binding Configure(
            WebHttpBinding binding)
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            binding = ConfigureDefault(binding) as WebHttpBinding;
            binding.ContentTypeMapper = new WebContentTypeMapperDefaultJson();

            return binding;
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        public virtual Binding Configure(
            WSHttpBinding binding)
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            return ConfigureDefault(binding);
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        public virtual Binding Configure(
            WSFederationHttpBinding binding)
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            return ConfigureDefault(binding);
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        public virtual Binding Configure(
            WS2007FederationHttpBinding binding)
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            return ConfigureDefault(binding);
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        public virtual Binding Configure(
            WSDualHttpBinding binding)
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            return ConfigureDefault(binding);
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        public virtual Binding Configure(
            NetNamedPipeBinding binding)
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            return ConfigureDefault(binding);
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        public virtual Binding Configure(
            NetTcpBinding binding)
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            return ConfigureDefault(binding);
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        public virtual Binding Configure(
            NetMsmqBinding binding)
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            return ConfigureDefault(binding);
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        public virtual Binding Configure(
            MsmqIntegrationBinding binding)
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            return ConfigureDefault(binding);
        }
    }
}
