using System;
using System.Diagnostics.CodeAnalysis;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.MsmqIntegration;
using vm.Aspects.Facilities;

namespace vm.Aspects.Wcf.Bindings
{
    /// <summary>
    /// Class RequestResponseNoSecurityConfigurator. Configures the bindings for request-response messaging pattern with no security.
    /// </summary>
    public class RequestResponseNoSecurityConfigurator : BindingConfigurator
    {
        /// <summary>
        /// The pattern name
        /// </summary>
        public const string PatternName = "RequestResponseNoSecurity";

        /// <summary>
        /// Gets the human readable messaging pattern identifier.
        /// </summary>
        public override string MessagingPattern => PatternName;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestResponseNoSecurityConfigurator"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public RequestResponseNoSecurityConfigurator(
            Lazy<IConfigurationProvider> config)
            : base(config)
        {
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "ConfigureDefault will validate it.")]
        public override Binding Configure(
            BasicHttpBinding binding)
        {
            base.Configure(binding);

            if (binding.MaxReceivedMessageSize == Constants.DefaultReceivedMessageSize)
                binding.MaxReceivedMessageSize = Constants.MaxReceivedMessage;

            binding.Security = new BasicHttpSecurity
            {
                Mode = BasicHttpSecurityMode.None,
            };

            return binding;
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "ConfigureDefault will validate it.")]
        public override Binding Configure(
            BasicHttpsBinding binding)
        {
            base.Configure(binding);

            if (binding.MaxReceivedMessageSize == Constants.DefaultReceivedMessageSize)
                binding.MaxReceivedMessageSize = Constants.MaxReceivedMessage;

            binding.Security = new BasicHttpsSecurity
            {
                Mode      = BasicHttpsSecurityMode.Transport,
                Transport = new HttpTransportSecurity
                {
                    ClientCredentialType = HttpClientCredentialType.None,
                }
            };

            return binding;
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "ConfigureDefault will validate it.")]
        public override Binding Configure(
            NetHttpBinding binding)
        {
            base.Configure(binding);

            if (binding.MaxReceivedMessageSize == Constants.DefaultReceivedMessageSize)
                binding.MaxReceivedMessageSize = Constants.MaxReceivedMessage;

            binding.Security = new BasicHttpSecurity
            {
                Mode = BasicHttpSecurityMode.None,
            };
            binding.ReliableSession = new OptionalReliableSession
            {
                Enabled = false,
                Ordered = false,
            };

            return binding;
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "ConfigureDefault will validate it.")]
        public override Binding Configure(
            NetHttpsBinding binding)
        {
            base.Configure(binding);

            if (binding.MaxReceivedMessageSize == Constants.DefaultReceivedMessageSize)
                binding.MaxReceivedMessageSize = Constants.MaxReceivedMessage;

            binding.Security = new BasicHttpsSecurity
            {
                Mode      = BasicHttpsSecurityMode.Transport,
                Transport = new HttpTransportSecurity
                {
                    ClientCredentialType = HttpClientCredentialType.None,
                },
            };
            binding.ReliableSession = new OptionalReliableSession
            {
                Enabled = false,
                Ordered = false,
            };

            return binding;
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "ConfigureDefault will validate it.")]
        public override Binding Configure(
            WebHttpBinding binding)
        {
            base.Configure(binding);

            if (binding.MaxReceivedMessageSize == Constants.DefaultReceivedMessageSize)
                binding.MaxReceivedMessageSize = Constants.MaxReceivedMessage;

            binding.BypassProxyOnLocal = true;
            binding.Security           = new WebHttpSecurity
            {
                Mode = WebHttpSecurityMode.None,
            };

            return binding;
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "ConfigureDefault will validate it.")]
        public override Binding Configure(
            WSHttpBinding binding)
        {
            base.Configure(binding);

            if (binding.MaxReceivedMessageSize == Constants.DefaultReceivedMessageSize)
                binding.MaxReceivedMessageSize = Constants.MaxReceivedMessage;

            binding.Security           = new WSHttpSecurity
            {
                Mode = SecurityMode.None,
            };
            binding.TransactionFlow    = false;
            binding.ReliableSession    = new OptionalReliableSession
            {
                Enabled = false,
                Ordered = false,
            };

            return binding;
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "ConfigureDefault will validate it.")]
        public override Binding Configure(
            NetNamedPipeBinding binding)
        {
            base.Configure(binding);

            if (binding.MaxReceivedMessageSize == Constants.DefaultReceivedMessageSize)
                binding.MaxReceivedMessageSize = Constants.MaxReceivedMessage;

            binding.Security = new NetNamedPipeSecurity
            {
                Mode = NetNamedPipeSecurityMode.None,
            };
            binding.TransactionFlow = false;

            return binding;
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "ConfigureDefault will validate it.")]
        public override Binding Configure(
            NetTcpBinding binding)
        {
            base.Configure(binding);

            if (binding.MaxReceivedMessageSize == Constants.DefaultReceivedMessageSize)
                binding.MaxReceivedMessageSize = Constants.MaxReceivedMessage;

            binding.Security        = new NetTcpSecurity
            {
                Mode = SecurityMode.None,
            };
            binding.TransactionFlow = false;
            binding.ReliableSession = new OptionalReliableSession
            {
                Enabled = false,
                Ordered = false,
            };

            return binding;
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        public override Binding Configure(
            NetMsmqBinding binding)
        {
            IncompatibleBinding(binding);
            return binding;
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        public override Binding Configure(
            MsmqIntegrationBinding binding)
        {
            IncompatibleBinding(binding);
            return binding;
        }
    }
}
