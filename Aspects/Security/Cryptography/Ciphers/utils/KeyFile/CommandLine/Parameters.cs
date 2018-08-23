using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

using CommandLine;

using vm.Aspects.Security.Cryptography.Ciphers.Utilities.KeyFile.Properties;

namespace vm.Aspects.Security.Cryptography.Ciphers.Utilities.KeyFile.CommandLine
{
    abstract class Parameters
    {
        protected const string RexByteArray = @"(?i:^[\?\.-]?[0-9a-f][0-9a-f]((?:-|\s)?[0-9a-f][0-9a-f])*$)";
        protected const string RexCanonize  = @"[^\da-fA-F]+";

        string _thumbprint;

        /// <summary>
        /// Gets or sets the name of the file that contains the encrypted symmetric key.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// </exception>
        [Value(0, Required = true,
            HelpText = "The name of the file that contains the encrypted symmetric key.")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the key encryption method. The default is DPAPI.
        /// </summary>
        [Option('m', "method", Default = KeyEncryptionMethod.DPAPI, Required = false,
            HelpText = "Specifies the method for encrypting the symmetric key in the key file. Allowed values: "+
                       "DPAPI - for use with instances of ProtectedKeyCipher; or "+
                       "Certificate - for use with EncryptedKeyCipher instances; or "+
                       "MAC - for use with KeyedHasher instances.")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public KeyEncryptionMethod KeyEncryptionMethod { get; set; }

        /// <summary>
        /// Gets or sets a hexadecimal sequence of the bytes of the thumbprint.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// </exception>
        [Option('t', "thumbprint", Required = false,
            HelpText = "Hexadecimal representation of the byte sequence of the certificate's thumbprint to be used for encrypting the symmetric key. "+
                       "The certificate must reside in the personal certificates store of the current user. "+
                       "The thumbprint must be in the form \"XX XX XX ... XX\", or XX-XX-...-XX, or XXXXXX...XX where X is a case insensitive, hexadecimal digit. "+
                       "Mandatory if the selected encryption method is certificate or MAC hasher, otherwise it is ignored.")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string Thumbprint
        {
            get
            {
                if (_thumbprint.IsNullOrWhiteSpace()  &&  KeyEncryptionMethod != KeyEncryptionMethod.DPAPI)
                    throw new ArgumentException(Resources.SpecifyThumbprint);

                return _thumbprint;
            }
            set
            {
                if (!Regex.IsMatch(value, RexByteArray))
                    throw new ArgumentException(Resources.InvalidThumbprint);

                _thumbprint = value;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [Option('w', "wait", Required = false, Hidden = true,
            HelpText = "Specify to prompt for any key and wait for it at the end of the program. For debug purposes only.")]
        public bool WaitAtTheEnd { get; set; }

        /// <summary>
        /// Gets the certificate with thumbprint <see cref="Thumbprint"/> from the current user's certificate store.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// </exception>
        public X509Certificate2 Certificate
        {
            get
            {
                if (Thumbprint == null)
                    throw new ArgumentException(Resources.InvalidThumbprint);

                using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
                {
                    store.Open(OpenFlags.ReadOnly);
                    return store.Certificates
                                .Find(X509FindType.FindByThumbprint, CanonizeHexBytes(Thumbprint), false)
                                .Find(X509FindType.FindByTimeValid, DateTime.Now, false)
                                .OfType<X509Certificate2>()
                                .FirstOrDefault()  ??  throw new ArgumentException(Resources.CannotFindCert);
                }
            }
        }

        /// <summary>
        /// Canonizes a hexadecimal representation of sequence of bytes by striping off all byte separating characters
        /// and converting all lowercase digits to uppercase. E.g. &quot;01-0a-0F&quot; becomes &quot;010A0F&quot;
        /// </summary>
        /// <param name="hexBytes">The hexadecimal representation of a sequence of byte.</param>
        /// <returns>System.String.</returns>
        protected static string CanonizeHexBytes(string hexBytes)
            => Regex
                .Replace(hexBytes, RexCanonize, string.Empty)
                .ToUpper(CultureInfo.InvariantCulture)
                ;

        /// <summary>
        /// Parses a canonized hexadecimal representation of sequence of bytes to byte array.
        /// </summary>
        /// <param name="canonizedHexBytes">The canonized hexadecimal representation of sequence of bytes.</param>
        /// <returns>byte[].</returns>
        protected static byte[] ParseHexString(string canonizedHexBytes)
        {
            var hexValue = new byte[canonizedHexBytes.Length/2];

            for (var i = 0; i<canonizedHexBytes.Length; i += 2)
                hexValue[i/2] = byte.Parse(canonizedHexBytes.Substring(i, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);

            return hexValue;
        }
    }
}
