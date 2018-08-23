
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using CommandLine;
using CommandLine.Text;

using vm.Aspects.Security.Cryptography.Ciphers.Utilities.KeyFile.Properties;

namespace vm.Aspects.Security.Cryptography.Ciphers.Utilities.KeyFile.CommandLine
{
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    [Verb("create",
        HelpText = "Creates a new file containing encrypted, randomly generated, symmetric key "+
                   "for use by instances of the classes ProtectedKeyCipher, EncryptedKeyCipher and KeyedHasher "+
                   "(from the vm.Aspects.Security.Cryptography.Ciphers namespace)")]
    class CreateParameters : Parameters
    {
        [Option('o', "overwrite", Required = false, Default = false,
            HelpText = "If specified, the utility will assume that if the key file exists the user wants to overwrite it.")]
        public bool Overwrite { get; set; }

        [Usage]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static IEnumerable<Example> Examples
        {
            get => new Example[]
            {
                new Example(
                    Resources.HelpTextCreateDpapi,
                    new CreateParameters
                    {
                        FileName            = Resources.HelpTextKeyFileName,
                    }),
                new Example(
                    Resources.HelpTextCreateCertificate,
                    new CreateParameters
                    {
                        FileName            = Resources.HelpTextKeyFileName,
                        Thumbprint          = Resources.ThumbprintExampleSpaces,
                        KeyEncryptionMethod = KeyEncryptionMethod.Certificate,
                    }),
                new Example(
                    Resources.AlternativeThumbprintFormatQuiet,
                    new CreateParameters
                    {
                        FileName            = Resources.HelpTextKeyFileName,
                        Thumbprint          = Resources.ThumbprintExampleNoSeparators,
                        KeyEncryptionMethod = KeyEncryptionMethod.Certificate,
                        Overwrite           = true,
                    }),
                new Example(
                    Resources.HelpTextCreateMac,
                    new CreateParameters
                    {
                        FileName            = Resources.HelpTextKeyFileName,
                        Thumbprint          = Resources.ThumbprintExampleDashes,
                        KeyEncryptionMethod = KeyEncryptionMethod.MAC,
                    }),
            };
        }
    }
}
