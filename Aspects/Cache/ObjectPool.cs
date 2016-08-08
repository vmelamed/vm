using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace vm.Aspects.Cache
{
    /// <summary>
    /// The objects of type <c>ObjectPool</c> maintain a pool of objects which may be somewhat expensive to create. The instances of this class are thread-safe.
    /// </summary>
    /// <typeparam name="T">The type of the objects in the pool.</typeparam>
    /// <remarks>
    /// <para>
    /// Before using the pool it needs to be initialized with at least two parameters: maximal number of objects tracked by the pool and a parameterless method-factory
    /// which creates and returns an instance of type <typeparamref name="T"/>.
    /// </para><para>
    /// An important requirement for the class <typeparamref name="T"/> is that the objects should be stateless or at least should not maintain any state while
    /// sitting in the pool. It is not predictable which object will be lent to which caller next.
    /// </para><para>
    /// Whenever a client needs an object of type <typeparamref name="T"/> from the pool, they need to call the method <see cref="M:ObjectPool.LendObject"/>.
    /// The pool will return an instance of <typeparamref name="T"/> wrapped in <see cref="LentObject{T}"/> object. In order to access the object itself, use the property 
    /// <see cref="P:LentObject{T}.Instance"/>. After using it, the caller must return the object back to the pool. This is achieved by disposing the <see cref="LentObject{T}"/> wrapper 
    /// but will not dispose the wrapped object.
    /// </para><para>
    /// The instances of the pool which are not held in static variables must be disposed if not needed in order to release the objects in the pool and other associated objects.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// class MyObject
    /// {
    ///     public void Method() {}
    /// }
    /// 
    /// public static ObjectPool<MyObject> myObjectsPool = new ObjectPool<MyObject>();
    /// 
    /// static void ApplicationInitialization()
    /// {
    ///     myObjectsPool.Initialize(
    ///         16,                     // initialize the pool to have no more than 16 instances of MyObject
    ///         () => new MyObject());  // MyObject object factory
    /// }
    /// 
    /// void SomeMethod()
    /// {
    ///     using (var lentObject = myObjectPool.LendObject())
    ///     {
    ///         lentObject.Instance.Method();
    ///         // the using statement above will call IDispose on lentObject which will return the object back to the pool
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    public class ObjectPool<T> : IDisposable, IIsDisposed where T : class
    {
        object _sync = new object();
        int _poolSize;
        bool _isInitialized;
        SemaphoreSlim _semaphore;
        Stack<T> _freeObjects;
        HashSet<T> _lentObjects;
        Func<T> _objectFactory;

        /// <summary>
        /// Creates a new uninitialized instance of the <see cref="ObjectPool{T}"/> class.
        /// After using this constructor the instance must be initialized.
        /// </summary>
        public ObjectPool()
        {
        }

        /// <summary>
        /// Creates and initializes a new instance of the <see cref="ObjectPool{T}"/> class.
        /// </summary>
        /// <param name="poolSize">Size of the pool.</param>
        /// <param name="objectFactory">The object factory.</param>
        /// <param name="lazy">
        /// If set to <see langword="true" /> the objects in the pool will be created only if there are no available objects to lend and 
        /// the total number of lent objects is less than the property <see cref="P:PoolSize"/>. Otherwise all objects in the pool will be created
        /// immediately in this method.
        /// </param>
        public ObjectPool(
            int poolSize,
            Func<T> objectFactory,
            bool lazy = true)
        {
            Contract.Requires<ArgumentException>(poolSize > 0, "The pool size must be a positive number.");
            Contract.Requires<ArgumentNullException>(objectFactory != null, nameof(objectFactory));

            Initialize(poolSize, objectFactory, lazy);
        }

        /// <summary>
        /// Initializes the pool with predetermined size and an object factory.
        /// </summary>
        /// <param name="poolSize">Size of the pool.</param>
        /// <param name="objectFactory">The object factory.</param>
        /// <param name="lazy">
        /// If set to <see langword="true" /> the objects in the pool will be created only if there are no available objects to lend and 
        /// the total number of lent objects is less than the property <see cref="P:PoolSize"/>. Otherwise all objects in the pool will be created
        /// immediately in this method.
        /// </param>
        /// <returns>This IObjectPool{T}.</returns>
        /// <exception cref="System.InvalidOperationException">The pool is already initialized.</exception>
        /// <exception cref="System.NotImplementedException"></exception>
        public ObjectPool<T> Initialize(
            int poolSize,
            Func<T> objectFactory,
            bool lazy = true)
        {
            Contract.Requires<ArgumentException>(poolSize > 0, "The pool size must be a positive number.");
            Contract.Requires<ArgumentNullException>(objectFactory != null, nameof(objectFactory));
            Contract.Requires<InvalidOperationException>(!IsInitialized, "The pool is already initialized.");
            Contract.Ensures(Contract.Result<ObjectPool<T>>() != null);
            Contract.Ensures(Contract.Result<ObjectPool<T>>() == this);

            lock (_sync)
            {
                if (IsInitialized)
                    throw new InvalidOperationException("The pool is already initialized.");

                _poolSize      = poolSize;
                _objectFactory = objectFactory;
                _semaphore     = new SemaphoreSlim(poolSize, poolSize);
                _freeObjects   = new Stack<T>();
                _lentObjects   = new HashSet<T>();

                if (!lazy)
                    for (var i = 0; i<poolSize; i++)
                        _freeObjects.Push(objectFactory());

                _isInitialized = true;
                return this;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is initialized.
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// Gets the number of immediately available instances. Use for testing only.
        /// The number is not reliable depending on 2 factors: the laziness of the pool and the number of concurrent clients.
        /// </summary>
        internal int AvailableInstances
        {
            get
            {
                Contract.Requires<InvalidOperationException>(IsInitialized, "The pool is not initialized.");

                return _freeObjects.Count();
            }
        }

        /// <summary>
        /// Gets the size of the pool.
        /// </summary>
        public int PoolSize
        {
            get
            {
                Contract.Requires<InvalidOperationException>(IsInitialized, "The pool is not initialized.");

                return _poolSize;
            }
        }

        /// <summary>
        /// Lends an object from the pool to the caller.
        /// </summary>
        /// <returns>An object from the pool.</returns>
        public LentObject<T> LendObject(
            int waitMilliseconds = Timeout.Infinite)
        {
            Contract.Requires<ArgumentException>(waitMilliseconds >= -1, "The timeout must be greater or equal to -1 (infinite).");
            Contract.Requires<InvalidOperationException>(IsInitialized, "The object pool is not initialized.");

            if (!_semaphore.Wait(waitMilliseconds))
                return null;

            lock (_sync)
            {
                T instance = _freeObjects.Any()
                                    ? _freeObjects.Pop()
                                    : _objectFactory();

                _lentObjects.Add(instance);
                return new LentObject<T>(instance, this);
            }
        }

        /// <summary>
        /// Lends an object from the pool to the caller.
        /// </summary>
        /// <param name="waitMilliseconds">How long to wait in milliseconds for available object.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task object representing the process of lending an object from the pool.</returns>
        public async Task<LentObject<T>> LendObjectAsync(
            int waitMilliseconds = Timeout.Infinite,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Contract.Requires<ArgumentException>(waitMilliseconds >= -1, "The timeout must be greater or equal to -1 (infinite).");
            Contract.Requires<InvalidOperationException>(IsInitialized, "The object pool is not initialized.");

            if (!await _semaphore.WaitAsync(waitMilliseconds, cancellationToken))
                return null;

            lock (_sync)
            {
                T instance = _freeObjects.Any()
                                    ? _freeObjects.Pop()
                                    : _objectFactory();

                _lentObjects.Add(instance);
                return new LentObject<T>(instance, this);
            }
        }

        /// <summary>
        /// Returns the specified object (wrapped in <see cref="LentObject{T}"/>) back to the pool.
        /// </summary>
        /// <param name="lentObject">The wrapper holding the lent object to be returned.</param>
        /// <returns>This IObjectPool{T}.</returns>
        internal ObjectPool<T> ReturnObject(
            LentObject<T> lentObject)
        {
            Contract.Requires<ArgumentNullException>(lentObject != null, nameof(lentObject));
            Contract.Requires<InvalidOperationException>(lentObject.Pool == this, "The object was not lent from this pool.");
            Contract.Requires<InvalidOperationException>(IsInitialized, "The object pool is not initialized.");
            Contract.Ensures(Contract.Result<ObjectPool<T>>() != null);
            Contract.Ensures(Contract.Result<ObjectPool<T>>() == this);

            lock (_sync)
            {
                if (!_lentObjects.Contains(lentObject.Instance))
                    throw new InvalidOperationException("The object is not from this pool.");

                _lentObjects.Remove(lentObject.Instance);
                _freeObjects.Push(lentObject.Instance);
            }

            _semaphore.Release();
            return this;
        }

        #region IDisposable pattern implementation
        /// <summary>
        /// The flag will be set just before the object is disposed.
        /// </summary>
        /// <value>0 - if the object is not disposed yet, any other value - the object is already disposed.</value>
        int _disposed;

        /// <summary>
        /// Returns <c>true</c> if the object has already been disposed, otherwise <c>false</c>.
        /// </summary>
        public bool IsDisposed => Interlocked.CompareExchange(ref _disposed, 1, 1) == 1;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>Invokes the protected virtual <see cref="M:Dispose(bool)"/>.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "It is correct.")]
        public void Dispose()
        {
            // these will be called only if the instance is not disposed and is not in a process of disposing.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs the actual job of disposing the object.
        /// </summary>
        /// <param name="disposing">
        /// Passes the information whether this method is called by <see cref="M:Dispose()"/> (explicitly or
        /// implicitly at the end of a <c>using</c> statement), or by the <see cref="M:~ObjectPool"/>.
        /// </param>
        /// <remarks>
        /// If the method is called with <paramref name="disposing"/><c>==true</c>, i.e. from <see cref="M:Dispose()"/>, 
        /// it will try to release all managed resources (usually aggregated objects which implement <see cref="T:IDisposable"/> as well) 
        /// and then it will release all unmanaged resources if any. If the parameter is <c>false</c> then 
        /// the method will only try to release the unmanaged resources.
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            // if it is disposed or in a process of disposing - return.
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            if (!disposing)
                return;

            lock (_sync)
            {
                if (_semaphore != null)
                    _semaphore.Dispose();

                if (_freeObjects != null)
                    foreach (var i in _freeObjects)
                        i.Dispose();
            }
        }
        #endregion
    }
}
