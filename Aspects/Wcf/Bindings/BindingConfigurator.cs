using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.MsmqIntegration;

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
        /// Gets the human readable messaging pattern identifier.
        /// </summary>
        public abstract string MessagingPattern { get; }

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
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public Binding Configure(
            Binding binding)
        {
            Contract.Requires<ArgumentNullException>(binding != null, nameof(binding));

            Func<BindingConfigurator, Binding, Binding> configure;

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
            Contract.Requires<ArgumentNullException>(binding != null, nameof(binding));

            if (string.IsNullOrWhiteSpace(binding.Name))
                binding.Name = $"{binding.GetType().Name}_{MessagingPattern}";

            // in DEBUG mode increase the timeouts, so that one can debug without timing out.
            binding.SendTimeout    = Constants.DefaultSendTimeout;
            binding.ReceiveTimeout = Constants.DefaultReceiveTimeout;

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
            Contract.Requires<ArgumentNullException>(binding != null, nameof(binding));

            throw new NotSupportedException(
                        $"A {binding.GetType().Name} binding is incompatible with messaging pattern {MessagingPattern}.");
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        public virtual Binding Configure(
            CustomBinding binding)
        {
            Contract.Requires<ArgumentNullException>(binding != null, nameof(binding));

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
            Contract.Requires<ArgumentNullException>(binding != null, nameof(binding));

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
            Contract.Requires<ArgumentNullException>(binding != null, nameof(binding));

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
            Contract.Requires<ArgumentNullException>(binding != null, nameof(binding));

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
            Contract.Requires<ArgumentNullException>(binding != null, nameof(binding));

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
            Contract.Requires<ArgumentNullException>(binding != null, nameof(binding));

            return ConfigureDefault(binding);
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        public virtual Binding Configure(
            WSHttpBinding binding)
        {
            Contract.Requires<ArgumentNullException>(binding != null, nameof(binding));

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
            Contract.Requires<ArgumentNullException>(binding != null, nameof(binding));

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
            Contract.Requires<ArgumentNullException>(binding != null, nameof(binding));

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
            Contract.Requires<ArgumentNullException>(binding != null, nameof(binding));

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
            Contract.Requires<ArgumentNullException>(binding != null, nameof(binding));

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
            Contract.Requires<ArgumentNullException>(binding != null, nameof(binding));

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
            Contract.Requires<ArgumentNullException>(binding != null, nameof(binding));

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
            Contract.Requires<ArgumentNullException>(binding != null, nameof(binding));

            return ConfigureDefault(binding);
        }
    }
}
