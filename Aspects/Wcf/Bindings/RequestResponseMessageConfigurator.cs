using System;
using System.Diagnostics.CodeAnalysis;
using System.ServiceModel;
using System.ServiceModel.Channels;
using vm.Aspects.Facilities;

namespace vm.Aspects.Wcf.Bindings
{
    /// <summary>
    /// Class RequestResponseMessageConfigurator. Configures the bindings for request-response messaging pattern with message transmit security no client authentication (anonymous client). 
    /// </summary>
    public class RequestResponseMessageConfigurator : RequestResponseMessageConfiguratorBase
    {
        /// <summary>
        /// The pattern name
        /// </summary>
        public new const string PatternName = "RequestResponseMessage";

        /// <summary>
        /// Gets the human readable messaging pattern identifier.
        /// </summary>
        public override string MessagingPattern => PatternName;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestResponseMessageConfigurator"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public RequestResponseMessageConfigurator(
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
            IncompatibleBinding(binding);
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

            binding.Security = new WSHttpSecurity
            {
                Mode    = SecurityMode.Message,
                Message = new NonDualMessageSecurityOverHttp
                {
                    ClientCredentialType = MessageCredentialType.None,
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
            NetTcpBinding binding)
        {
            base.Configure(binding);

            binding.Security = new NetTcpSecurity
            {
                Mode    = SecurityMode.Message,
                Message = new MessageSecurityOverTcp
                {
                    ClientCredentialType = MessageCredentialType.None,
                },
            };

            return binding;
        }
    }
}
