using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Remoting.Messaging;
using System.Security;
using System.Threading;

using Microsoft.Practices.Unity;

using vm.Aspects.Threading;

namespace vm.Aspects
{
#pragma warning disable CS3009 // Base type is not CLS-compliant
    /// <summary>
    /// Class PerCallContextLifetimeManager. Used for objects which lifetime should end with the end of the current 
    /// .NET remoting or WCF call context. The objects are stored in the current <see cref="T:System.Runtime.Remoting.Messaging.CallContext"/>.
    /// Usually you need to use something like the <see cref="T:vm.Aspects.Model.PerCallContextRepositoryCallHandler"/> that will dispose and remove the object from the call context.
    /// </summary>
    [DebuggerDisplay("{GetType().Name, nq}: {_key,nq}")]
    [SecurityCritical]
    public class PerCallContextLifetimeManager<T> : LifetimeManager, IDisposable, IEquatable<PerCallContextLifetimeManager<T>>
    {
        /// <summary>
        /// Makes the operations <see cref="GetValue"/> and <see cref="SetValue"/> atomic
        /// </summary>
        readonly ReaderWriterLockSlim _sync = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        /// <summary>
        /// The key of the object stored in the call context.
        /// </summary>
        readonly string _key = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Retrieve a value from the backing store associated with this Lifetime policy.
        /// </summary>
        /// <returns>the object desired, or null if no such object is currently stored.</returns>
        public override object GetValue()
        {
            using (_sync.ReaderLock())
                return CallContext.LogicalGetData(_key);
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
            {
                if (!(newValue is T))
                    throw new ArgumentException($"The argument {nameof(newValue)} must be of type {typeof(T).FullName}");

                using (_sync.WriterLock())
                    CallContext.LogicalSetData(_key, newValue);
            }
        }

        /// <summary>
        /// Remove the given object from backing store.
        /// </summary>
        public override void RemoveValue()
        {
            object value;

            using (_sync.WriterLock())
            {
                value = CallContext.LogicalGetData(_key);
                CallContext.LogicalSetData(_key, null);
            }

            value.Dispose();
        }

        #region Identity rules implementation.
        #region IEquatable<PerCallContextLifetimeManager<T>> Members
        /// <summary>
        /// Indicates whether the current object is equal to a reference to another object of the same type.
        /// </summary>
        /// <param name="other">A reference to another object of type <see cref="PerCallContextLifetimeManager{T}"/> to compare with the current object.</param>
        /// <returns>
        /// <list type="number">
        ///     <item><see langword="false"/> if <paramref name="other"/> is equal to <see langword="null"/>, otherwise</item>
        ///     <item><see langword="true"/> if <paramref name="other"/> refers to <c>this</c> object, otherwise</item>
        ///     <item><see langword="false"/> if <paramref name="other"/> is not the same type as <c>this</c> object, otherwise</item>
        ///     <item><see langword="true"/> if the current object and the <paramref name="other"/> are considered to be equal, 
        ///                                  e.g. their business identities are equal; otherwise, <see langword="false"/>.</item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// The <see cref="Equals(PerCallContextLifetimeManager{T})"/> and <see cref="Equals(object)"/> methods and
        /// the overloaded <c>operator==</c> and <c>operator!=</c> test for business identity, 
        /// i.e. they test for business same-ness by comparing the types and the business keys.
        /// </remarks>
        public virtual bool Equals(PerCallContextLifetimeManager<T> other)
        {
            if (other is null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            if (GetType() != other.GetType())
                return false;

            return _key == other._key;
        }
        #endregion

        /// <summary>
        /// Determines whether this <see cref="PerCallContextLifetimeManager{T}"/> instance is equal to the specified <see cref="object"/> reference.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> reference to compare with this <see cref="PerCallContextLifetimeManager{T}"/> object.</param>
        /// <returns>
        /// <list type="number">
        ///     <item><see langword="false"/> if <paramref name="obj"/> cannot be cast to <see cref="PerCallContextLifetimeManager{T}"/>, otherwise</item>
        ///     <item><see langword="false"/> if <paramref name="obj"/> is equal to <see langword="null"/>, otherwise</item>
        ///     <item><see langword="true"/> if <paramref name="obj"/> refers to <c>this</c> object, otherwise</item>
        ///     <item><see langword="false"/> if <paramref name="obj"/> is not the same type as <c>this</c> object, otherwise</item>
        ///     <item><see langword="true"/> if the current object and the <paramref name="obj"/> are considered to be equal, 
        ///                                  e.g. their business identities are equal; otherwise, <see langword="false"/>.</item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// The <see cref="Equals(PerCallContextLifetimeManager{T})"/> and <see cref="Equals(object)"/> methods and
        /// the overloaded <c>operator==</c> and <c>operator!=</c> test for business identity, 
        /// i.e. they test for business same-ness by comparing the types and the business keys.
        /// </remarks>
        public override bool Equals(object obj) => Equals(obj as PerCallContextLifetimeManager<T>);

        /// <summary>
        /// Serves as a hash function for the objects of <see cref="PerCallContextLifetimeManager{T}"/> and its derived types.
        /// </summary>
        /// <returns>A hash code for the current <see cref="PerCallContextLifetimeManager{T}"/> instance.</returns>
        public override int GetHashCode()
        {
            var hashCode = Constants.HashInitializer;

            unchecked
            {
                hashCode = Constants.HashMultiplier * hashCode + (_key?.GetHashCode() ?? 0);
            }
            return hashCode;
        }

        /// <summary>
        /// Compares two <see cref="PerCallContextLifetimeManager{T}"/> objects.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// <see langword="true"/> if the objects are considered to be equal (<see cref="Equals(PerCallContextLifetimeManager{T})"/>);
        /// otherwise <see langword="false"/>.
        /// </returns>
        public static bool operator ==(PerCallContextLifetimeManager<T> left, PerCallContextLifetimeManager<T> right)
            => left is null ? right is null : left.Equals(right);

        /// <summary>
        /// Compares two <see cref="PerCallContextLifetimeManager{T}"/> objects.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// <see langword="true"/> if the objects are not considered to be equal (<see cref="Equals(PerCallContextLifetimeManager{T})"/>);
        /// otherwise <see langword="false"/>.
        /// </returns>
        public static bool operator !=(PerCallContextLifetimeManager<T> left, PerCallContextLifetimeManager<T> right)
            => !(left==right);
        #endregion

        #region IDisposable pattern implementation
        /// <summary>
        /// The flag will be set just before the object is disposed.
        /// </summary>
        /// <value>0 - if the object is not disposed yet, any other value - the object is already disposed.</value>
        /// <remarks>
        /// Do not test or manipulate this flag outside of the property <see cref="IsDisposed"/> or the method <see cref="Dispose()"/>.
        /// The type of this field is Int32 so that it can be easily passed to the members of the class <see cref="Interlocked"/>.
        /// </remarks>
        int _disposed;

        /// <summary>
        /// Returns <see langword="true"/> if the object has already been disposed, otherwise <see langword="false"/>.
        /// </summary>
        public bool IsDisposed => Interlocked.CompareExchange(ref _disposed, 1, 1) == 1;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>Invokes the protected virtual <see cref="Dispose(bool)"/>.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "It is correct.")]
        public void Dispose()
        {
            // if it is disposed or in a process of disposing - return.
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            // these will be called only if the instance is not disposed and is not in a process of disposing.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // TODO: if there are no *unmanaged* resources in this class, remove the finalizer.
        /// <summary>
        /// Allows the object to attempt to free resources and perform other cleanup operations before it is reclaimed by garbage collection. 
        /// </summary>
        /// <remarks>Invokes the protected virtual <see cref="Dispose(bool)"/> with parameter <see langword="false"/>.</remarks>
        ~PerCallContextLifetimeManager()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs the actual job of disposing the object.
        /// </summary>
        /// <param name="disposing">
        /// Passes the information whether this method is called by <see cref="Dispose()"/> (explicitly or
        /// implicitly at the end of a <c>using</c> statement), or by the finalizer.
        /// </param>
        /// <remarks>
        /// If the method is called with <paramref name="disposing"/>==<see langword="true"/>, i.e. from <see cref="Dispose()"/>, 
        /// it will try to release all managed resources (usually aggregated objects which implement <see cref="IDisposable"/> as well) 
        /// and then it will release all unmanaged resources if any. If the parameter is <see langword="false"/> then 
        /// the method will only try to release the unmanaged resources.
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                _sync.Dispose();
        }
        #endregion
    }
#pragma warning restore CS3009 // Base type is not CLS-compliant
}