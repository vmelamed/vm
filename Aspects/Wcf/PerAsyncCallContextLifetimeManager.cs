using System;
using System.Diagnostics.Contracts;
using System.Runtime.Remoting.Messaging;
using vm.Aspects.Wcf.Behaviors;

namespace vm.Aspects.Wcf
{
    /// <summary>
    /// Class PerAsyncCallContextLifetimeManager. Used for objects which lifetime should end with the end of the current 
    /// .NET remoting or WCF call context. The objects are stored in the current <see cref="AsyncCallContext"/>.
    /// </summary>
    public class PerAsyncCallContextLifetimeManager : PerCallContextLifetimeManager
    {
        /// <summary>
        /// Retrieve a value from the backing store associated with this Lifetime policy.
        /// </summary>
        /// <returns>the object desired, or null if no such object is currently stored.</returns>
        public override object GetValue() => AsyncCallContext.GetData(Key);

        /// <summary>
        /// Stores the given value into backing store for retrieval later.
        /// </summary>
        /// <param name="newValue">The object being stored.</param>
        public override void SetValue(object newValue)
        {
            if (newValue == null)
                RemoveValue();
            else
                AsyncCallContext.SetData(Key, newValue);
        }

        /// <summary>
        /// Remove the given object from backing store.
        /// </summary>
        public override void RemoveValue()
        {
            AsyncCallContext.FreeDataSlot(Key);
        }

        static AsyncCallContext AsyncCallContext
        {
            get
            {
                Contract.Ensures(Contract.Result<AsyncCallContext>() != null);

                var asyncCallContext = CallContext.LogicalGetData(AsyncCallContextMessageInspector.CallContextSlotName) as AsyncCallContext;

                if (asyncCallContext == null)
                    throw new InvalidOperationException("Could not get asynchronous call context from CallContext. Is AsyncCallContextBehaviorAttribute applied to the service?");

                return asyncCallContext;
            }
        }
    }
}
