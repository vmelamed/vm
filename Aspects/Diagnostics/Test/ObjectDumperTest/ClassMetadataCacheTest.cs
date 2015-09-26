using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vm.Aspects.Diagnostics.ExternalMetadata;

namespace vm.Aspects.Diagnostics.ObjectDumper.Tests
{
    [TestClass]
    public class ClassMetadataCacheTest
    {
        static PrivateType ClassMetadataCacheAccessor = new PrivateType(typeof(ClassMetadataResolver));
        static int initialCacheSize = TypesDumpData.Count();

        static Dictionary<Type, ClassDumpData> TypesDumpData
        {
            get { return (Dictionary<Type, ClassDumpData>)ClassMetadataCacheAccessor.GetStaticProperty("TypesDumpData", null); }
        }

        [TestMethod]
        public void TestSetClassDumpData_NullArg2n3()
        {
            ClassMetadataResolver.SetClassDumpData(typeof(ClassMetadataCacheTest), null, null);

            Assert.AreEqual(initialCacheSize+1, TypesDumpData.Count());
            Assert.AreEqual(
                new ClassDumpData(typeof(ClassMetadataCacheTest), new DumpAttribute()),
                TypesDumpData[typeof(ClassMetadataCacheTest)]);
        }

        [TestMethod]
        public void TestSetClassDumpData_NullArg2()
        {
            ClassMetadataResolver.SetClassDumpData(typeof(ClassMetadataCacheTest), null, new DumpAttribute(false));

            Assert.AreEqual(initialCacheSize+1, TypesDumpData.Count());
            Assert.AreEqual(
                new ClassDumpData(typeof(ClassMetadataCacheTest), new DumpAttribute(false)),
                TypesDumpData[typeof(ClassMetadataCacheTest)]);
        }

        [TestMethod]
        public void TestSetClassDumpData_NullArg3()
        {
            ClassMetadataResolver.SetClassDumpData(typeof(ClassMetadataCacheTest), typeof(ClassMetadataResolver), null);

            Assert.AreEqual(initialCacheSize+1, TypesDumpData.Count());
            Assert.AreEqual(
                new ClassDumpData(typeof(ClassMetadataResolver), new DumpAttribute()),
                TypesDumpData[typeof(ClassMetadataCacheTest)]);
        }

        [TestMethod]
        public void TestSetClassDumpData()
        {
            ClassMetadataResolver.SetClassDumpData(typeof(ClassMetadataCacheTest), typeof(ClassMetadataResolver), new DumpAttribute(false));

            Assert.AreEqual(initialCacheSize+1, TypesDumpData.Count());
            Assert.AreEqual(
                new ClassDumpData(typeof(ClassMetadataResolver), new DumpAttribute(false)),
                TypesDumpData[typeof(ClassMetadataCacheTest)]);
        }

        [TestMethod]
        public void TestGetClassDumpAttribute_InCache()
        {
            ClassMetadataRegistrar.RegisterMetadata();

            var dumpAttribute = ClassMetadataResolver.GetClassDumpData(typeof(Exception));

            Assert.AreSame(typeof(ExceptionDumpMetadata), dumpAttribute.Metadata);
            Assert.AreSame(DumpAttribute.Default, dumpAttribute.DumpAttribute);
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
            var dumpAttribute = ClassMetadataResolver.GetClassDumpData(typeof(Test1));

            Assert.AreEqual(typeof(Test1), dumpAttribute.Metadata);
            Assert.AreEqual(DumpAttribute.Default, dumpAttribute.DumpAttribute);
        }

        [TestMethod]
        public void TestGetClassDumpAttribute_NotInCache2()
        {
            var dumpAttribute = ClassMetadataResolver.GetClassDumpData(typeof(Test2));

            Assert.AreEqual(typeof(Test2), dumpAttribute.Metadata);
            Assert.AreEqual(new DumpAttribute(false), dumpAttribute.DumpAttribute);
        }

        [TestMethod]
        public void TestGetClassDumpAttribute_NotInCache3()
        {
            var dumpAttribute = ClassMetadataResolver.GetClassDumpData(typeof(Test3));

            Assert.AreEqual(typeof(Test3Meta), dumpAttribute.Metadata);
            Assert.AreEqual(DumpAttribute.Default, dumpAttribute.DumpAttribute);
        }

        [TestMethod]
        public void TestGetClassDumpAttribute_NotInCache4()
        {
            var dumpAttribute = ClassMetadataResolver.GetClassDumpData(typeof(Test4));

            Assert.AreEqual(typeof(Test4Meta), dumpAttribute.Metadata);
            Assert.AreEqual(new DumpAttribute(false), dumpAttribute.DumpAttribute);
        }
    }
}
