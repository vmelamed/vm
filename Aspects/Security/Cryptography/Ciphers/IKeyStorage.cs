using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// The interface <c>IKeyStorage</c> defines the operations for storing or retrieving the encrypted symmetric keys
    /// to and from a storage with the given key location name, e.g. from a file with the given path and filename.
    /// </summary>
    [ContractClass(typeof(IKeyStorageContract))]
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

    [ContractClassFor(typeof(IKeyStorage))]
    abstract class IKeyStorageContract : IKeyStorage
    {
        #region IKeyStorage Members

        public bool KeyLocationExists(string keyLocation)
        {
            Contract.Requires<ArgumentNullException>(keyLocation!=null, nameof(keyLocation));
            Contract.Requires<ArgumentException>(keyLocation.Length > 0, "The argument "+nameof(keyLocation)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(keyLocation.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(keyLocation)+" cannot be empty or consist of whitespace characters only.");

            throw new NotImplementedException();
        }

        public void PutKey(byte[] key, string keyLocation)
        {
            Contract.Requires<ArgumentNullException>(key != null, nameof(key));
            Contract.Requires<ArgumentException>(key.Length != 0, "The length of the array in the argument "+nameof(key)+" cannot be 0.");
            Contract.Requires<ArgumentNullException>(keyLocation != null, nameof(keyLocation));
            Contract.Requires<ArgumentNullException>(keyLocation!=null, nameof(keyLocation));
            Contract.Requires<ArgumentException>(keyLocation.Length > 0, "The argument "+nameof(keyLocation)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(keyLocation.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(keyLocation)+" cannot be empty or consist of whitespace characters only.");

            throw new NotImplementedException();
        }

        public byte[] GetKey(string keyLocation)
        {
            Contract.Requires<ArgumentNullException>(keyLocation!=null, nameof(keyLocation));
            Contract.Requires<ArgumentException>(keyLocation.Length > 0, "The argument "+nameof(keyLocation)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(keyLocation.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(keyLocation)+" cannot be empty or consist of whitespace characters only.");
            Contract.Ensures(Contract.Result<byte[]>() != null, "The returned key is null.");
            Contract.Ensures(Contract.Result<byte[]>().Length != 0, "The returned key has 0 length.");

            throw new NotImplementedException();
        }

        public void DeleteKeyLocation(string keyLocation)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
