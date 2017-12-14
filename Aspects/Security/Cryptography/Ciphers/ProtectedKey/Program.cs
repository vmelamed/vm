using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using vm.Aspects.Security.Cryptography.Ciphers.Utilities.Properties;

namespace vm.Aspects.Security.Cryptography.Ciphers.Utilities
{
    class Program
    {
        const string CreateCommand = "create";
        const string ImportCommand = "import";
        const string ExportCommand = "export";
        const string HelpCommand = "help";

        const string RexByteArray = "(?i:^[0-9a-f][0-9a-f](-[0-9a-f][0-9a-f])*$)";

        static string _command;
        static string _keyFile;
        static byte[] _key;
        static int _exitCode;

        static int Main(string[] args)
        {
            try
            {
                if (ParseArguments(args))
                    switch (_command)
                    {
                    case CreateCommand:
                        _exitCode = Create();
                        break;

                    case ExportCommand:
                        _exitCode = Export();
                        break;

                    case ImportCommand:
                        _exitCode = Import();
                        break;

                    case HelpCommand:
                    default:
                        _exitCode = Usage();
                        break;
                    }
                else
                    Usage(1);
            }
            catch (Exception x)
            {
                Console.WriteLine(x.ToString());
                Usage(-1);
            }

#if DEBUG
            Console.Write(Resources.PressAnyKey);
            Console.ReadKey(true);
#endif
            return _exitCode;
        }

        static bool ParseArguments(string[] args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            if (args.Length == 0)
                return true;

            int argIndex = 0;

            if (!GetCommand(args[argIndex]))
                return false;

            if (_command == HelpCommand)
                return true;

            argIndex++;

            if (argIndex >= args.Length)
            {
                Console.WriteLine(Resources.SpecifyFileName);
                return false;
            }
            else
                if (!GetFile(args[argIndex]))
                return false;

            if (_command != ImportCommand)
                return true;

            argIndex++;

            if (argIndex >= args.Length)
            {
                Console.WriteLine(Resources.SpecifyKey);
                return false;
            }
            else
                if (!GetKey(args[argIndex]))
                return false;

            return true;
        }

        static bool GetCommand(string argument)
        {
            if (argument == null)
                throw new ArgumentNullException(nameof(argument));

            if (CreateCommand.StartsWith(argument, StringComparison.CurrentCultureIgnoreCase))
            {
                _command = CreateCommand;
                return true;
            }

            if (ExportCommand.StartsWith(argument, StringComparison.CurrentCultureIgnoreCase))
            {
                _command = ExportCommand;
                return true;
            }

            if (ImportCommand.StartsWith(argument, StringComparison.CurrentCultureIgnoreCase))
            {
                _command = ImportCommand;
                return true;
            }

            if (HelpCommand.StartsWith(argument, StringComparison.CurrentCultureIgnoreCase) ||
                argument == "?"  ||
                argument == "-?" ||
                argument == "/?")
            {
                _command = HelpCommand;
                return true;
            }

            Console.WriteLine(Resources.InvalidCommand, argument);

            return false;
        }

        static bool GetFile(string argument)
        {
            if (string.IsNullOrWhiteSpace(argument))
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(argument));

            var fileInfo = new FileInfo(argument);

            if (_command == ExportCommand && !fileInfo.Exists)
            {
                Console.WriteLine(Resources.FileNotExist, argument);
                return false;
            }

            if ((_command == CreateCommand || _command == ImportCommand) && fileInfo.Exists)
            {
                Console.Write(Resources.FileExist, argument);

                var y = Console.ReadKey(false).KeyChar;

                Console.WriteLine();

                if (y == 'n' || y == 'N')
                    return false;
            }

            _keyFile = argument;
            return true;
        }

        static bool GetKey(string argument)
        {
            if (string.IsNullOrWhiteSpace(argument))
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(argument));

            _key = ParseHexValue(GetHexValue(argument));
            return _key != null;
        }

        static byte[] ParseHexValue(string argument)
        {
            if (string.IsNullOrWhiteSpace(argument))
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(argument));
            if (argument.Length % 2 != 0)
                throw new ArgumentException("Invalid argument length.", nameof(argument));

            var hexValue = new byte[argument.Length/2];

            for (var i = 0; i<argument.Length; i += 2)
                hexValue[i/2] = byte.Parse(argument.Substring(i, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);

            return hexValue;
        }

        static string GetHexValue(string argument)
        {
            if (string.IsNullOrWhiteSpace(argument))
                throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(argument));

            if (!Regex.IsMatch(argument, RexByteArray))
            {
                Console.WriteLine(Resources.InvalidKey);
                return null;
            }

            return Regex.Replace(argument, @"[^\da-fA-F]+", string.Empty).ToUpper(CultureInfo.InvariantCulture);
        }

        static IKeyStorage GetKeyStorage()
            => new KeyFile();

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        static ICipherAsync GetCipher()
            => new ProtectedKeyCipher(null, _keyFile, null);

        static int Import()
        {
            if (GetCipher() is IKeyManagement keyManagement)
                keyManagement.ImportSymmetricKey(_key);
            return 0;
        }

        static int Export()
        {
            if (GetCipher() is IKeyManagement keyManagement)
            {
                _key = keyManagement.ExportSymmetricKey();
                Console.WriteLine(BitConverter.ToString(_key));
            }
            return 0;
        }

        static int Create()
        {
            var keyStorage = GetKeyStorage();

            if (keyStorage.KeyLocationExists(_keyFile))
                keyStorage.DeleteKeyLocation(_keyFile);

            var cipher = GetCipher();

            // this will create the key file
            cipher.Encrypt(new byte[] { 1, 2, 3 });

            Debug.Assert(keyStorage.KeyLocationExists(_keyFile));
            return 0;
        }

        static int Usage(int exitCode = 0)
        {
            Console.WriteLine(Resources.Usage);

            _exitCode = exitCode;

            return exitCode;
        }
    }
}
