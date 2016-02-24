using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace vm.Aspects.Wcf.Bindings
{
    /// <summary>
    /// Class FireAndForgetConfigurator. Configures bindings for fire-and-forget (events) messaging pattern.
    /// </summary>
    public class FireAndForgetNoSecurityConfigurator : RequestResponseNoSecurityConfigurator
    {
        /// <summary>
        /// The pattern name
        /// </summary>
        public new const string PatternName = "FireAndForgetNoSecurity";

        /// <summary>
        /// Gets the human readable messaging pattern identifier.
        /// </summary>
        public override string MessagingPattern => PatternName;

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="0", Justification="IncompatibleBinding will validate it.")]
        public override Binding Configure(
            NetHttpBinding binding)
        {
            // inherently session-full binding
            IncompatibleBinding(binding);
            return binding;
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="0", Justification="IncompatibleBinding will validate it.")]
        public override Binding Configure(
            NetHttpsBinding binding)
        {
            // inherently session-full binding
            IncompatibleBinding(binding);
            return binding;
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        public override Binding Configure(
            NetNamedPipeBinding binding)
        {
            // inherently session-full binding
            Debug.WriteLine("+++ Note that the NetNamedPipeBinding is inherently session-full and if the server is blocked, "+
                            "the client may block as well until the server dispatch queue can accommodate the new request. +++");
            return base.Configure(binding);
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        public override Binding Configure(
            NetTcpBinding binding)
        {
            Debug.WriteLine("+++ Note that the NetTcpBinding is inherently session-full and if the server is blocked, "+
                            "the client may block as well until the server dispatch queue can accommodate the new request. +++");
            return base.Configure(binding);
        }

        /// <summary>
        /// Configures the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>Binding.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId="0", Justification="ConfigureDefault will validate it.")]
        public override Binding Configure(
            NetMsmqBinding binding)
        {
            base.Configure(binding);

            binding.ExactlyOnce             = true;                            // The queue must be durable
            binding.Durable                 = true;
            binding.TimeToLive              = new TimeSpan(120, 0, 0);         // the message must be pulled within 5 days...
            binding.DeadLetterQueue         = DeadLetterQueue.Custom;
            // TODO: binding.CustomDeadLetterQueue  = new Uri(dlqAddress);          // client side: configure the queues
            binding.ReceiveRetryCount       = 2;                               // total of 3 retries per batch
            binding.RetryCycleDelay         = new TimeSpan(0, 5, 0);           // 5 minutes between batches
            binding.MaxRetryCycles          = 11;                              // total of 12 batches - 36 retries within 1 hours
            binding.ReceiveErrorHandling    = ReceiveErrorHandling.Reject;     // in the end reject it to the sender who will move it in the DLQ
            binding.MaxReceivedMessageSize  = Constants.MaxReceivedMessage;

            if (binding.SendTimeout == DefaultSendTimeout)
                binding.SendTimeout        = Constants.DefaultSendTimeout;

            if (binding.ReceiveTimeout == DefaultReceiveTimeout)
                binding.ReceiveTimeout      = Constants.DefaultSendTimeout;

            // TODO: turn security mode off:
            binding.Security               = new NetMsmqSecurity
            {
                Mode                            = NetMsmqSecurityMode.None,
                Transport                       = new MsmqTransportSecurity
                {
                    MsmqAuthenticationMode          = MsmqAuthenticationMode.None,
                }
            };

            return binding;
        }
    }
}
