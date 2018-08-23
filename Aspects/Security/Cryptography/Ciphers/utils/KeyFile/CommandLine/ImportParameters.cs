using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

using CommandLine;
using CommandLine.Text;

using vm.Aspects.Security.Cryptography.Ciphers.Utilities.KeyFile.Properties;

namespace vm.Aspects.Security.Cryptography.Ciphers.Utilities.KeyFile.CommandLine
{
    [Verb("import",
        HelpText = "Imports a clear-text, hexadecimal, byte sequence string from a command line parameter "+
                   "as a symmetric key into an encrypted key file.")]
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    class ImportParameters : Parameters
    {
        string _key;

        [Option('k', "key", Required = true,
            HelpText = @"Hexadecimal representation of the byte sequence of the clear-text, symmetric key in the form "+
                       @"XX-XX-XX-...-XX, or ""XX XX XX...XX"", or XXXXXX...XX where X are case insensitive, hexadecimal digits.")]
        public string Key
        {
            get => _key;
            set
            {
                if (!Regex.IsMatch(value, RexByteArray))
                    throw new ArgumentException(Resources.InvalidKey);

                _key = value;
            }
        }

        [Usage]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static IEnumerable<Example> Examples
        {
            get => new Example[]
            {
                new Example(
                    Resources.HelpTextImportDpapi,
                    new ImportParameters
                    {
                        FileName         = Resources.HelpTextKeyFileName,
                        Key              = Resources.KeyExampleNoSeparators,
                    }),
                new Example(
                    Resources.HelpTextImportCertificate,
                    new ImportParameters
                    {
                        FileName            = Resources.HelpTextKeyFileName,
                        Key                 = Resources.KeyExampleDashes,
                        Thumbprint          = Resources.ThumbprintExampleDashes,
                        KeyEncryptionMethod = KeyEncryptionMethod.Certificate,
                    }),
                new Example(
                    Resources.AlternativeThumbprintFormat,
                    new ImportParameters
                    {
                        FileName            = Resources.HelpTextKeyFileName,
                        Key                 = Resources.KeyExampleDashes,
                        Thumbprint          = Resources.ThumbprintExampleSpaces,
                        KeyEncryptionMethod = KeyEncryptionMethod.Certificate,
                    }),
                new Example(
                    Resources.HelpTextImportMac,
                    new ImportParameters
                    {
                        FileName            = Resources.HelpTextKeyFileName,
                        Key                 = Resources.KeyExampleSpaces,
                        Thumbprint          = Resources.ThumbprintExampleNoSeparators,
                        KeyEncryptionMethod = KeyEncryptionMethod.Certificate,
                    }),
            };
        }

        public byte[] BinaryKey => ParseHexString(CanonizeHexBytes(Key));
    }
}
