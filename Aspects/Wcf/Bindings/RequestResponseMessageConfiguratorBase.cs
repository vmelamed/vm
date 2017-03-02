using System.Diagnostics.CodeAnalysis;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace vm.Aspects.Wcf.Bindings
{
    /// <summary>
    /// Class RequestResponseMessageConfiguratorBase. Configures the bindings that do not support message security to throw exception.
    /// </summary>
    public abstract class RequestResponseMessageConfiguratorBase : RequestResponseNoSecurityConfigurator
    {
        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "ConfigureDefault will validate it.")]
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
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "ConfigureDefault will validate it.")]
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
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "ConfigureDefault will validate it.")]
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
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "ConfigureDefault will validate it.")]
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
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "ConfigureDefault will validate it.")]
        public override Binding Configure(
            NetNamedPipeBinding binding)
        {
            IncompatibleBinding(binding);
            return binding;
        }
    }
}
