using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace vm.Aspects.Wcf.Bindings
{
    /// <summary>
    /// Class BindingExtensions. Defines a few useful extensions related to binding.
    /// </summary>
    public static class BindingExtensions
    {
        /// <summary>
        /// Gets the transfer mode from the specified binding.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns>The binding's transfer mode.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="binding"/> is null.</exception>
        public static TransferMode GetTransferMode(
            this Binding binding)
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            var netTcpBinding = binding as NetTcpBinding;

            if (netTcpBinding != null)
                return netTcpBinding.TransferMode;

            var netNamedPipeBinding = binding as NetNamedPipeBinding;

            if (netNamedPipeBinding != null)
                return netNamedPipeBinding.TransferMode;

            var basicHttpBinding = binding as BasicHttpBinding;

            if (basicHttpBinding != null)
                return basicHttpBinding.TransferMode;

            var webHttpBinding = binding as WebHttpBinding;

            if (webHttpBinding != null)
                return webHttpBinding.TransferMode;

            // only the bindings above support true streaming
            return TransferMode.Buffered;
        }

        /// <summary>
        /// Determines whether the specified binding supports request, response or both streaming modes.
        /// </summary>
        /// <param name="binding">The binding.</param>
        /// <returns><see langword="true"/> if the specified binding is streaming; otherwise, <see langword="false"/>.</returns>
        public static bool IsStreaming(
            this Binding binding)
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            return binding.GetTransferMode() != TransferMode.Buffered;
        }
    }
}
