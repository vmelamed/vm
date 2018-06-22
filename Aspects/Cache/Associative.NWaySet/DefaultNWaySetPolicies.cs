using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace vm.Aspects.Cache.Associative.NWaySet
{
    /// <summary>
    /// The default implementation of <see cref="INWaySetPolicies{TKey, TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the cache's key.</typeparam>
    /// <typeparam name="TValue">The type of the cache's value.</typeparam>
    /// <seealso cref="INWaySetPolicies{TKey, TValue}" />
    public class DefaultNWaySetPolicies<TKey, TValue> : INWaySetPolicies<TKey, TValue>
    {
        #region Policies
        /// <summary>
        /// Gets the hash of a key value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The result of the hash function.</returns>
        /// <remarks>
        /// The default implementation of the policy is to simply invoke the <see cref="object.GetHashCode"/> on the key object.
        /// The clients can override this by implementing more elaborate algorithm. E.g. if the client knows that the keys are URL-s,
        /// they can return the hash of the server address only.
        /// </remarks>
        /// <example>
        /// <![CDATA[public override int GetHash(Uri key) => key.Host.GetHashCode();]]>
        /// </example>
        public virtual int GetKeyHash(TKey key) => key.GetHashCode();

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
        /// The clients may override this policy, so that upon replacement the existing key must be disposed first. See the example below.
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
        /// The clients may override this policy, so that upon replacement the existing data must be disposed first. See the example below.
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
    }
}
