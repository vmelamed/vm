using System;
using System.ComponentModel.DataAnnotations;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using vm.Aspects.Diagnostics.ExternalMetadata;

namespace vm.Aspects.Diagnostics.ObjectTextDumperTests
{
    [TestClass]
    public class ClassMetadataCacheTest
    {
        static void Reset()
        {
            ClassMetadataResolver.ResetClassDumpMetadata();
            Assert.AreEqual(0, ClassMetadataResolver.GetSnapshotTypesDumpData().Count);
        }

        [TestMethod]
        public void TestSetClassDumpMetadata_NullArg2n3()
        {
            Reset();
            ClassMetadataRegistrar.RegisterMetadata();
            var initialCacheSize = ClassMetadataResolver.GetSnapshotTypesDumpData().Count;

            ClassMetadataResolver.SetClassDumpMetadata(typeof(ClassMetadataCacheTest), null, null, true);

            Assert.AreEqual(initialCacheSize+1, ClassMetadataResolver.GetSnapshotTypesDumpData().Count);
            Assert.AreEqual(
                new ClassDumpMetadata(typeof(ClassMetadataCacheTest), new DumpAttribute()),
                ClassMetadataResolver.GetSnapshotTypesDumpData()[typeof(ClassMetadataCacheTest)]);
        }

        [TestMethod]
        public void TestSetClassDumpMetadata_NullArg2()
        {
            Reset();
            ClassMetadataRegistrar.RegisterMetadata();
            var initialCacheSize = ClassMetadataResolver.GetSnapshotTypesDumpData().Count;

            ClassMetadataResolver.SetClassDumpMetadata(typeof(ClassMetadataCacheTest), null, new DumpAttribute(false), true);

            Assert.AreEqual(initialCacheSize+1, ClassMetadataResolver.GetSnapshotTypesDumpData().Count);
            Assert.AreEqual(
                new ClassDumpMetadata(typeof(ClassMetadataCacheTest), new DumpAttribute(false)),
                ClassMetadataResolver.GetSnapshotTypesDumpData()[typeof(ClassMetadataCacheTest)]);
        }

        [TestMethod]
        public void TestSetClassDumpMetadata_NullArg3()
        {
            Reset();
            ClassMetadataRegistrar.RegisterMetadata();
            var initialCacheSize = ClassMetadataResolver.GetSnapshotTypesDumpData().Count;

            ClassMetadataResolver.SetClassDumpMetadata(typeof(ClassMetadataCacheTest), typeof(ClassMetadataResolver), null, true);

            Assert.AreEqual(initialCacheSize+1, ClassMetadataResolver.GetSnapshotTypesDumpData().Count);
            Assert.AreEqual(
                new ClassDumpMetadata(typeof(ClassMetadataResolver), null),
                ClassMetadataResolver.GetSnapshotTypesDumpData()[typeof(ClassMetadataCacheTest)]);
        }

        [TestMethod]
        public void TestSetClassDumpMetadata()
        {
            Reset();
            ClassMetadataRegistrar.RegisterMetadata();
            var initialCacheSize = ClassMetadataResolver.GetSnapshotTypesDumpData().Count;

            ClassMetadataResolver.SetClassDumpMetadata(typeof(ClassMetadataCacheTest), typeof(ClassMetadataResolver), new DumpAttribute(false), true);

            Assert.AreEqual(initialCacheSize+1, ClassMetadataResolver.GetSnapshotTypesDumpData().Count);
            Assert.AreEqual(
                new ClassDumpMetadata(typeof(ClassMetadataResolver), new DumpAttribute(false)),
                ClassMetadataResolver.GetSnapshotTypesDumpData()[typeof(ClassMetadataCacheTest)]);
        }

        //////////////////////////////////////////////////////////////////////////////

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestSetClassDumpMetadata_NullArg2n3_Exception()
        {
            ClassMetadataResolver.SetClassDumpMetadata(typeof(ClassMetadataCacheTest), null, null, true);
            ClassMetadataResolver.SetClassDumpMetadata(typeof(ClassMetadataCacheTest), typeof(ClassMetadataResolver), null, false);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestSetClassDumpMetadata_NullArg2_Exception()
        {
            ClassMetadataResolver.SetClassDumpMetadata(typeof(ClassMetadataCacheTest), null, new DumpAttribute(false), true);
            ClassMetadataResolver.SetClassDumpMetadata(typeof(ClassMetadataCacheTest), null, new DumpAttribute(true), false);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestSetClassDumpMetadata_NullArg3_Exception()
        {
            ClassMetadataResolver.SetClassDumpMetadata(typeof(ClassMetadataCacheTest), typeof(ClassMetadataResolver), null, true);
            ClassMetadataResolver.SetClassDumpMetadata(typeof(ClassMetadataCacheTest), typeof(ClassMetadataCacheTest), null, false);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestSetClassDumpMetadata_Exception()
        {
            ClassMetadataResolver.SetClassDumpMetadata(typeof(ClassMetadataCacheTest), typeof(ClassMetadataResolver), new DumpAttribute(false), true);
            ClassMetadataResolver.SetClassDumpMetadata(typeof(ClassMetadataCacheTest), typeof(ClassMetadataCacheTest), new DumpAttribute(true), false);
        }

        //////////////////////////////////////////////////////////////////////////////

        [TestMethod]
        public void TestGetClassDumpAttribute_InCache()
        {
            ClassMetadataRegistrar.RegisterMetadata();

            var dumpAttribute = ClassMetadataResolver.GetClassDumpMetadata(typeof(Exception));

            Assert.AreEqual(typeof(ExceptionDumpMetadata), dumpAttribute.Metadata);
            Assert.AreEqual(DumpAttribute.Default, dumpAttribute.DumpAttribute);
        }

        class Test1
        {
        }

        [Dump(false)]
        class Test2
        {
        }

        [MetadataType(typeof(Test3Meta))]
        class Test3
        {
        }

        class Test3Meta
        {
            private Test3Meta() { }
        }

        [MetadataType(typeof(Test4Meta))]
        class Test4
        {
        }

        [Dump(false)]
        class Test4Meta
        {
            private Test4Meta() { }
        }

        [TestMethod]
        public void TestGetClassDumpAttribute_NotInCache1()
        {
            var dumpAttribute = ClassMetadataResolver.GetClassDumpMetadata(typeof(Test1));

            Assert.AreEqual(typeof(Test1), dumpAttribute.Metadata);
            Assert.AreEqual(DumpAttribute.Default, dumpAttribute.DumpAttribute);
        }

        [TestMethod]
        public void TestGetClassDumpAttribute_NotInCache2()
        {
            var dumpAttribute = ClassMetadataResolver.GetClassDumpMetadata(typeof(Test2));

            Assert.AreEqual(typeof(Test2), dumpAttribute.Metadata);
            Assert.AreEqual(new DumpAttribute(false), dumpAttribute.DumpAttribute);
        }

        [TestMethod]
        public void TestGetClassDumpAttribute_NotInCache3()
        {
            var dumpAttribute = ClassMetadataResolver.GetClassDumpMetadata(typeof(Test3));

            Assert.AreEqual(typeof(Test3Meta), dumpAttribute.Metadata);
            Assert.AreEqual(DumpAttribute.Default, dumpAttribute.DumpAttribute);
        }

        [TestMethod]
        public void TestGetClassDumpAttribute_NotInCache4()
        {
            var dumpAttribute = ClassMetadataResolver.GetClassDumpMetadata(typeof(Test4));

            Assert.AreEqual(typeof(Test4Meta), dumpAttribute.Metadata);
            Assert.AreEqual(new DumpAttribute(false), dumpAttribute.DumpAttribute);
        }
    }
}
