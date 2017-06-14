# AspectObjectDumper
The AspectObjectDumper is a project that implements an easy to integrate and easy to use component that dumps the value of an arbitrary .NET object in an easy to read text form. Helpful for debugging and logging purposes.
The source code of the project can be found at [GitHub](https://github.com/vmelamed/vm/tree/master/Aspects/Diagnostics). Also you can install the package in your solution from [NuGet](https://www.nuget.org/packages/AspectObjectDumper).
## Usage
The dumper is implemented by the class `ObjectTextDumper` in the namespace `vm.Aspects.Diagnostics`.
Here is the first usage example that we are going to improve on further down:
```csharp
using System;
using System.IO;
using vm.Aspects.Diagnostics;

namespace ObjectDumperSamples
{
    class MyClass
    {
        public bool BoolProperty { get; set; }
        public int IntProperty { get; set; }
        public Guid GuidProperty { get; set; }
        public Uri UriProperty { get; set; }
    }

    class Program
    {
        static void Main()
        {
            int anInt = 5;
            var anObject = new MyClass
            {
                BoolProperty = true,
                IntProperty  = 3,
                GuidProperty = Guid.NewGuid(),
            };

            using (var writer = new StringWriter())
            {
                var dumper = new ObjectTextDumper(writer);

                // dump a primitive value:
                dumper.Dump(anInt);
                // dump complex value:
                dumper.Dump(anObject);
                Console.WriteLine(writer.GetStringBuilder().ToString());
            }

            Console.ReadKey(true);
        }
    }
}
```
This is the output from this program:
```
5
MyClass (ObjectDumperSamples.MyClass, ObjectDumperSamples, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null):
  BoolProperty             = True
  GuidProperty             = 6e27359b-c1b1-48c5-bf69-967b7fda886c
  IntProperty              = 3
  UriProperty              = <null>
```

I prefer the following two simple facades implemented as extension methods of `System.Object`. Just replace the `using` statement above with this code snippet:
```csharp
            using (var writer = new StringWriter())
            {
                anInt.DumpText(writer);
                anObject.DumpText(writer);
                Console.WriteLine(writer.GetStringBuilder().ToString());
            }
```
Or replace the entire `using` statement with:
```csharp
            Console.WriteLine(anInt.DumpString());
            Console.WriteLine(anObject.DumpString());
```
If you don't have better ideas for overriding `MyClass.ToString()`, why not implement it like this:
```csharp
            public override string ToString()
            {
                return this.DumpString();
            }
```
And now we can replace the last line in the snippet with:
```csharp
            Console.WriteLine(anObject);
```
The programmer has control over the dump through attributes associated with the classes, properties and fields of the dumped objects. The attributes can be applied:
* Directly on the class and its properties and fields.
* Indirectly in a so called buddy class - class referred to in a `MetadataTypeAttribute` applied to your class.
* By using the parameters of the `Dump` method associate a type of objects with after-the-fact written buddy-class.
* By using the method of the static class `ClassMetadataResolver.SetClassDumpData`. The signature of the method is self-explanatory:
```csharp
        public static void SetClassDumpData(
            Type type,
            Type metadata = null,
            DumpAttribute dumpAttribute = null);
```
If the second and third parameters are `null`-s their respective values are extracted from the attributes applied to the target type and if not found there - default values are assumed. The `ClassMetadataResolver` contains a static cache associating types with dumping meta-data. This facility is most useful for BCL or third party classes.
Let's go back to the constructor of the `ObjectTextDumper` class:
```csharp
        public ObjectTextDumper(
            TextWriter writer,
            int indentLevel = 0,
            int indentLength = 2,
            int maxDumpLength = 0,
            BindingFlags propertiesBindingFlags = BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly,
            BindingFlags fieldsBindingFlags = BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly);
```
It takes:
  1. an object of type descending from `TextWriter`
  1. the initial value for the indent level
  1. the size of the indent - the number of spaces in a single indent
  1. the limit on the dumped text. This parameter would be useful when dumping objects with enormous graphs and you may run out of memory. The default value is 4 million characters (~ 8MB)
  1. The last two parameters control which properties and fields should be dumped with respect to their visibility and lifetime modifiers. By default all public and non-public instance properties and all public instance fields are being dumped. From that point on, properties and fields are treated equally and for the sake of brevity in this document I will use only the word "property" unless explicitly stated otherwise, please assume that the same applies to fields.
Let's look at the Dump method too:
```csharp
        public object Dump(
            object value,
            Type dumpMetadata = null,
            DumpAttribute dumpAttribute = null);
```
The method returns the dumper object itself, just in case you want to chain the `Dump` calls. As you can see here, it takes actually three parameters, where the last two default to null-s.
The first parameter is the object that we want to dump.
The second parameter is supposed to be a `Type` object representing the dumping meta-data for the type of the dumped object.
The third parameter applies a `DumpAttribute` to the type of the dumped object.
The last two parameters override the dumped object's buddy class and `DumpAttribute`.
The heart of the customization features of the dumper is the attribute class `DumpAttribute`. For a more detailed description take a look at the documentation comments in the source code or the class documentation generated off of them.
The `DumAttribute` attribute can be applied to `class`es, `struct`s, properties and fields. When applied to `class`es or `struct`s, only a few of the parameters are applicable. They affect the dump of all properties from the type, unless overridden by `DumAttribute` applied to a specific property.
It is much more common though to apply the attribute to properties. Let's look at specific scenarios that will demonstrate the customization of the dump behavior.
### Suppress dumping of a property:
```csharp
[Dump(false)] public string Unimportant { get; set; }
```
Alternatively:
```csharp
[Dump(Skip=ShouldDump.Skip)]
public string Unimportant { get; set; }
```
### Control the order of dumping:
```csharp
    class MyClass
    {
        [Dump(2)]                               // or [Dump(Order=2)]
        public bool BoolProperty { get; set; }
        [Dump(1)]
        public int IntProperty { get; set; }
        [Dump(-1)]
        public Guid GuidProperty { get; set; }
        [Dump(0)]
        public Uri UriProperty { get; set; }
    }
```
This change to the type of the dumped object produces the following output:
```
MyClass (ObjectDumperSamples.MyClass, ObjectDumperSamples, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null):
  UriProperty              = <null>
  IntProperty              = 3
  BoolProperty             = True
  GuidProperty             = d8de41d8-14f3-4cf0-a1b6-fb18396be0e6
```
These are the recursive ordering rules:
1. First are dumped the properties of the base class (if any) with positive or non-specified order. Properties with the same order number are dumped in alphabetical order.
1. Next are dumped the current class properties with positive or unspecified order.
1. Follow the properties with positive or unspecified order of the derived class (if any.)
1. Then are dumped the properties with negative order in the opposite order: the derived, the current, the base class'.
1. Finally are dumped all properties from all classes with dump order `int.MinValue`. This pushes some properties really to the end of the dump, for example the stack in an Exception object.
* Note, that virtual properties are dumped with the class where they were defined, not where they were overridden.

So if we inherit from `MyClass` in `MyClassDescendant`:
```csharp
    class MyClassDescendant : MyClass
    {
        [Dump(0)]
        public string StringProperty { get; set; }
    }
```
a dump of an object of type `MyClassDescendant` will look like this:
```
MyClassDescendant (ObjectDumperSamples.MyClassDescendant, ObjectDumperSamples, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null):
  UriProperty = <null>

  IntProperty              = 3
  BoolProperty             = True
  StringProperty           = StringProperty
  GuidProperty             = fe0e72b2-196f-424a-a011-7f8119c04ead
```

### Dump some of the properties only if they are not null:
```csharp
        [Dump(0, DumpNullValues=ShouldDump.Skip)]
        public Uri UriProperty { get; set; }
```
With this modification the last dump becomes:
```
MyClassDescendant (ObjectDumperSamples.MyClassDescendant, ObjectDumperSamples, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null):
  IntProperty              = 3
  BoolProperty             = True
  StringProperty           = StringProperty
  GuidProperty             = e6559163-6b53-4c9a-aaf5-8bf620d9155a
```
Note that the property `DumpNullValues` can be applied also on a class level. Then any property with value `null` will be skipped in the dump output.
### Mask the values of some (e.g. PII) properties
```csharp
        [Dump(Mask=true)]
        public string SSN { get; set }
```
If we add this property with its attribute to `MyClass` the dump will look something like this:
```
MyClass (ObjectDumperSamples.MyClass, ObjectDumperSamples, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null):
  IntProperty              = 3
  BoolProperty             = True
  GuidProperty             = 1b862d91-ccb1-4b92-977d-d4f723c4d39d
  SSN                      = ******
```
Note that if SSN is null the output would be:
```
MyClass (ObjectDumperSamples.MyClass, ObjectDumperSamples, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null):
  IntProperty              = 3
  BoolProperty             = True
  GuidProperty             = 64d2112c-b8d9-4a67-922e-87d02e5da3ca
  SSN                      = <null>
```
If you want different string for mask, use the `DumpAttribute`'s property `MaskValue`:
```csharp
        [Dump(Mask=true, MaskValue="------")]
        public string SSN { get; set }
```
```
MyClass (ObjectDumperSamples.MyClass, ObjectDumperSamples, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null):
  IntProperty              = 3
  BoolProperty             = True
  GuidProperty             = 4e7fe09e-81b7-4527-b4b6-32e42ade950c
  SSN                      = ------
```
Clearly, these features were introduced with logging in mind where we want to protect some private information.
### Control the length of the output with the property `MaxLength`.
When applied to `string`s it specifies to dump only the first `MaxLength` characters. By default the dumper will dump the entire string. Let's add this property:
```csharp
        [Dump(MaxLength=25)]
        public string Description { get; set; }
```
and assign a long value to it:
```csharp
        var anObject = new MyClass
        {
            ...
            Description = "This is one very very very very very long description",
        };
```
The output would be:
```
MyClass (ObjectDumperSamples.MyClass, ObjectDumperSamples, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null):
  IntProperty              = 3
  BoolProperty             = True
  Description              = This is one very very ver...
  GuidProperty             = 07d23136-1ae5-40d8-bebc-8ff2a4fefb44
```
The attribute property can be applied to sequences of objects too - arrays, lists, etc. By default the dumper outputs no more than the first 10 elements of the sequence. If you want to dump them all (at your own risk!) specify a negative value, e.g. -1.
### Customize the format of the property name by using the property `LabelFormat`.
By default the property label is dumped using the format string "{0,-24} = ", where the property’s name is passed as parameter 0. Let's reuse the previous example:
```csharp
        [Dump(MaxLength=25, LabelFormat="{0,-12} (Truncated)")]
        public string Description { get; set; }
```
The property will be dumped like this:
```
  Description (Truncated)  = This is one very very ver...
```
### Customize the format of the dumped value with `ValueFormat`.
The default value of this property is "{0}". I.e. the default format of the value is used. Let's add another property with custom format for the value, e.g.
```csharp
        [Dump(ValueFormat="{0:o}")]
        public DateTime CreatedAt { get; set; }
```
The dump is:
```
MyClass (ObjectDumperSamples.MyClass, ObjectDumperSamples, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null):
  IntProperty              = 3
  BoolProperty             = True
  CreatedAt                = 2013-08-25T21:25:54.7103441-04:00
  GuidProperty             = 04dee1cf-1027-444b-87c8-639504461abf
```
This property recognizes a special format string "ToString()", which inserts the result of the invoking the property's method `ToString()` into the output stream.
### Customize the format of the dumped value with `DumpClass` and `DumpMethod`.
These two properties allow you to plug in you own custom dumping for certain properties of your class. They define where to find your method which will be invoked in order to produce the text representation of the property to which it is applied. The attribute's property `DumpClass` is required if the method is implemented in a class different from the property's class. In this case the requirements for the method are:  `public static string DumpMethod(...)`, and must take a single argument of the type of the property or base class of the property's type. The `DumpMethod` property specifies the name of the method. If it is omitted the object dumper will assume default dump method name "`Dump`".
If the `DumpClass` property is not specified but the property `DumpMethod` is, the object dumper will assume that the class containing the method is the class of the property and will try to find in it either an instance method with the specified name that returns `string` and takes no parameters or a static method which returns `string` and has a single parameter of the type of the property. In other words these two attributes are equivalent:
```csharp
        [Dump(DumpMethod="ToString")]
        [Dump(ValueFormat="ToString()")]
```
### Changing the format strings of the object dumper
For dumping various elements of the dumped objects the formatter uses a number of format strings that can be replaced. For example to display a type by default the dumper uses the format string `"{0} ({2}): "` where the placeholder `{0}` is replaced by the short name of the type and `{2}` by the fully qualified name of the type. If you want to change some of these, assign format strings to the static properties of the class `DumpFormat`. These properties are well documented: they list each replacment parameter. For example I often replace the format string for type to just `"{0}"`:
```csharp
        DumpFormat.Type = "{0}:";
```
which would change the output above to:
```
MyClass:
  IntProperty              = 3
  BoolProperty             = True
  CreatedAt                = 2013-08-25T21:25:54.7103441-04:00
  GuidProperty             = 04dee1cf-1027-444b-87c8-639504461abf
```
### `RecurceDump`
This property controls whether to go into the complex property or just dump the type of it. When applied to a class the property affects dumping of all complex and sequence properties of the class. The type of this parameter is `enum ShouldDump` which has three values:
* `Default` - use the default dump behavior.
* `Dump` - indent and dump recursively the properties of the associated object.
* `Skip` - do not dump the associated object, just output its type or its "default property".
### The `DefaultProperty`
If the dump of a property of complex type is suppressed with `RecurceDump=ShouldDump.Skip` besides of its type you can still dump one of its properties which you may consider characteristic or identifying for this type. For the purpose use the `DefaultProperty` property with `string` value the name of the property, e.g.
```csharp
        [Dump(RecurceDump=ShouldDump.Skip, DefaultProperty="Key")]
        public ComplexType Associate { get; }
```
### `MaxDepth`
If the chain of associated objects is too long, you can cut it short with the property `MaxDepth` on a class level:
```csharp
        [Dump(MaxDepth=3)]
        class MyClassDescendant : MyClass
        {
            ...
```
Here is the dump that I got for this scenario:
```
MyClassDescendant (ObjectDumperSamples.MyClassDescendant, ObjectDumperSamples, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null):
  IntProperty              = 3
  BoolProperty             = True
  CreatedAt                = 2013-09-10T20:43:48.9853660-04:00
  StringProperty           = StringProperty
  Description              = This is one very very ver...
  Associate                = ComplexType (ObjectDumperSamples.ComplexType, ObjectDumperSamples, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null):
    Key                      = IL.TX.2013-09-10:20:23:34.85930
    UniqueId                 = d481f7fe-2094-4e0c-b753-f99188db18eb
    Other                    = ComplexType (ObjectDumperSamples.ComplexType, ObjectDumperSamples, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null):
      Key                      = IL.TX.2013-09-10:20:23:34.85931
      UniqueId                 = c914a5a2-201c-4f48-8159-0e75a2809031
      Other                    = ComplexType (ObjectDumperSamples.ComplexType, ObjectDumperSamples, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null):
        Key                      = IL.TX.2013-09-10:20:23:34.85932
        UniqueId                 = 9c4d4937-a1c0-4b2f-b719-daadd12c29c5
        Other                    = ...object dump reached the maximum depth level. Use the DumpAttribute.MaxDepth to increase the depth level if needed.
  GuidProperty             = bdbda70b-95c8-4a62-a8df-dcdf4f8e2912
```
### Enumerating sequences (classes implementing `IEnumerable`)
Generally the dump recurses only into arrays and sequences from the BCL. The reason is that recursing into custom sequences may have unintended side effects. However the author of a custom sequence object may override this by using the `Enumerate=ShouldDump.Dump` property of  `DumpAttribute` applied on a class level:
```csharp
[Dump(Enumerate=ShouldDump.Dump)]
class MyCollection : IEnumerable<Item>
{
    . . .
}
```
### Dumped dictionaries
Objects of type `IDictionary<TKey,TValue>`, where the `TKey` parameter is a C# basic type (e.g. `int, decimal, string, DatTime`) are dumped in a more reader-friendly fashion (see unit tests `TestDictionaryBaseTypes` and `TestDictionaryBaseTypeAndObject`):
```
Dictionary<string, int>[3]: (System.Collections.Generic.Dictionary`2[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089)
{
  [one] = 1
  [two] = 2
  [three] = 3
}
```
```
Dictionary<int, Object4_1>[3]: (System.Collections.Generic.Dictionary`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+Object4_1, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089)
{
  [1] = Object4_1 (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+Object4_1, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393):
    Property1                = one
    Property2                = Property2
  [2] = Object4_1 (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+Object4_1, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393):
    Property1                = two
    Property2                = Property2
  [3] = Object4_1 (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+Object4_1, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393):
    Property1                = three
    Property2                = Property2
}
```
If the dictionary's keys are not from a basic C# type the dictionary will be dumped as a sequence of `KeyValuePair<TKey,TValue>` objects.
### Dumping `Expression` trees as C#-like text.
This feature is not directly related to the object dumper. Basically it is an extension method on the `Expression` class. Under the hood of the method it uses an internal class `CSharpDumpExpression` which inherits from the `ExpressionVisitor`. The method uses a `CSharpDumpExpression` object to traverse the expression tree and translate its contents to a C#-like text:
```csharp
        Expression<Func<int, int>> expression = a => 3*a + 5;

        Console.WriteLine(expression.DumpCSharpText());
```
will produce:
```
(int a) => 3 * a + 5
```
The object dumper recognizes expression trees and is using this method to dump the C# text as well as the rest of the tree. So if you dump the above expression with the object dumper (see also the unit test `TestDumpExpression`), you will get the following output - notice the second and third lines:
```
Expression<Func<int, int>> (System.Linq.Expressions.Expression`1[[System.Func`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089):
  C#-like expression text:
    (int a) => 3 * a + 5
  NodeType                 = ExpressionType.Lambda
  Type                     = (TypeInfo): System.Func`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
  Name                     = <null>
  ReturnType               = (TypeInfo): System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
  Parameters               = TrueReadOnlyCollection<ParameterExpression>[1]: (System.Runtime.CompilerServices.TrueReadOnlyCollection`1[[System.Linq.Expressions.ParameterExpression, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089)
    PrimitiveParameterExpression<int> (System.Linq.Expressions.PrimitiveParameterExpression`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089):
      NodeType                 = ExpressionType.Parameter
      Type                     = (TypeInfo): System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
      Name                     = a
      IsByRef                  = False
      NodeType                 = ExpressionType.Parameter
      Type                     = (TypeInfo): System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
      CanReduce                = False
  Body                     = SimpleBinaryExpression (System.Linq.Expressions.SimpleBinaryExpression, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089):
    NodeType                 = ExpressionType.Add
    Type                     = (TypeInfo): System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
    Left                     = SimpleBinaryExpression (System.Linq.Expressions.SimpleBinaryExpression, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089):
      NodeType                 = ExpressionType.Multiply
      Type                     = (TypeInfo): System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
      Left                     = ConstantExpression (System.Linq.Expressions.ConstantExpression, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089):
        NodeType                 = ExpressionType.Constant
        Type                     = (TypeInfo): System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
        Value                    = 3
        NodeType                 = ExpressionType.Constant
        CanReduce                = False
      Right                    = PrimitiveParameterExpression<int> (see above)
      IsLiftedLogical          = False
      IsReferenceComparison    = False
      NodeType                 = ExpressionType.Multiply
      Type                     = (TypeInfo): System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
      Method                   = <null>
      Conversion               = <null>
      IsLifted                 = False
      IsLiftedToNull           = False
      CanReduce                = False
    Right                    = ConstantExpression (System.Linq.Expressions.ConstantExpression, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089):
      NodeType                 = ExpressionType.Constant
      Type                     = (TypeInfo): System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
      Value                    = 5
      NodeType                 = ExpressionType.Constant
      CanReduce                = False
    IsLiftedLogical          = False
    IsReferenceComparison    = False
    NodeType                 = ExpressionType.Add
    Type                     = (TypeInfo): System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
    Method                   = <null>
    Conversion               = <null>
    IsLifted                 = False
    IsLiftedToNull           = False
    CanReduce                = False
  NodeType                 = ExpressionType.Lambda
  Type                     = (TypeInfo): System.Func`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
  TailCall                 = False
  CanReduce                = False
```
### All instances from the object graph are dumped at most once
In the example above notice that the right operand of the multiplication operation is dumped like this:
```
      Right                    = PrimitiveParameterExpression<int> (see above)
```
Or more precisely it is not dumped at all. The reason for that is to cut the possibility of infinite dump loops, e.g. when we have objects that are mutually referring to each other. In the sample above the `PrimitiveParameterExpression` with a name `"a"` has been dumped already.
### Dumping values of `enum` types with `FlagsAttribute`
The class `Object90` has a `Flags` `enum` (see unit test `TestDumpObject9_1`):
```csharp
        [Flags]
        enum TestFlags
        {
            One = 1 << 0,
            Two = 1 << 1,
            Four = 1 << 2,
            Eight = 1 << 3,
        }

        class Object90
        {
            public TestEnum Prop { get; set; }
            public TestFlags Flags { get; set; }
        }
```
The class `Object91` has a property of type `Object9`, which has a property `Flags` of type `TestFlags`. Here is a sample dump of `Object91`:
```
Object91 (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+Object91, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393):
  Object90                 = Object90 (vm.Aspects.Diagnostics.ObjectDumper.Tests.ObjectTextDumperTest+Object90, vm.Aspects.Diagnostics.ObjectDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393):
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
  Prop94                   = 5
```
Notice that for the value of the property `Flags` it dumps all set values (bits) by name.
### Performance and the dump cache (as of v1.7.0)
You can imagine that the implementation of the object dumper is one huge exercise on .NET reflection. However this would make it not particularly good performer - on average an object is dumped in a few dozens of milliseconds. While working on the [expression serialization](https://github.com/vmelamed/vm/tree/master/Aspects/Linq/Expressions/Serialization) I realized that all that reflection code can be used to generate expression trees that represent the dumping code for each class. Then all I need to do is compile these expression trees into delegates and cache them. So, the next time when I need to dump an object of the same type, instead of traversing the object graph with all its properties and nested objects using reflection again, all I need is really to pull the cached delegates and execute them with parameter the current object. The result was significant improvement. If on average dumping of an average object for first time takes something in the order of 40-60 milliseconds, the second time (executing a delegate from the cache) takes say 300 microseconds or less - a few hundreds times better. 

If for some reason (memory concerns or bugs in the dumper) you want to suppress the cache, you can do so by setting the static property `UseDumpScriptCache`:
```csharp
        ObjectTextDumper.UseDumpScriptCache = false;
```
