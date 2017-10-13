using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Security.Cryptography.Ciphers.Tests
{
    public abstract class GenericHasherTest<THasher> where THasher : IHasherAsync
    {
        public virtual IHasherAsync GetHasher()
        {
            throw new NotImplementedException();
        }

        public virtual IHasherAsync GetHasher(int saltLength)
        {
            throw new NotImplementedException();
        }

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void HashNullTest()
        {
            var target = GetHasher();
            var output = target.Hash((byte[])null);

            Assert.IsNull(output);
        }

        [TestMethod]
        public void Length0HashTest()
        {
            var target = GetHasher();
            var output = target.Hash(new byte[0]);

            Assert.IsNotNull(output);
            Assert.IsTrue(output.Length > 0);
        }

        [TestMethod]
        public void HashTest()
        {
            var target = GetHasher();
            var input = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
            var output = target.Hash(input);

            Assert.IsNotNull(output);
            Assert.IsTrue(output.Length > 0);
        }


        [TestMethod]
        public void VerifyNullDataHashTest()
        {
            var target = GetHasher();
            var actual = target.TryVerifyHash((byte[])null, null);

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void Verify0LengthHashTest()
        {
            var target = GetHasher();
            var actual = target.TryVerifyHash(new byte[0], new byte[0]);

            if (target is NullHasher)
                Assert.IsTrue(actual);
            else
                Assert.IsFalse(actual);
        }

        [TestMethod]
        public void RoundTripHashTest()
        {
            var target = GetHasher();
            var input = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
            var hash = target.Hash(input);
            var target1 = GetHasher();
            var output =  target1.TryVerifyHash(input, hash);

            Assert.IsTrue(output);
        }

        [TestMethod]
        public void CompareHashesTest()
        {
            var target = GetHasher();

            var input = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
            var hash = target.Hash(input);
            var target1 = GetHasher();
            var hash1 =  target1.Hash(input);

            Assert.IsTrue(hash.SequenceEqual(hash1) == (target.SaltLength == 0));
        }

        [TestMethod]
        public void RoundTripSaltLength0HashTest()
        {
            var target = GetHasher();

            if (target is PasswordHasher)
                return;

            target.SaltLength = 0;

            var input = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
            var hash = target.Hash(input);
            var target1 = GetHasher();
            var output =  target1.TryVerifyHash(input, hash);

            Assert.IsTrue(output);
        }

        [TestMethod]
        public void CompareHashesSaltLength0Test()
        {
            var target = GetHasher();
            if (target is PasswordHasher)
                return;

            target.SaltLength = 0;
            var input = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
            var hash = target.Hash(input);
            var target1 = GetHasher();
            target1.SaltLength = 0;
            var hash1 =  target1.Hash(input);

            Assert.IsTrue(hash.SequenceEqual(hash1));
        }

        [TestMethod]
        public void RoundTripWrongLengthHashTest()
        {
            var target = GetHasher();
            var input = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
            var hash = target.Hash(input);

            hash = hash.Take(hash.Length/2).ToArray();

            var target1 = GetHasher();
            var output =  target1.TryVerifyHash(input, hash);

            if (target is NullHasher)
                Assert.IsTrue(output);
            else
                Assert.IsFalse(output);
        }

        [TestMethod]
        public void RoundTripFailedHashTest()
        {
            var target = GetHasher();
            var input = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
            var hash = target.Hash(input);

            hash[hash.Length-1] = unchecked((byte)~hash[hash.Length-1]);

            var target1 = GetHasher();
            var output = target1.TryVerifyHash(input, hash);

            if (target is NullHasher)
                Assert.IsTrue(output);
            else
                Assert.IsFalse(output);
        }

        [TestMethod]
        public void HashStreamNullInputHashTest()
        {
            var target = GetHasher();
            var input = (Stream)null;
            var hash = target.Hash(input);

            Assert.IsNull(hash);
        }

        [TestMethod]
        public void StreamNullInputAsyncHashTest()
        {
            var target = GetHasher();
            var input = (Stream)null;
            var hash = target.HashAsync(input);

            Assert.IsNull(hash.Result);
        }

        [TestMethod]
        public void StreamAsyncHashTest()
        {
            var target = GetHasher();
            var input = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }, false);

            var hashTask = target.HashAsync(input);

            Assert.IsNotNull(hashTask.Result);
            Assert.IsTrue(hashTask.Result.Length > 0);
        }

        [TestMethod]
        public void VerifyStreamNullInputAsyncHashTest()
        {
            var target = GetHasher();
            var input = (Stream)null;
            var output = new MemoryStream();
            var target1 = GetHasher();
            var verifiedTask = target1.TryVerifyHashAsync(input, null);

            Assert.IsTrue(verifiedTask.Result);
        }

        [TestMethod]
        public void VerifyStreamNullInputHashTest()
        {
            var target = GetHasher();
            var input = (Stream)null;
            var output = new MemoryStream();
            var target1 = GetHasher();
            var verified = target1.TryVerifyHash(input, null);

            Assert.IsTrue(verified);
        }

        [TestMethod]
        public void RoundTripWrongLength2HashTest()
        {
            var clearData = new byte[10].FillRandom();
            var input = new MemoryStream(clearData);
            byte[] hash;

            using (var target = GetHasher())
            {
                hash = target.Hash(input);

                Assert.IsNotNull(hash);
                Assert.IsTrue(hash.Length > 0);
            }

            hash = hash.Take(hash.Length/2).ToArray();

            input.Seek(0, SeekOrigin.Begin);

            using (var target = GetHasher())
                if (target is NullHasher)
                    Assert.IsTrue(target.TryVerifyHash(input, hash));
                else
                    Assert.IsFalse(target.TryVerifyHash(input, hash));
        }

        [TestMethod]
        public void RoundTripWrongLengthAsyncHashTest()
        {
            var clearData = new byte[10].FillRandom();
            var input = new MemoryStream(clearData);
            byte[] hash;

            using (var target = GetHasher())
            {
                hash = target.Hash(input);

                Assert.IsNotNull(hash);
                Assert.IsTrue(hash.Length > 0);
            }

            hash = hash.Take(hash.Length/2).ToArray();

            input.Seek(0, SeekOrigin.Begin);

            using (var target = GetHasher())
                if (target is NullHasher)
                    Assert.IsTrue(target.TryVerifyHashAsync(input, hash).Result);
                else
                    Assert.IsFalse(target.TryVerifyHashAsync(input, hash).Result);
        }

        void StreamParameterizedHashTest(int length, int saltLength = 8)
        {
            using (var target = GetHasher(saltLength))
            {
                var inputData = new byte[length].FillRandom();
                var input = new MemoryStream(inputData);
                var hash = target.Hash(input);

                Assert.IsNotNull(hash);
                Assert.IsTrue(hash.Length > 0);
            }
        }

        void StreamParameterizedAsyncHashTest(int length, int saltLength = 8)
        {
            using (var target = GetHasher(saltLength))
            {
                var inputData = new byte[length].FillRandom();
                var input = new MemoryStream(inputData);
                var hashTask = target.HashAsync(input);

                Assert.IsNotNull(hashTask.Result);
                Assert.IsTrue(hashTask.Result.Length > 0);
            }
        }

        void RoundTripParameterizedHashTest(int length, int saltLength = 8, bool failed = false)
        {
            var clearData = new byte[length].FillRandom();
            var input = new MemoryStream(clearData);
            byte[] hash;

            using (var target = GetHasher(saltLength))
            {
                hash = target.Hash(input);

                Assert.IsNotNull(hash);
                Assert.IsTrue(hash.Length > 0);
            }

            if (failed)
                hash[hash.Length-1] = unchecked((byte)~hash[hash.Length-1]);

            input.Seek(0, SeekOrigin.Begin);

            using (var target = GetHasher(saltLength))
            {
                var actual = target.TryVerifyHash(input, hash);

                if (target is NullHasher)
                    Assert.IsTrue(actual);
                else
                    Assert.AreEqual(actual, !failed);
            }
        }

        void CompareHashesParameterizedHashTest(int length, int saltLength = 8)
        {
            var clearData = new byte[length].FillRandom();
            var input = new MemoryStream(clearData);
            byte[] hash;

            using (var target = GetHasher(saltLength))
            {
                hash = target.Hash(input);

                Assert.IsNotNull(hash);
                Assert.IsTrue(hash.Length > 0);
            }

            input.Seek(0, SeekOrigin.Begin);

            using (var target = GetHasher(saltLength))
            {
                var actual = target.TryVerifyHash(input, hash);

                Assert.IsTrue(actual);
            }
        }

        void RoundTripParameterizedAsyncHashTest(int length, int saltLength = 8, bool failed = false)
        {
            var clearData = new byte[length].FillRandom();
            var input = new MemoryStream(clearData);
            byte[] hash;

            using (var target = GetHasher(saltLength))
            {
                hash = target.HashAsync(input).Result;

                Assert.IsNotNull(hash);
                Assert.IsTrue(hash.Length > 0);
            }

            if (failed)
                hash[hash.Length-1] = unchecked((byte)~hash[hash.Length-1]);

            input.Seek(0, SeekOrigin.Begin);

            using (var target = GetHasher(saltLength))
            {
                var actual = target.TryVerifyHashAsync(input, hash).Result;

                if (target is NullHasher)
                    Assert.IsTrue(actual);
                else
                    Assert.AreEqual(actual, !failed);
            }
        }

        [TestMethod]
        public void Stream0HashTest()
        {
            StreamParameterizedHashTest(0);
        }

        [TestMethod]
        public void RoundTripStream0HashTest()
        {
            RoundTripParameterizedHashTest(0);
        }

        [TestMethod]
        public void CompareHashesStream0HashTest()
        {
            CompareHashesParameterizedHashTest(0);
        }

        [TestMethod]
        public void StreamLessThan4kHashTest()
        {
            StreamParameterizedHashTest(1024);
        }

        [TestMethod]
        public void StreamLessThan4kNotSaltedHashTest()
        {
            StreamParameterizedHashTest(1024);
        }

        [TestMethod]
        public void RoundTripStreamLessThan4kHashTest()
        {
            RoundTripParameterizedHashTest(1024);
        }

        [TestMethod]
        public void CompareHashesStreamLessThan4kHashTest()
        {
            CompareHashesParameterizedHashTest(1024);
        }

        [TestMethod]
        public void RoundTripStreamLessThan4kSaltLength0HashTest()
        {
            if (GetHasher() is PasswordHasher)
                return;
            RoundTripParameterizedHashTest(1024, 0);
        }

        [TestMethod]
        public void CompareHashesStreamLessThan4kSaltLength0HashTest()
        {
            if (GetHasher() is PasswordHasher)
                return;
            CompareHashesParameterizedHashTest(1024, 0);
        }

        [TestMethod]
        public void RoundTripStreamLessThan4kSaltLength16HashTest()
        {
            RoundTripParameterizedHashTest(1024, 16);
        }

        [TestMethod]
        public void RoundTripStreamLessThan4kNotSaltedHashTest()
        {
            RoundTripParameterizedHashTest(1024);
        }

        [TestMethod]
        public void RoundTripStreamLessThan4kFailedHashTest()
        {
            RoundTripParameterizedHashTest(1024, 8, true);
        }

        [TestMethod]
        public void RoundTripStreamLessThan4kNotSaltedFailedHashTest()
        {
            RoundTripParameterizedHashTest(1024, 8, true);
        }

        [TestMethod]
        public void Stream4kHashTest()
        {
            StreamParameterizedHashTest(4096);
        }

        [TestMethod]
        public void RoundTripStream4kHashTest()
        {
            RoundTripParameterizedHashTest(4096);
        }

        [TestMethod]
        public void CompareHashesStream4kHashTest()
        {
            CompareHashesParameterizedHashTest(4096);
        }

        [TestMethod]
        public void StreamNx4kHashTest()
        {
            StreamParameterizedHashTest(3*4096);
        }

        [TestMethod]
        public void RoundTripStreamNx4kHashTest()
        {
            RoundTripParameterizedHashTest(3*4096);
        }

        [TestMethod]
        public void CompareHashesStreamNx4kHashTest()
        {
            CompareHashesParameterizedHashTest(3*4096);
        }

        [TestMethod]
        public void StreamMoreThanNx4kHashTest()
        {
            StreamParameterizedHashTest(3*4096+734);
        }

        [TestMethod]
        public void RoundTripStreamMoreThanNx4kTest()
        {
            RoundTripParameterizedHashTest(3*4096+734);
        }

        [TestMethod]
        public void CompareHashesStreamMoreThanNx4kTest()
        {
            CompareHashesParameterizedHashTest(3*4096+734);
        }

        // --------------------------------------------

        [TestMethod]
        public void Stream0AsyncHashTest()
        {
            StreamParameterizedAsyncHashTest(0);
        }

        [TestMethod]
        public void RoundTripStream0AsyncHashTest()
        {
            RoundTripParameterizedAsyncHashTest(0);
        }

        [TestMethod]
        public void StreamLessThan4kAsyncHashTest()
        {
            StreamParameterizedAsyncHashTest(1024);
        }

        [TestMethod]
        public void StreamLessThan4kNotSaltedAsyncHashTest()
        {
            StreamParameterizedAsyncHashTest(1024);
        }

        [TestMethod]
        public void RoundTripStreamLessThan4kAsyncHashTest()
        {
            RoundTripParameterizedAsyncHashTest(1024);
        }

        [TestMethod]
        public void RoundTripStreamLessThan4kSaltLength0AsyncHashTest()
        {
            if (GetHasher() is PasswordHasher)
                return;
            RoundTripParameterizedAsyncHashTest(1024, 0);
        }

        [TestMethod]
        public void RoundTripStreamLessThan4kSaltLength16AsyncHashTest()
        {
            RoundTripParameterizedAsyncHashTest(1024, 16);
        }

        [TestMethod]
        public void RoundTripStreamLessThan4kNotSaltedAsyncHashTest()
        {
            RoundTripParameterizedAsyncHashTest(1024);
        }

        [TestMethod]
        public void RoundTripStreamLessThan4kFailedAsyncHashTest()
        {
            RoundTripParameterizedAsyncHashTest(1024, 8, true);
        }

        [TestMethod]
        public void RoundTripStreamLessThan4kNotSaltedFailedAsyncHashTest()
        {
            RoundTripParameterizedAsyncHashTest(1024, 8, true);
        }

        [TestMethod]
        public void Stream4kAsyncHashTest()
        {
            StreamParameterizedAsyncHashTest(4096);
        }

        [TestMethod]
        public void RoundTripStream4kAsyncHashTest()
        {
            RoundTripParameterizedAsyncHashTest(4096);
        }

        [TestMethod]
        public void StreamNx4kAsyncHashTest()
        {
            StreamParameterizedAsyncHashTest(3*4096);
        }

        [TestMethod]
        public void RoundTripStreamNx4kAsyncHashTest()
        {
            RoundTripParameterizedAsyncHashTest(3*4096);
        }

        [TestMethod]
        public void StreamMoreThanNx4kAsyncHashTest()
        {
            StreamParameterizedAsyncHashTest(3*4096+734);
        }

        [TestMethod]
        public void RoundTripStreamMoreThanNx4kAsyncHashTest()
        {
            RoundTripParameterizedAsyncHashTest(3*4096+734);
        }
    }
}
