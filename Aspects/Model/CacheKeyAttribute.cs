using System;

namespace vm.Aspects.Model
{
    /// <summary>
    /// Marks a cached class's property as a cache key by which the instances can be found in the cache.
    /// This class cannot be inherited.
    /// </summary>
    /// <remarks>
    /// Note that the attribute is ignored if the class is not marked with <see cref="CachedAttribute"/>.
    /// </remarks>
    /// <seealso cref="Attribute" />
    [AttributeUsage(
        AttributeTargets.Property | AttributeTargets.Field,
        AllowMultiple = false,
        Inherited = true)]
    public sealed class CacheKeyAttribute : Attribute
    {
        /// <summary>
        /// Gets a value indicating whether the marked property should be the one and only primary key.
        /// If a primary key is not designated, then if the class implements <see cref="IHasStoreId{TId}"/> the caching system should
        /// assume <see cref="IHasStoreId{TId}.Id"/> to be the primary index. And if the class does not implement the interface the 
        /// system may throw <see cref="InvalidOperationException"/>.
        /// </summary>
        public bool IsPrimary { get; set; }
    }
}
