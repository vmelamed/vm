using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace vm.Aspects.Cache.Associative.NWaySet
{
    /// <summary>
    /// Class NWayAssociativeSet implements N-way associative set cache.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys associated with the cached values.</typeparam>
    /// <typeparam name="TValue">The type of the cached values.</typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public partial class NWaySetAssociativeCache<TKey, TValue> :
        IDictionary<TKey, TValue>,
        ICollection<KeyValuePair<TKey, TValue>>,
        IEnumerable<KeyValuePair<TKey, TValue>>,
        IEnumerable,
        IDictionary,
        ICollection,
        IReadOnlyDictionary<TKey, TValue>,
        IReadOnlyCollection<KeyValuePair<TKey, TValue>>
    {
        #region Public properties
        /// <summary>
        /// Gets the number of sets (sachets).
        /// </summary>
        public int NumberOfSets { get; }

        /// <summary>
        /// Gets the size of a single set - the number of entries in a set.
        /// </summary>
        public int SetSize { get; }

        /// <summary>
        /// Gets the total size of the cache (the total number of entries).
        /// </summary>
        public int Size { get; }
        #endregion

        #region Internal properties
        /// <summary>
        /// Gets the array of sets controlling the contents of the cache entries <see cref="Entries"/>.
        /// </summary>
        internal EntrySet<TKey, TValue>[] Sets { get; }

        /// <summary>
        /// Gets the array of the cache entries.
        /// </summary>
        internal Entry<TKey, TValue>[] Entries { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="NWaySetAssociativeCache{TKey, TData}" /> class.
        /// </summary>
        /// <param name="numberOfSets">The number of sets.</param>
        /// <param name="setSize">The size of the sets in number of cache entries. This is the N in the N-way associative set.</param>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <item>
        /// The argument cannot be zero or negative. - numberOfSets
        /// </item><item>
        /// The argument cannot be zero or negative. - setSize
        /// </item>
        /// </list>
        /// </exception>
        /// <seealso cref="EvictionPolicy"/>
        public NWaySetAssociativeCache(
            int numberOfSets,
            int setSize)
        {
            NumberOfSets   = numberOfSets > 0 ? numberOfSets : throw new ArgumentOutOfRangeException(nameof(numberOfSets), "The argument cannot be zero or negative.");
            SetSize        = setSize > 0 ? setSize : throw new ArgumentOutOfRangeException(nameof(setSize), "The argument cannot be zero or negative.");
            Size           = NumberOfSets * SetSize;
            Entries        = new Entry<TKey, TValue>[numberOfSets * setSize];
            Sets           = Enumerable
                                .Range(0, NumberOfSets)
                                .Select(i => new EntrySet<TKey, TValue>(this, i*SetSize, (i+1)*SetSize))
                                .ToArray()
                                ;
        }
        #endregion

        #region Policies
        /// <summary>
        /// Gets the hash value of a key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>System.Int32.</returns>
        /// <remarks>
        /// The default implementation of the policy is to simply invoke the <see cref="object.GetHashCode"/> on the key.
        /// The clients can override this by implementing more elaborate algorithm. E.g. if the client knows that the keys are URL-s,
        /// they can return the hash of the server address only.
        /// </remarks>
        /// <example>
        /// <![CDATA[public override int GetHash(Uri key) => key.Host.GetHashCode();]]>
        /// </example>
        public virtual int GetHash(TKey key) => key.GetHashCode();

        /// <summary>
        /// The policy uses the &quot;usage stamps&quot; of two cache entries. The &quot;usage stamp&quot; is a positive <see cref="long"/> value which 
        /// represents how recently an item was used - the more recently an item was used - the bigger the value of the usage stamp.
        /// The usage stamp is not a time value. Based on the comparison of two usage stamps the policy returns 
        /// <see langword="true"/> if the first item should remain in the cache and the second item maybe evicted from the cache and replaced by a new item;
        /// and <see langword="false"/> if the second item should remain in the cache but first item may get evicted and replaced.
        /// </summary>
        /// <remarks>
        /// This class provides implementation of an LRU policy, i.e. the least recently used item in a given set is being evicted and replaced.
        /// See the example below of how to override this policy and implement MRU policy - evict the most recently used item.
        /// </remarks>
        /// <example>
        /// <![CDATA[public override bool EvictionPolicy(long item1Usage, long item2Usage) => item1Usage < item2Usage;]]>
        /// </example>
        public virtual bool EvictionPolicy(
            long item1Usage,
            long item2Usage) => item1Usage > item2Usage;

        /// <summary>
        /// Policy which determines what happens if a certain key is not found in the cache.
        /// </summary>
        /// <returns>TValue.</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <remarks>
        /// The default implementation throws <see cref="KeyNotFoundException"/> exception.
        /// The clients may override the policy, e.g. to return a default value. See the example below.
        /// </remarks>
        /// <example>
        /// <![CDATA[
        /// public override TValue CacheMissPolicy() => return default(TValue);
        /// ]]>
        /// </example>
        public virtual TValue CacheMissPolicy() => throw new KeyNotFoundException();

        /// <summary>
        /// The method for replacing an existing key in the cache with a new key.
        /// </summary>
        /// <param name="key">A reference to the existing key.</param>
        /// <param name="newKey">The new key.</param>
        /// <remarks>
        /// The default implementation simply assigns the new key value to the cache entry's key field.
        /// The clients may override this policy, e.g. when upon replacement the existing key must be disposed first. See the example below.
        /// </remarks>
        /// <example>
        /// <![CDATA[
        /// public override void ReplaceKey(ref MyKey key, MyKey newKey)
        /// {
        ///     if (!ReferenceEquals(key, newKey))
        ///     {
        ///         key?.Dispose();
        ///         key = newKey;
        ///     }
        /// }
        /// ]]>
        /// </example>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#")]
        public virtual void ReplaceKey(
            ref TKey key,
            TKey newKey) => key = newKey;

        /// <summary>
        /// The method for replacing an existing value in the cache with a new value.
        /// </summary>
        /// <param name="value">A reference to the existing value to be replaced with <paramref name="newValue"/>.</param>
        /// <param name="newValue">The new value to replace the <paramref name="value"/>.</param>
        /// <remarks>
        /// The default implementation simply assigns the new data value to the cache entry's data.
        /// The clients may override this policy, e.g. when upon replacement the existing data must be disposed first. See the example below.
        /// </remarks>
        /// <example>
        /// <![CDATA[
        /// public override void ReplaceValue(ref MyValue value, MyValue newValue)
        /// {
        ///     if (!ReferenceEquals(value, newValue))
        ///     {
        ///         value?.Dispose();
        ///         value = newValue;
        ///     }
        /// }
        /// ]]>
        /// </example>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#")]
        public virtual void ReplaceValue(
            ref TValue value,
            TValue newValue) => value = newValue;
        #endregion

        #region IDictionary<TKey, TData>
        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</param>
        /// <returns><see langword="true" /> if the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the key; otherwise, <see langword="false" />.</returns>
        public bool ContainsKey(
            TKey key)
        {
            var keyHash = GetHash(key);

            return Sets[SetIndex(keyHash)].ContainsKey(key, keyHash);
        }

        /// <summary>
        /// Adds a new value with a given key. In a case of a collision the method will execute the <see cref="EvictionPolicy(long, long)"/>
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Add(
            TKey key,
            TValue value)
        {
            var keyHash = GetHash(key);

            Sets[SetIndex(keyHash)].Add(key, keyHash, value);
        }

        /// <summary>
        /// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns><see langword="true" /> if the element is successfully removed; otherwise, <see langword="false" />.  This method also returns <see langword="false" /> if <paramref name="key" /> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool Remove(
            TKey key)
        {
            var keyHash = GetHash(key);

            return Sets[SetIndex(keyHash)].Remove(key, keyHash);
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        /// <returns><see langword="true" /> if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key; otherwise, <see langword="false" />.</returns>
        public bool TryGetValue(
            TKey key,
            out TValue value)
        {
            var keyHash = GetHash(key);

            return Sets[SetIndex(keyHash)].TryGetValue(key, keyHash, out value);
        }

        /// <summary>
        /// Gets or sets the value with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>TValue.</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public TValue this[TKey key]
        {
            get
            {
                var keyHash = GetHash(key);

                return Sets[SetIndex(keyHash)].TryGetValue(key, keyHash, out var value)
                            ? value
                            : CacheMissPolicy();
            }

            set
            {
                var keyHash = GetHash(key);

                Sets[SetIndex(keyHash)].Add(key, keyHash, value);
            }
        }

        /// <summary>
        /// Gets an <see cref="ICollection{TKey}" /> containing the keys of the cache.
        /// </summary>
        public ICollection<TKey> Keys => SafeReadAllEntries(() => Entries.Where(e => e.IsUsed).Select(e => e.Key).ToList());

        /// <summary>
        /// Gets an <see cref="ICollection{TValue}" /> containing the values in the cache.
        /// </summary>
        public ICollection<TValue> Values => SafeReadAllEntries(() => Entries.Where(e => e.IsUsed).Select(e => e.Value).ToList());

        /// <summary>
        /// Adds a key-value pair to the cache.
        /// </summary>
        /// <param name="item">The key-value pair to add to the cache.</param>
        public void Add(
            KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

        /// <summary>
        /// Removes all items from the cache.
        /// </summary>
        public void Clear()
            => SafeWriteAllEntries(
                () =>
                {
                    for (var i = 0; i < Sets.Length; i++)
                        Sets[i].Reset();
                });

        /// <summary>
        /// Determines whether the cache contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the cache.</param>
        /// <returns><see langword="true" /> if <paramref name="item" /> is found in the cache; otherwise, <see langword="false" />.
        /// </returns>
        public bool Contains(
            KeyValuePair<TKey, TValue> item) => TryGetValue(item.Key, out var value)  &&  value.Equals(item.Value);

        /// <summary>
        /// Copies the key-value pairs from the cache to an <see cref="Array" />,
        /// starting at a particular <see cref="Array" /> index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array" /> that is the destination of the elements copied from the cache.
        /// The <see cref="Array" /> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in <paramref name="array" /> at which copying begins.
        /// </param>
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        public void CopyTo(
            KeyValuePair<TKey, TValue>[] array,
            int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), "The argument is negative.");

            SafeReadAllEntries(
                () =>
                {
                    if (array.Length - arrayIndex < Count)
                        throw new ArgumentException("The number of elements in the cache is greater than the available space from arrayIndex to the end of the destination array.", nameof(arrayIndex));

                    var i = arrayIndex;

                    foreach (var kv in this)
                        array[i++] = kv;

                    return true;
                });
        }

        /// <summary>
        /// Removes a specific key-value pair from the cache.
        /// </summary>
        /// <param name="item">The object to remove from the cache.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="item" /> was successfully removed from the cache; otherwise, <see langword="false" />.
        /// This method also returns <see langword="false" /> if <paramref name="item" /> is not found in the cache.
        /// </returns>
        public bool Remove(
            KeyValuePair<TKey, TValue> item)
        {
            var keyHash = item.Key.GetHashCode();

            return Sets[SetIndex(keyHash)].Remove(item, keyHash);
        }

        /// <summary>
        /// Gets the number of elements contained in the cache.
        /// </summary>
        public int Count => SafeReadAllEntries(() => Entries.Count(e => e.IsUsed));

        /// <summary>
        /// Gets a value indicating whether the cache is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Returns an enumerator that iterates through the key-value items in the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        /// <remarks>
        /// Note that until the returned enumerator object is disposed, all requests for adding or removing key-values to the cache will be blocked.
        /// Moreover, if a request to add or remove a key-value pair to the cache is made on the same thread where the enumerator is supposed to be disposed,
        /// the thread will dead-lock.
        /// </remarks>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => new Enumerator(this);

        /// <summary>
        /// Returns an enumerator that iterates through the cache key-value pairs.
        /// </summary>
        /// <returns>An <see cref="IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion

        T SafeReadAllEntries<T>(Func<T> read)
        {
            T value;
            try
            {
                foreach (var set in Sets)
                    set.Lock.EnterReadLock();

                value = read();
            }
            finally
            {
                foreach (var set in Sets)
                    set.Lock.ExitReadLock();
            }

            return value;
        }

        void SafeWriteAllEntries(Action write)
        {
            try
            {
                foreach (var set in Sets)
                    set.Lock.EnterWriteLock();

                write();
            }
            finally
            {
                foreach (var set in Sets)
                    set.Lock.ExitWriteLock();
            }
        }

        int SetIndex(int keyHash) => keyHash % NumberOfSets;

        #region IDictionary
        bool IDictionary.Contains(object key) => ContainsKey((TKey)key);
        void IDictionary.Add(object key, object value) => Add((TKey)key, (TValue)value);
        void IDictionary.Clear() => Clear();
        IDictionaryEnumerator IDictionary.GetEnumerator() => new Enumerator(this);
        void IDictionary.Remove(object key) => Remove((TKey)key);
        object IDictionary.this[object key]
        {
            get => this[(TKey)key];
            set => this[(TKey)key] = (TValue)value;
        }
        ICollection IDictionary.Keys => Keys as ICollection;
        ICollection IDictionary.Values => Values as ICollection;
        bool IDictionary.IsReadOnly => false;
        bool IDictionary.IsFixedSize => true;
        void ICollection.CopyTo(Array array, int index) => CopyTo((KeyValuePair<TKey, TValue>[])array, index);
        int ICollection.Count => Count;
        object ICollection.SyncRoot => this;
        bool ICollection.IsSynchronized => true;
        #endregion

        #region IReadOnlyDictionary<TKey, TValue>
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;
        #endregion
    }
}
