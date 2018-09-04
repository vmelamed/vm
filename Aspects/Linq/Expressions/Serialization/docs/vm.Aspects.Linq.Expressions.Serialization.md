# Linq Expressions Serialization

The package vm.Aspects.Linq.Expressions.Serialization contains code for serialization deserialization of LINQ expression trees to and from XML documents. The documents conform to the XML schema [`urn:schemas-vm-com:Aspects.Linq.Expressions.Serialization`](https://github.com/vmelamed/vm/blob/master/Aspects/Linq/Expressions/Serialization/Documents/Expression.xsd).

The package exposes just one class - `vm.Aspects.Linq.Expressions.Serialization.XmlExpressionSerializer` with four methods:

1. `public XDocument ToXmlDocument(Expression expression)`
2. `public static XElement ToXmlElement(Expression expression)`
3. `public static Expression ToExpression(XDocument document)`
4. `public static Expression ToExpression(XElement element)`