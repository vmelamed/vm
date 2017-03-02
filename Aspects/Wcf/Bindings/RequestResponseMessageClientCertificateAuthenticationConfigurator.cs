using System.Diagnostics.CodeAnalysis;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace vm.Aspects.Wcf.Bindings
{
    /// <summary>
    /// Class RequestResponseMessageClientWindowsAuthenticationConfigurator. Configures the bindings for request-response messaging pattern with message transmit security and Windows client authentication. 
    /// </summary>
    public class RequestResponseMessageClientCertificateAuthenticationConfigurator : RequestResponseMessageConfiguratorBase
    {
        /// <summary>
        /// The pattern name
        /// </summary>
        public new const string PatternName = "RequestResponseMessageClientCertificateAuthentication";

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
        public override Binding Configure(
            BasicHttpBinding binding)
        {
            base.Configure(binding);

            binding.Security = new BasicHttpSecurity
            {
                Mode    = BasicHttpSecurityMode.Message,
                Message = new BasicHttpMessageSecurity
                {
                    ClientCredentialType = BasicHttpMessageCredentialType.Certificate
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
            WSHttpBinding binding)
        {
            base.Configure(binding);

            binding.Security = new WSHttpSecurity
            {
                Mode    = SecurityMode.Message,
                Message = new NonDualMessageSecurityOverHttp
                {
                    ClientCredentialType = MessageCredentialType.Certificate,
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
                    ClientCredentialType = MessageCredentialType.Certificate,
                },
            };

            return binding;
        }
    }
}
