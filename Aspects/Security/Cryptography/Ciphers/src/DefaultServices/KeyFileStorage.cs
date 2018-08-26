using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security;
using System.Threading.Tasks;

using vm.Aspects.Security.Cryptography.Ciphers.Properties;

namespace vm.Aspects.Security.Cryptography.Ciphers.DefaultServices
{
    /// <summary>
    /// DefaultKeyFileStorage implements the interface <see cref="IKeyStorage"/> by storing and retrieving the keys to and from a file,
    /// where the store specific key location name is the path and filename of the file containing the key.
    /// </summary>
    public sealed class KeyFileStorage : IKeyStorageTasks
    {
        #region IKeyStorage Members

        /// <summary>
        /// Tests whether the keys file exists.
        /// </summary>
        /// <param name="keyLocation">Here, the key path and filename of the key file.</param>
        /// <returns><see langword="true" /> if the file exists, otherwise <see langword="false" />.</returns>
        public bool KeyLocationExists(
            string keyLocation)
        {
            if (keyLocation.IsNullOrWhiteSpace())
                throw new ArgumentException(Resources.NullOrEmptyArgument, nameof(keyLocation));

            return File.Exists(keyLocation);
        }

        /// <summary>
        /// Tests whether the key's storage location name exists.
        /// </summary>
        /// <param name="keyLocation">Here, the key path and filename of the key file.</param>
        /// <returns><see langword="true"/> if the location exists, otherwise <see langword="false"/>.</returns>
        public async Task<bool> KeyLocationExistsAsync(
            string keyLocation)
        {
            if (keyLocation.IsNullOrWhiteSpace())
                throw new ArgumentException(Resources.NullOrEmptyArgument, nameof(keyLocation));

            return await Task.FromResult(File.Exists(keyLocation));
        }

        /// <summary>
        /// Puts the key in the specified file.
        /// If the file doesn't exist, it creates it, stores the key in it, and sets the appropriate security on the file.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="keyLocation">The key location, here the key path and filename of the key file.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="key"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="key"/> is empty array, or
        /// <paramref name="keyLocation"/> is <see langword="null"/>, empty, or consist of whitespace characters only.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="keyLocation"/> refers to a non-file device, such as &quot;con:&quot;, &quot;com1:&quot;, &quot;lpt1:&quot;, etc. in an NTFS environment.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <paramref name="keyLocation"/> refers to a non-file device, such as &quot;con:&quot;, &quot;com1:&quot;, &quot;lpt1:&quot;, etc. in an NTFS environment.
        /// </exception>
        /// <exception cref="IOException">
        /// I/O error occurred.
        /// </exception>
        /// <exception cref="SecurityException">
        /// The caller does not have the required permission.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// The specified path is invalid, such as being on an unmapped drive. 
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The access requested is not permitted by the operating system for the specified path, 
        /// such as when access is Write or ReadWrite and the file or directory is set for read-only access.
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// The specified path, file name, or both exceed the system-defined maximum length. 
        /// For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. 
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void PutKey(
            byte[] key,
            string keyLocation)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (key.Length == 0)
                throw new ArgumentException("The length of the key cannot be 0.", nameof(key));
            if (keyLocation.IsNullOrWhiteSpace())
                throw new ArgumentException(Resources.NullOrEmptyArgument, nameof(keyLocation));

            var created = !KeyLocationExists(keyLocation);

            using (var stream = new FileStream(keyLocation, FileMode.Create, FileAccess.Write, FileShare.Read))
                stream.Write(key, 0, key.Length);

            if (created)
                SetKeyFileSecurity(keyLocation);
        }

        /// <summary>
        /// Gets the key from the specified file. The file must exist.
        /// </summary>
        /// <param name="keyLocation">The key location, i.e. path and filename of the key file.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="keyLocation"/> is <see langword="null"/>, empty or consists of whitespace characters only.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="keyLocation"/> refers to a non-file device, such as &quot;con:&quot;, &quot;com1:&quot;, &quot;lpt1:&quot;, etc. in an NTFS environment.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <paramref name="keyLocation"/> refers to a non-file device, such as &quot;con:&quot;, &quot;com1:&quot;, &quot;lpt1:&quot;, etc. in an NTFS environment.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// <paramref name="keyLocation"/> refers to a non-existent file.
        /// </exception>
        /// <exception cref="IOException">
        /// I/O error occurred.
        /// </exception>
        /// <exception cref="SecurityException">
        /// The caller does not have the required permission.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// The specified path is invalid, such as being on an unmapped drive. 
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The access requested is not permitted by the operating system for the specified path, 
        /// such as when access is Write or ReadWrite and the file or directory is set for read-only access.
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// The specified path, file name, or both exceed the system-defined maximum length. 
        /// For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. 
        /// </exception>
        public byte[] GetKey(
            string keyLocation)
        {
            byte[] key;

            using (var stream = new FileStream(keyLocation, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                key = new byte[stream.Length];
                stream.Read(key, 0, key.Length);
                return key;
            }
        }

        /// <summary>
        /// Asynchronously puts the key in the specified file. 
        /// If the file doesn't exist it creates it, stores the key and sets the appropriate security on the file.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="keyLocation">The key location, i.e. the path and filename of the key file.</param>
        /// <returns>A <see cref="Task"/> object representing the process of putting the key in the file.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="key"/> is <see langword="null"/> or if the <paramref name="keyLocation"/> is <see langword="null"/>,
        /// empty or consists of whitespace characters only.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="keyLocation"/> refers to a non-file device, such as &quot;con:&quot;, &quot;com1:&quot;, &quot;lpt1:&quot;, etc. in an NTFS environment.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <paramref name="keyLocation"/> refers to a non-file device, such as &quot;con:&quot;, &quot;com1:&quot;, &quot;lpt1:&quot;, etc. in an NTFS environment.
        /// </exception>
        /// <exception cref="IOException">
        /// I/O error occurred.
        /// </exception>
        /// <exception cref="System.Security.SecurityException">
        /// The caller does not have the required permission.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// The specified path is invalid, such as being on an unmapped drive. 
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The access requested is not permitted by the operating system for the specified path, 
        /// such as when access is Write or ReadWrite and the file or directory is set for read-only access.
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// The specified path, file name, or both exceed the system-defined maximum length. 
        /// For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. 
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public async Task PutKeyAsync(
            byte[] key,
            string keyLocation)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (keyLocation.IsNullOrWhiteSpace())
                throw new ArgumentException(Resources.NullOrEmptyArgument, nameof(keyLocation));

            var created = !KeyLocationExists(keyLocation);

            using (var stream = new FileStream(keyLocation, FileMode.Create, FileAccess.Write, FileShare.Read))
                await stream.WriteAsync(key, 0, key.Length);

            if (created)
                SetKeyFileSecurity(keyLocation);
        }

        /// <summary>
        /// Asynchronously gets the key from the specified file. The file must exist.
        /// </summary>
        /// <param name="keyLocation">The key location.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the task of getting the key.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="keyLocation"/> is <see langword="null"/>, empty or consists of whitespace characters only.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="keyLocation"/> refers to a non-file device, such as &quot;con:&quot;, &quot;com1:&quot;, &quot;lpt1:&quot;, etc. in an NTFS environment.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <paramref name="keyLocation"/> refers to a non-file device, such as &quot;con:&quot;, &quot;com1:&quot;, &quot;lpt1:&quot;, etc. in an NTFS environment.
        /// </exception>
        /// <exception cref="T:System.IO.FileNotFoundException">
        /// <paramref name="keyLocation"/> refers to a non-existent file.
        /// </exception>
        /// <exception cref="IOException">
        /// I/O error occurred.
        /// </exception>
        /// <exception cref="System.Security.SecurityException">
        /// The caller does not have the required permission.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// The specified path is invalid, such as being on an unmapped drive. 
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The access requested is not permitted by the operating system for the specified path, 
        /// such as when access is Write or ReadWrite and the file or directory is set for read-only access.
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// The specified path, file name, or both exceed the system-defined maximum length. 
        /// For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. 
        /// </exception>
        public async Task<byte[]> GetKeyAsync(
            string keyLocation)
        {
            if (keyLocation.IsNullOrWhiteSpace())
                throw new ArgumentException(Resources.NullOrEmptyArgument, nameof(keyLocation));

            byte[] key;

            using (var stream = new FileStream(keyLocation, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                key = new byte[stream.Length];
                await stream.ReadAsync(key, 0, key.Length);
                return key;
            }
        }

        /// <summary>
        /// Deletes the storage (the file) with the specified location name (path and filename).
        /// </summary>
        /// <param name="keyLocation">
        /// The key location name to be deleted.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="keyLocation"/> is <see langword="null"/>, empty or consists of only whitespace characters.
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">
        /// The specified path in <paramref name="keyLocation"/> is invalid (for example, it is on an unmapped drive).
        /// </exception>
        /// <exception cref="IOException">
        /// An I/O error occurred.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <paramref name="keyLocation"/> is an invalid file format. 
        /// </exception>
        /// <exception cref="PathTooLongException">
        /// The specified path, file name, or both exceed the system-defined maximum length. For example, 
        /// on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The caller does not have the required permission.
        /// </exception>
        public void DeleteKeyLocation(
            string keyLocation)
        {
            if (!KeyLocationExists(keyLocation))
                return;

            File.Delete(keyLocation);
        }

        /// <summary>
        /// Deletes the storage with the specified location name.
        /// </summary>
        /// <param name="keyLocation">The key location name to be deleted.</param>
        public async Task DeleteKeyLocationAsync(
            string keyLocation)
        {
            if (!await KeyLocationExistsAsync(keyLocation))
                return;

            File.Delete(keyLocation);
        }
        #endregion

        /// <summary>
        /// Sets the key file security by removing all ACL entries and then allowing full control to the file to 
        ///     <list type="bullet">
        ///         <item>the current user,</item>
        ///         <item>the SYSTEM account and</item>
        ///         <item>the Administrators group.</item>
        ///     </list>
        /// </summary>
        /// <param name="keyLocation">The key location.</param>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="keyLocation"/> is <see langword="null"/>, empty or consists of only whitespace characters.
        /// </exception>
        /// <exception cref="IOException">
        /// An I/O error occurred.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The caller does not have the required permission.
        /// </exception>
        /// <exception cref="SystemException">
        /// The file could not be found.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// 
        /// </exception>
        static void SetKeyFileSecurity(
            string keyLocation)
        {
#if NETFRAMEWORK
            var acl = File.GetAccessControl(keyLocation);

            // remove inheritance
            acl.SetAccessRuleProtection(true, false);
            // remove all entries
            foreach (FileSystemAccessRule ace in acl.GetAccessRules(true, true, typeof(SecurityIdentifier)))
                acl.PurgeAccessRules(ace.IdentityReference);

            // add only SYSTEM, Administrators and the current user with full control
            acl.AddAccessRule(
                    new FileSystemAccessRule(
                            new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null),
                            FileSystemRights.FullControl,
                            AccessControlType.Allow));
            acl.AddAccessRule(
                    new FileSystemAccessRule(
                            new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null),
                            FileSystemRights.FullControl,
                            AccessControlType.Allow));
            acl.AddAccessRule(
                    new FileSystemAccessRule(
                            WindowsIdentity.GetCurrent().User,
                            FileSystemRights.FullControl,
                            AccessControlType.Allow));

            // set it
            File.SetAccessControl(keyLocation, acl); 
#endif
        }
    }
}
