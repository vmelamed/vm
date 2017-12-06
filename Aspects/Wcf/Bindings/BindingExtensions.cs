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
            switch (binding)
            {
            case null:
                throw new ArgumentNullException(nameof(binding));

            case NetTcpBinding t:
                return t.TransferMode;

            case NetNamedPipeBinding p:
                return p.TransferMode;

            case BasicHttpBinding b:
                return b.TransferMode;

            case WebHttpBinding w:
                return w.TransferMode;

            default:
                // only the bindings above support true streaming
                return TransferMode.Buffered;
            }
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
