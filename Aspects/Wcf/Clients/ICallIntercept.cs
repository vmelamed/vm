using System.Diagnostics.CodeAnalysis;
using System.ServiceModel.Channels;

namespace vm.Aspects.Wcf.Clients
{
    /// <summary>
    /// Interface ICallIntercept defines the interception behavior requirement.
    /// </summary>
    public interface ICallIntercept
    {
        /// <summary>
        /// Invoked before the call.
        /// </summary>
        /// <param name="request">The request.</param>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId="0#", Justification="see IClientMessageInspector.")]
        void PreInvoke(ref Message request);

        /// <summary>
        /// Invoked after the call.
        /// </summary>
        /// <param name="reply">The reply.</param>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId="0#", Justification="see IClientMessageInspector.")]
        void PostInvoke(ref Message reply);
    }
}
