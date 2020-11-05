﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using vm.Aspects.Diagnostics.Implementation;

#if NETFRAMEWORK
using System.Security;
using System.Security.Policy;

using vm.Aspects.Diagnostics.Tests.ObjectDumper.PartialTrust;
#endif

#pragma warning disable 67, 649

namespace vm.Aspects.Diagnostics.Tests.ObjectDumper
{
    [TestClass]
    public partial class ObjectTextDumperTest
    {
#if NETCOREAPP  ||  NETSTANDARD
        public const string CoreDotNetAssembly           = "System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e";
        public const string CoreDotNetAssembly1          = "System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e";
        public const string CSharpLambda                 = "Expression1<Func<";
        public const string LinqAssembly                 = "System.Linq.Expressions, Version=4.2.1.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
        public const string LinqExpression               = "System.Linq.Expressions.Expression1";
        public const string ObjectTextDumperTestAssembly = "vm.Aspects.Diagnostics.Tests.ObjectDumper.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393";
        public const string ReadOnlyCollection           = "ReadOnlyCollection";
        public const string RoCollectionNamespace        = "System.Collections.ObjectModel";
        public const string RuntimeExtensionsAssembly    = "System.Runtime.Extensions, Version=4.2.1.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
#elif NETFRAMEWORK
        public const string CoreDotNetAssembly           = "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
        public const string CoreDotNetAssembly1          = "System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
        public const string CSharpLambda                 = "Expression<Func<";
        public const string LinqAssembly                 = "System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
        public const string LinqExpression               = "System.Linq.Expressions.Expression";
        public const string ObjectTextDumperTestAssembly = "vm.Aspects.Diagnostics.Tests.ObjectDumper, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393";
        public const string ReadOnlyCollection           = "TrueReadOnlyCollection";
        public const string RoCollectionNamespace        = "System.Runtime.CompilerServices";
        public const string RuntimeExtensionsAssembly    = "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
#endif

        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //

        // Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize]
        //public static void ClassInitialize(
        //    TestContext testContext)
        //{
        //}

        // Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup]
        //public static void ClassCleanup()
        //{
        //}

        // Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void TestInitialize()
            => ObjectTextDumper.DefaultDumpSettings = DumpSettings.Default;

        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        #endregion

        ObjectTextDumper GetDumper(
            int indentSize = 2)
            => GetDumper(new StringWriter(CultureInfo.InvariantCulture), indentSize);

        ObjectTextDumper GetDumper(
            StringWriter w,
            int indentSize = 2)
        {
            if (indentSize != DumpSettings.DefaultIndentSize)
            {
                var settings = DumpSettings.Default;

                ObjectTextDumper.DefaultDumpSettings = settings;
            }

            return new ObjectTextDumper(w);
        }

        Stopwatch _sw = new Stopwatch();

        [TestMethod]
        public void TestIsBasicType()
        {
            Debug.WriteLine(nameof(TestIsBasicType));

            for (var i = 2; i<_basicValues.Length; i++)
                Assert.IsTrue(_basicValues[i].GetType().IsBasicType());
        }

        [TestMethod]
        public void TestDumpedBasicValue()
        {
            Debug.WriteLine(nameof(TestDumpedBasicValue));
            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                Assert.IsFalse(w.DumpedBasicValue(this, null));
                foreach (var v in _basicValues)
                    Assert.IsTrue(w.DumpedBasicValue(v, null));
            }
        }

        void TestDumpedBasicValueText(
            string expected,
            object value,
            DumpAttribute dumpAttribute = null)
        {
            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                Assert.IsTrue(w.DumpedBasicValue(value, dumpAttribute));

                var actual = w.GetStringBuilder().ToString();

                TestContext.WriteLine("{0}", actual);
                Assert.AreEqual(expected, actual);
            }
        }

        void TestDumpObjectBasicValueText(
            string expected,
            object value,
            Type metadata = null,
            DumpAttribute dumpAttribute = null,
            int indentValue = 0)
        {
            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var target = GetDumper(w);

                Assert.AreEqual(target, target.Dump(value, metadata, dumpAttribute, indentValue));

                var actual = w.GetStringBuilder().ToString();

                TestContext.WriteLine("{0}", actual);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void TestDumpedBasicValueText()
        {
            Debug.WriteLine(nameof(TestDumpedBasicValueText));
            for (var i = 0; i<_basicValues.Length; i++)
                TestDumpedBasicValueText(_basicValuesStrings[i], _basicValues[i]);
        }

        [TestMethod]
        public void TestDumpedBasicValueTextIndent()
        {
            Debug.WriteLine(nameof(TestDumpedBasicValueTextIndent));
            for (var i = 0; i<_basicValues.Length; i++)
                TestDumpedBasicValueText(_basicValuesStrings[i], _basicValues[i], null);
        }

        [TestMethod]
        public void TestDumpMaskedBasicValueText()
        {
            Debug.WriteLine(nameof(TestDumpMaskedBasicValueText));
            for (var i = 0; i<_basicValues.Length; i++)
                TestDumpedBasicValueText(_basicValues[i]==null ? "<null>" : "------", _basicValues[i], new DumpAttribute { Mask = true, MaskValue = "------" });
        }

        [TestMethod]
        public void TestDumpMaskedBasicValueText1()
        {
            Debug.WriteLine(nameof(TestDumpMaskedBasicValueText1));
            for (var i = 0; i<_basicValues.Length; i++)
                TestDumpedBasicValueText(_basicValues[i]==null ? "<null>" : "******", _basicValues[i], new DumpAttribute { Mask = true });
        }

        [TestMethod]
        public void TestDumpedBasicValueFormat()
        {
            Debug.WriteLine(nameof(TestDumpedBasicValueFormat));
            TestDumpedBasicValueText("{00000000-0000-0000-0000-000000000000}", Guid.Empty, new DumpAttribute { ValueFormat = "{0:B}" });
            TestDumpedBasicValueText("¤1.00", 1.0, new DumpAttribute { ValueFormat = "{0:C}" });
            TestDumpedBasicValueText("¤1.00", 1M, new DumpAttribute { ValueFormat = "{0:C}" });
        }

        [TestMethod]
        public void TestStringValueLength()
        {
            Debug.WriteLine(nameof(TestStringValueLength));
            TestDumpedBasicValueText("012345678901...", "01234567890123456789", new DumpAttribute { MaxLength = 12 });
        }

        [TestMethod]
        public void TestDumpObjectBasicValueText()
        {
            Debug.WriteLine(nameof(TestDumpObjectBasicValueText));
            for (var i = 0; i<_basicValues.Length; i++)
                TestDumpObjectBasicValueText(_basicValuesStrings[i], _basicValues[i]);
        }

        [TestMethod]
        public void TestDumpObjectBasicValueTextIndent()
        {
            Debug.WriteLine(nameof(TestDumpObjectBasicValueTextIndent));
            for (var i = 0; i<_basicValues.Length; i++)
                TestDumpObjectBasicValueText(_basicValuesStrings[i], _basicValues[i], null, null, 2);
        }

        [TestMethod]
        public void TestDumpObjectMaskedBasicValueText()
        {
            Debug.WriteLine(nameof(TestDumpObjectMaskedBasicValueText));
            for (var i = 0; i<_basicValues.Length; i++)
                TestDumpObjectBasicValueText(_basicValues[i]==null ? "<null>" : "------", _basicValues[i], null, new DumpAttribute { Mask = true, MaskValue = "------" });
        }

        [TestMethod]
        public void TestDumpObjectMaskedBasicValueText1()
        {
            Debug.WriteLine(nameof(TestDumpObjectMaskedBasicValueText1));
            for (var i = 0; i<_basicValues.Length; i++)
                TestDumpObjectBasicValueText(_basicValues[i]==null ? "<null>" : "******", _basicValues[i], null, new DumpAttribute { Mask = true });
        }

        [TestMethod]
        public void TestDumpObjectBasicValueFormat()
        {
            Debug.WriteLine(nameof(TestDumpObjectBasicValueFormat));
            TestDumpObjectBasicValueText("{00000000-0000-0000-0000-000000000000}", Guid.Empty, null, new DumpAttribute { ValueFormat = "{0:B}" });
            TestDumpObjectBasicValueText("¤1.00", 1.0, null, new DumpAttribute { ValueFormat = "{0:C}" });
            TestDumpObjectBasicValueText("¤1.00", 1M, null, new DumpAttribute { ValueFormat = "{0:C}" });
        }

        [TestMethod]
        public void TestObjectStringValueLength()
        {
            Debug.WriteLine(nameof(TestObjectStringValueLength));
            TestDumpObjectBasicValueText("012345678901...", "01234567890123456789", null, new DumpAttribute { MaxLength = 12 });
        }

        string Act(
            StringWriter w,
            object obj,
            Type metadata,
            DumpAttribute classDumpAttribute,
            Func<TextWriter, ObjectTextDumper> dumperFactory)
        {
            var target = dumperFactory(w);

            _sw.Reset();
            _sw.Start();
            target.Dump(obj, metadata, classDumpAttribute);
            _sw.Stop();

            var result = w.GetStringBuilder().ToString();

            w.GetStringBuilder().Clear();

            return result;
        }

        const string _firstDump  = "First dump";
        const string _secondDump = "Second dump";
        const string _thirdDump  = "Third dump";

        void AssertResult(
            string expected,
            string actual,
            string dumpId)
        {
            var dump = $"{dumpId} ({_sw.Elapsed}):{actual}";

            TestContext.WriteLine(dump);
            Debug.WriteLine(dump);

            Assert.AreEqual(expected, actual, $"{dumpId} assertion failed.");
        }

        void AssertResultStartsWith(
            string expected,
            string actual,
            string dumpId)
        {
            var dump = $"{dumpId} ({_sw.Elapsed}):{actual}";

            TestContext.WriteLine(dump);
            Debug.WriteLine(dump);

            var assertion = actual.StartsWith(expected);

            if (!assertion)
                TestContext.WriteLine($@"Expected:<{expected}>
Actual:<{actual}>
");

            Assert.IsTrue(assertion, $"{dumpId} assertion failed.");
        }

        void ActAndAssert(
            string testName,
            string expected,
            object obj,
            Type metadata,
            DumpAttribute classDumpAttribute = null,
            Func<TextWriter, ObjectTextDumper> dumperFactory = null)
        {
            if (dumperFactory == null)
                dumperFactory = w => new ObjectTextDumper(w);

            Debug.WriteLine(testName);

            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var actual1 = Act(w, obj, metadata, classDumpAttribute, dumperFactory);

                AssertResult(expected, actual1, _firstDump);

                // --------------------------

                if (ObjectTextDumper.DefaultDumpSettings.UseDumpScriptCache)
                {
                    var actual2 = Act(w, obj, metadata, classDumpAttribute, dumperFactory);

                    AssertResult(expected, actual2, _secondDump);
                }
            }
            if (ObjectTextDumper.DefaultDumpSettings.UseDumpScriptCache)
                using (var w = new StringWriter(CultureInfo.InvariantCulture))
                {
                    var actual3 = Act(w, obj, metadata, classDumpAttribute, dumperFactory);

                    AssertResult(expected, actual3, _thirdDump);
                }
        }

        void ActAndAssertStartsWith(
            string testName,
            string expected,
            object obj,
            Type metadata,
            DumpAttribute classDumpAttribute = null,
            Func<TextWriter, ObjectTextDumper> dumperFactory = null)
        {
            if (dumperFactory == null)
                dumperFactory = w => new ObjectTextDumper(w);

            Debug.WriteLine(testName);

            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var actual1 = Act(w, obj, metadata, classDumpAttribute, dumperFactory);

                AssertResultStartsWith(expected, actual1, _firstDump);

                // --------------------------

                if (ObjectTextDumper.DefaultDumpSettings.UseDumpScriptCache)
                {
                    var actual2 = Act(w, obj, metadata, classDumpAttribute, dumperFactory);

                    AssertResultStartsWith(expected, actual2, _secondDump);
                }
            }
            if (ObjectTextDumper.DefaultDumpSettings.UseDumpScriptCache)
                using (var w = new StringWriter(CultureInfo.InvariantCulture))
                {
                    var actual3 = Act(w, obj, metadata, classDumpAttribute, dumperFactory);

                    AssertResultStartsWith(expected, actual3, _thirdDump);
                }
        }

        void ActAndAssert(
            string testName,
            string expected,
            object obj,
            Func<TextWriter, ObjectTextDumper> dumperFactory = null)
        {
            ActAndAssert(testName, expected, obj, null, null, dumperFactory);
        }

        void ActAndAssertStartsWith(
            string testName,
            string expected,
            object obj,
            Func<TextWriter, ObjectTextDumper> dumperFactory = null)
        {
            ActAndAssertStartsWith(testName, expected, obj, null, null, dumperFactory);
        }

        [TestMethod]
        public void TestDumpObject1_1()
        {
            ActAndAssert(
                nameof(TestDumpObject1_1),
                @"
Object1 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object1, "+ObjectTextDumperTestAssembly+@"): 
  BoolField                = True
  ByteField                = 1
  CharField                = A
  DateTimeField            = 2013-01-13T00:00:00.0000000
  DateTimeOffsetField      = 2013-01-13T00:00:00.0000000-05:00
  DecimalField             = 1
  DoubleField              = 1
  FloatField               = 1
  GuidField                = 00000000-0000-0000-0000-000000000000
  IntField                 = 1
  LongField                = 1
  NullIntField             = <null>
  NullLongField            = 1
  ObjectField              = <null>
  SByteField               = 1
  ShortField               = 1
  TimeSpanField            = 00:00:00.0000123
  UIntField                = 1
  ULongField               = 1
  UShortField              = 1
  UriField                 = http://localhost/
  BoolProperty             = True
  ByteProperty             = 1
  CharProperty             = A
  DateTimeOffsetProperty   = 2013-01-13T00:00:00.0000000-05:00
  DateTimeProperty         = 2013-01-13T00:00:00.0000000
  DecimalProperty          = 1
  DoubleProperty           = 1
  FloatProperty            = 1
  GuidProperty             = 00000000-0000-0000-0000-000000000000
  IntProperty              = 1
  LongProperty             = 1
  NullIntProperty          = <null>
  NullLongProperty         = 1
  ObjectProperty           = <null>
  SByteProperty            = 1
  ShortProperty            = 1
  TimeSpanProperty         = 00:00:00.0000123
  UIntProperty             = 1
  ULongProperty            = 1
  UShortProperty           = 1
  UriProperty              = http://localhost/",
                new Object1());
        }

        [TestMethod]
        public void TestDumpObject1WithFields_1()
        {
            ActAndAssert(
                nameof(TestDumpObject1WithFields_1),
                @"
Object1 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object1, "+ObjectTextDumperTestAssembly+@"): 
  BoolField                = True
  ByteField                = 1
  CharField                = A
  DateTimeField            = 2013-01-13T00:00:00.0000000
  DateTimeOffsetField      = 2013-01-13T00:00:00.0000000-05:00
  DecimalField             = 1
  DoubleField              = 1
  FloatField               = 1
  GuidField                = 00000000-0000-0000-0000-000000000000
  IntField                 = 1
  LongField                = 1
  NullIntField             = <null>
  NullLongField            = 1
  ObjectField              = <null>
  SByteField               = 1
  ShortField               = 1
  TimeSpanField            = 00:00:00.0000123
  UIntField                = 1
  ULongField               = 1
  UShortField              = 1
  UriField                 = http://localhost/
  BoolProperty             = True
  ByteProperty             = 1
  CharProperty             = A
  DateTimeOffsetProperty   = 2013-01-13T00:00:00.0000000-05:00
  DateTimeProperty         = 2013-01-13T00:00:00.0000000
  DecimalProperty          = 1
  DoubleProperty           = 1
  FloatProperty            = 1
  GuidProperty             = 00000000-0000-0000-0000-000000000000
  IntProperty              = 1
  LongProperty             = 1
  NullIntProperty          = <null>
  NullLongProperty         = 1
  ObjectProperty           = <null>
  SByteProperty            = 1
  ShortProperty            = 1
  TimeSpanProperty         = 00:00:00.0000123
  UIntProperty             = 1
  ULongProperty            = 1
  UShortProperty           = 1
  UriProperty              = http://localhost/",
                new Object1(),
                w =>
                {
                    var settings = ObjectTextDumper.DefaultDumpSettings;

                    settings.PropertyBindingFlags = BindingFlags.DeclaredOnly|
                                                      BindingFlags.Instance|
                                                      BindingFlags.NonPublic|
                                                      BindingFlags.Public;
                    settings.FieldBindingFlags     = BindingFlags.DeclaredOnly|
                                                      BindingFlags.Instance|
                                                      BindingFlags.NonPublic|
                                                      BindingFlags.Public;
                    settings.IndentSize             = 2;
                    settings.MaxDumpLength          = DumpTextWriter.DefaultMaxLength;

                    ObjectTextDumper.DefaultDumpSettings = settings;

                    return new ObjectTextDumper(w);
                });
        }

        [TestMethod]
        public void TestDumpObject1_1_Limited()
        {
            ActAndAssertStartsWith(
                nameof(TestDumpObject1_1_Limited),
                @"
Object1 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object1, "+ObjectTextDumperTestAssembly+@"): 
  BoolField                = True
  ByteField                = 1
  CharField                = A
  DateTimeField            = 2013-01-13T00:00:00.0000000
  DateTimeOffsetField      = 2013-01-13T00:00:00.0000000-05:00
  DecimalField             = 1
  DoubleField              = 1
  FloatField               = 1",
                new Object1(),
                //w => new ObjectTextDumper(w, 0, 2, 500));
                w =>
                {
                    var settings = ObjectTextDumper.DefaultDumpSettings;

                    settings.IndentSize             = 2;
                    settings.MaxDumpLength          = 500;

                    ObjectTextDumper.DefaultDumpSettings = settings;

                    return new ObjectTextDumper(w);
                });
        }

        [TestMethod]
        public void TestDumpObject1_2()
        {
            ActAndAssert(
                nameof(TestDumpObject1_2),
                @"
Object1 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object1, "+ObjectTextDumperTestAssembly+@"): 
  ObjectProperty           : <null>
  NullIntProperty          = <null>
  NullLongProperty         = 1
  BoolProperty             = True
  CharProperty             = A
  ByteProperty             = 1
  SByteProperty            = 1
  ShortProperty            = 1
  IntProperty              = 1
  LongProperty             = 1
  BoolField                = True
  ByteField                = 1
  CharField                = A
  DateTimeField            = 2013-01-13T00:00:00.0000000
  DateTimeOffsetField      = 2013-01-13T00:00:00.0000000-05:00
  DecimalField             = 1
  DoubleField              = 1
  FloatField               = 1
  GuidField                = 00000000-0000-0000-0000-000000000000
  IntField                 = 1
  LongField                = 1
  NullIntField             = <null>
  NullLongField            = 1
  ObjectField              = <null>
  SByteField               = 1
  ShortField               = 1
  TimeSpanField            = 00:00:00.0000123
  UIntField                = 1
  ULongField               = 1
  UShortField              = 1
  UriField                 = http://localhost/
  UIntProperty             = 1
  ULongProperty            = 1
  DoubleProperty           = 1.0
  FloatProperty            = 1.0
  DecimalProperty          = 1
  GuidProperty             = 00000000-0000-0000-0000-000000000000
  UriProperty              = http://localhost/
  DateTimeProperty         = 2013-01-13T00:00:00.0000000
  TimeSpanProperty         = 00:00:00.0000123",
                new Object1(),
                typeof(Object1Metadata));
        }

        [TestMethod]
        public void TestDumpObject1WithFieldsMetadata_2()
        {
            ActAndAssert(
                nameof(TestDumpObject1WithFieldsMetadata_2),
                @"
Object1 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object1, "+ObjectTextDumperTestAssembly+@"): 
  ObjectProperty           : <null>
  NullIntProperty          = <null>
  NullLongProperty         = 1
  BoolProperty             = True
  CharProperty             = A
  ByteProperty             = 1
  SByteProperty            = 1
  ShortProperty            = 1
  IntProperty              = 1
  LongProperty             = 1
  BoolField                = True
  ByteField                = 1
  CharField                = A
  DateTimeField            = 2013-01-13T00:00:00.0000000
  DateTimeOffsetField      = 2013-01-13T00:00:00.0000000-05:00
  DecimalField             = 1
  DoubleField              = 1
  FloatField               = 1
  GuidField                = 00000000-0000-0000-0000-000000000000
  IntField                 = 1
  LongField                = 1
  NullIntField             = <null>
  NullLongField            = 1
  ObjectField              = <null>
  SByteField               = 1
  ShortField               = 1
  TimeSpanField            = 00:00:00.0000123
  UIntField                = 1
  ULongField               = 1
  UShortField              = 1
  UriField                 = http://localhost/
  UIntProperty             = 1
  ULongProperty            = 1
  DoubleProperty           = 1.0
  FloatProperty            = 1.0
  DecimalProperty          = 1
  GuidProperty             = 00000000-0000-0000-0000-000000000000
  UriProperty              = http://localhost/
  DateTimeProperty         = 2013-01-13T00:00:00.0000000
  TimeSpanProperty         = 00:00:00.0000123",
                new Object1(),
                typeof(Object1FieldsMetadata));
        }

        [TestMethod]
        public void TestDumpObject1_3()
        {
            ActAndAssert(
                nameof(TestDumpObject1_3),
                @"
Object1 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object1, "+ObjectTextDumperTestAssembly+@"): 
  NullLongProperty         = 1
  BoolProperty             = True
  CharProperty             = A
  ByteProperty             = 1
  SByteProperty            = 1
  ShortProperty            = 1
  IntProperty              = 1
  LongProperty             = 1
  BoolField                = True
  ByteField                = 1
  CharField                = A
  DateTimeField            = 2013-01-13T00:00:00.0000000
  DateTimeOffsetField      = 2013-01-13T00:00:00.0000000-05:00
  DecimalField             = 1
  DoubleField              = 1
  FloatField               = 1
  GuidField                = 00000000-0000-0000-0000-000000000000
  IntField                 = 1
  LongField                = 1
  NullLongField            = 1
  SByteField               = 1
  ShortField               = 1
  TimeSpanField            = 00:00:00.0000123
  UIntField                = 1
  ULongField               = 1
  UShortField              = 1
  UriField                 = http://localhost/
  UIntProperty             = 1
  ULongProperty            = 1
  DoubleProperty           = 1.0
  FloatProperty            = 1.0
  DecimalProperty          = 1
  GuidProperty             = 00000000-0000-0000-0000-000000000000
  UriProperty              = http://localhost/
  DateTimeProperty         = 2013-01-13T00:00:00.0000000
  TimeSpanProperty         = 00:00:00.0000123",
                new Object1(),
                typeof(Object1Metadata),
                new DumpAttribute { DumpNullValues = ShouldDump.Skip });
        }

        [TestMethod]
        public void TestDumpObject2_1()
        {
            ActAndAssert(
                nameof(TestDumpObject2_1),
                @"
Object2 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object2, "+ObjectTextDumperTestAssembly+@"): 
  NullLongProperty         = 1
  BoolProperty             = True
  CharProperty             = A
  ByteProperty             = 1
  SByteProperty            = 1
  ShortProperty            = 1
  IntProperty              = 1
  LongProperty             = 1
  UIntProperty             = 1
  ULongProperty            = 1
  DoubleProperty           = 1.0
  FloatProperty            = 1.0
  DecimalProperty          = 1
  GuidProperty             = 00000000-0000-0000-0000-000000000000
  UriProperty              = http://localhost/
  DateTimeProperty         = 2013-01-13T00:00:00.0000000
  DateTimeProperty1        = 1/25/2013 11:23:45 AM
  TimeSpanProperty         = 00:00:00.0000123",
                new Object2());
        }

        [TestMethod]
        public void TestDumpObject3_1()
        {
            ActAndAssert(
                nameof(TestDumpObject3_1),
                @"
Object3 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object3, "+ObjectTextDumperTestAssembly+@"): 
  NullLongProperty         = 1
  BoolProperty             = True
  CharProperty             = A
  ByteProperty             = 1
  SByteProperty            = 1
  ShortProperty            = 1
  IntProperty              = 1
  LongProperty             = 1
  UIntProperty             = 1
  ULongProperty            = 1
  DoubleProperty           = 1.0
  FloatProperty            = 1.0
  DecimalProperty          = 1
  GuidProperty             = 00000000-0000-0000-0000-000000000000
  UriProperty              = http://localhost/
  DateTimeProperty         = 2013-01-13T00:00:00.0000000
  TimeSpanProperty         = 00:00:00.0000123",
                new Object3());
        }

        [TestMethod]
        public void TestDumpObject5_1()
        {
            ActAndAssert(
                nameof(TestDumpObject5_1),
                @"
Object5_1 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object5_1, "+ObjectTextDumperTestAssembly+@"): 
  PropertyA                = PropertyA
  PropertyB                = PropertyB
  Associate                = Object4_1 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object4_1, "+ObjectTextDumperTestAssembly+@"): 
    Property1                = Property1
  Associate2               = Object4_1 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object4_1, "+ObjectTextDumperTestAssembly+@"): 
    Property1                = Property1
    Property2                = Property2",
                new Object5_1());
        }

        static bool TestMethod(int a) => a < 0;

        [TestMethod]
        public void TestDumpDBNull()
        {
            ActAndAssert(
                nameof(TestDumpDBNull),
                @"DBNull",
                DBNull.Value);
        }

        [TestMethod]
        public void TestDumpExpression()
        {
            Expression<Func<int, int>> expression = a => 3*a + 5;

            var settings = DumpSettings.Default;

            settings.PropertyBindingFlags &= ~BindingFlags.NonPublic;
            ObjectTextDumper.DefaultDumpSettings = settings;
            ClassMetadataRegistrar.RegisterMetadata();

            ActAndAssertStartsWith(
                nameof(TestDumpExpression),
                @"
"+CSharpLambda+@"int, int>> ("+LinqExpression+@"`1[[System.Func`2[[System.Int32, "+CoreDotNetAssembly+@"],[System.Int32, "+CoreDotNetAssembly+@"]], "+CoreDotNetAssembly+@"]], "+LinqAssembly+@"): 
  C#-like expression text:
    (int a) => 3 * a + 5
  NodeType                 = ExpressionType.Lambda
  Type                     = (TypeInfo): System.Func`2[[System.Int32, "+CoreDotNetAssembly+@"],[System.Int32, "+CoreDotNetAssembly+@"]], "+CoreDotNetAssembly+@"
  Name                     = <null>
  ReturnType               = (TypeInfo): System.Int32, "+CoreDotNetAssembly+@"
  Parameters               = "+ReadOnlyCollection+@"<ParameterExpression>[1]: ("+RoCollectionNamespace+"."+ReadOnlyCollection+@"`1[[System.Linq.Expressions.ParameterExpression, "+LinqAssembly+@"]], "+CoreDotNetAssembly1+@")
    PrimitiveParameterExpression<int> (System.Linq.Expressions.PrimitiveParameterExpression`1[[System.Int32, "+CoreDotNetAssembly+@"]], "+LinqAssembly+@"): 
      NodeType                 = ExpressionType.Parameter
      Type                     = (TypeInfo): System.Int32, "+CoreDotNetAssembly+@"
      Name                     = a
      IsByRef                  = False
      NodeType                 = ExpressionType.Parameter
      Type                     = (TypeInfo): System.Int32, "+CoreDotNetAssembly+@"
      CanReduce                = False
  Body                     = SimpleBinaryExpression (System.Linq.Expressions.SimpleBinaryExpression, "+LinqAssembly+@"): 
    NodeType                 = ExpressionType.Add
    Type                     = (TypeInfo): System.Int32, "+CoreDotNetAssembly+@"
    Left                     = SimpleBinaryExpression (System.Linq.Expressions.SimpleBinaryExpression, "+LinqAssembly+@"): 
      NodeType                 = ExpressionType.Multiply
      Type                     = (TypeInfo): System.Int32, "+CoreDotNetAssembly+@"
      Left                     = ConstantExpression (System.Linq.Expressions.ConstantExpression, "+LinqAssembly+@"): 
        NodeType                 = ExpressionType.Constant
        Type                     = (TypeInfo): System.Int32, "+CoreDotNetAssembly+@"
        Value                    = 3
        NodeType                 = ExpressionType.Constant
        CanReduce                = False
      Right                    = PrimitiveParameterExpression<int> (see above)
      IsLiftedLogical          = False
      IsReferenceComparison    = False
      NodeType                 = ExpressionType.Multiply
      Type                     = (TypeInfo): System.Int32, "+CoreDotNetAssembly+@"
      Method                   = <null>
      Conversion               = <null>
      IsLifted                 = False
      IsLiftedToNull           = False
      CanReduce                = False
    Right                    = ConstantExpression (System.Linq.Expressions.ConstantExpression, "+LinqAssembly+@"): 
      NodeType                 = ExpressionType.Constant
      Type                     = (TypeInfo): System.Int32, "+CoreDotNetAssembly+@"
      Value                    = 5
      NodeType                 = ExpressionType.Constant
      CanReduce                = False
    IsLiftedLogical          = False
    IsReferenceComparison    = False
    NodeType                 = ExpressionType.Add
    Type                     = (TypeInfo): System.Int32, "+CoreDotNetAssembly+@"
    Method                   = <null>
    Conversion               = <null>
    IsLifted                 = False
    IsLiftedToNull           = False
    CanReduce                = False
  NodeType                 = ExpressionType.Lambda",
                expression);

            settings.PropertyBindingFlags &= ~BindingFlags.NonPublic;
            ObjectTextDumper.DefaultDumpSettings = DumpSettings.Default;
        }

        [TestMethod]
        public void TestDumpObject6_1()
        {
            ActAndAssert(
                nameof(TestDumpObject6_1),
                @"
Object6 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object6, "+ObjectTextDumperTestAssembly+@"): 
  D                        = static ObjectTextDumperTest.TestMethod
  Ex                       = p => (p > 0)
  Property1                = Property1
  Property2                = Property2",
                new Object6());
        }

        [TestMethod]
        public void TestDumpObject7_1()
        {
            ActAndAssert(
                nameof(TestDumpObject7_1),
                @"
Object7 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object7, "+ObjectTextDumperTestAssembly+@"): 
  Array                    = int[6]: (System.Int32[], "+CoreDotNetAssembly+@")
    0
    1
    2
    3
    4
    5
  Property1                = Property1
  Property2                = Property2",
                new Object7());
        }

        [TestMethod]
        public void TestDumpObject7_2()
        {
            ActAndAssert(
                nameof(TestDumpObject7_2),
                @"
Object7 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object7, "+ObjectTextDumperTestAssembly+@"): 
  Array                    = int[18]: (System.Int32[], "+CoreDotNetAssembly+@")
    0
    1
    2
    3
    4
    5
    0
    1
    2
    3
    ... dumped the first 10/18 elements.
  Property1                = Property1
  Property2                = Property2",
                new Object7
                {
                    Array = new[] { 0, 1, 2, 3, 4, 5, 0, 1, 2, 3, 4, 5, 0, 1, 2, 3, 4, 5, },
                });
        }

        [TestMethod]
        public void TestDumpObject8_1()
        {
            ActAndAssert(
                nameof(TestDumpObject8_1),
                @"
Object8 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object8, "+ObjectTextDumperTestAssembly+@"): 
  Array                    = int[6]: (System.Int32[], "+CoreDotNetAssembly+@")
    0
    1
    2
    ... dumped the first 3/6 elements.
  Array2                   = int[6]: (System.Int32[], "+CoreDotNetAssembly+@")
  Array3                   = byte[18]: 00-01-02-03-04-05-00-01-02-03-04-05-00-01-02-03-04-05
  Array4                   = byte[18]: 00-01-02-03-04-05-00-01-02-03... dumped the first 10/18 elements.
  Array5                   = byte[18]: 00-01-02... dumped the first 3/18 elements.
  Array6                   = ArrayList[18]: (System.Collections.ArrayList, "+RuntimeExtensionsAssembly+@")
    0
    1
    2
    ... dumped the first 3/18 elements.
  Property1                = Property1
  Property2                = Property2",
                new Object8());
        }

        [TestMethod]
        public void TestDumpObject7_1_1()
        {
            ActAndAssert(
                nameof(TestDumpObject7_1_1),
                @"
Object7_1 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object7_1, "+ObjectTextDumperTestAssembly+@"): 
  List                     = List<int>[6]: (System.Collections.Generic.List`1[[System.Int32, "+CoreDotNetAssembly+@"]], "+CoreDotNetAssembly+@")
    0
    1
    2
    3
    4
    5
  Property1                = Property1
  Property2                = Property2",
                new Object7_1());
        }

        [TestMethod]
        public void TestDumpObject7_1_2()
        {
            ActAndAssert(
                nameof(TestDumpObject7_1_2),
                @"
Object7_1 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object7_1, "+ObjectTextDumperTestAssembly+@"): 
  List                     = List<int>[18]: (System.Collections.Generic.List`1[[System.Int32, "+CoreDotNetAssembly+@"]], "+CoreDotNetAssembly+@")
    0
    1
    2
    3
    4
    5
    0
    1
    2
    3
    ... dumped the first 10/18 elements.
  Property1                = Property1
  Property2                = Property2",
                new Object7_1
                {
                    List = new List<int> { 0, 1, 2, 3, 4, 5, 0, 1, 2, 3, 4, 5, 0, 1, 2, 3, 4, 5, },
                });
        }

        [TestMethod]
        public void TestDumpObject7_1_3()
        {
            ActAndAssert(
                nameof(TestDumpObject7_1_3),
                @"
Object7_1 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object7_1, "+ObjectTextDumperTestAssembly+@"): 
  List                     = List<int>[18]: (System.Collections.Generic.List`1[[System.Int32, "+CoreDotNetAssembly+@"]], "+CoreDotNetAssembly+@")
    0
    1
    2
    3
    4
    5
    0
    1
    2
    3
    4
    5
    0
    1
    2
    3
    4
    5
  Property1                = Property1
  Property2                = Property2",
                new Object7_1
                {
                    List = new List<int> { 0, 1, 2, 3, 4, 5, 0, 1, 2, 3, 4, 5, 0, 1, 2, 3, 4, 5, },
                },
                typeof(MetaObject7_1));
        }

        [TestMethod]
        public void TestDumpObject8_1_1()
        {
            ActAndAssert(
                nameof(TestDumpObject8_1_1),
                @"
Object8_1 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object8_1, "+ObjectTextDumperTestAssembly+@"): 
  List                     = int[6]: (System.Int32[], "+CoreDotNetAssembly+@")
    0
    1
    2
    ... dumped the first 3/6 elements.
  List2                    = int[6]: (System.Int32[], "+CoreDotNetAssembly+@")
  Property1                = Property1
  Property2                = Property2",
                new Object8_1());
        }

        [TestMethod]
        public void TestDumpObject9_1()
        {
            ActAndAssert(
                nameof(TestDumpObject9_1),
                @"
Object91 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object91, "+ObjectTextDumperTestAssembly+@"): 
  Object90                 = Object90 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object90, "+ObjectTextDumperTestAssembly+@"): 
    Flags                    = TestFlags (Two | Four)
    Prop                     = TestEnum.One
  Prop91                   = 0
  Prop92                   = 1
  Prop911                  = 2
  Prop912                  = 3
  Prop913                  = 6
  Prop914                  = 7
  InheritedObject90        = Object90 (see above)
  Prop93                   = 4
  Prop94                   = 5",
                new Object91());
        }

        [TestMethod]
        public void TestDumpObject9_1null()
        {
            ActAndAssert(
                nameof(TestDumpObject9_1),
                @"
Object91 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object91, "+ObjectTextDumperTestAssembly+@"): 
  Object90                 = Object90 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object90, "+ObjectTextDumperTestAssembly+@"): 
    Flags                    = TestFlags (Two | Four)
    Prop                     = TestEnum.One
  Prop91                   = 0
  Prop92                   = 1
  Prop911                  = 2
  Prop912                  = 3
  Prop913                  = 6
  Prop914                  = 7
  InheritedObject90        = Object90 (see above)
  Prop93                   = 4
  Prop94                   = 5",
                new Object91());

            ActAndAssert(
                nameof(TestDumpObject9_1),
                @"
Object91 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object91, "+ObjectTextDumperTestAssembly+@"): 
  Object90                 = <null>
  Prop91                   = 0
  Prop92                   = 1
  Prop911                  = 2
  Prop912                  = 3
  Prop913                  = 6
  Prop914                  = 7
  InheritedObject90        = <null>
  Prop93                   = 4
  Prop94                   = 5",
                new Object91() { Object90 = null });
        }

        [TestMethod]
        public void TestDumpObjectWithDelegates()
        {
            ActAndAssert(
                nameof(TestDumpObjectWithDelegates),
                @"
ObjectWithDelegates (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+ObjectWithDelegates, "+ObjectTextDumperTestAssembly+@"): 
  DelegateProp0            = <null>
  DelegateProp1            = static Object10.Static
  DelegateProp2            = Object10.Instance
  DelegateProp3            = static ObjectTextDumperTest.TestMethod
  Object10Prop             = Object10 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object10, "+ObjectTextDumperTestAssembly+@"): 
    Offset                   = 23",
                new ObjectWithDelegates());
        }

        [TestMethod]
        public void TestDumpObjectWithMyEnumerable()
        {
            ActAndAssert(
                nameof(TestDumpObjectWithMyEnumerable),
                @"
ObjectWithMyEnumerable (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+ObjectWithMyEnumerable, "+ObjectTextDumperTestAssembly+@"): 
  MyEnumerable             = MyEnumerable (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+MyEnumerable, "+ObjectTextDumperTestAssembly+@"): 
    Property                 = foo
    MyEnumerable[]: (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+MyEnumerable, "+ObjectTextDumperTestAssembly+@")
      0
      1
      3
  Stuff                    = stuff",
                new ObjectWithMyEnumerable());
        }

        [TestMethod]
        public void TestDumpObjectWithMemberInfos()
        {
            ActAndAssert(
                nameof(TestDumpObjectWithMemberInfos),
                @"
ObjectWithMemberInfos (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+ObjectWithMemberInfos, "+ObjectTextDumperTestAssembly+@"): 
  EventInfo                = (Event): EventHandler ObjectWithMembers.Event
  IndexerIntInfo           = (Property): String ObjectWithMembers.this[Int32] { get; }
  IndexerStringInfo        = (Property): String ObjectWithMembers.this[String, Int32] { get;set; }
  MemberInfo               = (Field): Int32 ObjectWithMembers.member
  MemberInfos              = MemberInfo[22]: (System.Reflection.MemberInfo[], "+CoreDotNetAssembly+@")
    (Constructor): .ctor
    (Method): Void ObjectWithMembers.add_Event(EventHandler value)
    (Method): Boolean Object.Equals(Object obj)
    (Event): EventHandler ObjectWithMembers.Event
    (Method): String ObjectWithMembers.get_Item(Int32 index)
    (Method): String ObjectWithMembers.get_Item(String index, Int32 index1)
    (Method): Int32 ObjectWithMembers.get_Property()
    (Method): Int32 Object.GetHashCode()
    (Method): Type Object.GetType()
    (Property): String ObjectWithMembers.this[Int32] { get; }
    ... dumped the first 10/22 elements.
  Method1Info              = (Method): Void ObjectWithMembers.Method1()
  Method2Info              = (Method): Int32 ObjectWithMembers.Method2(Int32 a)
  Method3Info              = (Method): String ObjectWithMembers.Method3(Int32 a, Int32 b)
  Method4Info              = (Method): T ObjectWithMembers.Method4<T>(Int32 a, Int32 b)
  Method5Info              = (Method): T ObjectWithMembers.Method5<T, U>(Int32 a, Int32 b)
  PropertyInfo             = (Property): Int32 ObjectWithMembers.Property { get;set; }
  Type                     = (NestedType): vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+ObjectWithMembers, "+ObjectTextDumperTestAssembly+@"",
                new ObjectWithMemberInfos());
        }

        [Dump(MaxDepth = 3)]
        class NestedItem
        {
            public int Property { get; set; }
            public NestedItem Next { get; set; }
        }

        [TestMethod]
        public void TestDumpNestedObject()
        {
            ActAndAssert(
                nameof(TestDumpNestedObject),
                @"
NestedItem (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+NestedItem, "+ObjectTextDumperTestAssembly+@"): 
  Next                     = NestedItem (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+NestedItem, "+ObjectTextDumperTestAssembly+@"): 
    Next                     = NestedItem (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+NestedItem, "+ObjectTextDumperTestAssembly+@"): 
      Next                     = NestedItem (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+NestedItem, "+ObjectTextDumperTestAssembly+@"): 
        Next                     = ...object dump reached the maximum depth level. Use the DumpAttribute.MaxDepth to increase the depth level if needed.
        Property                 = 3
      Property                 = 2
    Property                 = 1
  Property                 = 0",
                new NestedItem
                {
                    Property = 0,
                    Next = new NestedItem
                    {
                        Property = 1,
                        Next = new NestedItem
                        {
                            Property = 2,
                            Next = new NestedItem
                            {
                                Property = 3,
                                Next = new NestedItem
                                {
                                    Property = 4,
                                    Next = null,
                                }
                            }
                        },
                    },
                });
        }

        [TestMethod]
        public void TestCollectionObject()
        {
            ActAndAssert(
                nameof(TestCollectionObject),
                @"
List<string>[3]: (System.Collections.Generic.List`1[[System.String, "+CoreDotNetAssembly+@"]], "+CoreDotNetAssembly+@")
  one
  two
  three",
                new List<string> { "one", "two", "three" });
        }

        [TestMethod]
        public void TestDictionaryBaseTypes()
        {
            ActAndAssert(
                nameof(TestDictionaryBaseTypes),
                @"
Dictionary<string, int>[3]: (System.Collections.Generic.Dictionary`2[[System.String, "+CoreDotNetAssembly+@"],[System.Int32, "+CoreDotNetAssembly+@"]], "+CoreDotNetAssembly+@")
{
  [one] = 1
  [two] = 2
  [three] = 3
}",
                new Dictionary<string, int>
                {
                    ["one"] = 1,
                    ["two"] = 2,
                    ["three"] = 3,
                });
        }

        [TestMethod]
        public void TestDictionaryBaseTypeAndObject()
        {
            ActAndAssert(
                nameof(TestDictionaryBaseTypeAndObject),
                @"
Dictionary<int, Object4_1>[3]: (System.Collections.Generic.Dictionary`2[[System.Int32, "+CoreDotNetAssembly+@"],[vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object4_1, "+ObjectTextDumperTestAssembly+@"]], "+CoreDotNetAssembly+@")
{
  [1] = Object4_1 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object4_1, "+ObjectTextDumperTestAssembly+@"): 
    Property1                = one
    Property2                = Property2
  [2] = Object4_1 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object4_1, "+ObjectTextDumperTestAssembly+@"): 
    Property1                = two
    Property2                = Property2
  [3] = Object4_1 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object4_1, "+ObjectTextDumperTestAssembly+@"): 
    Property1                = three
    Property2                = Property2
}",
                new Dictionary<int, Object4_1>
                {
                    [1] = new Object4_1 { Property1 = "one" },
                    [2] = new Object4_1 { Property1 = "two" },
                    [3] = new Object4_1 { Property1 = "three" },
                });
        }

        [TestMethod]
        public void TestVirtualProperties()
        {
            ActAndAssert(
                nameof(TestVirtualProperties),
                @"
Derived (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Derived, "+ObjectTextDumperTestAssembly+@"): 
  IntProperty              = 0
  StringProperty           = StringProperty
  IntProperty              = 5",
                new Derived
                {
                    StringProperty = "StringProperty",
                    IntProperty    = 5,
                });
        }

        [TestMethod]
        public void TestDumpOfDynamic()
        {
            dynamic test = new { IntProperty = 10, StringProperty = "hello", DoubleProperty = Math.PI, };

            ActAndAssert(
                nameof(TestDumpOfDynamic),
                @"
<>f__AnonymousType0<int, string, double> (<>f__AnonymousType0`3[[System.Int32, "+CoreDotNetAssembly+@"],[System.String, "+CoreDotNetAssembly+@"],[System.Double, "+CoreDotNetAssembly+@"]], "+ObjectTextDumperTestAssembly+@"): 
  DoubleProperty           = 3.14159265358979
  IntProperty              = 10
  StringProperty           = hello",
                test);
        }

        [TestMethod]
        public void TestDumpOfExpando()
        {
            dynamic test = new ExpandoObject();

            test.IntProperty = 10;
            test.StringProperty = "hello";
            test.DoubleProperty = Math.PI;

            ActAndAssert(
                nameof(TestDumpOfExpando),
                @"
ExpandoObject[]: (System.Dynamic.ExpandoObject, "+LinqAssembly+@")
  KeyValuePair<string, object> (System.Collections.Generic.KeyValuePair`2[[System.String, "+CoreDotNetAssembly+@"],[System.Object, "+CoreDotNetAssembly+@"]], "+CoreDotNetAssembly+@"): 
    Key                      = IntProperty
    Value                    = 10
  KeyValuePair<string, object> (System.Collections.Generic.KeyValuePair`2[[System.String, "+CoreDotNetAssembly+@"],[System.Object, "+CoreDotNetAssembly+@"]], "+CoreDotNetAssembly+@"): 
    Key                      = StringProperty
    Value                    = hello
  KeyValuePair<string, object> (System.Collections.Generic.KeyValuePair`2[[System.String, "+CoreDotNetAssembly+@"],[System.Object, "+CoreDotNetAssembly+@"]], "+CoreDotNetAssembly+@"): 
    Key                      = DoubleProperty
    Value                    = 3.14159265358979",
                test);
        }

#if NETFRAMEWORK
        T GetSandboxedObject<T>(SecurityZone zone)
        {
            // get the permission set:
            Evidence evidence = new Evidence();

            evidence.AddHostEvidence(new Zone(zone));

            var permissionSet = SecurityManager.GetStandardSandbox(evidence);

            // create the app domain:
            AppDomainSetup setup = new AppDomainSetup
            {
                ApplicationBase = Path.GetDirectoryName(typeof(T).Assembly.Location),
            };

            AppDomain domain = AppDomain.CreateDomain(
                                    "TestSandbox",
                                    null,
                                    setup,
                                    permissionSet);

            //create a remote instance of the T class
            return (T)Activator.CreateInstanceFrom(
                                    domain,
                                    typeof(T).Assembly.ManifestModule.FullyQualifiedName,
                                    typeof(T).FullName)
                               .Unwrap();
        }

        [TestMethod]
        public void TestCallFromSandbox()
        {
            Debug.WriteLine(nameof(TestCallFromSandbox));
            Object1Dump od = GetSandboxedObject<Object1Dump>(SecurityZone.Internet);

            var actual = od.DumpObject1();
            var expected = @"
vm.Aspects.Diagnostics.Tests.ObjectDumper.PartialTrust.Object1 (Note: The caller does not have the permission to use reflection. Therefore System.Object.ToString() on the object has been dumped instead.)";

            TestContext.WriteLine("{0}", actual);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCallFromSandbox1()
        {
            Debug.WriteLine(nameof(TestCallFromSandbox1));
            var actual = GetSandboxedObject<Object1Dump>(SecurityZone.MyComputer).DumpObject1();
            var expected = @"
Object1 (vm.Aspects.Diagnostics.Tests.ObjectDumper.PartialTrust.Object1, vm.Aspects.Diagnostics.Tests.ObjectDumper.PartialTrust, Version=1.0.0.0, Culture=neutral, PublicKeyToken=a64e726b1908ae7b): 
  BoolProperty             = True
  ByteProperty             = 1
  CharProperty             = A
  DateTimeOffsetProperty   = 2013-01-13T00:00:00.0000000-05:00
  DateTimeProperty         = 2013-01-13T00:00:00.0000000
  DecimalProperty          = 1
  DoubleProperty           = 1
  FloatProperty            = 1
  GuidProperty             = 00000000-0000-0000-0000-000000000000
  IntProperty              = 1
  LongProperty             = 1
  NullIntProperty          = <null>
  NullLongProperty         = 1
  ObjectProperty           = <null>
  SByteProperty            = 1
  ShortProperty            = 1
  TimeSpanProperty         = 00:00:00.0000123
  UIntProperty             = 1
  ULongProperty            = 1
  UShortProperty           = 1
  UriProperty              = http://localhost/";

            TestContext.WriteLine("{0}", actual);
            Assert.AreEqual(expected, actual);
        }
#endif

        [TestMethod]
        public void TestDumpClassDumpMethod()
        {
            ActAndAssert(
                nameof(TestDumpClassDumpMethod),
                @"
Object12 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object12, "+ObjectTextDumperTestAssembly+@"): 
  Object11Property_1       = Dumped by Objec11Dumper.Dump1: string value
  Object11Property_11      = Dumped by Objec11Dumper.Dump1: string value
  Object11Property_2       = Dumped by Objec11Dumper.Dump: string value
  Object11Property_21      = Dumped by Objec11Dumper.Dump: string value
  Object11Property_3       = Dumped by Objec11.DumpMe: string value
  Object11Property_31      = Dumped by Objec11.DumpMe: string value
  Object11Property_4       = Dumped by Objec11.DumpMeStatic: string value
  Object11Property_41      = Dumped by Objec11.DumpMeStatic: string value
  Object11Property_51      = *** Could not find a public, static, method DumpMeNoneSuch, with return type of System.String, with a single parameter of type vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object11_1 in the class vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object11_1.",
                new Object12());
        }

        [TestMethod]
        public void TestOptInDump()
        {
            DumpAttribute.Default.Skip = ShouldDump.Skip;

            ActAndAssert(
                nameof(TestOptInDump),
                @"
Object13 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object13, "+ObjectTextDumperTestAssembly+@"): 
  Prop2                    = <null>
  Prop3                    = <null>",
                new Object13());

            DumpAttribute.Default.Skip = ShouldDump.Dump;
        }

        [TestMethod]
        public void TestArrayIndentationCreep()
        {
            ActAndAssert(
                nameof(TestArrayIndentationCreep),
                @"
DavidATest (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+DavidATest, "+ObjectTextDumperTestAssembly+@"): 
  A                        = 10
  Array                    = int[3]: (System.Int32[], "+CoreDotNetAssembly+@")
    1
    2
    3
  B                        = 6",
                new DavidATest());
        }

        [TestMethod]
        public void TestObjectWithNullCollection()
        {
            ActAndAssert(
                nameof(TestObjectWithNullCollection),
                @"
Object14 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object14, "+ObjectTextDumperTestAssembly+@"): 
  Collection               = <null>
  Property11               = 0
  Property12               = <null>",
                new Object14());
            ActAndAssert(
                nameof(TestObjectWithNullCollection),
                @"
Object14 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object14, "+ObjectTextDumperTestAssembly+@"): 
  Collection               = <null>
  Property11               = 0
  Property12               = <null>",
                new Object14());
        }

        [TestMethod]
        public void TestObjectWithNotNullCollection()
        {
            ActAndAssert(
                nameof(TestObjectWithNotNullCollection),
                @"
Object14 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object14, "+ObjectTextDumperTestAssembly+@"): 
  Collection               = List<Object14_1>[2]: (System.Collections.Generic.List`1[[vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object14_1, "+ObjectTextDumperTestAssembly+@"]], "+CoreDotNetAssembly+@")
    Object14_1 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object14_1, "+ObjectTextDumperTestAssembly+@"): 
      Property1                = 0
      Property2                = zero
    Object14_1 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object14_1, "+ObjectTextDumperTestAssembly+@"): 
      Property1                = 1
      Property2                = one
  Property11               = 1
  Property12               = one.two",
                new Object14
                {
                    Property11 = 1,
                    Property12 = "one.two",
                    Collection = new List<Object14_1>
                    {
                        new Object14_1
                        {
                            Property1 = 0,
                            Property2 = "zero"
                        },
                        new Object14_1
                        {
                            Property1 = 1,
                            Property2 = "one"
                        },
                    }
                });
            ActAndAssert(
                nameof(TestObjectWithNotNullCollection),
                @"
Object14 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Object14, "+ObjectTextDumperTestAssembly+@"): 
  Collection               = <null>
  Property11               = 0
  Property12               = <null>",
                new Object14());
        }

        [TestMethod]
        public void TestVirtualPropertiesVariations()
        {
            ActAndAssert(
                nameof(TestVirtualPropertiesVariations),
                @"
BaseClass (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+BaseClass, "+ObjectTextDumperTestAssembly+@"): 
  Property                 = 0
  VirtualProperty1         = 1
  VirtualProperty2         = 2",
                new BaseClass());
            ActAndAssert(
                nameof(TestVirtualPropertiesVariations),
                @"
Descendant1 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Descendant1, "+ObjectTextDumperTestAssembly+@"): 
  Property                 = 0
  VirtualProperty1         = 1
  VirtualProperty2         = 2",
                new Descendant1());
            ActAndAssert(
                nameof(TestVirtualPropertiesVariations),
                @"
Descendant2 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Descendant2, "+ObjectTextDumperTestAssembly+@"): 
  Property                 = 0
  VirtualProperty1         = 21
  VirtualProperty2         = 2",
                new Descendant2());
            ActAndAssert(
                nameof(TestVirtualPropertiesVariations),
                @"
Descendant3 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Descendant3, "+ObjectTextDumperTestAssembly+@"): 
  Property                 = 0
  VirtualProperty1         = 21
  VirtualProperty2         = 32",
                new Descendant3());


            ActAndAssert(
                nameof(TestVirtualPropertiesVariations),
                @"
BaseClass (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+BaseClass, "+ObjectTextDumperTestAssembly+@"): 
  Property                 = 0
  VirtualProperty1         = 1
  VirtualProperty2         = 2",
                new BaseClass
                {
                    Property         = 0,
                    VirtualProperty1 = 1,
                    VirtualProperty2 = 2,
                });
            ActAndAssert(
                nameof(TestVirtualPropertiesVariations),
                @"
Descendant1 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Descendant1, "+ObjectTextDumperTestAssembly+@"): 
  Property                 = 10
  VirtualProperty1         = 11
  VirtualProperty2         = 12",
                new Descendant1
                {
                    Property         = 10,
                    VirtualProperty1 = 11,
                    VirtualProperty2 = 12,
                });
            ActAndAssert(
                nameof(TestVirtualPropertiesVariations),
                @"
Descendant2 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Descendant2, "+ObjectTextDumperTestAssembly+@"): 
  Property                 = 20
  VirtualProperty1         = 21
  VirtualProperty2         = 22",
                new Descendant2
                {
                    Property         = 20,
                    VirtualProperty1 = 21,
                    VirtualProperty2 = 22,
                });
            ActAndAssert(
                nameof(TestVirtualPropertiesVariations),
                @"
Descendant3 (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+Descendant3, "+ObjectTextDumperTestAssembly+@"): 
  Property                 = 30
  VirtualProperty1         = 31
  VirtualProperty2         = 32",
                new Descendant3
                {
                    Property         = 30,
                    VirtualProperty1 = 31,
                    VirtualProperty2 = 32,
                });
        }

        [TestMethod]
        public void TestWrappedByteArray()
        {
            ActAndAssert(
                nameof(TestWrappedByteArray),
                @"
WrappedByteArray (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+WrappedByteArray, "+ObjectTextDumperTestAssembly+@"): 
  Bytes                    = byte[8]: 00-00-00-00-00-00-00-00",
                new WrappedByteArray { Bytes = new byte[8] });
        }

        [TestMethod]
        public void TestGenericWithBuddy()
        {
            ActAndAssert(
                nameof(TestGenericWithBuddy),
                @"
GenericWithBuddy<int> (vm.Aspects.Diagnostics.Tests.ObjectDumper.ObjectTextDumperTest+GenericWithBuddy`1[[System.Int32, "+CoreDotNetAssembly+@"]], "+ObjectTextDumperTestAssembly+@"): 
  Property2                = ******",
                new GenericWithBuddy<int> { Property1 = 7, Property2 = 3 });
        }
    }
}