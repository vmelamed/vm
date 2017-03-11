using System;

namespace vm.Aspects.Model
{
    /// <summary>
    /// The attribute marks the class (presumably entity or value class) to which it is applied as cached in a second level cache facility.
    /// This class cannot be inherited.
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(
        AttributeTargets.Class,
        AllowMultiple = false,
        Inherited = true)]
    public sealed class CachedAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of a class's property that will be used as the cache's primary key.
        /// </summary>
        public string PrimaryKey { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedAttribute"/> class.
        /// </summary>
        /// <param name="primaryKey">
        /// The name of a class's property that will be used as the cache's primary key.
        /// If omitted the primary database key will be used.
        /// </param>
        public CachedAttribute(string primaryKey = null)
        {
            PrimaryKey = primaryKey;
        }
    }
}
