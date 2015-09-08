using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Security.Cryptography.Ciphers.Test
{
    [TestClass]
    public class DpapiCipherTest1 : GenericCipherTest<DpapiCipher>
    {
        public override ICipherAsync GetCipher(bool base64 = false)
        {
            var cipher = new DpapiCipher();

            cipher.Base64Encoded = base64;
            return cipher;
        }

        public override ICipherAsync GetPublicCertCipher(bool base64 = false)
        {
            throw new InvalidOperationException();
        }

        [TestMethod]
        [ExpectedException(typeof(CryptographicException))]
        public new void Length0DecryptTest()
        {
            using (var target = GetCipher())
            {
                var expected = new byte[0];
                var actual = target.Decrypt(new byte[0]);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(CryptographicException))]
        public new void Length0DecryptTest64()
        {
            using (var target = GetCipher(true))
            {
                var expected = new byte[0];
                var actual = target.Decrypt(new byte[0]);
            }
        }

        //[TestMethod]
        //[ExpectedException(typeof(InvalidOperationException))]
        //public new void Decrypt0LengthAsyncTest()
        //{
        //    TestUtilities.AsyncTestWrapper(TestContext, () =>
        //    {
        //        using (var target = GetCipher())
        //        {
        //            target.DecryptAsync(new MemoryStream(new byte[0]), new MemoryStream()).Wait();
        //        }
        //    });
        //}

        [TestMethod]
        [ExpectedException(typeof(CryptographicException))]
        public new void Length6DecryptTest()
        {
            using (var target = GetCipher())
            {
                var actual = target.Decrypt(new byte[] { 8, 0, 0, 0, 0, 0 });
            }
        }

        [TestMethod]
        [ExpectedException(typeof(CryptographicException))]
        public new void Length6DecryptTest64()
        {
            using (var target = GetCipher(true))
            {
                var actual = target.Decrypt(new byte[] { 8, 0, 0, 0, 0, 0 });
            }
        }

        //[TestMethod]
        //[ExpectedException(typeof(InvalidOperationException))]
        //public new void Decrypt6LengthAsyncTest()
        //{
        //    TestUtilities.AsyncTestWrapper(TestContext, () =>
        //    {
        //        using (var target = GetCipher())
        //            target.DecryptAsync(new MemoryStream(new byte[] { 8, 0, 0, 0, 0, 0 }), new MemoryStream()).Wait();
        //    });
        //}

        [TestMethod]
        public void RoundTripLocalUserTest()
        {
            using (var target = GetCipher())
            {
                ((DpapiCipher)target).Scope = DataProtectionScope.CurrentUser;

                var input = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
                var encrypted = target.Encrypt(input);
                var output =  target.Decrypt(encrypted);

                Assert.IsTrue(input.SequenceEqual(output));
            }
        }

        [TestMethod]
        public void RoundTripWithEntropyTest()
        {
            using (var target = GetCipher())
            {

                ((DpapiCipher)target).Entropy = Encoding.Unicode.GetBytes("CosmicEntropy");

                var input = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
                var encrypted = target.Encrypt(input);
                var output =  target.Decrypt(encrypted);

                Assert.IsTrue(input.SequenceEqual(output));
            }
        }

        [TestMethod]
        public void RoundTripWithEntropyLocalUserTest()
        {
            using (var target = GetCipher())
            {

                ((DpapiCipher)target).Scope = DataProtectionScope.CurrentUser;
                ((DpapiCipher)target).Entropy = Encoding.Unicode.GetBytes("CosmicEntropy");

                var input = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
                var encrypted = target.Encrypt(input);
                var output =  target.Decrypt(encrypted);

                Assert.IsTrue(input.SequenceEqual(output));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DecryptBadLength()
        {
            using (var target = GetCipher())
            {
                var inputData = new byte[100].FillRandom();
                var input = new MemoryStream(inputData);
                var output = new MemoryStream();

                target.Encrypt(input, output);

                input.Seek(0, SeekOrigin.Begin);
                output.Seek(0, SeekOrigin.Begin);

                var outputData = output.ToArray().Take(3).ToArray();
                var decrypted = new MemoryStream();

                using (var target1 = GetCipher())
                    target1.Decrypt(new MemoryStream(outputData), decrypted);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DecryptBadBuffer()
        {
            using (var target = GetCipher())
            {
                var inputData = new byte[100].FillRandom();
                var input = new MemoryStream(inputData);
                var output = new MemoryStream();

                target.Encrypt(input, output);

                input.Seek(0, SeekOrigin.Begin);
                output.Seek(0, SeekOrigin.Begin);

                var outputData = output.ToArray().Take(50).ToArray();
                var decrypted = new MemoryStream();

                using (var target1 = GetCipher())
                    target1.Decrypt(new MemoryStream(outputData), decrypted);
            }
        }
    }
}
