using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using FileCrypt.Properties;
using vm.Aspects.Security.Cryptography.Ciphers;

namespace FileCrypt
{
    static class Program
    {
        static string _certSubject;
        static string _certThumbprint;
        static string _clearText;
        static string _encryptedText;
        static string _decryptedText;
        static bool _encrypt;
        static bool _decrypt;

        static void Main(string[] args)
        {
            if (ParseArguments(args))
                try
                {
                    var cert = GetCert(_certSubject, _certThumbprint);

                    using (var cipher = new EncryptedNewKeyCipher(cert))
                    {
                        if (_encrypt)
                            using (var decryptedStream = new FileStream(_clearText, FileMode.Open, FileAccess.Read, FileShare.None))
                            using (var encryptedStream = new FileStream(_encryptedText, FileMode.Create, FileAccess.Write, FileShare.None))
                                cipher.Encrypt(decryptedStream, encryptedStream);

                        if (_decrypt)
                            using (var encryptedStream = new FileStream(_encryptedText, FileMode.Open, FileAccess.Read, FileShare.None))
                            using (var decryptedStream = new FileStream(_decryptedText, FileMode.Create, FileAccess.Write, FileShare.None))
                                cipher.Decrypt(encryptedStream, decryptedStream);
                    }
                }
                catch (Exception x)
                {
                    Console.WriteLine(x.ToString());
                    Usage();
                }

#if DEBUG
            Console.WriteLine(Resources.PressAnyKey);
            Console.ReadKey(true);
#endif
        }

        private static bool ParseArguments(string[] args)
        {
            if (args.Length == 0)
            {
                Usage();
                return false;
            }

            string file1 = null;
            string file2 = null;
            string file3 = null;

            for (var i = 0; i<args.Length; i++)
            {
                var arg = args[i];

                if (arg.StartsWith("/s", StringComparison.OrdinalIgnoreCase) ||
                    arg.StartsWith("-s", StringComparison.OrdinalIgnoreCase))
                {
                    if (args.Length <= i+1)
                    {
                        Console.WriteLine(Resources.MissingSubject);
                        Usage();
                        return false;
                    }

                    _certSubject = args[++i];
                    continue;
                }

                if (arg.StartsWith("/t", StringComparison.OrdinalIgnoreCase) ||
                    arg.StartsWith("-t", StringComparison.OrdinalIgnoreCase))
                {
                    if (args.Length <= i+1  ||  !GetThumbprint(args[++i]))
                    {
                        Console.WriteLine(Resources.MissingThumbprint);
                        Usage();
                        return false;
                    }
                    continue;
                }

                if (arg.StartsWith("/e", StringComparison.OrdinalIgnoreCase) ||
                    arg.StartsWith("-e", StringComparison.OrdinalIgnoreCase))
                {
                    _encrypt = true;
                    continue;
                }

                if (arg.StartsWith("/d", StringComparison.OrdinalIgnoreCase) ||
                    arg.StartsWith("-d", StringComparison.OrdinalIgnoreCase))
                {
                    _decrypt = true;
                    continue;
                }

                if (arg.StartsWith("/s", StringComparison.OrdinalIgnoreCase) ||
                    arg.StartsWith("-s", StringComparison.OrdinalIgnoreCase))
                {
                    if (args.Length <= i+1)
                    {
                        Console.WriteLine(Resources.MissingSubject);
                        Usage();
                        return false;
                    }

                    _certSubject = args[++i];
                    continue;
                }

                if (file1 == null)
                    file1 = arg;
                else
                if (file2 == null)
                    file2 = arg;
                else
                if (file3 == null)
                    file3 = arg;
            }

            if (_certSubject == null  &&  _certThumbprint == null)
            {
                Console.WriteLine(Resources.MissingCertificate);
                Usage();
                return false;
            }

            if (file1 == null)
            {
                Console.WriteLine(Resources.MissingSource);
                Usage();
                return false;
            }

            if (file2 == null)
            {
                Console.WriteLine(Resources.MissingTarget);
                Usage();
                return false;
            }

            if (_encrypt  &&  _decrypt)
            {
                Console.WriteLine(Resources.MissingRoundtrip);
                Usage();
                return false;
            }

            if (file1 != null  &&  file2 != null  &&  file3 != null)
            {
                _encrypt = true;
                _decrypt = true;
            }

            if (file1 != null  &&   file2 != null  &&
                !_encrypt  &&  !_decrypt)
                _encrypt = true;

            if (_encrypt)
            {
                _clearText = file1;
                _encryptedText = file2;
                if (_decrypt)
                    _decryptedText = file3;
            }
            else
            {
                _encryptedText = file1;
                _decryptedText = file2;
            }

            return true;
        }

        private static void Usage()
        {
            Console.WriteLine(Resources.Usage);
        }

        static X509Certificate2 GetCert(
            string subject,
            string thumbprint)
        {
            using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadOnly);
                var certs = subject != null
                                ? store.Certificates.Find(X509FindType.FindBySubjectName, subject, false)
                                : store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);

                var cert = certs.Count > 0 ? certs[0] : null;

                if (cert == null)
                    throw new InvalidOperationException(Resources.CertNotFound);

                return cert;
            }
        }

        static bool GetThumbprint(string argument)
        {
            _certThumbprint = GetHexValue(argument);
            return _certThumbprint != null;
        }

        const string RexByteArray = @"(?i:((-|\s|\?)?[0-9a-f][0-9a-f])*)";

        static string GetHexValue(string argument)
        {
            if (!Regex.IsMatch(argument, RexByteArray))
            {
                Console.WriteLine(Resources.InvalidThumbprint);
                return null;
            }

            return Regex.Replace(argument, @"[^\da-fA-F]+", string.Empty).ToUpper(CultureInfo.InvariantCulture);
        }

        static byte[] ParseHexValue(string argument)
        {
            var hexValue = new byte[(argument.Length+1)/2];

            for (var i = 0; i<argument.Length; i += 2)
                hexValue[i/2] = byte.Parse(argument.Substring(i, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);

            return hexValue;
        }
    }
}
