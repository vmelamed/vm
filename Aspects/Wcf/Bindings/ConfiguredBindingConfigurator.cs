using System;
using System.ServiceModel.Channels;
using vm.Aspects.Facilities;

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
        public override string MessagingPattern => PatternName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfiguredBindingConfigurator"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public ConfiguredBindingConfigurator(
            Lazy<IConfigurationProvider> config)
            : base(config)
        {
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        public override Binding Configure(
            CustomBinding binding) => (CustomBinding)ConfigureDefault(binding);
    }
}
