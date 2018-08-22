using System;
using System.Collections.Generic;
using System.Diagnostics;

using CommandLine;

using vm.Aspects.Diagnostics;
using vm.Aspects.Security.Cryptography.Ciphers.Utilities.KeyFile.CommandLine;
using vm.Aspects.Security.Cryptography.Ciphers.Utilities.KeyFile.Properties;

namespace vm.Aspects.Security.Cryptography.Ciphers.Utilities.KeyFile
{
    class Program
    {
        static int Main(string[] args)
        {
            int result = 0;

            try
            {
                var parser = new Parser(s =>
                {
                    s.CaseInsensitiveEnumValues = true;
                    s.CaseSensitive             = false;
                });

                result = parser
                            .ParseArguments<CreateParameters, ImportParameters, ExportParameters>(args)
                            .MapResult(
                                (CreateParameters p) => Create(p),
                                (ImportParameters p) => Import(p),
                                (ExportParameters p) => Export(p),
                                errors => 1);
            }
            catch (Exception x)
            {
#if DEBUG
                ClassMetadataRegistrar.RegisterMetadata();
                Debug.WriteLine(x.DumpString());
#endif
                Console.WriteLine(x.Message);

                result = 1;
            }

#if DEBUG
            Console.Write(Resources.PressAnyKey);
            Console.ReadKey(true);
#endif
            return result;
        }

        static IKeyStorage GetKeyStorage()
            => new DefaultServices.KeyFileStorage();

        static readonly IDictionary<KeyEncryptionMethod, Func<Parameters, IKeyManagement>> _cipherFactories =
            new SortedDictionary<KeyEncryptionMethod, Func<Parameters, IKeyManagement>>
            {
                [KeyEncryptionMethod.DPAPI]       = p => new ProtectedKeyCipher(p.FileName),
                [KeyEncryptionMethod.Certificate] = p => new EncryptedKeyCipher(p.Certificate, p.FileName),
                [KeyEncryptionMethod.MAC]         = p => new KeyedHasher(p.Certificate, p.FileName),
            };

        static readonly IDictionary<KeyEncryptionMethod, Action<IKeyManagement>> _keyGenerators =
            new SortedDictionary<KeyEncryptionMethod, Action<IKeyManagement>>
            {
                [KeyEncryptionMethod.DPAPI]       = k => ((ICipher)k).Encrypt(new byte[] { 0, 1, 2, 3 }),
                [KeyEncryptionMethod.Certificate] = k => ((ICipher)k).Encrypt(new byte[] { 0, 1, 2, 3 }),
                [KeyEncryptionMethod.MAC]         = k => ((IHasher)k).Hash(new byte[] { 0, 1, 2, 3 }),
            };

        static IKeyManagement GetKeyManager(Parameters p)
            => _cipherFactories[p.KeyEncryptionMethod](p);

        static void GenerateKey(Parameters p, IKeyManagement keyManagement)
            => _keyGenerators[p.KeyEncryptionMethod](keyManagement);

        static int Import(
            ImportParameters p)
        {
            if (GetKeyManager(p) is IKeyManagement keyManagement)
                keyManagement.ImportSymmetricKey(p.BinaryKey);
            return 0;
        }

        static int Export(
            ExportParameters p)
        {
            if (GetKeyManager(p) is IKeyManagement keyManagement)
            {
                Console.WriteLine(
                    BitConverter.ToString(
                        keyManagement.ExportSymmetricKey()));
            }
            return 0;
        }

        static int Create(
            CreateParameters p)
        {
            var keyStorage = GetKeyStorage();

            if (keyStorage.KeyLocationExists(p.FileName))
                keyStorage.DeleteKeyLocation(p.FileName);

            var keyManager = GetKeyManager(p);

            GenerateKey(p, keyManager);

            Debug.Assert(keyStorage.KeyLocationExists(p.FileName));
            return 0;
        }
    }
}
