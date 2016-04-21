using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using Microsoft.Practices.Unity;
using vm.Aspects.Facilities;
using vm.Aspects.Threading;

namespace vm.Aspects
{
    /// <summary>
    /// Class PerCallContextLifetimeManager. Used for objects which lifetime should end with the end of the current 
    /// .NET remoting or WCF call context. The objects are stored in the current <see cref="T:System.Runtime.Remoting.Messaging.CallContext"/>.
    /// </summary>
    public class PerCallContextLifetimeManager : LifetimeManager
    {
        ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
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
            object value;

            using (_lock.ReaderLock())
            {
                value = CallContext.LogicalGetData(_key);
                Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, "*** Get  object {0} from {1}", GetId(value), _key));
            }

            return value;
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
                using (_lock.WriterLock())
                {
                    CallContext.LogicalSetData(_key, newValue);
                    Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, "*** Set  object {0} to   {1}", GetId(newValue), _key));
                }
        }

        /// <summary>
        /// Remove the given object from backing store.
        /// </summary>
        public override void RemoveValue()
        {
            object value;

            using (_lock.WriterLock())
            {
                value = CallContext.LogicalGetData(_key);

                CallContext.LogicalSetData(_key, null);
                CallContext.FreeNamedDataSlot(_key);
                Contract.Assume(CallContext.LogicalGetData(_key) == null);
                Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, "*** Free object {0} from {1}", GetId(value), _key));
            }

            value.Dispose();
        }

        string GetId(object obj) => obj?.GetType()?.GetProperty("InstanceId")?.GetValue(obj)?.ToString();
    }
}
