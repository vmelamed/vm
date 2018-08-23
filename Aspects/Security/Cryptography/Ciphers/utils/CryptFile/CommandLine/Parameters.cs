using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.RegularExpressions;

using CommandLine;

using vm.Aspects.Security.Cryptography.Ciphers.Utilities.FileCrypt.Properties;

namespace vm.Aspects.Security.Cryptography.Ciphers.Utilities.FileCrypt.CommandLine
{
    abstract class Parameters
    {
        protected const string RexByteArray = @"(?i:^[\?\.-]?[0-9a-f][0-9a-f]((?:-|\s)?[0-9a-f][0-9a-f])*$)";
        protected const string RexCanonize  = @"[^\da-fA-F]+";
        string _thumbprint;
        string _subject;
        string _sourceFileName;
        string _destinationFileName;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is for displaying help usage purpose only.
        /// </summary>
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

        [Value(1, Required = true,
            HelpText = "The name of the destination file that will contain the decrypted or encrypted text of the source file.")]
        public string DestinationFileName
        {
            get => _destinationFileName;
            set
            {
                if (!HelpUsage  &&  new FileInfo(value).Exists)
                {
                    Console.Write(Resources.OverwriteDestinationFile, value);

                    var y = Console.ReadKey(false).KeyChar;

                    Console.WriteLine();
                    if (char.ToUpperInvariant(y) != 'Y')
                        throw new ArgumentException(Resources.DestinationFileAlreadyExists);

                    throw new ArgumentException(Resources.SourceFileNotFound);
                }

                _destinationFileName = value;
            }
        }

        [Option('t', "thumbprint", Required = false,
            HelpText = "A hexadecimal representation of the byte-sequence of the encription certificate's thumbprint. "+
                       "Either the thumbprint or the subject of the certificate must be specified, but not both on the same command line. "+
                       "The certificate must reside in the personal certificates store of the current user. "+
                       "The thumbprint must be in the form \"XX XX XX ... XX\", or XX-XX-...-XX, or XXXXXX...XX where X is a case insensitive, hexadecimal digit.")]
        public string Thumbprint
        {
            get => _thumbprint;
            set
            {
                if (!Subject.IsNullOrWhiteSpace())
                    throw new ArgumentException(Resources.SpecifiedThumbprintAndSubject);

                if (value != null  &&
                    !Regex.IsMatch(value, RexByteArray))
                    throw new ArgumentException(Resources.InvalidThumbprint);

                _thumbprint = value;
            }
        }

        [Option('t', "thumbprint", Required = false,
            HelpText = "The subject of the encription certificate's thumbprint. "+
                       "Either the subject or the thumbprint of the certificate must be specified, but not both on the same command line.")]
        public string Subject
        {
            get => _subject;
            set
            {
                if (!Thumbprint.IsNullOrWhiteSpace())
                    throw new ArgumentException(Resources.SpecifiedThumbprintAndSubject);

                _subject = value;
            }
        }
    }
}
