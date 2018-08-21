using System.Threading.Tasks;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// The interface IKeyManagement defines the behavior of managing the encryption key(s) used by the ciphers.
    /// </summary>
    public interface IKeyManagementTasks
    {
        /// <summary>
        /// Gets the physical storage location name of a symmetric key as it would be understood by the <see cref="IKeyStorageTasks"/>,
        /// e.g. the path and filename of a file.
        /// </summary>
        string KeyLocation { get; }

        /// <summary>
        /// Asynchronously exports the symmetric key as a clear text.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> object representing the process of asynchronously exporting the symmetric key including the result -
        /// array of bytes of the symmetric key or <see langword="null"/> if the cipher does not have a symmetric key.
        /// </returns>
        Task<byte[]> ExportSymmetricKeyAsync();

        /// <summary>
        /// Asynchronously imports the symmetric key as a clear text.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the process of asynchronously importing the symmetric key.
        /// </returns>
        Task ImportSymmetricKeyAsync(byte[] key);
    }
}
