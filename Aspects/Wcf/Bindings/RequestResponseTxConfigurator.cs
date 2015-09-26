using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.MsmqIntegration;
using System.Text;
using System.Threading.Tasks;

namespace vm.Aspects.Wcf.Bindings
{
    /// <summary>
    /// Class RequestResponseTxConfigurator. Configures the bindings for request-response messaging pattern with Windows (Kerberos) security and support for distributed transactions.
    /// </summary>
    public class RequestResponseTxConfigurator : RequestResponseConfigurator
    {
        /// <summary>
        /// The pattern name
        /// </summary>
        public new const string PatternName = "RequestResponseTx";

        /// <summary>
        /// Gets the human readable messaging pattern identifier.
        /// </summary>
        public override string MessagingPattern
        {
            get { return PatternName; }
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="0", Justification="Unity will dispose it.")]
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
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="0", Justification="ConfigureDefault will validate it.")]
        public override Binding Configure(
            BasicHttpsBinding binding)
        {
            IncompatibleBinding(binding);
            return binding;
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="0", Justification="ConfigureDefault will validate it.")]
        public override Binding Configure(
            NetHttpBinding binding)
        {
            IncompatibleBinding(binding);
            return binding;
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="0", Justification="ConfigureDefault will validate it.")]
        public override Binding Configure(
            NetHttpsBinding binding)
        {
            IncompatibleBinding(binding);
            return binding;
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="0", Justification="ConfigureDefault will validate it.")]
        public override Binding Configure(
            WebHttpBinding binding)
        {
            IncompatibleBinding(binding);
            return binding;
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="0", Justification="ConfigureDefault will validate it.")]
        public override Binding Configure(
            WSHttpBinding binding)
        {
            base.Configure(binding);

            binding.TransactionFlow    = true;
            binding.BypassProxyOnLocal = true;
            binding.ReliableSession    = new OptionalReliableSession
            {
                Enabled = true,
                Ordered = true,
            };

            return binding;
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="0", Justification="ConfigureDefault will validate it.")]
        public override Binding Configure(
            NetNamedPipeBinding binding)
        {
            base.Configure(binding);

            binding.TransactionFlow = true;

            return binding;
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="0", Justification="ConfigureDefault will validate it.")]
        public override Binding Configure(
            NetTcpBinding binding)
        {
            base.Configure(binding);

            binding.TransactionFlow = true;
            binding.ReliableSession = new OptionalReliableSession
            {
                Enabled = true,
                Ordered = true,
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
