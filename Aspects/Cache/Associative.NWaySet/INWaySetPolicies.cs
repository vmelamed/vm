using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace vm.Aspects.Cache.Associative.NWaySet
{
    /// <summary>
    /// Interface INWaySetPolicies
    /// </summary>
    /// <typeparam name="TKey">The type of the cache's key.</typeparam>
    /// <typeparam name="TValue">The type of the cache's value.</typeparam>
    public interface INWaySetPolicies<TKey, TValue>
    {
        /// <summary>
        /// Gets the hash of a key value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The result of the hash function.</returns>
        int GetKeyHash(
            TKey key);

        /// <summary>
        /// The policy uses the "usage stamps" of two cache entries. The "usage stamp" is a positive <see cref="long" /> value which
        /// represents how recently an item was used - the more recently an item was used - the bigger the value of the usage stamp.
        /// The usage stamp is not a time value. Based on the comparison of two usage stamps the policy returns
        /// <see langword="true" /> if the first item should remain in the cache and the second item maybe evicted from the cache and replaced by a new item;
        /// and <see langword="false" /> if the second item should remain in the cache but first item may get evicted and replaced.
        /// </summary>
        /// <param name="item1Usage">The item1 usage.</param>
        /// <param name="item2Usage">The item2 usage.</param>
        /// <returns>
        /// <see langword="true"/> if the first item should remain in the cache and the second item maybe evicted from the cache and replaced by a new item;
        /// <see langword="false"/> otherwise.</returns>
        bool EvictionPolicy(
            long item1Usage,
            long item2Usage);

        /// <summary>
        /// Policy which determines what happens if a certain key is not found in the cache.
        /// </summary>
        /// <returns>TValue.</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        TValue CacheMissPolicy();

        /// <summary>
        /// The method for replacing an existing key in the cache with a new key.
        /// </summary>
        /// <param name="key">A reference to the existing key.</param>
        /// <param name="newKey">The new key.</param>
        /// <remarks>
        /// The clients may override this policy, so that upon replacement the existing key must be disposed first. See the example below.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#")]
        void ReplaceKey(
            ref TKey key,
            TKey newKey);

        /// <summary>
        /// The method for replacing an existing value in the cache with a new value.
        /// </summary>
        /// <param name="value">A reference to the existing value to be replaced with <paramref name="newValue"/>.</param>
        /// <param name="newValue">The new value to replace the <paramref name="value"/>.</param>
        /// <remarks>
        /// The clients may implement this policy, so that upon replacement the existing data must be disposed first.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#")]
        void ReplaceValue(
            ref TValue value,
            TValue newValue);
    }
}
