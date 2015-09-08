using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vm.Aspects.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Class PasswordDerivationConstants defines several minimal and default constants for deriving byte arrays from (typically) passwords.
    /// </summary>
    public static class PasswordDerivationConstants
    {
        /// <summary>
        /// The minimum number of iterations - 1024.
        /// </summary>
        public const int MinNumberOfIterations = 0x0400;
        /// <summary>
        /// The minimum and default number of iterations - 16384.
        /// </summary>
        public const int DefaultNumberOfIterations = 0x4000;

        /// <summary>
        /// The minimum hash length in bytes - 24.
        /// </summary>
        public const int MinHashLength = 24;
        /// <summary>
        /// The default hash length in bytes - 64.
        /// </summary>
        public const int DefaultHashLength = 64;

        /// <summary>
        /// The minimum salt length in bytes - 8.
        /// </summary>
        public const int MinSaltLength = 8;
        /// <summary>
        /// The default salt length in bytes - 24.
        /// </summary>
        public const int DefaultSaltLength = 24;
    }
}
