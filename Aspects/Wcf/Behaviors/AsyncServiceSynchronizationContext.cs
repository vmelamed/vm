using System.Runtime.Remoting.Messaging;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading;

namespace vm.Aspects.Wcf.Behaviors
{
    /// <summary>
    /// Class AsyncServiceSynchronizationContext is meant to be used by asynchronous service implementations.
    /// When asynchronous calls inside the service return, the service is usually on a different thread.
    /// At this point thread bound facilities like <see cref="OperationContext"/> are inaccessible anymore.
    /// Here we are storing these in the context's fields and are setting them right before dispatching the message.
    /// </summary>
    /// <seealso cref="System.Threading.SynchronizationContext" />
    public class AsyncServiceSynchronizationContext : SynchronizationContext
    {
        OperationContext _operationContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncServiceSynchronizationContext"/> class in a service.
        /// </summary>
        public AsyncServiceSynchronizationContext()
        {
            _operationContext    = OperationContext.Current;

            CallContext.LogicalSetData(nameof(WebOperationContext), WebOperationContext.Current);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncServiceSynchronizationContext"/> class in a client application.
        /// </summary>
        /// <param name="channel">The client channel.</param>
        public AsyncServiceSynchronizationContext(IContextChannel channel)
        {
            _operationContext = new OperationContext(channel);
        }

        /// <summary>
        /// When overridden in a derived class, dispatches a synchronous message to a synchronization context.
        /// </summary>
        /// <param name="d">The <see cref="T:System.Threading.SendOrPostCallback" /> delegate to call.</param>
        /// <param name="state">The object passed to the delegate.</param>
        public override void Send(
            SendOrPostCallback d,
            object state)
        {
            OperationContext.Current = _operationContext;
            base.Send(d, state);
        }

        /// <summary>
        /// When overridden in a derived class, dispatches an asynchronous message to a synchronization context.
        /// </summary>
        /// <param name="d">The <see cref="T:System.Threading.SendOrPostCallback" /> delegate to call.</param>
        /// <param name="state">The object passed to the delegate.</param>
        public override void Post(
            SendOrPostCallback d,
            object state)
        {
            OperationContext.Current = _operationContext;
            base.Post(d, state);
        }
    }
}
