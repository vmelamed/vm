using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

using CommandLine;

using vm.Aspects.Security.Cryptography.Ciphers.Tools.FileCrypt.Properties;

namespace vm.Aspects.Security.Cryptography.Ciphers.Tools.FileCrypt.CommandLine
{
    abstract class Parameters
    {
        protected const string RexByteArray = @"(?i:^[\?\.-]?[0-9a-f][0-9a-f]((?:-|\s)?[0-9a-f][0-9a-f])*$)";
        protected const string RexCanonize  = @"[^\da-fA-F]+";
        string _thumbprint;
        string _subject;
        string _sourceFileName;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is for displaying help usage purpose only.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public bool HelpUsage { get; set; }

        /// <summary>
        /// Gets or sets the name of the file that contains the encrypted symmetric key.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// </exception>
        [Value(0, Required = true,
            HelpText = "The name of the source file that needs to be encrypted or decrypted.")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string SourceFileName
        {
            get => _sourceFileName;
            set
            {
                if (!HelpUsage  &&  !new FileInfo(value).Exists)
                    throw new ArgumentException(Resources.SourceFileNotFound);

                _sourceFileName = value;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [Value(1, Required = true,
            HelpText = "The name of the destination file that will contain the decrypted or encrypted text of the source file.")]
        public string DestinationFileName { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [Option('t', "thumbprint", Required = false,
            HelpText = "A hexadecimal representation of the byte-sequence of the encryption certificate's thumbprint. "+
                       "Either the thumbprint or the subject of the certificate must be specified, but not both on the same command line. "+
                       "The certificate must reside in the personal certificates store of the current user. "+
                       "The thumbprint must be in the form \"XX XX XX ... XX\", or XX-XX-...-XX, or XXXXXX...XX where X is a case insensitive, hexadecimal digit.")]
        public string Thumbprint
        {
            get => _thumbprint;
            set
            {
                if (value!=null  &&  !Subject.IsNullOrWhiteSpace())
                    throw new ArgumentException(Resources.SpecifiedThumbprintAndSubject);

                if (value != null  &&
                    !Regex.IsMatch(value, RexByteArray))
                    throw new ArgumentException(Resources.InvalidThumbprint);

                _thumbprint = value;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [Option('s', "subject", Required = false,
            HelpText = "The subject of the encryption certificate's thumbprint. "+
                       "Either the subject or the thumbprint of the certificate must be specified, but not both on the same command line.")]
        public string Subject
        {
            get => _subject;
            set
            {
                if (value!=null  &&  !Thumbprint.IsNullOrWhiteSpace())
                    throw new ArgumentException(Resources.SpecifiedThumbprintAndSubject);

                _subject = value;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [Option('o', "overwrite", Required = false,
            HelpText = "Overwrite the destination file if it already exists.")]
        public bool Overwrite { get; set; }

        public X509Certificate2 Certificate
        {
            get
            {
                if (!(Thumbprint.IsNullOrWhiteSpace()  ^  Subject.IsNullOrWhiteSpace()))
                    throw new ArgumentException(Resources.MissingCertificate);

                using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
                {
                    store.Open(OpenFlags.ReadOnly);

                    return (!Subject.IsNullOrWhiteSpace()
                                    ? store.Certificates.Find(X509FindType.FindBySubjectName, Subject, false)
                                    : store.Certificates.Find(X509FindType.FindByThumbprint, Thumbprint, false))
                                .Find(X509FindType.FindByTimeValid, DateTime.Now, false)
                                .OfType<X509Certificate2>()
                                .FirstOrDefault()  ??  throw new ArgumentException(Resources.CertNotFound);
                }
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [Option('w', "wait", Required = false, Hidden = true,
            HelpText = "Specify to prompt for any key and wait for it at the end of the program. For debug purposes only.")]
        public bool WaitAtTheEnd { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        byte[] ThumbprintBytes => ParseHexString(CanonizeHexBytes(Thumbprint));

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        static string CanonizeHexBytes(string hexBytes)
            => Regex
                .Replace(hexBytes, RexCanonize, string.Empty)
                .ToUpper(CultureInfo.InvariantCulture)
                ;

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        static byte[] ParseHexString(string canonizedHexBytes)
        {
            var hexValue = new byte[canonizedHexBytes.Length/2];

            for (var i = 0; i<canonizedHexBytes.Length; i += 2)
                hexValue[i/2] = byte.Parse(canonizedHexBytes.Substring(i, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);

            return hexValue;
        }

        public override string ToString() => this.DumpString();
    }
}
