using System.Diagnostics.CodeAnalysis;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.MsmqIntegration;

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
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "ConfigureDefault will validate it.")]
        public override Binding Configure(BasicHttpBinding binding)
        {
            base.Configure(binding);

            if (binding.MaxReceivedMessageSize == Constants.DefaultReceivedMessageSize)
                binding.MaxReceivedMessageSize = Constants.MaxReceivedMessage;

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
            NetHttpsBinding binding)
        {
            base.Configure(binding);

            if (binding.MaxReceivedMessageSize == Constants.DefaultReceivedMessageSize)
                binding.MaxReceivedMessageSize = Constants.MaxReceivedMessage;

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
            WebHttpBinding binding)
        {
            base.Configure(binding);

            if (binding.MaxReceivedMessageSize == Constants.DefaultReceivedMessageSize)
                binding.MaxReceivedMessageSize = Constants.MaxReceivedMessage;

            binding.ContentTypeMapper  = new WebContentTypeMapperDefaultJson();
            binding.BypassProxyOnLocal = true;

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

            binding.TransactionFlow    = false;
            binding.ReliableSession    = new OptionalReliableSession
            {
                Enabled = false,
                Ordered = false,
            };
            binding.Security           = new WSHttpSecurity
            {
                Mode        = SecurityMode.Transport,
                Transport   = new HttpTransportSecurity
                {
                    ClientCredentialType = HttpClientCredentialType.None,
                },
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

            binding.TransactionFlow = false;
            binding.Security        = new NetNamedPipeSecurity
            {
                Mode      = NetNamedPipeSecurityMode.None,
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
            NetTcpBinding binding)
        {
            base.Configure(binding);

            if (binding.MaxReceivedMessageSize == Constants.DefaultReceivedMessageSize)
                binding.MaxReceivedMessageSize = Constants.MaxReceivedMessage;

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
