using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Security;
using System.Xml.Linq;
using vm.Aspects.Linq.Expressions.Serialization.Implementation;

namespace vm.Aspects.Linq.Expressions.Serialization
{
    /// <summary>
    /// The instances of this class serialize LINQ expression trees of type <see cref="Expression"/> to XML document or element and vice versa: 
    /// de-serialize XML documents or elements conforming to schema &quot;urn:schemas-vm-com:Aspects.Linq.Expression&quot; to <see cref="Expression"/> objects.
    /// </summary>
    [SuppressMessage("Microsoft.Security", "CA2136:TransparencyAnnotationsShouldNotConflictFxCopRule")]
    [SecuritySafeCritical]
    public class XmlExpressionSerializer
    {
        string _xmlVersion    = "1.0";
        string _xmlEncoding   = "utf-8";
        string _xmlStandalone = "yes";
        bool _addComment      = 
#if DEBUG
                                true;
#else
                                false;
#endif

        /// <summary>
        /// Gets or sets the XML document's version. Default - &quot;1.0&quot;
        /// </summary>
        public string XmlVersion
        {
            get { return _xmlVersion; }
            set { _xmlVersion = value; }
        }

        /// <summary>
        /// Gets or sets the XML document encoding. Default - &quot;UTF-8&quot;
        /// </summary>
        public string XmlEncoding
        {
            get { return _xmlEncoding; }
            set { _xmlEncoding = value; }
        }

        /// <summary>
        /// Gets or sets the XML standalone attribute. Default - &quot;yes&quot;
        /// </summary>
        public string XmlStandalone
        {
            get { return _xmlStandalone; }
            set { _xmlStandalone = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to add expression comment to the XML element. Default - <see langword="true"/> in debug mode, 
        /// <see langword="false"/> in release mode.
        /// </summary>
        /// <value><see langword="true"/> to add comment; otherwise, <see langword="false"/>.</value>
        public bool AddComment
        {
            get { return _addComment; }
            set { _addComment = value; }
        }

        /// <summary>
        /// Serializes the <paramref name="expression" /> to an XML document.
        /// </summary>
        /// <param name="expression">The expression to be serialized.</param>
        /// <returns>The XML document representing the <paramref name="expression" />.</returns>
        /// <exception cref="System.ArgumentNullException">If <paramref name="expression"/> is <see langword="null"/></exception>
        public XDocument ToXmlDocument(Expression expression)
        {
            Contract.Requires<ArgumentNullException>(expression != null, nameof(expression));
            Contract.Ensures(Contract.Result<XDocument>() != null);

            return new XDocument(
                            new XDeclaration(XmlVersion, XmlEncoding, XmlStandalone),
                            AddComment
                                ? new XComment(
                                        string.Format(
                                            CultureInfo.InvariantCulture,
                                            " {0} ",
                                            expression.ToString()))
                                : null,
                            ToXmlElement(expression));
        }

        /// <summary>
        /// Serializes the <paramref name="expression"/> to an XML element.
        /// </summary>
        /// <param name="expression">The expression to be serialized.</param>
        /// <returns>The element representing the <paramref name="expression"/>.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// If <paramref name="expression"/> is <see langword="null"/></exception>
        public static XElement ToXmlElement(Expression expression)
        {
            Contract.Requires<ArgumentNullException>(expression != null, nameof(expression));
            Contract.Ensures(Contract.Result<XElement>() != null);
            
            var visitor = new ExpressionSerializingVisitor();

            visitor.Visit(expression);

            return new XElement(
                            XNames.Elements.Expression,
                            new XAttribute(
                                    "xmlns", XNames.Xxp),
                            visitor.Result);
        }

        /// <summary>
        /// De-serializes the <paramref name="document"/> to an expression tree instance.
        /// </summary>
        /// <param name="document">
        /// The document to be deserialized. The document must conform to the schema &quot;urn:schemas-vm-com:Aspects.Linq.Expression&quot;
        /// </param>
        /// <returns>The created expression tree.</returns>
        /// <exception cref="System.ArgumentNullException">If the <paramref name="document"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.Xml.XmlException">If the <paramref name="document"/> is not a well-formed or valid document.</exception>
        public static Expression ToExpression(XDocument document)
        {
            Contract.Requires<ArgumentNullException>(document != null, nameof(document));

            return ToExpression(document.Root);
        }

        /// <summary>
        /// De-serializes the <paramref name="element"/> to an expression tree instance.
        /// </summary>
        /// <param name="element">
        /// The element to be deserialized. The element must conform to the schema &quot;urn:schemas-vm-com:Aspects.Linq.Expression&quot;
        /// </param>
        /// <returns>The created expression tree.</returns>
        /// <exception cref="System.ArgumentNullException">If the <paramref name="element"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.Xml.XmlException">If the <paramref name="element"/> is not a well-formed or valid document.</exception>
        public static Expression ToExpression(
            XElement element)
        {
            Contract.Requires<ArgumentNullException>(element != null, nameof(element));

            if (element.Name != XNames.Elements.Expression)
                throw new ArgumentException(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "Expected {0} element.",
                                XNames.Elements.Expression),
                            nameof(element));

            var visitor = new ExpressionDeserializingVisitor();

            return visitor.Visit(element.Elements().First());
        }
    }
}
