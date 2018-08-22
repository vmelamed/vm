namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// The interface <c>IKeyStorage</c> defines the operations for storing or retrieving the encrypted symmetric keys
    /// to and from a storage with the given key location name, e.g. from a file with the given path and filename.
    /// </summary>
    public interface IKeyStorage
    {
        /// <summary>
        /// Tests whether the key's storage location name exists.
        /// </summary>
        /// <param name="keyLocation">The key location.</param>
        /// <returns><see langword="true"/> if the location exists, otherwise <see langword="false"/>.</returns>
        bool KeyLocationExists(string keyLocation);

        /// <summary>
        /// Puts the key to the storage with the specified location name.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="keyLocation">The key location.</param>
        void PutKey(byte[] key, string keyLocation);

        /// <summary>
        /// Gets the key from the storage with the specified location name.
        /// </summary>
        /// <param name="keyLocation">The key location name.</param>
        /// <returns>The encrypted symmetric key.</returns>
        byte[] GetKey(string keyLocation);

        /// <summary>
        /// Deletes the storage with the specified location name.
        /// </summary>
        /// <param name="keyLocation">The key location name to be deleted.</param>
        void DeleteKeyLocation(string keyLocation);
    }
}
