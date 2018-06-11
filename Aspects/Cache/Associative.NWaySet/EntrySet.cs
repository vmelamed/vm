using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace vm.Aspects.Cache.Associative.NWaySet
{
    /// <summary>
    /// The structure contains properties and methods used for control of a single set (a.k.a. sachet) of cache <see cref="Entry{TKey, TValue}"/>-s.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    struct EntrySet<TKey, TValue>
    {
        /// <summary>
        /// Synchronizes the access to the <see cref="EntrySet{TKey, TData}"/>
        /// </summary>
        internal ReaderWriterLockSlim Lock { get; }
        /// <summary>
        /// Reference to the cache object owning the instances of this type.
        /// </summary>
        readonly NWaySetAssociativeCache<TKey, TValue> _cache;
        /// <summary>
        /// The index of the cache <see cref="Entry{TKey, TData}"/> where the set controlled by this instance begins. 
        /// </summary>
        readonly int _entriesBegin;
        /// <summary>
        /// The index of the cache <see cref="Entry{TKey, TData}"/> where the set controlled by this instance ends. 
        /// </summary>
        readonly int _entriesEnd;
        /// <summary>
        /// When an entry is being used (read or written) the value of this field is incremented and assigned to the entry's <see cref="Entry{TKey, TData}.UsageStamp"/> property.
        /// Basically serves as a usage stamp generator. 
        /// </summary>
        long _nextUsage;

        long NextUsage => Interlocked.Increment(ref _nextUsage);

        /// <summary>
        /// Initializes a new instance of the <see cref="EntrySet{TKey, TValue}"/> struct.
        /// </summary>
        /// <param name="cache">The cache that this set belongs to.</param>
        /// <param name="entriesBegin">The beginning index of the range of entry's indexes managed by this instance.</param>
        /// <param name="entriesEnd">The end index of the range of entry's indexes managed by this instance.</param>
        /// <exception cref="ArgumentException">
        /// The indexes of the entries cannot be negative.
        /// or
        /// The index of the entry where this set begins cannot be greater or equal to the index where they end. - entriesBegin
        /// </exception>
        /// <exception cref="ArgumentNullException">cache</exception>
        internal EntrySet(
            NWaySetAssociativeCache<TKey, TValue> cache,
            int entriesBegin,
            int entriesEnd)
        {
            if (entriesBegin < 0  ||  entriesEnd < 0)
                throw new ArgumentException("The indexes of the entries cannot be negative.");
            if (entriesBegin >= entriesEnd)
                throw new ArgumentException("The index of the entry where this set begins cannot be greater than the index where they end.", nameof(entriesBegin));

            _cache        = cache ?? throw new ArgumentNullException(nameof(cache));
            _entriesBegin = entriesBegin;
            _entriesEnd   = entriesEnd;
            _nextUsage     = 0;
            Lock          = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        }

        /// <summary>
        /// Tries to get a value from this set mapped to the <paramref name="key"/> (and its <paramref name="keyHash"/>).
        /// </summary>
        /// <param name="key">The key to which the value is mapped.</param>
        /// <param name="keyHash">The hash of the key.
        /// </param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool TryGetValue(
            TKey key,
            int keyHash,
            out TValue value)
        {
            try
            {
                Lock.EnterReadLock();

                var i = ScanEntries(key, keyHash);

                if (i < 0)
                {
                    value = default(TValue);
                    return false;
                }

                _cache.Entries[i].UsageStamp = NextUsage;
                value = _cache.Entries[i].Value;
                return true;
            }
            finally
            {
                Lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Adds the specified key and value to the set. Process:
        /// <list type="number">
        /// <item>
        /// If the key already exist in the set - update the value otherwise
        /// </item>
        /// <item>
        /// Occupy an available cache entry belonging to the set or
        /// </item>
        /// <item>
        /// Evict an existing key-value according to the cache's policy and replace it with the key-value from the parameters
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="keyHash">The key hash (for performance optimization).</param>
        /// <param name="value">The value.</param>
        internal void Add(
            TKey key,
            int keyHash,
            TValue value)
        {
            Lock.EnterWriteLock();
            try
            {
                var existingKeyIndex = -1;
                var notUsedIndex     = -1;
                var evictIndex       = -1;
                var lastUsageStamp   = _cache.EvictionPolicy(0, long.MaxValue) ? 0 : long.MaxValue;

                for (var i = _entriesBegin; i < _entriesEnd; i++)
                {
                    if (!_cache.Entries[i].IsUsed)
                    {
                        notUsedIndex = i;
                        continue;
                    }

                    if (_cache.Entries[i].KeyHash == keyHash  &&
                        _cache.Entries[i].Key.Equals(key))
                    {
                        existingKeyIndex = i;
                        break;
                    }

                    // if we have an empty slot in the set, no need to evict anything
                    if (notUsedIndex == -1  &&
                        _cache.EvictionPolicy(lastUsageStamp, _cache.Entries[i].UsageStamp))
                    {
                        evictIndex = i;
                        lastUsageStamp  = _cache.Entries[i].UsageStamp;
                    }
                }

                if (existingKeyIndex > -1)
                {
                    // we have a hit - replace the value only:
                    _cache.ReplaceValue(ref _cache.Entries[existingKeyIndex].Value, value);
                    _cache.Entries[existingKeyIndex].UsageStamp = NextUsage;
                    return;
                }

                if (notUsedIndex > -1)
                {
                    // no hit, we have an empty slot - use it
                    InitializeEntry(notUsedIndex, key, value, keyHash, NextUsage);
                    return;
                }

                // no hit, all slots are taken - evict the candidate
                Debug.Assert(evictIndex > -1);

                InitializeEntry(evictIndex, key, value, keyHash, NextUsage);
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Determines whether this instance contains an entry with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="keyHash">The key hash.</param>
        /// <returns><c>true</c> if the set contains the key; otherwise, <c>false</c>.</returns>
        internal bool ContainsKey(
            TKey key,
            int keyHash)
        {
            Lock.EnterReadLock();
            try
            {
                return ScanEntries(key, keyHash) > -1;
            }
            finally
            {
                Lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Removes the value mapped to the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="keyHash">The key hash.</param>
        /// <returns><c>true</c> if a key-value pair was removed, <c>false</c> otherwise.</returns>
        internal bool Remove(
            TKey key,
            int keyHash)
        {
            Lock.EnterWriteLock();
            try
            {
                var i = ScanEntries(key, keyHash);

                if (i < 0)
                    return false;

                InitializeEntry(i, default(TKey), default(TValue), 0, 0);
                return true;
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }


        /// <summary>
        /// Removes the specified key-value pair from this set.
        /// </summary>
        /// <param name="item">The key-value pair.</param>
        /// <param name="keyHash">The key hash.</param>
        /// <returns><c>true</c> if a key-value pair was removed, <c>false</c> otherwise.</returns>
        internal bool Remove(
            KeyValuePair<TKey, TValue> item,
            int keyHash)
        {
            Lock.EnterWriteLock();
            try
            {
                var i = ScanEntries(item.Key, keyHash);

                if (i < 0  ||  !_cache.Entries[i].Value.Equals(item.Value))
                    return false;

                InitializeEntry(i, default(TKey), default(TValue), 0, 0);
                return true;
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Resets this entry set.
        /// </summary>
        internal void Reset()
        {
            Lock.EnterWriteLock();
            try
            {
                for (var i = _entriesBegin; i < _entriesEnd; i++)
                    InitializeEntry(i, default(TKey), default(TValue), 0, 0);
                _nextUsage = 0;
            }
            finally
            {
                Lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Scans the entries looking for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="keyHash">The key hash.</param>
        /// <returns>System.Int32.</returns>
        int ScanEntries(
            TKey key,
            int keyHash)
        {
            for (var i = _entriesBegin; i < _entriesEnd; i++)
                if (_cache.Entries[i].IsUsed              &&
                    _cache.Entries[i].KeyHash == keyHash  &&
                    _cache.Entries[i].Key.Equals(key))
                    return i;

            return -1;
        }

        /// <summary>
        /// Initializes an entry at the specified index.
        /// </summary>
        /// <param name="index">The index of the entry to be initialized.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="keyHash">The key hash.</param>
        /// <param name="usageStamp">The usage stamp.</param>
        void InitializeEntry(
            int index,
            TKey key,
            TValue value,
            int keyHash,
            long usageStamp)
        {
            _cache.ReplaceKey(ref _cache.Entries[index].Key, key);
            _cache.ReplaceValue(ref _cache.Entries[index].Value, value);
            _cache.Entries[index].KeyHash    = keyHash;
            _cache.Entries[index].UsageStamp = usageStamp;
        }
    }
}
