using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Security.Cryptography.Ciphers.Tests
{
    [TestClass]
    public class NullCipherTest : GenericCipherTest<NullCipher>
    {
        public override ICipherAsync GetCipher(bool base64 = false)
        {
            var cipher = new NullCipher();

            cipher.Base64Encoded = base64;
            return cipher;
        }

        public override ICipherAsync GetPublicCertCipher(bool base64 = false)
        {
            throw new InvalidOperationException();
        }
    }
}
