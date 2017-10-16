using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using vm.Aspects.Facilities;

namespace vm.Aspects.Wcf.Bindings
{
    /// <summary>
    /// Class RequestResponseConfigurator. Configures the bindings for request-response messaging pattern with Windows (Kerberos ор NTLM) security. 
    /// </summary>
    public class RequestResponseConfigurator : RequestResponseNoSecurityConfigurator
    {
        /// <summary>
        /// The pattern name
        /// </summary>
        public new const string PatternName = "RequestResponse";

        /// <summary>
        /// Gets the human readable messaging pattern identifier.
        /// </summary>
        public override string MessagingPattern => PatternName;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestResponseConfigurator"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public RequestResponseConfigurator(
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

            binding.Security = new BasicHttpSecurity
            {
                Mode      = BasicHttpSecurityMode.Transport,
                Transport = new HttpTransportSecurity
                {
                    ClientCredentialType = HttpClientCredentialType.Windows,
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
            BasicHttpsBinding binding)
        {
            base.Configure(binding);

            binding.Security = new BasicHttpsSecurity
            {
                Mode      = BasicHttpsSecurityMode.Transport,
                Transport = new HttpTransportSecurity
                {
                    ClientCredentialType = HttpClientCredentialType.Windows,
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
            NetHttpBinding binding)
        {
            base.Configure(binding);

            binding.Security = new BasicHttpSecurity
            {
                Mode      = BasicHttpSecurityMode.Transport,
                Transport = new HttpTransportSecurity
                {
                    ClientCredentialType = HttpClientCredentialType.Windows,
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
            NetHttpsBinding binding)
        {
            base.Configure(binding);

            binding.Security = new BasicHttpsSecurity
            {
                Mode      = BasicHttpsSecurityMode.Transport,
                Transport = new HttpTransportSecurity
                {
                    ClientCredentialType = HttpClientCredentialType.Windows,
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
            WebHttpBinding binding)
        {
            base.Configure(binding);

            binding.Security = new WebHttpSecurity
            {
                Mode      = WebHttpSecurityMode.Transport,
                Transport = new HttpTransportSecurity
                {
                    ClientCredentialType = HttpClientCredentialType.None,   // usually we use something like OpenID Connect here
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
                Mode      = SecurityMode.Transport,
                Transport = new HttpTransportSecurity
                {
                    ClientCredentialType = HttpClientCredentialType.Windows
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

            binding.Security = new NetNamedPipeSecurity
            {
                Mode      = NetNamedPipeSecurityMode.Transport,
                Transport = new NamedPipeTransportSecurity
                {
                    ProtectionLevel = ProtectionLevel.EncryptAndSign,
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
                Mode      = SecurityMode.Transport,
                Transport = new TcpTransportSecurity
                {
                    ProtectionLevel      = ProtectionLevel.EncryptAndSign,
                    ClientCredentialType = TcpClientCredentialType.Windows,
                },
            };

            return binding;
        }
    }
}
