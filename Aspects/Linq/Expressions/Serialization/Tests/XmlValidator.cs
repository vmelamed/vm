using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace vm.Aspects.Linq.Expressions.Serialization.Tests
{
    static class XmlValidator
    {
        static XmlSchemaSet _schemas;

        static XmlValidator()
        {
            _schemas = new XmlSchemaSet();
            _schemas.Add(
                "http://schemas.microsoft.com/2003/10/Serialization/",
                XmlReader.Create(
                    new FileStream("Microsoft.Serialization.xsd", FileMode.Open, FileAccess.Read, FileShare.Read),
                    new XmlReaderSettings { DtdProcessing = DtdProcessing.Parse }));
            _schemas.Add(
                "http://schemas.datacontract.org/2004/07/System",
                XmlReader.Create(
                    new FileStream("DataContract.xsd", FileMode.Open, FileAccess.Read, FileShare.Read),
                    new XmlReaderSettings { DtdProcessing = DtdProcessing.Parse }));
            _schemas.Add(
                "urn:schemas-vm-com:Aspects.Linq.Expression",
                XmlReader.Create(
                    new FileStream("Expression.xsd", FileMode.Open, FileAccess.Read, FileShare.Read),
                    new XmlReaderSettings { DtdProcessing = DtdProcessing.Parse }));
        }

        public static bool Validate(XDocument doc, TestContext testContext)
        {
            var valid = true;

            doc.Validate(
                    _schemas,
                    (o, e) =>
                    {
                        testContext.WriteLine("{0}", e.DumpString());
                        valid = false;
                    });

            return valid;
        }
    }
}
