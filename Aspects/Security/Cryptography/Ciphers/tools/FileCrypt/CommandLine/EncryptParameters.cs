using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using CommandLine;
using CommandLine.Text;

using vm.Aspects.Security.Cryptography.Ciphers.Tools.FileCrypt.Properties;

namespace vm.Aspects.Security.Cryptography.Ciphers.Tools.FileCrypt.CommandLine
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    [Verb("encrypt",
        HelpText = "Encrypts the source file into the destination file using the specified certificate.")]
    class EncryptParameters : Parameters
    {
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [Option('b', "base64", Required = false,
            HelpText = "The encrypted text is or should be Base64 encoded.")]
        public bool Base64Encoded { get; set; }

        [Usage]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static IEnumerable<Example> Examples
        {
            get => new Example[]
            {
                new Example(
                    Resources.HelpTextEncryptWithThumbprint,
                    new EncryptParameters
                    {
                        HelpUsage           = true,
                        SourceFileName      = Resources.SourceClearText,
                        DestinationFileName = Resources.DestinationEncryptedBin,
                        Thumbprint          = Resources.SampleThumbprint,
                        Overwrite           = false,
                        Base64Encoded       = false,
                    }),
                new Example(
                    Resources.HelpTextEncryptWithSubject,
                    new EncryptParameters
                    {
                        HelpUsage           = true,
                        SourceFileName      = Resources.SourceClearText,
                        DestinationFileName = Resources.DestinationEncryptedTxt,
                        Subject             = Resources.SampleSubject,
                        Overwrite           = true,
                        Base64Encoded       = true,
                    }),
            };
        }
    }
}
