using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading.Tasks;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// The class <c>KeyFile</c> implements the interface <see cref="IKeyStorage"/> by storing and retrieving the keys from a file where the key location name is the path and filename.
    /// </summary>
    public sealed class KeyFile : IKeyStorageAsync
    {
        #region IKeyStorage Members

        /// <summary>
        /// Tests whether the keys file exists.
        /// </summary>
        /// <param name="keyLocation">The key file name.</param>
        /// <returns><see langword="true" /> if the file exists, otherwise <see langword="false" />.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool KeyLocationExists(
            string keyLocation)
        {
            return File.Exists(keyLocation);
        }

        /// <summary>
        /// Puts the key in the specified file. If the file doesn't exist it creates it, stores the key and sets the appropriate security on the file.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="keyLocation">The key location.</param>
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
        /// <exception cref="T:System.Security.SecurityException">
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
            var created = !KeyLocationExists(keyLocation);

            using (var stream = new FileStream(keyLocation, FileMode.Create, FileAccess.Write, FileShare.Read))
                stream.Write(key, 0, key.Length);

            if (created)
                SetKeyFileSecurity(keyLocation);
        }

        /// <summary>
        /// Gets the key from the specified file. The file must exist.
        /// </summary>
        /// <param name="keyLocation">The key location.</param>
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
        /// <param name="keyLocation">The key location.</param>
        /// <returns>A <see cref="T:Task"/> object representing the process of putting the key in the file.</returns>
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
        /// A <see cref="T:Task"/> object representing the task of getting the key.
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
            byte[] key;

            using (var stream = new FileStream(keyLocation, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                key = new byte[stream.Length];
                await stream.ReadAsync(key, 0, key.Length);
                return key;
            }
        }

        /// <summary>
        /// Deletes the storage (the file) with the specified location name.
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
        }
    }
}
