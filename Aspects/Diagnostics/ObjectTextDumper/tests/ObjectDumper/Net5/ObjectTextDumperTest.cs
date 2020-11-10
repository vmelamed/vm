using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using vm.Aspects.Diagnostics.Implementation;

namespace vm.Aspects.Diagnostics.ObjectTextDumperTests
{
    [TestClass]
    public partial class ObjectTextDumperTest
    {
        public static readonly Func<TextWriter, ObjectTextDumper> DefaultDumperFactory = w => new ObjectTextDumper(w);

        readonly Stopwatch _sw = new Stopwatch();

        public TestContext? TestContext { get; set; }

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

        static ObjectTextDumper GetDumper(
            int indentSize = 2)
            => GetDumper(new StringWriter(CultureInfo.InvariantCulture), indentSize);

        static ObjectTextDumper GetDumper(
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

        static string MethodName(int stackLevel = 1)
        {
            var stackTrace = new StackTrace();

            // get calling method name
            return stackTrace.GetFrame(stackLevel)?.GetMethod()?.Name ?? "(could not get the name from the stack frames)";
        }

        [TestMethod]
        public void TestIsBasicType()
        {
            Debug.WriteLine(MethodName());

            for (var i = 2; i<_basicValues.Length; i++)
                Assert.IsTrue(_basicValues[i]!.GetType().IsBasicType());
        }

        [TestMethod]
        public void TestDumpedBasicValue()
        {
            Debug.WriteLine(MethodName());

            using var w = new StringWriter(CultureInfo.InvariantCulture);

            Assert.IsFalse(w.DumpedBasicValue(this, null));
            foreach (var v in _basicValues)
                Assert.IsTrue(w.DumpedBasicValue(v, null));
        }

        void TestDumpedBasicValueText(
            string expected,
            object value,
            DumpAttribute? dumpAttribute = null)
        {
            using var w = new StringWriter(CultureInfo.InvariantCulture);

            Assert.IsTrue(w.DumpedBasicValue(value, dumpAttribute));

            var actual = w.GetStringBuilder().ToString();

            TestContext!.WriteLine("{0}", actual);
            Assert.AreEqual(expected, actual);
        }

        void TestDumpObjectBasicValueText(
            string expected,
            object value,
            Type? metadata = null,
            DumpAttribute? dumpAttribute = null,
            int indentValue = 0)
        {
            using var w = new StringWriter(CultureInfo.InvariantCulture);
            var target = GetDumper(w);

            Assert.AreEqual(target, target.Dump(value, metadata, dumpAttribute, indentValue));

            var actual = w.GetStringBuilder().ToString();

            TestContext!.WriteLine("{0}", actual);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestDumpedBasicValueText()
        {
            Debug.WriteLine(MethodName());
            for (var i = 0; i<_basicValues.Length; i++)
                TestDumpedBasicValueText(_basicValuesStrings[i], _basicValues[i]!);
        }

        [TestMethod]
        public void TestDumpedBasicValueTextIndent()
        {
            Debug.WriteLine(MethodName());
            for (var i = 0; i<_basicValues.Length; i++)
                TestDumpedBasicValueText(_basicValuesStrings[i], _basicValues[i]!, null);
        }

        [TestMethod]
        public void TestDumpMaskedBasicValueText()
        {
            Debug.WriteLine(MethodName());
            for (var i = 0; i<_basicValues.Length; i++)
                TestDumpedBasicValueText(_basicValues[i]==null ? "<null>" : "------", _basicValues[i]!, new DumpAttribute { Mask = true, MaskValue = "------" });
        }

        [TestMethod]
        public void TestDumpMaskedBasicValueText1()
        {
            Debug.WriteLine(MethodName());
            for (var i = 0; i<_basicValues.Length; i++)
                TestDumpedBasicValueText(_basicValues[i]==null ? "<null>" : "******", _basicValues[i]!, new DumpAttribute { Mask = true });
        }

        [TestMethod]
        public void TestDumpedBasicValueFormat()
        {
            Debug.WriteLine(MethodName());
            TestDumpedBasicValueText("{00000000-0000-0000-0000-000000000000}", Guid.Empty, new DumpAttribute { ValueFormat = "{0:B}" });
            TestDumpedBasicValueText("¤1.00", 1.0, new DumpAttribute { ValueFormat = "{0:C}" });
            TestDumpedBasicValueText("¤1.00", 1M, new DumpAttribute { ValueFormat = "{0:C}" });
        }

        [TestMethod]
        public void TestStringValueLength()
        {
            Debug.WriteLine(MethodName());
            TestDumpedBasicValueText("012345678901...", "01234567890123456789", new DumpAttribute { MaxLength = 12 });
        }

        [TestMethod]
        public void TestDumpObjectBasicValueText()
        {
            Debug.WriteLine(MethodName());
            for (var i = 0; i<_basicValues.Length; i++)
                TestDumpObjectBasicValueText(_basicValuesStrings[i], _basicValues[i]!);
        }

        [TestMethod]
        public void TestDumpObjectBasicValueTextIndent()
        {
            Debug.WriteLine(MethodName());
            for (var i = 0; i<_basicValues.Length; i++)
                TestDumpObjectBasicValueText(_basicValuesStrings[i], _basicValues[i]!, null, null, 2);
        }

        [TestMethod]
        public void TestDumpObjectMaskedBasicValueText()
        {
            Debug.WriteLine(MethodName());
            for (var i = 0; i<_basicValues.Length; i++)
                TestDumpObjectBasicValueText(_basicValues[i]==null ? "<null>" : "------", _basicValues[i]!, null, new DumpAttribute { Mask = true, MaskValue = "------" });
        }

        [TestMethod]
        public void TestDumpObjectMaskedBasicValueText1()
        {
            Debug.WriteLine(MethodName());
            for (var i = 0; i<_basicValues.Length; i++)
                TestDumpObjectBasicValueText(_basicValues[i]==null ? "<null>" : "******", _basicValues[i]!, null, new DumpAttribute { Mask = true });
        }

        [TestMethod]
        public void TestDumpObjectBasicValueFormat()
        {
            Debug.WriteLine(MethodName());
            TestDumpObjectBasicValueText("{00000000-0000-0000-0000-000000000000}", Guid.Empty, null, new DumpAttribute { ValueFormat = "{0:B}" });
            TestDumpObjectBasicValueText("¤1.00", 1.0, null, new DumpAttribute { ValueFormat = "{0:C}" });
            TestDumpObjectBasicValueText("¤1.00", 1M, null, new DumpAttribute { ValueFormat = "{0:C}" });
        }

        [TestMethod]
        public void TestObjectStringValueLength()
        {
            Debug.WriteLine(MethodName());
            TestDumpObjectBasicValueText("012345678901...", "01234567890123456789", null, new DumpAttribute { MaxLength = 12 });
        }

        string Act(
            StringWriter w,
            object obj,
            Type? metadata = null,
            DumpAttribute? classDumpAttribute = null,
            Func<TextWriter, ObjectTextDumper>? dumperFactory = null)
        {
            dumperFactory ??= DefaultDumperFactory;

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

        void AssertResultIsEqualToExpected(
            string result,
            string expected,
            string dumpId)
        {
            var dump = $"{dumpId} ({_sw.Elapsed}):{result}";

            TestContext!.WriteLine(dump);
            Debug.WriteLine(dump);

            Assert.AreEqual(expected, result, $"{dumpId} assertion failed.");
        }

        void AssertResultStartsWith(
            string result,
            string startsWith,
            string dumpId)
        {
            var dump = $"{dumpId} ({_sw.Elapsed}):{result}";

            TestContext!.WriteLine(dump);
            Debug.WriteLine(dump);

            var assertion = result.StartsWith(startsWith);

            if (!assertion)
                TestContext!.WriteLine($@"Expected:<{startsWith}>
Actual:<{result}>
");

            Assert.IsTrue(assertion, $"{dumpId} assertion failed.");
        }

        void ActAndAssert(
            string expected,
            object obj,
            Type? metadata = null,
            DumpAttribute? classDumpAttribute = null,
            Func<TextWriter, ObjectTextDumper>? dumperFactory = null)
        {
            dumperFactory ??= DefaultDumperFactory;

            Debug.WriteLine(MethodName(2));

            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var result1 = Act(w, obj, metadata, classDumpAttribute, dumperFactory);

                AssertResultIsEqualToExpected(result1, expected, _firstDump);

                // --------------------------

                if (ObjectTextDumper.DefaultDumpSettings.UseDumpScriptCache)
                {
                    var result2 = Act(w, obj, metadata, classDumpAttribute, dumperFactory);

                    AssertResultIsEqualToExpected(result2, expected, _secondDump);
                }
            }
            if (ObjectTextDumper.DefaultDumpSettings.UseDumpScriptCache)
            {
                using var w = new StringWriter(CultureInfo.InvariantCulture);
                var result3 = Act(w, obj, metadata, classDumpAttribute, dumperFactory);

                AssertResultIsEqualToExpected(result3, expected, _thirdDump);
            }
        }

        void ActAndAssertStartsWith(
            string startsWith,
            object obj,
            Type? metadata = null,
            DumpAttribute? classDumpAttribute = null,
            Func<TextWriter, ObjectTextDumper>? dumperFactory = null)
        {
            dumperFactory ??= DefaultDumperFactory;

            Debug.WriteLine(MethodName(2));

            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                var result1 = Act(w, obj, metadata, classDumpAttribute, dumperFactory);

                AssertResultStartsWith(result1, startsWith, _firstDump);

                // --------------------------

                if (ObjectTextDumper.DefaultDumpSettings.UseDumpScriptCache)
                {
                    var result2 = Act(w, obj, metadata, classDumpAttribute, dumperFactory);

                    AssertResultStartsWith(result2, startsWith, _secondDump);
                }
            }
            if (ObjectTextDumper.DefaultDumpSettings.UseDumpScriptCache)
                using (var w = new StringWriter(CultureInfo.InvariantCulture))
                {
                    var result3 = Act(w, obj, metadata, classDumpAttribute, dumperFactory);

                    AssertResultStartsWith(result3, startsWith, _thirdDump);
                }
        }

        [TestMethod]
        public void TestDumpObjectWithBasicProperties()
        {
            ActAndAssert(
                TestDumpObject1_1_expected,
                new Object1());
        }

        [TestMethod]
        public void TestDumpObject1WithFields_1()
        {
            ActAndAssert(
                TestDumpObject1WithFields_1_expected,
                new Object1(),
                null,
                null,
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
                TestDumpObject1_1_Limited_expected,
                new Object1(),
                null,
                null,
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
        public void TestDumpObject1_2_WithMetadata()
        {
            ActAndAssert(
                TestDumpObject1_2_expected,
                new Object1(),
                typeof(Object1Metadata));
        }

        [TestMethod]
        public void TestDumpObject1_3_WithClassDumpAttribute_SkipDumpNullValues()
        {
            ActAndAssert(
                TestDumpObject1_3_expected,
                new Object1(),
                typeof(Object1Metadata),
                new DumpAttribute { DumpNullValues = ShouldDump.Skip });
        }

        [TestMethod]
        public void TestDumpObject1WithFieldsMetadata_2()
        {
            var obj = new Object1();
            obj.NullObjectProperty = new object();
            ActAndAssert(
                TestDumpObject1WithFieldsMetadata_2_expected,
                obj,
                typeof(Object1FieldsMetadata));
        }

        [TestMethod]
        public void TestDumpObject2_1()
        {
            ActAndAssert(
                TestDumpObject2_1_expected,
                new Object2());
        }

        [TestMethod]
        public void TestDumpObject3_1()
        {
            ActAndAssert(
                TestDumpObject3_1_expected,
                new Object3());
        }

        [TestMethod]
        public void TestDumpObject5_1()
        {
            ActAndAssert(
                TestDumpObject5_1_expected,
                new Object5_1());
        }

        static bool TestMethod(int a) => a < 0;

        [TestMethod]
        public void TestDumpDBNull()
        {
            ActAndAssert(
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
                TestDumpExpression_expected,
                expression);

            settings.PropertyBindingFlags &= ~BindingFlags.NonPublic;
            ObjectTextDumper.DefaultDumpSettings = DumpSettings.Default;
        }

        [TestMethod]
        public void TestDumpObject6_1()
        {
            ActAndAssert(
                TestDumpObject6_1_expected,
                new Object6());
        }

        [TestMethod]
        public void TestDumpObject7_1()
        {
            ActAndAssert(
                TestDumpObject7_1_expected,
                new Object7());
        }

        [TestMethod]
        public void TestDumpObject7_1_1()
        {
            ActAndAssert(
                TestDumpObject7_1_1_expected,
                new Object7_1());
        }

        [TestMethod]
        public void TestDumpObject7_1_2()
        {
            ActAndAssert(
                TestDumpObject7_1_2_expected,
                new Object7_1
                {
                    List = new List<int> { 0, 1, 2, 3, 4, 5, 0, 1, 2, 3, 4, 5, 0, 1, 2, 3, 4, 5, },
                });
        }

        [TestMethod]
        public void TestDumpObject7_1_3()
        {
            ActAndAssert(
                TestDumpObject7_1_3_expected,
                new Object7_1
                {
                    List = new List<int> { 0, 1, 2, 3, 4, 5, 0, 1, 2, 3, 4, 5, 0, 1, 2, 3, 4, 5, },
                },
                typeof(MetaObject7_1));
        }

        [TestMethod]
        public void TestDumpObject7_2()
        {
            ActAndAssert(
                TestDumpObject7_2_expected,
                new Object7
                {
                    Array = new[] { 0, 1, 2, 3, 4, 5, 0, 1, 2, 3, 4, 5, 0, 1, 2, 3, 4, 5, },
                });
        }

        [TestMethod]
        public void TestDumpObject8_1()
        {
            ActAndAssert(
                TestDumpObject8_1_expected,
                new Object8());
        }

        [TestMethod]
        public void TestDumpObject8_1_1()
        {
            ActAndAssert(
                TestDumpObject8_1_1_expected,
                new Object8_1());
        }

        [TestMethod]
        public void TestDumpObject9_1()
        {
            ActAndAssert(
                TestDumpObject9_1_expected,
                new Object91());
        }

        [TestMethod]
        public void TestDumpObject9_1null()
        {
            ActAndAssert(
                TestDumpObject9_1_expected,
                new Object91());

            ActAndAssert(
                TestDumpObject9_1null_expected,
                new Object91() { Object90 = null });
        }

        [TestMethod]
        public void TestDumpObjectWithDelegates()
        {
            ActAndAssert(
                TestDumpObjectWithDelegates_expected,
                new ObjectWithDelegates());
        }

        [TestMethod]
        public void TestDumpObjectWithMyEnumerable()
        {
            ActAndAssert(
                TestDumpObjectWithMyEnumerable_expected,
                new ObjectWithMyEnumerable());
        }

        [TestMethod]
        public void TestDumpObjectWithMemberInfos()
        {
            ActAndAssert(
                TestDumpObjectWithMemberInfos_expected,
                new ObjectWithMemberInfos());
        }

        [Dump(MaxDepth = 3)]
        class NestedItem
        {
            public int Property { get; set; }
            public NestedItem? Next { get; set; }
        }

        [TestMethod]
        public void TestDumpNestedObject()
        {
            ActAndAssert(
                TestDumpNestedObject_expected,
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
                TestCollectionObject_expected,
                new List<string> { "one", "two", "three" });
        }

        [TestMethod]
        public void TestDictionaryBaseTypes()
        {
            ActAndAssert(
                TestDictionaryBaseTypes_expected,
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
                TestDictionaryBaseTypeAndObject_expected,
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
                TestVirtualProperties_expected,
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
                TestDumpOfDynamic_expected,
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
                TestDumpOfExpando_expected,
                test);
        }

        [TestMethod]
        public void TestDumpClassDumpMethod()
        {
            ActAndAssert(
                TestDumpClassDumpMethod_expected,
                new Object12());
        }

        [TestMethod]
        public void TestOptInDump()
        {
            DumpAttribute.Default.Skip = ShouldDump.Skip;

            ActAndAssert(
                TestOptInDump_expected,
                new Object13());

            DumpAttribute.Default.Skip = ShouldDump.Dump;
        }

        [TestMethod]
        public void TestArrayIndentationCreep()
        {
            ActAndAssert(
                TestArrayIndentationCreep_expected,
                new DavidATest());
        }

        [TestMethod]
        public void TestObjectWithNullCollection()
        {
            ActAndAssert(
                TestObjectWithNullCollection_expected,
                new Object14());
            ActAndAssert(
                TestObjectWithNullCollection_expected2,
                new Object14());
        }

        [TestMethod]
        public void TestObjectWithNotNullCollection()
        {
            ActAndAssert(
                TestObjectWithNotNullCollection_expected,
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
                TestObjectWithNotNullCollection_expected2,
                new Object14());
        }

        [TestMethod]
        public void TestVirtualPropertiesVariations()
        {
            ActAndAssert(
                TestVirtualPropertiesVariations_expected,
                new BaseClass());
            ActAndAssert(
                TestVirtualPropertiesVariations_expected2,
                new Descendant1());
            ActAndAssert(
                TestVirtualPropertiesVariations_expected3,
                new Descendant2());
            ActAndAssert(
                TestVirtualPropertiesVariations_expected4,
                new Descendant3());

            ActAndAssert(
                TestVirtualPropertiesVariations_expected5,
                new BaseClass
                {
                    Property         = 0,
                    VirtualProperty1 = 1,
                    VirtualProperty2 = 2,
                });
            ActAndAssert(
                TestVirtualPropertiesVariations_expected6,
                new Descendant1
                {
                    Property         = 10,
                    VirtualProperty1 = 11,
                    VirtualProperty2 = 12,
                });
            ActAndAssert(
                TestVirtualPropertiesVariations_expected7,
                new Descendant2
                {
                    Property         = 20,
                    VirtualProperty1 = 21,
                    VirtualProperty2 = 22,
                });
            ActAndAssert(
                TestVirtualPropertiesVariations_expected8,
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
                TestWrappedByteArray_expected,
                new WrappedByteArray { Bytes = new byte[8] });
        }

        [TestMethod]
        public void TestGenericWithBuddy()
        {
            ActAndAssert(
                TestGenericWithBuddy_expected,
                new GenericWithBuddy<int> { Property1 = 7, Property2 = 3 });
        }
    }
}