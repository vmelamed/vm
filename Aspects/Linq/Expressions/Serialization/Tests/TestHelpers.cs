using System;
using System.IO;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using vm.Aspects.Diagnostics;

namespace vm.Aspects.Linq.Expressions.Serialization.Tests
{
    [TestClass]
    [DeploymentItem("..\\..\\..\\docs\\Expression.xsd")]
    [DeploymentItem("..\\..\\..\\docs\\Microsoft.Serialization.xsd")]
    [DeploymentItem("..\\..\\..\\docs\\DataContract.xsd")]
    [DeploymentItem("..\\..\\TestFiles", "TestFiles")]
    public static class TestHelpers
    {
        static XmlSchemaSet _schemas;

        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            _schemas = new XmlSchemaSet();
            _schemas.Add(
                "urn:schemas-vm-com:Aspects.Linq.Expressions.Serialization",
                XmlReader.Create(
                    new FileStream(@"Expression.xsd", FileMode.Open, FileAccess.Read, FileShare.Read),
                    new XmlReaderSettings { DtdProcessing = DtdProcessing.Parse }));

            ClassMetadataRegistrar.RegisterMetadata();
        }

        public static void TestSerializeExpression(
            TestContext testContext,
            Expression expression,
            string fileName,
            bool validate = true)
        {
            try
            {
                var expectedDoc = !string.IsNullOrWhiteSpace(fileName) ? XDocument.Load(fileName) : null;
                var expected = expectedDoc!=null ? expectedDoc.ToString(SaveOptions.OmitDuplicateNamespaces) : "";

                testContext.WriteLine("EXPECTED:\n{0}\n", expected);

                var actualDoc = new XmlExpressionSerializer().ToXmlDocument(expression);
                var actual = actualDoc.ToString(SaveOptions.OmitDuplicateNamespaces);

                testContext.WriteLine("ACTUAL:\n{0}\n", actual);

                if (validate)
                {
                    Assert.IsTrue(XmlValidator.Validate(expectedDoc, testContext));
                    Assert.IsTrue(XmlValidator.Validate(actualDoc, testContext));
                }
                Assert.IsTrue(
                    XDocument.DeepEquals(actualDoc, expectedDoc) ||
                    actual.Equals(expected));
            }
            catch (AssertFailedException)
            {
                throw;
            }
            catch (Exception x)
            {
                testContext.WriteLine("{0}", x.DumpString());
                throw;
            }
        }

        public static void TestDeserializeExpression(
            TestContext testContext,
            string fileName,
            Expression expected)
        {
            try
            {
                var expectedString = expected.DumpString();

                testContext.WriteLine(
                    "EXPECTED:\n{0}{1}\n",
                    expected.ToString(),
                    expectedString);

                var actual = XmlExpressionSerializer.ToExpression(XDocument.Load(fileName));
                var actualString = actual.DumpString();

                testContext.WriteLine(
                    "ACTUAL:\n{0}{1}\n",
                    actual.ToString(),
                    actualString);

                Assert.AreEqual(ReplaceEquivalents(expected.DumpString()), ReplaceEquivalents(actual.DumpString()));
            }
            catch (AssertFailedException)
            {
                throw;
            }
            catch (Exception x)
            {
                testContext.WriteLine("{0}", x.DumpString());
                throw;
            }
        }

        static string ReplaceEquivalents(string input)
        {
            // these strings are equal provided everything else is equal as well
            input = Regex.Replace(input, "MethodCallExpression1", "MethodCallExpressionN", RegexOptions.Multiline);
            input = Regex.Replace(input, @"System\.Collections\.ObjectModel\.ReadOnlyCollection", @"System.Runtime.CompilerServices.TrueReadOnlyCollection", RegexOptions.Multiline);
            input = Regex.Replace(input, @"System.Core", @"mscorlib", RegexOptions.Multiline);
            input = Regex.Replace(input, @"System.Collections.Generic.List", @"System.Collections.Generic.IEnumerable", RegexOptions.Multiline);
            input = Regex.Replace(input, @"System.Collections.Generic.Dictionary", @"System.Collections.Generic.IDictionary", RegexOptions.Multiline);

            return input;
        }
    }
}
