using System.Threading.Tasks;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// The interface <c>IKeyStorageAsync</c> extends <c>IKeyStorage</c> with asynchronous versions of the methods 
    /// </summary>
    public interface IKeyStorageAsync : IKeyStorage
    {
        /// <summary>
        /// Tests whether the key's storage location name exists.
        /// </summary>
        /// <param name="keyLocation">The key location.</param>
        /// <returns><see langword="true"/> if the location exists, otherwise <see langword="false"/>.</returns>
        Task<bool> KeyLocationExistsAsync(string keyLocation);

        /// <summary>
        /// Asynchronously puts the key to the storage with the specified location name.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="keyLocation">The key location.</param>
        /// <returns>A <see cref="Task"/> object representing the process of putting the encrypted symmetric key in the storage.</returns>
        Task PutKeyAsync(byte[] key, string keyLocation);

        /// <summary>
        /// Asynchronously gets the key from the storage with the specified location name.
        /// </summary>
        /// <param name="keyLocation">The key location name.</param>
        /// <returns>A <see cref="Task"/> object representing the process of getting the encrypted symmetric key from the storage.</returns>
        Task<byte[]> GetKeyAsync(string keyLocation);

        /// <summary>
        /// Deletes the storage with the specified location name.
        /// </summary>
        /// <param name="keyLocation">The key location name to be deleted.</param>
        Task DeleteKeyLocationAsync(string keyLocation);
    }
}
