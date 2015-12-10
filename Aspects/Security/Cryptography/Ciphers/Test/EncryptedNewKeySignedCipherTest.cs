using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Security.Cryptography.Ciphers.Tests
{
    [TestClass]
    [DeploymentItem("..\\..\\Readme.txt")]
    public class EncryptedNewKeySignedCipherTest : GenericCipherTest<EncryptedNewKeySignedCipher>
    {
        static ICipherAsync GetCipherImpl()
        {
            return new EncryptedNewKeySignedCipher(
                            CertificateFactory.GetDecryptingSha256Certificate(),
                            CertificateFactory.GetSigningCertificate()); // SHA1 also works with this cert
        }

        static ICipherAsync GetPublicCertCipherImpl()
        {
            return new EncryptedNewKeySignedCipher(
                            CertificateFactory.GetEncryptingCertificate(),
                            CertificateFactory.GetSigningCertificate()); // default SHA1, SHA256 doesn't work here
        }

        public override ICipherAsync GetCipher(bool base64 = false)
        {
            var cipher = GetCipherImpl();

            // ignore the parameter base64
            return cipher;
        }

        public override ICipherAsync GetPublicCertCipher(bool base64 = false)
        {
            var cipher = GetPublicCertCipherImpl();

            // ignore the parameter base64
            return cipher;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetCipherNullSignCertTest()
        {
            new EncryptedNewKeySignedCipher(CertificateFactory.GetDecryptingCertificate(), null); // default SHA1, SHA256 doesn't work here
        }

        [ExpectedException(typeof(CryptographicException))]
        [TestMethod]
        public void SignatureVerificationFailTest()
        {
            var input = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
            var target = GetCipher();
            var encrypted = target.Encrypt(input);

            encrypted[10] ^= encrypted[10];

            target = GetCipher();

            var output =  target.Decrypt(encrypted);
        }

        [ExpectedException(typeof(CryptographicException))]
        [TestMethod]
        public void SignatureVerificationFailAsyncTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {

                var input = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 });
                var output = new MemoryStream();
                var target = GetCipher();

                target.EncryptAsync(input, output).Wait();

                var buf = output.GetBuffer();

                buf[10] ^= buf[10];

                input = new MemoryStream(buf);
                output = new MemoryStream();

                target = GetCipher();

                target.DecryptAsync(input, output).Wait();
            });
        }

        class InheritedEncryptedNewKeySignedCipher : EncryptedNewKeySignedCipher
        {
            public InheritedEncryptedNewKeySignedCipher()
                : base(CertificateFactory.GetEncryptingCertificate(), CertificateFactory.GetSigningCertificate(), null)
            {
            }

            public void PublicReserveSpaceForHash(
                Stream encryptedStream)
            {
                base.ReserveSpaceForHash(encryptedStream);
            }

            public void PublicWriteHashInReservedSpace(
                Stream encryptedStream,
                byte[] hash)
            {
                base.WriteHashInReservedSpace(encryptedStream, hash);
            }

            public void PublicLoadHashToValidate(
                Stream encryptedStream)
            {
                base.LoadHashToValidate(encryptedStream);
            }

            public async Task PublicLoadHashToValidateAsync(
                   Stream encryptedStream)
            {
                await base.LoadHashToValidateAsync(encryptedStream);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ReserveSpaceForHashNonWritableStreamTest()
        {
            var target = new InheritedEncryptedNewKeySignedCipher();

            target.PublicReserveSpaceForHash(TestUtilities.CreateNonWritableStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void WriteHashInReservedSpaceNonWritableStreamTest()
        {
            var target = new InheritedEncryptedNewKeySignedCipher();

            target.PublicWriteHashInReservedSpace(TestUtilities.CreateNonWritableStream(), new byte[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LoadHashToValidateNonReadableStreamTest()
        {
            var target = new InheritedEncryptedNewKeySignedCipher();

            using (var stream = TestUtilities.CreateNonReadableStream())
                target.PublicLoadHashToValidate(stream);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LoadHashToValidateAsyncNullStreamTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                var target = new InheritedEncryptedNewKeySignedCipher();

                target.PublicLoadHashToValidateAsync(null).Wait();
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LoadHashToValidateAsyncNonReadableStreamTest()
        {
            TestUtilities.AsyncTestWrapper(TestContext, () =>
            {
                var target = new InheritedEncryptedNewKeySignedCipher();

                using (var stream = TestUtilities.CreateNonReadableStream())
                    target.PublicLoadHashToValidateAsync(stream).Wait();
            });
        }
    }
}
