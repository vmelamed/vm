
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using CommandLine;
using CommandLine.Text;

using vm.Aspects.Security.Cryptography.Ciphers.Tools.KeyFile.Properties;

namespace vm.Aspects.Security.Cryptography.Ciphers.Tools.KeyFile.CommandLine
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    [Verb("export",
        HelpText = "Exports the symmetric key from an encrypted key file to the console as a clear-text, hexadecimal byte sequence.")]
    class ExportParameters : Parameters
    {
        [Usage]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static IEnumerable<Example> Examples
        {
            get => new Example[]
            {
                new Example(
                    Resources.HelpTextExportDpapi,
                    new ExportParameters
                    {
                        FileName         = Resources.HelpTextKeyFileName,
                    }),
                new Example(
                    Resources.HelpTextExportCertificate,
                    new ExportParameters
                    {
                        FileName            = Resources.HelpTextKeyFileName,
                        Thumbprint          = Resources.ThumbprintExampleDashes,
                        KeyEncryptionMethod = KeyEncryptionMethod.Certificate,
                    }),
                new Example(
                    Resources.AlternativeThumbprintFormat,
                    new ExportParameters
                    {
                        FileName            = Resources.HelpTextKeyFileName,
                        Thumbprint          = Resources.ThumbprintExampleSpaces,
                        KeyEncryptionMethod = KeyEncryptionMethod.Certificate,
                    }),
                new Example(
                    Resources.HelpTextExportMac,
                    new ExportParameters
                    {
                        FileName            = Resources.HelpTextKeyFileName,
                        Thumbprint          = Resources.ThumbprintExampleNoSeparators,
                        KeyEncryptionMethod = KeyEncryptionMethod.Certificate,
                    }),
            };
        }
    }
}
