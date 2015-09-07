using System;

namespace vm.Aspects.Facilities
{
    /// <summary>
    /// Accesses the system GUID generator provided by <see cref="M:System.Guid.NewGuid"/>
    /// </summary>
    public class GuidGenerator : IGuidGenerator
    {
        /// <summary>
        /// Creates a new GUID value.
        /// </summary>
        /// <returns>New GUID.</returns>
        public Guid NewGuid()
        {
            return Guid.NewGuid();
        }
    }
}
