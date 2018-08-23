using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using CommandLine;

using vm.Aspects.Diagnostics;
using vm.Aspects.Security.Cryptography.Ciphers.Utilities.FileCrypt.CommandLine;
using vm.Aspects.Security.Cryptography.Ciphers.Utilities.FileCrypt.Properties;

namespace vm.Aspects.Security.Cryptography.Ciphers.Utilities
{
    static class Program
    {
        const string _base64Regex = @"^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$";

        static Parameters _arguments;

        static int Main(string[] args)
        {
            int result = 0;

            try
            {
                ClassMetadataRegistrar.RegisterMetadata();

                using (var parser = new Parser(s =>
                {
                    s.CaseInsensitiveEnumValues = true;
                    s.CaseSensitive             = false;
                    s.HelpWriter                = Console.Error;
                }))
                    result = parser
                                .ParseArguments<EncryptParameters, DecryptParameters>(args)
                                .MapResult(
                                    (EncryptParameters p) => Encrypt(p),
                                    (DecryptParameters p) => Decrypt(p),
                                    errors => 1);
            }
            catch (Exception x)
            {
                Debug.WriteLine(x.DumpString());
                Console.WriteLine(x.Message);

                result = 1;
            }

#if DEBUG
            if (_arguments?.WaitAtTheEnd == true)
            {
                Console.Write(Resources.PressAnyKey);
                Console.ReadKey(true);
            }
#endif
            return result;
        }

        static int Encrypt(EncryptParameters p)
        {
            _arguments = p;

            EnsureCanOverwriteDestination(p);

            using (var decryptedStream = new FileStream(p.SourceFileName, FileMode.Open, FileAccess.Read, FileShare.None))
            using (var encryptedStream = new FileStream(p.DestinationFileName, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var cipher = new EncryptedNewKeyCipher(p.Certificate))
            {
                cipher.Base64Encoded = p.Base64Encoded;
                cipher.Encrypt(decryptedStream, encryptedStream);
            }

            return 0;
        }

        static int Decrypt(DecryptParameters p)
        {
            _arguments = p;

            EnsureCanOverwriteDestination(p);

            var retryWithBase64 = false;

            do
                try
                {
                    using (var encryptedStream = new FileStream(p.SourceFileName, FileMode.Open, FileAccess.Read, FileShare.None))
                    using (var decryptedStream = new FileStream(p.DestinationFileName, FileMode.Create, FileAccess.Write, FileShare.None))
                    using (var cipher = new EncryptedNewKeyCipher(p.Certificate))
                    {
                        cipher.Base64Encoded = retryWithBase64;
                        cipher.Decrypt(encryptedStream, decryptedStream);
                        retryWithBase64 = false;
                    }
                }
                catch (ArgumentException x) when (x.ParamName == "encryptedStream")
                {
                    if (retryWithBase64)
                        throw;

                    using (var reader = new StreamReader(p.SourceFileName, Encoding.ASCII))
                    {
                        for (var i = 0; i<5  &&  !reader.EndOfStream; i++)
                        {
                            var line = reader.ReadLine();
                            if (!Regex.IsMatch(line, _base64Regex))
                                throw;
                        }
                        retryWithBase64 = true;
                    }
                }
            while (retryWithBase64);

            return 0;
        }

        static void EnsureCanOverwriteDestination(Parameters p)
        {
            if (p.Overwrite  ||  !new FileInfo(p.DestinationFileName).Exists)
                return;

            Console.Write(Resources.OverwriteDestinationFile);

            var y = 
#if DEBUG
                    (char)Console.Read();
#else
                    Console.ReadKey(false).KeyChar;
#endif

            Console.WriteLine(y);

            if (char.ToUpperInvariant(y) != 'Y')
                throw new ArgumentException(Resources.DestinationFileAlreadyExists);
        }
    }
}
