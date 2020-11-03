using System;
using System.Collections.Generic;
using System.Reflection;

namespace vm.Aspects.Diagnostics
{
    /// <summary>
    /// Interface IMemberInfoComparer implements property or field dump order comparison strategy,
    /// e.g. by <see cref="DumpAttribute.Order"/> and then alphabetically.
    /// </summary>
    public interface IMemberInfoComparer : IComparer<MemberInfo>
    {
        /// <summary>
        /// Sets the metadata which provides ordering info by means of <see cref="DumpAttribute.Order"/> and others.
        /// This method can be called before calling the <see cref="IComparer{MemberInfo}"/> methods. If called
        /// more than once and the <paramref name="metadata"/> is different the method should throw an exception.
        /// </summary>
        /// <param name="metadata">The metadata to be set.</param>
        /// <returns>This <see cref="IMemberInfoComparer"/> object, allowing for method chaining.</returns>
        IMemberInfoComparer SetMetadata(Type metadata);
    }
}
