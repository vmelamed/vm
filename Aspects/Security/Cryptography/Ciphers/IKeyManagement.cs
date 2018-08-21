namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// The interface IKeyManagement defines the behavior of managing the encryption key(s) used by the ciphers.
    /// </summary>
    public interface IKeyManagement
    {
        /// <summary>
        /// Gets the physical storage location name of a symmetric key as it would be understood by the <see cref="IKeyStorageTasks"/>,
        /// e.g. the path and filename of a file.
        /// </summary>
        string KeyLocation { get; }

        /// <summary>
        /// Exports the symmetric key as a clear text from the implementing instance.
        /// If the key is not loaded yet by the implementing instance, the method may access an associated <see cref="IKeyStorage"/> to retrieve it.
        /// </summary>
        /// <returns>Array of bytes of the symmetric key or <see langword="null"/> if the cipher does not have a symmetric key.</returns>
        byte[] ExportSymmetricKey();

        /// <summary>
        /// Imports the symmetric key as a clear text into the implementing instance.
        /// The method may access an associated <see cref="IKeyStorage"/> to store the key.
        /// </summary>
        /// <param name="key">The key.</param>
        void ImportSymmetricKey(byte[] key);
    }
}
