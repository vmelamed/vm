using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace vm.Aspects.Wcf.Bindings
{
    /// <summary>
    /// Class ConfiguredBindingConfigurator does not modify the binding, assuming that it is already configured, e.g. from the configuration file.
    /// </summary>
    public class ConfiguredBindingConfigurator : BindingConfigurator
    {
        /// <summary>
        /// The pattern name
        /// </summary>
        public const string PatternName = "";

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
        public override Binding Configure(
            CustomBinding binding)
        {
            return (CustomBinding)ConfigureDefault(binding);
        }
    }
}
