using System.Collections.Generic;

namespace vm.Aspects.Diagnostics.Implementation
{
    /// <summary>
    /// Register of the objects that have already been dumped.
    /// </summary>
    class DumpedObjects
    {
        /// <summary>
        /// Gets the object register.
        /// </summary>
        /// <value>
        /// The register.
        /// </value>
        IDictionary<object, string> Register { get; } = new Dictionary<object, string>();

        /// <summary>
        /// Gets a value indicating whether the register is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is empty;
        ///   otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty => Register.Count == 0;

        /// <summary>
        /// Adds the specified object to the register.
        /// </summary>
        /// <param name="obj">
        /// The object to register.
        /// </param>
        /// <returns>The reference identifier.</returns>
        public string Add(object obj) =>
            Register.TryGetValue(obj, out var reference)
                ? reference
                : (Register[obj] = GetReference(obj));

        /// <summary>Determines whether this register contains the object.</summary>
        /// <param name="obj">The object.</param>
        /// <returns>
        ///   <para>
        ///   If the object is registered (i.e. already dumped) the first element of the returned tuple will be
        ///   <c>true</c> and the second will be a unique reference identifier; otherwise the first element will be
        ///   <c>false</c> and the second - empty string.</para>
        /// </returns>
        public (bool, string) Contains(object obj) =>
            (Register.TryGetValue(obj, out var reference), reference ?? "");

        /// <summary>
        /// Empties the register.
        /// </summary>
        public void Clear() => Register.Clear();

        /// <summary>
        /// Generates a unique reference to the dumped already object.
        /// </summary>
        /// <param name="obj">
        /// The object.
        /// </param>
        /// <returns>
        /// A string that can be used as a ref. identifier.
        /// </returns>
        string GetReference(object obj) => $"{obj.GetType().Name}--{Register.Count}";
    }
}
