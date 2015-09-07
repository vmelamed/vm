using System;
using System.Diagnostics.CodeAnalysis;

namespace vm.Aspects.Facilities
{
    /// <summary>
    /// Interface IGuidGenerator of a facility that generates new GUID-s.
    /// Intended to abstract out the <see cref="M:System.Guid.NewGuid"/> behavior, 
    /// which would allow for introduction of predictable GUID-s used in unit tests.
    /// </summary>
    public interface IGuidGenerator
    {
        /// <summary>
        /// Creates a new GUID value.
        /// </summary>
        /// <returns>New GUID.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification="n/a")]
        Guid NewGuid();
    }
}
