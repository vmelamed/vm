using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Security.Cryptography.Ciphers;

namespace vm.Aspects.Model.Tests
{
    /// <summary>
    /// Summary description for EncryptedPartsTest
    /// </summary>
    [TestClass]
    public class EncryptedPartsTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        public ICipher GetCipher()
        {
            return new DpapiCipher();
        }

        class ClassOnePropertyOneEncrypted
        {
            [Decrypted]
            public string Decrypted1 { get; set; }

            public byte[] Encrypted { get; set; }
        }

        class ClassTwoPropertiesOneEncrypted : ClassOnePropertyOneEncrypted
        {
            [Decrypted]
            public int Decrypted2 { get; set; }
        }

        class ClassTwoPropertiesTwoEncrypted : ClassOnePropertyOneEncrypted
        {
            [Decrypted(nameof(Encrypted2))]
            protected int Decrypted2 { get; set; }

            protected byte[] Encrypted2 { get; set; }
        }

        class ClassTwoPropertiesOneFieldTwoEncrypted : ClassTwoPropertiesTwoEncrypted
        {
            [Decrypted("Encrypted2")]
            int _decrypted3;

            public void SetDecrypted3(int v)
            {
                _decrypted3 = v;
            }

            public int GetDecrypted3()
            {
                return _decrypted3;
            }
        }

        class ClassTwoPropertiesTwoEncryptedWrongType : ClassOnePropertyOneEncrypted
        {
            [Decrypted(nameof(Encrypted2))]
            protected int Decrypted2 { get; set; }

            protected int[] Encrypted2 { get; set; }
        }

        class ClassTwoPropertiesOneFieldWrongEncrypted : ClassTwoPropertiesTwoEncrypted
        {
            [Decrypted("EncryptedX")]
            int _decrypted3;

            public void SetDecrypted3(int v)
            {
                _decrypted3 = v;
            }

            public int GetDecrypted3()
            {
                return _decrypted3;
            }
        }

        [TestMethod]
        public void TestEncryptedParts_Constructor_Object()
        {
            var cipher = GetCipher();
            var instance = new object();
            var target = new EncryptedParts(instance, cipher);

            // should succeed
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestEncryptedParts_ConstructorFor_ClassOnePropertyOneEncrypted()
        {
            var cipher = GetCipher();
            var instance = new ClassOnePropertyOneEncrypted();
            var target = new EncryptedParts(instance, cipher);

            // should succeed
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestEncryptedParts_ConstructorFor_ClassTwoPropertiesOneEncrypted()
        {
            var cipher = GetCipher();
            var instance = new ClassTwoPropertiesOneEncrypted();
            var target = new EncryptedParts(instance, cipher);

            // should succeed
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestEncryptedParts_ConstructorFor_ClassTwoPropertiesTwoEncrypted()
        {
            var cipher = GetCipher();
            var instance = new ClassTwoPropertiesTwoEncrypted();
            var target = new EncryptedParts(instance, cipher);

            // should succeed
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestEncryptedParts_ConstructorFor_ClassTwoPropertiesOneFieldTwoEncrypted()
        {
            var cipher = GetCipher();
            var instance = new ClassTwoPropertiesOneFieldTwoEncrypted();
            var target = new EncryptedParts(instance, cipher);

            // should succeed
            Assert.IsTrue(true);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestEncryptedParts_ConstructorFor_ClassTwoPropertiesTwoEncryptedWrongType()
        {
            var cipher = GetCipher();
            var instance = new ClassTwoPropertiesTwoEncryptedWrongType();
            var target = new EncryptedParts(instance, cipher);

            // should not succeed
            Assert.IsTrue(false);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestEncryptedParts_ConstructorFor_ClassTwoPropertiesOneFieldWrongEncrypted()
        {
            var cipher = GetCipher();
            var instance = new ClassTwoPropertiesOneFieldWrongEncrypted();
            var target = new EncryptedParts(instance, cipher);

            // should succeed
            Assert.IsTrue(false);
        }

        [TestMethod]
        public void TestEncryptedParts_Encrypt_Object()
        {
            var cipher = GetCipher();
            var instance = new object();
            var target = new EncryptedParts(instance, cipher);

            target.Encrypt();

            // should succeed
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestEncryptedParts_Decrypt_Object()
        {
            var cipher = GetCipher();
            var instance = new object();
            var target1 = new EncryptedParts(instance, cipher);

            target1.Encrypt();

            var target2 = new EncryptedParts(instance, cipher);

            target2.Decrypt();

            // should succeed
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestEncryptedParts_Encrypt_Default_ClassOnePropertyOneEncrypted()
        {
            var cipher = GetCipher();
            var instance = new ClassOnePropertyOneEncrypted();
            var target = new EncryptedParts(instance, cipher);

            target.Encrypt();

            Assert.IsTrue(instance.Encrypted != null);
            Assert.IsTrue(instance.Encrypted.Length != 0);
        }

        [TestMethod]
        public void TestEncryptedParts_RoundTrip_Default_ClassOnePropertyOneEncrypted()
        {
            var cipher = GetCipher();
            var instance = new ClassOnePropertyOneEncrypted();
            var target1 = new EncryptedParts(instance, cipher);

            target1.Encrypt();

            Assert.IsTrue(instance.Encrypted != null);
            Assert.IsTrue(instance.Encrypted.Length != 0);

            var target2 = new EncryptedParts(instance, cipher);

            target2.Decrypt();

            Assert.IsNull(instance.Decrypted1);
        }

        [TestMethod]
        public void TestEncryptedParts_RoundTrip_ClassOnePropertyOneEncrypted()
        {
            var cipher = GetCipher();
            var instance = new ClassOnePropertyOneEncrypted { Decrypted1 = "pipi" };
            var target1 = new EncryptedParts(instance, cipher);

            target1.Encrypt();

            Assert.IsTrue(instance.Encrypted != null);
            Assert.IsTrue(instance.Encrypted.Length != 0);

            var target2 = new EncryptedParts(instance, cipher);

            target2.Decrypt();

            Assert.AreEqual("pipi", instance.Decrypted1);
        }

        [TestMethod]
        public void TestEncryptedParts_Encrypt_Default_ClassTwoPropertiesOneEncrypted()
        {
            var cipher = GetCipher();
            var instance = new ClassTwoPropertiesOneEncrypted();
            var target = new EncryptedParts(instance, cipher);

            target.Encrypt();

            // should succeed
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestEncryptedParts_Decrypt_Default_ClassTwoPropertiesOneEncrypted()
        {
            var cipher = GetCipher();
            var instance = new ClassTwoPropertiesOneEncrypted();
            var target1 = new EncryptedParts(instance, cipher);

            target1.Encrypt();

            Assert.IsTrue(instance.Encrypted != null);
            Assert.IsTrue(instance.Encrypted.Length != 0);

            var target2 = new EncryptedParts(instance, cipher);

            target2.Decrypt();

            Assert.IsNull(instance.Decrypted1);
            Assert.AreEqual(0, instance.Decrypted2);
        }

        [TestMethod]
        public void TestEncryptedParts_Decrypt_ClassTwoPropertiesOneEncrypted()
        {
            var cipher = GetCipher();
            var instance = new ClassTwoPropertiesOneEncrypted { Decrypted1 = "gogo", Decrypted2 = 42 };
            var target1 = new EncryptedParts(instance, cipher);

            target1.Encrypt();

            Assert.IsTrue(instance.Encrypted != null);
            Assert.IsTrue(instance.Encrypted.Length != 0);

            var target2 = new EncryptedParts(instance, cipher);

            target2.Decrypt();

            Assert.AreEqual("gogo", instance.Decrypted1);
            Assert.AreEqual(42, instance.Decrypted2);
        }

        [TestMethod]
        public void TestEncryptedParts_Encrypt_Default_ClassTwoPropertiesTwoEncrypted()
        {
            var cipher = GetCipher();
            var instance = new ClassTwoPropertiesTwoEncrypted();
            var target = new EncryptedParts(instance, cipher);

            target.Encrypt();

            Assert.IsTrue(instance.Encrypted != null);
            Assert.IsTrue(instance.Encrypted.Length != 0);
        }

        [TestMethod]
        public void TestEncryptedParts_Decrypt_Default_ClassTwoPropertiesTwoEncrypted()
        {
            var cipher = GetCipher();
            var instance = new ClassTwoPropertiesTwoEncrypted();
            var target1 = new EncryptedParts(instance, cipher);

            target1.Encrypt();

            Assert.IsTrue(instance.Encrypted != null);
            Assert.IsTrue(instance.Encrypted.Length != 0);

            var target2 = new EncryptedParts(instance, cipher);

            target2.Decrypt();

            Assert.IsNull(instance.Decrypted1);
        }

        [TestMethod]
        public void TestEncryptedParts_Decrypt_ClassTwoPropertiesTwoEncrypted()
        {
            var cipher = GetCipher();
            var instance = new ClassTwoPropertiesTwoEncrypted { Decrypted1 = "Cooper" };
            var target1 = new EncryptedParts(instance, cipher);

            target1.Encrypt();

            Assert.IsTrue(instance.Encrypted != null);
            Assert.IsTrue(instance.Encrypted.Length != 0);

            var target2 = new EncryptedParts(instance, cipher);

            target2.Decrypt();

            Assert.AreEqual("Cooper", instance.Decrypted1);
        }

        [TestMethod]
        public void TestEncryptedParts_Encrypt_Default_ClassTwoPropertiesOneFieldTwoEncrypted()
        {
            var cipher = GetCipher();
            var instance = new ClassTwoPropertiesOneFieldTwoEncrypted();
            var target = new EncryptedParts(instance, cipher);

            target.Encrypt();

            Assert.IsTrue(instance.Encrypted != null);
            Assert.IsTrue(instance.Encrypted.Length != 0);
        }

        [TestMethod]
        public void TestEncryptedParts_Decrypt_Default_ClassTwoPropertiesOneFieldTwoEncrypted()
        {
            var cipher = GetCipher();
            var instance = new ClassTwoPropertiesOneFieldTwoEncrypted();
            var target1 = new EncryptedParts(instance, cipher);

            target1.Encrypt();

            Assert.IsTrue(instance.Encrypted != null);
            Assert.IsTrue(instance.Encrypted.Length != 0);

            var target2 = new EncryptedParts(instance, cipher);

            target2.Decrypt();

            Assert.IsNull(instance.Decrypted1);
        }

        [TestMethod]
        public void TestEncryptedParts_Decrypt_ClassTwoPropertiesOneFieldTwoEncrypted()
        {
            var cipher = GetCipher();
            var instance = new ClassTwoPropertiesOneFieldTwoEncrypted { Decrypted1 = "gogo", };
            instance.SetDecrypted3(3);
            var target1 = new EncryptedParts(instance, cipher);

            target1.Encrypt();

            Assert.IsTrue(instance.Encrypted != null);
            Assert.IsTrue(instance.Encrypted.Length != 0);

            var target2 = new EncryptedParts(instance, cipher);

            target2.Decrypt();

            Assert.AreEqual("gogo", instance.Decrypted1);
            Assert.AreEqual(3, instance.GetDecrypted3());
        }
    }
}
