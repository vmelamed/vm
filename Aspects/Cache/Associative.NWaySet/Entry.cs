using System;

namespace vm.Aspects.Cache.Associative.NWaySet
{
    /// <summary>
    /// Cache entry holds a single cached value and the associated key; hash of the key; and the value item's usage stamp, used when the value is evicted.
    /// </summary>
    /// <typeparam name="TValue">The type of the cached values.</typeparam>
    /// <typeparam name="TKey">The type of the keys of the cached values.</typeparam>
    struct Entry<TKey, TValue>
    {
        /// <summary>
        /// Gets or sets the key associated with the value.
        /// </summary>
        public TKey Key;

        /// <summary>
        /// Gets or sets the cached value.
        /// </summary>
        public TValue Value;

        /// <summary>
        /// Gets or sets the hash of the value's key.
        /// <para>
        /// This field is for scan performance optimization. Comparing keys may be expensive operation which invokes <see cref="Object.Equals(object)"/>.
        /// We can first compare the hashes of the keys (comparing int values) and then if they are equal compare the full values of the keys.
        /// </para>
        /// </summary>
        public int KeyHash;

        /// <summary>
        /// Gets or sets a usage stamp - the more recently the value in the entry was used, the higher the value of the usage stamp.
        /// </summary>
        public long UsageStamp;

        /// <summary>
        /// Gets or sets a value indicating whether this entry is associated with a key-value pair.
        /// </summary>
        public bool IsUsed => UsageStamp != 0;
    }
}
