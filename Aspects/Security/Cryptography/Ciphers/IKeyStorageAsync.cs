using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// The interface <c>IKeyStorageAsync</c> extends <c>IKeyStorage</c> with asynchronous versions of the methods 
    /// </summary>
    [ContractClass(typeof(IKeyStorageAsyncContract))]
    public interface IKeyStorageAsync : IKeyStorage
    {
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
    }

    [ContractClassFor(typeof(IKeyStorageAsync))]
    abstract class IKeyStorageAsyncContract : IKeyStorageAsync
    {
        #region IKeyStorage Members
        public bool KeyLocationExists(
            string keyLocation)
        {
            throw new NotImplementedException();
        }

        public void PutKey(
            byte[] key,
            string keyLocation)
        {
            throw new NotImplementedException();
        }

        public byte[] GetKey(
            string keyLocation)
        {
            throw new NotImplementedException();
        }

        public void DeleteKeyLocation(
            string keyLocation)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IKeyStorageAsync Members
        public Task PutKeyAsync(byte[] key, string keyLocation)
        {
            Contract.Requires<ArgumentException>(keyLocation != null  &&  keyLocation.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(keyLocation)+" cannot be null, empty string or consist of whitespace characters only.");
            Contract.Requires<ArgumentNullException>(key != null, nameof(key));

            Contract.Ensures(Contract.Result<Task>() != null);

            throw new NotImplementedException();
        }

        public Task<byte[]> GetKeyAsync(string keyLocation)
        {
            Contract.Requires<ArgumentException>(keyLocation != null  &&  keyLocation.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(keyLocation)+" cannot be null, empty string or consist of whitespace characters only.");

            Contract.Ensures(Contract.Result<byte[]>() != null, "The returned value is null.");
            Contract.Ensures(Contract.Result<byte[]>().Length != 0, "The returned value has 0 length.");

            throw new NotImplementedException();
        }
        #endregion
    }
}
