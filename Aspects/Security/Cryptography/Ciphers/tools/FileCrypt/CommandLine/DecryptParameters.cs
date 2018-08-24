using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using CommandLine;
using CommandLine.Text;

using vm.Aspects.Security.Cryptography.Ciphers.Tools.FileCrypt.Properties;

namespace vm.Aspects.Security.Cryptography.Ciphers.Tools.FileCrypt.CommandLine
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    [Verb("decrypt",
        HelpText = "Decrypts the source file into the destination file using the specified certificate.")]
    class DecryptParameters : Parameters
    {
        [Usage]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static IEnumerable<Example> Examples
        {
            get => new Example[]
            {
                new Example(
                    Resources.HelpTextDecryptWithThumbprint,
                    new DecryptParameters
                    {
                        HelpUsage           = true,
                        SourceFileName      = Resources.SourceEncryptedBin,
                        DestinationFileName = Resources.DestinationClearTxt,
                        Thumbprint          = "31 89 82 59 8e 4a c1 43 df c9 58 96 a8 f7 2b f0 8c 5d f9 f4",
                        Overwrite           = false,
                    }),
                new Example(
                    Resources.HelpTextDecryptWithSubject,
                    new DecryptParameters
                    {
                        HelpUsage           = true,
                        SourceFileName      = Resources.SourceEncryptedTxt,
                        DestinationFileName = Resources.DestinationClearTxt,
                        Subject             = Resources.SampleSubject,
                        Overwrite           = true,
                    }),
            };
        }
    }
}
