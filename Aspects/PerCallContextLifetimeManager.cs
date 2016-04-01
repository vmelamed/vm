using System;
using System.Runtime.Remoting.Messaging;
using Microsoft.Practices.Unity;
using vm.Aspects.Facilities;

namespace vm.Aspects
{
    /// <summary>
    /// Class PerCallContextLifetimeManager. Used for objects which lifetime should end with the end of the current 
    /// .NET remoting or WCF call context. The objects are stored in the current <see cref="T:System.Runtime.Remoting.Messaging.CallContext"/>.
    /// </summary>
    public class PerCallContextLifetimeManager : LifetimeManager
    {
        readonly string _key = Facility.GuidGenerator.NewGuid().ToString("N");

        /// <summary>
        /// Gets the key of the object stored in the call context.
        /// </summary>
        public string Key => _key;

        /// <summary>
        /// Retrieve a value from the backing store associated with this Lifetime policy.
        /// </summary>
        /// <returns>the object desired, or null if no such object is currently stored.</returns>
        public override object GetValue()
        {
            var obj = CallContext.LogicalGetData(_key);

            return obj;
        }

        /// <summary>
        /// Stores the given value into backing store for retrieval later.
        /// </summary>
        /// <param name="newValue">The object being stored.</param>
        public override void SetValue(object newValue)
        {
            if (newValue == null)
                RemoveValue();
            else
                CallContext.LogicalSetData(_key, newValue);
        }

        /// <summary>
        /// Remove the given object from backing store.
        /// </summary>
        public override void RemoveValue()
        {
            var obj = CallContext.LogicalGetData(_key);

            CallContext.FreeNamedDataSlot(_key);

            var disposable = obj as IDisposable;

            if (disposable != null)
                disposable.Dispose();
        }
    }
}
