
namespace vm.Aspects.Security.Cryptography.Ciphers.Xml.Tests
{
    static class TestData
    {
        public const string XmlOrder = @"<?xml version='1.0' encoding='utf-8'?>
<order>
  <items>
    <item quantity='1'>.NET Framework Security</item>
    <item quantity='1'>Essential XML Quick Reference</item>
  </items>
  <shipping>
    <to>Joe Smith</to>
    <street>110 Denny Way</street>
    <city>Seattle</city>
    <zip>98109</zip>
  </shipping>
  <billing>
    <paymentInfo type='Visa'>
      <number>0000-0000-0000-0000</number>
      <expirationDate>09/15/80</expirationDate>
      <billingAddress>
        <who>Microsoft Corporation</who>
        <street>1 Microsoft Way</street>
        <city>Redmond</city>
        <zip>98052</zip>
      </billingAddress>
    </paymentInfo>
  </billing>
</order>";

        public const string XmlOrder2 = @"<?xml version='1.0' encoding='utf-8'?>
<order>
  <items>
    <item quantity='1'>.NET Framework Security</item>
    <item quantity='1'>Essential XML Quick Reference</item>
  </items>
  <shipping>
    <to>Joe Smith</to>
    <street>110 Denny Way</street>
    <city>Seattle</city>
    <zip>98109</zip>
  </shipping>
  <billing>
    <paymentInfo type='Visa'>
      <number>0000-0000-0000-0000</number>
      <expirationDate>09/15/80</expirationDate>
      <billingAddress>
        <who>Microsoft Corporation</who>
        <street>1 Microsoft Way</street>
        <city>Redmond</city>
        <zip>98052</zip>
      </billingAddress>
    </paymentInfo>
  </billing>
  <billing>
    <paymentInfo type='Visa'>
      <number>1111-1111-1111-1111</number>
      <expirationDate>09/15/80</expirationDate>
      <billingAddress>
        <who>Microsoft Corporation</who>
        <street>1 Microsoft Way</street>
        <city>Redmond</city>
        <zip>98052</zip>
      </billingAddress>
    </paymentInfo>
  </billing>
</order>";

        public const string XmlOrderWithIds = @"<?xml version='1.0' encoding='utf-8'?>
<order>
  <items>
    <item quantity='1'>.NET Framework Security</item>
    <item quantity='1'>Essential XML Quick Reference</item>
  </items>
  <shipping>
    <to>Joe Smith</to>
    <street>110 Denny Way</street>
    <city>Seattle</city>
    <zip>98109</zip>
  </shipping>
  <billing xml:Id='id1'>
    <paymentInfo type='Visa'>
      <number>0000-0000-0000-0000</number>
      <expirationDate>09/15/80</expirationDate>
      <billingAddress>
        <who>Microsoft Corporation</who>
        <street>1 Microsoft Way</street>
        <city>Redmond</city>
        <zip>98052</zip>
      </billingAddress>
    </paymentInfo>
  </billing>
  <billing xml:Id='signed-1'>
    <paymentInfo type='Visa'>
      <number>1111-1111-1111-1111</number>
      <expirationDate>09/15/80</expirationDate>
      <billingAddress>
        <who>Microsoft Corporation</who>
        <street>1 Microsoft Way</street>
        <city>Redmond</city>
        <zip>98052</zip>
      </billingAddress>
    </paymentInfo>
  </billing>
  <billing>
    <paymentInfo type='Visa'>
      <number>2222-2222-2222-2222</number>
      <expirationDate>09/15/80</expirationDate>
      <billingAddress>
        <who>Microsoft Corporation</who>
        <street>1 Microsoft Way</street>
        <city>Redmond</city>
        <zip>98052</zip>
      </billingAddress>
    </paymentInfo>
  </billing>
</order>";

        public const string XmlNsOrder = @"<?xml version='1.0' encoding='utf-8'?>
<order>
  <items>
    <item quantity='1'>.NET Framework Security</item>
    <item quantity='1'>Essential XML Quick Reference</item>
  </items>
  <shipping>
    <to>Joe Smith</to>
    <street>110 Denny Way</street>
    <city>Seattle</city>
    <zip>98109</zip>
  </shipping>
  <billing>
    <pi:paymentInfo xmlns:pi='urn:test:paymentInfo' type='Visa'>
      <pi:number>0000-0000-0000-0000</pi:number>
      <pi:expirationDate>09/15/80</pi:expirationDate>
      <pi:billingAddress>
        <pi:who>Microsoft Corporation</pi:who>
        <pi:street>1 Microsoft Way</pi:street>
        <pi:city>Redmond</pi:city>
        <pi:zip>98052</pi:zip>
      </pi:billingAddress>
    </pi:paymentInfo>
  </billing>
</order>";

        public const string XmlNsOrder2 = @"<?xml version='1.0' encoding='utf-8'?>
<order>
  <items>
    <item quantity='1'>.NET Framework Security</item>
    <item quantity='1'>Essential XML Quick Reference</item>
  </items>
  <shipping>
    <to>Joe Smith</to>
    <street>110 Denny Way</street>
    <city>Seattle</city>
    <zip>98109</zip>
  </shipping>
  <billing>
    <pi:paymentInfo xmlns:pi='urn:test:paymentInfo' type='Visa'>
      <pi:number>0000-0000-0000-0000</pi:number>
      <pi:expirationDate>09/15/80</pi:expirationDate>
      <pi:billingAddress>
        <pi:who>Microsoft Corporation</pi:who>
        <pi:street>1 Microsoft Way</pi:street>
        <pi:city>Redmond</pi:city>
        <pi:zip>98052</pi:zip>
      </pi:billingAddress>
    </pi:paymentInfo>
  </billing>
  <billing>
    <pi:paymentInfo xmlns:pi='urn:test:paymentInfo' type='Visa'>
      <pi:number>1111-1111-1111-1111</pi:number>
      <pi:expirationDate>09/15/80</pi:expirationDate>
      <pi:billingAddress>
        <pi:who>Microsoft Corporation</pi:who>
        <pi:street>1 Microsoft Way</pi:street>
        <pi:city>Redmond</pi:city>
        <pi:zip>98052</pi:zip>
      </pi:billingAddress>
    </pi:paymentInfo>
  </billing>
</order>";

        public const string XmlNsOrderWithIds = @"<?xml version='1.0' encoding='utf-8'?>
<order>
  <items>
    <item quantity='1'>.NET Framework Security</item>
    <item quantity='1'>Essential XML Quick Reference</item>
  </items>
  <shipping>
    <to>Joe Smith</to>
    <street>110 Denny Way</street>
    <city>Seattle</city>
    <zip>98109</zip>
  </shipping>
  <billing>
    <pi:paymentInfo xml:Id='id1' xmlns:pi='urn:test:paymentInfo' type='Visa'>
      <pi:number>0000-0000-0000-0000</pi:number>
      <pi:expirationDate>09/15/80</pi:expirationDate>
      <pi:billingAddress>
        <pi:who>Microsoft Corporation</pi:who>
        <pi:street>1 Microsoft Way</pi:street>
        <pi:city>Redmond</pi:city>
        <pi:zip>98052</pi:zip>
      </pi:billingAddress>
    </pi:paymentInfo>
  </billing>
  <billing>
    <pi:paymentInfo xml:Id='signed-2' xmlns:pi='urn:test:paymentInfo' type='Visa'>
      <pi:number>1111-1111-1111-1111</pi:number>
      <pi:expirationDate>09/15/80</pi:expirationDate>
      <pi:billingAddress>
        <pi:who>Microsoft Corporation</pi:who>
        <pi:street>1 Microsoft Way</pi:street>
        <pi:city>Redmond</pi:city>
        <pi:zip>98052</pi:zip>
      </pi:billingAddress>
    </pi:paymentInfo>
  </billing>
  <billing>
    <pi:paymentInfo xmlns:pi='urn:test:paymentInfo' type='Visa'>
      <pi:number>2222-2222-2222-2222</pi:number>
      <pi:expirationDate>09/15/80</pi:expirationDate>
      <pi:billingAddress>
        <pi:who>Microsoft Corporation</pi:who>
        <pi:street>1 Microsoft Way</pi:street>
        <pi:city>Redmond</pi:city>
        <pi:zip>98052</pi:zip>
      </pi:billingAddress>
    </pi:paymentInfo>
  </billing>
</order>";

        public const string XmlNsOrderWithCustomIds = @"<?xml version='1.0' encoding='utf-8'?>
<order>
  <items>
    <item quantity='1'>.NET Framework Security</item>
    <item quantity='1'>Essential XML Quick Reference</item>
  </items>
  <shipping>
    <to>Joe Smith</to>
    <street>110 Denny Way</street>
    <city>Seattle</city>
    <zip>98109</zip>
  </shipping>
  <billing>
    <pi:paymentInfo customId='id1' xmlns:pi='urn:test:paymentInfo' type='Visa'>
      <pi:number>0000-0000-0000-0000</pi:number>
      <pi:expirationDate>09/15/80</pi:expirationDate>
      <pi:billingAddress>
        <pi:who>Microsoft Corporation</pi:who>
        <pi:street>1 Microsoft Way</pi:street>
        <pi:city>Redmond</pi:city>
        <pi:zip>98052</pi:zip>
      </pi:billingAddress>
    </pi:paymentInfo>
  </billing>
  <billing>
    <pi:paymentInfo xml:Id='signed-2' xmlns:pi='urn:test:paymentInfo' type='Visa'>
      <pi:number>1111-1111-1111-1111</pi:number>
      <pi:expirationDate>09/15/80</pi:expirationDate>
      <pi:billingAddress>
        <pi:who>Microsoft Corporation</pi:who>
        <pi:street>1 Microsoft Way</pi:street>
        <pi:city>Redmond</pi:city>
        <pi:zip>98052</pi:zip>
      </pi:billingAddress>
    </pi:paymentInfo>
  </billing>
  <billing>
    <pi:paymentInfo xmlns:pi='urn:test:paymentInfo' type='Visa'>
      <pi:number>2222-2222-2222-2222</pi:number>
      <pi:expirationDate>09/15/80</pi:expirationDate>
      <pi:billingAddress>
        <pi:who>Microsoft Corporation</pi:who>
        <pi:street>1 Microsoft Way</pi:street>
        <pi:city>Redmond</pi:city>
        <pi:zip>98052</pi:zip>
      </pi:billingAddress>
    </pi:paymentInfo>
  </billing>
</order>";

    }
}
