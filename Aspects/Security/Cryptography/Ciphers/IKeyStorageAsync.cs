using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using vm.Aspects.Security.Cryptography.Ciphers.Contracts;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// The interface <c>IKeyStorageAsync</c> extends <c>IKeyStorage</c> with asynchronous versions of the methods 
    /// <see cref="M:PutKey"/> and <see cref="M:GetKey"/>.
    /// </summary>
    [ContractClass(typeof(IKeyStorageAsyncContract))]
    public interface IKeyStorageAsync : IKeyStorage
    {
        /// <summary>
        /// Asynchronously puts the key to the storage with the specified location name.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="keyLocation">The key location.</param>
        /// <returns>A <see cref="T:Task"/> object representing the process of putting the encrypted symmetric key in the storage.</returns>
        Task PutKeyAsync(byte[] key, string keyLocation);

        /// <summary>
        /// Asynchronously gets the key from the storage with the specified location name.
        /// </summary>
        /// <param name="keyLocation">The key location name.</param>
        /// <returns>A <see cref="T:Task"/> object representing the process of getting the encrypted symmetric key from the storage.</returns>
        Task<byte[]> GetKeyAsync(string keyLocation);
    }
}
