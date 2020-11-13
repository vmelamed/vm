# vm.Aspects.Diagnostics.ObjectTextDumper
This component can dump the value of an arbitrary .NET object to an easy to read text. The values of basic types like int, string, decimal, etc. are dumped as their most natural text representation, e.g. `"this is a string value"` or `3.1415926`. The dump of each class starts with the name of the class on the first line and then on separate, indented lines the names and the values of each property. Properties that represent aggregated or composed objects follow the same rule but with incremented indentation. See the samples below.

The `vm.Aspects.Diagnostics.ObjectTextDumper` (further referred to as "the dumper") class is very easy to integrate and easy to use. If desired, the dump output is highly customizable. This component can be very helpful in debugging, tracing, logging, etc. output scenarios targeting developers and system engineers.

The source code of the project can be found [at GitHub](https://github.com/vmelamed/vm/tree/master/Aspects/Diagnostics). Also, you can download and install the [NuGet package `vm.Aspects.Diagnostics.ObjectTextDumper`](https://www.nuget.org/packages/vm.Aspects.Diagnostics.ObjectTextDumper/) directly into your solution.

Please note that this document describes the `vm.Aspects.Diagnostics.ObjectTextDumper` version 3.0.0 or higher. This version replaces the previous NuGet packages  [`AspectObjectDumper` 1.x](https://www.nuget.org/packages/AspectObjectDumper/) and [`vm.Aspects.Diagnostics.ObjectTextDumper` 2.x](https://www.nuget.org/packages/vm.Aspects.Diagnostics.ObjectTextDumper/). The latter will be still available but will not be supported anymore. The big differences between the 2.x and 1.x versions are:
1. The names of the project and the package have been changed for consistency's sake with other [vm.Aspects projects](https://github.com/vmelamed/vm/tree/master/Aspects)
1. The package is no longer dependent on an internal or external implementations of the Common Service Locator
1. Some of the dump settings that were passed in the constructor of the dumper and the Dump method itself are now encapsulated in the struct `DumpSettings`.
1. As a result of the above changes the signatures of the constructor and the Dump method have changed.

The version 3.0 is a port of version 2.0 to .NET 5.0 and some bug fixes.

## Basic Usage
The dumper is implemented by the class `ObjectTextDumper` from the namespace `vm.Aspects.Diagnostics`.
Here is the first usage example that we are going to improve and customize further down in this document:
```csharp
using System;
using System.IO;
using vm.Aspects.Diagnostics;

namespace vm.Aspects.Diagnostics.ObjectDumper.Samples.Sample1
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
                GuidProperty = Guid.Empty,
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

            Console.Write("Press any key to finish.");
            Console.ReadKey(true);
        }
    }
}
```
Here is the output from this program:
```
5
MyClass (vm.Aspects.Diagnostics.ObjectDumper.Samples.Sample1.MyClass, vm.Aspects.Diagnostics.ObjectDumper.Samples.Sample1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null):
  BoolProperty             = True
  GuidProperty             = 00000000-0000-0000-0000-000000000000
  IntProperty              = 3
  UriProperty              = <null>
```

I prefer using the facades `DumpText` and `DumpString` instead. They are implemented as extension methods of `System.Object`. Just replace the entire `using` statement above with this code snippet:
```csharp
            using (var writer = new StringWriter())
            {
                anInt.DumpText(writer);
                anObject.DumpText(writer);
                Console.WriteLine(writer.GetStringBuilder().ToString());
            }
```
Or even simpler usage: replace the entire `using` statement with:
```csharp
            Console.WriteLine(anInt.DumpString());
            Console.WriteLine(anObject.DumpString());
```
If you don't have better ideas for overriding `MyClass.ToString()`, why not implement it like this:
```csharp
            public override string ToString() => this.DumpString();
```
Now we can replace the last line in the snippet with:
```csharp
            Console.WriteLine(anObject);
```
## The `ObjectTextDumper` Constructor
```csharp
        public ObjectTextDumper(
            TextWriter writer,
            IMemberInfoComparer memberInfoComparer = null);
```

As you see the constructor takes two parameters:
  1. A `TextWriter` descendant where the dump text will be written to.
  1. An object that implements comparison between `System.Reflection.MemberInfo` descending objects (e.g. `PropertyInfo` or `FieldInfo`). It is used to determine the dump order of the members of the dumped objects. If `null`, the dumper will use an internal default implementation.

Hint: this constructor allows for resolving the dumper from a dependency injection container.

## The `ObjectTextDumper.Dump` Method
```csharp
        public object Dump(
            object value,
            Type dumpMetadata = null,
            DumpAttribute dumpAttribute = null,
            int initialIndentLevel = DumpSettings.DefaultInitialIndentLevel);
```
The method returns the dumper object itself, just in case you want to chain the `Dump` calls. As you can see here, it takes actually four parameters, where the last three have default values.
1. The first parameter is the object that we want to dump
1. The second parameter is supposed to be a `Type` object representing the dumping metadata for the type of the dumped object. This parameter (if not-null) will override any other metadata already associated with the dumped object by means of a "buddy class" or `ClassMetadataResolver` (see below the sections *"Metadata Classes (Buddy Classes)"* and  *"Formatting the Dump of Objects for which You Have no Access to Their Source Code"*)
1. The third parameter is a `DumpAttribute` to be applied to the type of the dumped object and it also will override the `DumpAttribute` already associated with the dumped class (see below the sections *"Metadata Classes (Buddy Classes)"* and  *"Formatting the Dump of Objects for which You Have no Access to Their Source Code"*)
1. The last parameter is the initial indentation of the dump text - the default is 0

**The method `Dump` is not thread safe.**

**In general all static classes, methods and properties in the component are thread-safe. The `ObjectTextDumper` and the `Dump` method are not thread-safe.**

## Customizing the Dump Text
The programmer has control over the dump through the dump struct `DumpSettings` and the attribute `DumpAttribute`. 

### Dump Settings
The dumper uses a few dumping settings encapsulated in the structure `vm.Aspects.Diagnostics.DumpSettings`:
```csharp
        public struct DumpSettings : IEquatable<DumpSettings>
        {
            public const int DefaultInitialIndentLevel = 0;
            public const int DefaultIndentSize = 2;
            public const int DefaultMaxDumpLength = DumpTextWriter.DefaultMaxLength;
            public const BindingFlags DefaultPropertyBindingFlags = BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance;
            public const BindingFlags DefaultFieldBindingFlags = BindingFlags.Public|BindingFlags.Instance;

            public DumpSettings(
                bool useDumpScriptCache,
                int indentSize,
                int maxDumpLength,
                BindingFlags propertyBindingFlags,
                BindingFlags fieldBindingFlags) {...}

            public static DumpSettings Default => new DumpSettings(
                                                            true,
                                                            DefaultIndentSize,
                                                            DumpTextWriter.DefaultMaxLength,
                                                            DefaultPropertyBindingFlags,
                                                            DefaultFieldBindingFlags);

            // Gets or sets a value indicating whether to use the dump script cache facility.
            public bool UseDumpScriptCache { get; set; }

            // Gets or sets the size of the indents.
            public int IndentSize { get; set; }

            // Gets or sets the maximum length of the dump text.
            public int MaxDumpLength { get; set; }

            // Gets or sets the properties binding flags controlling which properties should be dumped, e.g. private vs public, static vs. instance, etc.
            public BindingFlags PropertiesBindingFlags { get; set; }

            // Gets or sets the fields binding flags controlling which fields should be dumped, e.g. private vs public, static vs. instance, etc.
            public BindingFlags FieldsBindingFlags { get; set; }
        }
```

The settings can be set in two different ways:
1. By assigning a `DumpSettings` object to the thread-safe static property `ObjectTextDumper.DefaultDumpSettings`. Upon creating new `ObjectTextDumper` instances, the default dump settings in the static property are copied to the instance property `InstanceSettings`.
1. By assigning a `DumpSettings` object to the instance property `InstanceSettings` before invoking the Dump methods.

When the Dump method is invoked the `InstanceSettings` property is copied to an internal per-dump settings property.

The `DumpSettings` structure has the following properties:

  1. `Default` - gets a `DumpSettings` instance initialized with default values.  
  1. `UseDumpScriptCache` - for performance the dumper can generate dumping code (dump script) for each dumped type, cache it, and subsequently reuse it. This boosts the dumping performance usually by more than 500 times. By default the script cache is enabled but if certain issues are discovered, it can be disabled until the code of the dumper is fixed. See also the section *"Performance of the vm.Aspects.Diagnostics.ObjectTextDumper and the Dump Cache"* below.
  1. `IndentSize` - the number of spaces in a single indent. By default 2.
  1. `MaxDumpLength` - the limit on the dumped text. This parameter would be useful when dumping objects with very big object graphs and you may run out of memory. The default value is 4 million characters (~ 8MB)
  1. The binding flags in the last two properties control which properties and fields should be dumped with respect to their visibility and lifetime modifiers. By default all public and non-public instance properties and all public instance fields are being dumped. From that point on, properties and fields are treated equally and for the sake of brevity in this document I will use only the word "property" unless explicitly stated otherwise, please assume that the same applies to fields.

### The Attribute `DumpAttribute`
The heart of the customization features of the dumper is the attribute class `DumpAttribute`. For a more detailed description take a look at the documentation comments in the source code.

The `DumpAttribute` attribute can be applied to `class`-es, `struct`-s, properties and fields. When applied to `class`-es or `struct`-s, only a few of the attribute properties are applicable. They affect the dump of all properties from the type, unless overridden by `DumpAttribute` applied to a specific property. However, it is much more common to apply the attribute to properties. 

The `Dump` attributes can be associated with your types in several ways:
1. Directly, by using the .NET language syntax for applying attributes to custom types and their properties and fields
1. Indirectly, in a so called "buddy class" - class referred to in a `MetadataTypeAttribute` applied to your class. This is my favorite method for my custom classes. I even have a Visual Studio extension that would generate a metadata class - a "buddy class" - out of any class
1. If you do not have access to the source code of the dumped object, the parameters of the `Dump` method allow you to associate its type with after-the-fact written buddy-class, as described above
1. Alternatively, use the method of the static class `ClassMetadataResolver.SetClassDumpMetadata`. The signature of the method is pretty self-explanatory:
```csharp
        public static void SetClassDumpMetadata(
            Type type,                              // the targeted type will be associated with:
            Type metadata = null,                   //      this metadata class
            DumpAttribute dumpAttribute = null);    //      this attribute object
```
If the second and third parameters are `null`-s, their respective values will be sought in the attributes (if any) applied to the target type. If not found there - some sensible defaults are assumed. The `ClassMetadataResolver` is a thread-safe, static cache of associations of types with dumping meta-data objects. This facility is most useful for objects from the .NET base class libraries or third party classes where you don't have access to the class's code.

Let's look at specific scenarios that will demonstrate the customization of the dump behavior.

### Suppress Dumping of a Property:
```csharp
[Dump(false)]
public string Unimportant { get; set; }
```
Alternatively:
```csharp
[Dump(Skip=ShouldDump.Skip)]
public string Unimportant { get; set; }
```
### Control the Order of Properties and Fields in the Dump:
```csharp
    class MyClass
    {
        [Dump(0)]
        public virtual string StringProperty { get; set; } = "myClass.StringProperty";
        [Dump(2)]                               // or [Dump(Order=2)]
        public bool BoolProperty { get; set; } = true;
        [Dump(1)]
        public int IntProperty { get; set; } = 3;
        [Dump(-1)]
        public Guid GuidProperty { get; set; } = Guid.NewGuid();
        [Dump(2)]
        public Uri UriProperty { get; set; }
        [Dump(false)]
        public string Unimportant { get; set; } = "whatever";
    }
```
This change to the type of the dumped object produces the following output:
```
MyClass (vm.Aspects.Diagnostics.ObjectDumper.Samples.Sample2.MyClass, vm.Aspects.Diagnostics.ObjectDumper.Samples.Sample2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null):
  StringProperty           = myClass.StringProperty
  IntProperty              = 3
  BoolProperty             = True
  UriProperty              = <null>
  GuidProperty             = c7c9912a-1d61-4cf8-9ff9-e20caa3ccbba
```
These are the ordering rules:
1. First are dumped the properties of the base class (if any) with positive or non-specified order. Properties with the same order number are dumped in alphabetical order
1. Next are dumped the current class properties with positive or unspecified order
1. Follow the properties with positive or unspecified order of the derived class (if any)
1. Then are dumped the properties with negative order in the opposite order: declared in the derived class, the current class, and the base class
1. Finally are dumped all properties from all classes with dump order `int.MinValue`. This pushes some properties to the end of the dump, for example the stack in an `Exception` object.

**Note, that virtual properties are dumped with the class where they were defined, not where they were overridden.**

So if we inherit `MyClassDescendant` from `MyClass`:
```csharp
    class MyClassDescendant : MyClass
    {
        [Dump(5)]
        public override string StringProperty { get; set; } = "myClassDescendant.StringProperty";
    }
```
a dump of an object of type `MyClassDescendant` will look like this:
```
MyClassDescendant (vm.Aspects.Diagnostics.ObjectDumper.Samples.Sample2.MyClassDescendant, vm.Aspects.Diagnostics.ObjectDumper.Samples.Sample2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null):
  StringProperty           = myClassDescendant.StringProperty
  IntProperty              = 3
  BoolProperty             = True
  UriProperty              = <null>
  GuidProperty             = b792f986-4e4d-4ae5-b498-b47f8d03d9dd
```

### Dump Properties Only If They Are Not `null`:
```csharp
        [Dump(0, DumpNullValues=ShouldDump.Skip)]
        public Uri UriProperty { get; set; }
```
With this modification the last dump would become:
```
MyClassDescendant (vm.Aspects.Diagnostics.ObjectDumper.Samples.Sample2.MyClassDescendant, vm.Aspects.Diagnostics.ObjectDumper.Samples.Sample2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null):
  StringProperty           = myClassDescendant.StringProperty
  IntProperty              = 3
  BoolProperty             = True
  GuidProperty             = b792f986-4e4d-4ae5-b498-b47f8d03d9dd
```
Note that the property `DumpNullValues` can be applied also on a class level. Then any property with value `null` will be skipped in the dump output.

### Mask the Values of Some Properties (e.g. PII)
```csharp
        [Dump(Mask=true)]
        public string SSN { get; set }
```
The dump will look something like this:
```
MyClass1 (vm.Aspects.Diagnostics.ObjectDumper.Samples.Sample2.MyClass1, vm.Aspects.Diagnostics.ObjectDumper.Samples.Sample2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null):
  SSN                      = ******
  IntProperty              = 3
  BoolProperty             = True
  UriProperty              = <null>
  GuidProperty             = 4b459714-afc8-46e0-bb90-16c2b5ebbdd8
```
Note that if SSN is null the output would be:
```
MyClass1 (vm.Aspects.Diagnostics.ObjectDumper.Samples.Sample2.MyClass1, vm.Aspects.Diagnostics.ObjectDumper.Samples.Sample2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null):
  SSN                      = <null>
  IntProperty              = 3
  BoolProperty             = True
  UriProperty              = <null>
  GuidProperty             = 4b459714-afc8-46e0-bb90-16c2b5ebbdd8
```
If you want different string for the mask, use the `DumpAttribute`'s property `MaskValue`:
```csharp
        [Dump(Mask=true, MaskValue="------")]
        public string SSN { get; set }
```
This will produce:
```
MyClass1 (vm.Aspects.Diagnostics.ObjectDumper.Samples.Sample2.MyClass1, vm.Aspects.Diagnostics.ObjectDumper.Samples.Sample2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null):
  SSN                      = ------
  IntProperty              = 3
  BoolProperty             = True
  UriProperty              = <null>
  GuidProperty             = 6aad74e3-9bbd-4568-8e86-6f08441f5f07
```
Clearly, these features were introduced with logging in mind where we want to protect some private information.

### Control the Length of the Output with the Property `MaxLength`.
When applied to `string`s it specifies to dump only the first `MaxLength` characters. By default the dumper will dump the entire string. Let's add this property:
```csharp
        [Dump(MaxLength = 25)]
        public string Description { get; set; } = "This is one very very very very very long description";
```
The output would be:
```
MyClass1 (vm.Aspects.Diagnostics.ObjectDumper.Samples.Sample2.MyClass1, vm.Aspects.Diagnostics.ObjectDumper.Samples.Sample2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null):
  SSN                      = ******
  IntProperty              = 3
  BoolProperty             = True
  UriProperty              = <null>
  Description              = This is one very very ver...
  GuidProperty             = 6fde3d03-e1d0-4af3-95f1-158d1fe5a9c0
```
This attribute property can also be applied to sequences - arrays, lists, sets, etc. By default the dumper outputs no more than the first 10 elements of sequences. If you want to dump them all (at your own risk!) specify a negative value, e.g. -1.

### Customize the Format of the Property Name by Using the Property `LabelFormat`.
By default the property label is dumped using the format string "{0,-24} = ", where the name of the property is passed as parameter 0. Let's reuse the previous example:
```csharp
        [Dump(MaxLength=25, LabelFormat="{0,-12} (Truncated) = ")]
        public string Description { get; set; }
```
The property will be dumped like this:
```
MyClass1 (vm.Aspects.Diagnostics.ObjectDumper.Samples.Sample2.MyClass1, vm.Aspects.Diagnostics.ObjectDumper.Samples.Sample2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null):
  SSN                      = ******
  IntProperty              = 3
  BoolProperty             = True
  UriProperty              = <null>
  Description  (Truncated) = This is one very very ver...
  GuidProperty             = 12ae5eaa-2a23-4bd1-97ee-a3f2ac8ccac4
```

### Customize the Format of the Dumped Value with `ValueFormat`.
The default value of this property is "{0}". I.e. the default format of the value is used. Let's add another property with custom format for the value, e.g.
```csharp
        [Dump(ValueFormat="{0:o}")]
        public DateTime CreatedAt { get; set; }
```
The dump is:
```
MyClass1 (vm.Aspects.Diagnostics.ObjectDumper.Samples.Sample2.MyClass1, vm.Aspects.Diagnostics.ObjectDumper.Samples.Sample2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null):
  SSN                      = ******
  IntProperty              = 3
  BoolProperty             = True
  UriProperty              = <null>
  CreatedAt                = 2018-07-25T22:04:28.7068607-04:00
  Description  (Truncated) = This is one very very ver...
  GuidProperty             = 824547ce-253b-4a3b-a802-61fa1a4411b9
```
This property recognizes a special format string "ToString()", which inserts the result of the invoking the property's method `ToString()` into the output stream.

### Customize the Format of the Dumped Value with `DumpClass` and `DumpMethod`.
These two properties allow you to plug in your own custom dumping for certain properties of your class. They define where to find your method to be invoked to produce the text representation of the property to which it is applied. The property `DumpClass` is required if the method is implemented in a class different from the property's class. In this case the requirements for the method are: `public static string DumpMethod(PropertyType property)`, and must take a single argument of the type of the property or a base class of the property's type. The `DumpMethod` property specifies the name of the method. If it is omitted, the object dumper will assume default dump method named "`Dump`".
If the `DumpClass` property is not specified but the property `DumpMethod` is, the object dumper will assume that the class containing the method is the class of the property and will try to find in it either an instance method with the specified name that returns `string` and takes no parameters or a static method which returns `string` and has a single parameter of the type of the property. In other words these two attributes are equivalent:
```csharp
        [Dump(DumpMethod="ToString")]
        [Dump(ValueFormat="ToString()")]
```
### Changing the Format Strings Used by the Object Dumper
For dumping various elements of the dumped objects the formatter uses a number of format strings that can be replaced. For example to display a type by default the dumper uses the format string `"{0} ({2}): "` where the placeholder `{0}` is replaced by the short name of the type and `{2}` by the fully qualified name of the type. If you want to change some of these, assign format strings to the static properties of the class `DumpFormat`. These properties are well documented: they list each replacement parameter. 

 For example I often replace the format string for type to just `"{0}"`:
```csharp
        DumpFormat.Type = "{0}:";
```
which would change the output above to:
```
MyClass1:
  SSN                      = ******
  IntProperty              = 3
  BoolProperty             = True
  UriProperty              = <null>
  CreatedAt                = 2018-07-25T22:29:25.8671023-04:00
  Description  (Truncated) = This is one very very ver...
  GuidProperty             = 7c3dcc4d-0a6d-428b-b8b8-e3322d3dd467
```

The changes to the `DumpFormat` should happen before any dumping, e.g. in the initialization phase of your application, because the the values in the class are stored in the dump script cache.

### `RecurceDump`
This property controls whether to go into the complex property or just dump the type of it. When applied to a class the property affects dumping of all complex and sequence properties of the class. The type of this parameter is `enum ShouldDump` which has three values:
* `Default` - use the default dump behavior.
* `Dump` - indent and dump recursively the properties of the associated object.
* `Skip` - do not dump the associated object, just output its type or its "default property".

### The `DefaultProperty`
If the dump of a property of complex type is suppressed with `RecurseDump=ShouldDump.Skip` you still can dump one of its properties which you may consider characteristic or identifying for this type. For the purpose use the `DefaultProperty` property with `string` value the name of the property, e.g.
```csharp
        [Dump(RecurseDump=ShouldDump.Skip, DefaultProperty="Key")]
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
Here is the dump in Sample3 that I got for this scenario:
```
MyClassDescendant (vm.Aspects.Diagnostics.ObjectDumper.Samples.MyClassDescendant, vm.Aspects.Diagnostics.ObjectDumper.Samples.Sample3, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null):
  UriProperty              = https://github.com/vmelamed/vm
  IntProperty              = 3
  BoolProperty             = True
  CreatedAt                = 2018-07-25T22:46:39.9477567-04:00
  StringProperty           = StringProperty
  Description              = This is one very very ver...
  Associate                = ComplexType (vm.Aspects.Diagnostics.ObjectDumper.Samples.ComplexType, vm.Aspects.Diagnostics.ObjectDumper.Samples.Sample3, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null):
    Key                      = IL.TX.2013-09-10:20:23:34.85930
    UniqueId                 = fc4a0c53-ba50-4b29-b0eb-cb0a027d1c41
    Other                    = ComplexType (vm.Aspects.Diagnostics.ObjectDumper.Samples.ComplexType, vm.Aspects.Diagnostics.ObjectDumper.Samples.Sample3, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null):
      Key                      = IL.TX.2013-09-10:20:23:34.85931
      UniqueId                 = ac24033c-b0dd-4cbb-a92d-f9bf083669b4
      Other                    = ComplexType (vm.Aspects.Diagnostics.ObjectDumper.Samples.ComplexType, vm.Aspects.Diagnostics.ObjectDumper.Samples.Sample3, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null):
        Key                      = IL.TX.2013-09-10:20:23:34.85932
        UniqueId                 = 663b0bad-4d76-4a9a-a13f-c62a95b25017
        Other                    = ...object dump reached the maximum depth level. Use the DumpAttribute.MaxDepth to increase the depth level if needed.
  GuidProperty             = 2a191ccf-840d-4cf7-9683-0c3bc148136f
```
### Enumerating Objects that Implement `IEnumerable`
Generally the dump recurses only into arrays and sequences from the BCL. The reason is that recursing into custom sequences may have unintended side effects. However the author of a custom sequence object may override this by using the `Enumerate=ShouldDump.Dump` property of  `DumpAttribute` applied on a class level:
```csharp
[Dump(Enumerate=ShouldDump.Dump)]
class MyCollection : IEnumerable<Item>
{
    . . .
}
```

## Other Features of the Dumper

### Metadata Classes (buddy classes)

I personally do not like too many attributes in my code. It's been since .NET 3.5 that the concept of a buddy class was introduced. The idea is to have something like a parallel class (some call it "buddy class") for each otherwise heavily adorned class. The parallel class is designated with the attribute `MetadataTypeAttribute`. The idea is very simple: apply the attibute to the main class with a parameter the type of the metadata class. By convention the main class is marked with the keyword `partial`, although it is not absolutely mandatory. The metadata class should never be instantiated and to prevent you from doing so it is usually marked as `abstract`. The metadata class contains only properties with the same names as the properties of the main class and since they are never used, they should be (again by convention) of type `object`. Now you can apply all attributes to the properties of the buddy class instead of to the main class. There are several packages that make use of buddy classes: Entity Framework, WPF, Enterprise Library and of course the AspectObjectDumper. Here is a small example of a class coupled with a buddy class (it is from the unit tests of the ObjectTextDumper):
```csharp
        [MetadataType(typeof(GenericWithBuddyMetadata))]
        public class GenericWithBuddy<T>
        {
            public T Property1 { get; set; }

            [Dump(Mask = true)]
            public T Property2 { get; set; }
        }

        class GenericWithBuddyMetadata
        {
            [Dump(false)]
            public object Property1 { get; set; }

            public object Property2 { get; set; }
        }
```
Note that the dumper can use `DumpAttribute`-s from both the main class and the buddy class (the buddy class has a preference) and can be applied even to generic classes. Dumping this object 
```csharp
    new GenericWithBuddy<int, int> { Property1 = 7, Property2 = 3}
```
produces the following output:
```
GenericWithBuddy<int> (vm.Aspects.Diagnostics.ObjectTextDumper.Tests.ObjectTextDumperTest+GenericWithBuddy`1[[System.Int32, mscorlib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], vm.Aspects.Diagnostics.ObjectTextDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393): 
  Property2                = ******
```
### Formatting the dump of objects for which you have no access to their source code
Very often you want to dump objects for which you have no access to the source class (e.g. `System.InvalidOperationException`). AspectObjectDumper allows you to associate buddy classes for those type as well. Use the static method
```csharp
    ClassMetadataCache.SetClassDumpMetadata(Type type, Type metadataType = null, DumpAttribute dumpAttribute = null, bool replace = false)
```
It associates the parameter `type` with the `metadataType` as it was a buddy class and also takes a class level `dumpAttribute`. The AspectObjectDumper defines a number of buddy classes and associates them to some BCL classes. Just take a look at the project folder `ExternalMetadata` and the utility `ClassMetadataRegistrar`. To use these predefined buddy classes just call:
```csharp
    ClassMetadataRegistrar.RegisterMetadata();
```
Feel free to add more mappings like this to the metadata cache. Note that the folder with metadata classes contains more classes than those that are actually registered. I just didn't want to increase the dependency footprint of the dumper by including System.Data, Microsoft.Practices.EnterpriseLibrary.Validation, etc.

Here is how the registrar registers metadata class for the `Task<>` class:
```csharp
    ClassMetadataResolver.SetClassDumpMetadata(typeof(Task<>), typeof(TaskDumpMetadata));
```

### Dumped Dictionaries
Objects of type `IDictionary<TKey,TValue>`, where the `TKey` parameter is a C# basic type (e.g. `int, decimal, string, DatTime`) are dumped in a more reader-friendly fashion (see unit tests `TestDictionaryBaseTypes` and `TestDictionaryBaseTypeAndObject`):
```
Dictionary<string, int>[3]: (System.Collections.Generic.Dictionary`2[[System.String, mscorlib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Int32, mscorlib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089)
{
  [one] = 1
  [two] = 2
  [three] = 3
}
```
```
Dictionary<int, Object4_1>[3]: (System.Collections.Generic.Dictionary`2[[System.Int32, mscorlib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[vm.Aspects.Diagnostics.ObjectTextDumper.Tests.ObjectTextDumperTest+Object4_1, vm.Aspects.Diagnostics.ObjectTextDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393]], mscorlib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089)
{
  [1] = Object4_1 (vm.Aspects.Diagnostics.ObjectTextDumper.Tests.ObjectTextDumperTest+Object4_1, vm.Aspects.Diagnostics.ObjectTextDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393):
    Property1                = one
    Property2                = Property2
  [2] = Object4_1 (vm.Aspects.Diagnostics.ObjectTextDumper.Tests.ObjectTextDumperTest+Object4_1, vm.Aspects.Diagnostics.ObjectTextDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393):
    Property1                = two
    Property2                = Property2
  [3] = Object4_1 (vm.Aspects.Diagnostics.ObjectTextDumper.Tests.ObjectTextDumperTest+Object4_1, vm.Aspects.Diagnostics.ObjectTextDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393):
    Property1                = three
    Property2                = Property2
}
```
If the dictionary's keys are not from a basic C# type the dictionary will be dumped as a sequence of `KeyValuePair<TKey,TValue>` objects.

### All Instances from the Object Graph Are Dumped at Most Once
In the example above notice that the right operand of the multiplication operation is dumped like this:
```
      Right                    = PrimitiveParameterExpression<int> (see above)
```
Or more precisely it is not actually dumped. The reason for this is to cut the possibility of infinite dump loops, e.g. when we have objects that are mutually referring to each other. In the sample above the `PrimitiveParameterExpression` `a` has been dumped already.

### Dumping Values of `enum` Types with `FlagsAttribute`
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
Object91 (vm.Aspects.Diagnostics.ObjectTextDumper.Tests.ObjectTextDumperTest+Object91, vm.Aspects.Diagnostics.ObjectTextDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393):
  Object90                 = Object90 (vm.Aspects.Diagnostics.ObjectTextDumper.Tests.ObjectTextDumperTest+Object90, vm.Aspects.Diagnostics.ObjectTextDumper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393):
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

### Dumping `Expression` Trees as C#-like Text.
This feature is not directly related to the object dumper. Basically it is an extension method on the `System.Linq.Expressions.Expression` class (the lambda expressions). Under the hood of the method it uses an internal class `CSharpDumpExpression` which inherits from the `ExpressionVisitor`. The method uses a `CSharpDumpExpression` object to traverse the expression tree and translate its contents to a C#-like text:
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
Expression<Func<int, int>> (System.Linq.Expressions.Expression`1[[System.Func`2[[System.Int32, mscorlib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Int32, mscorlib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], System.Core, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089):
  C#-like expression text:
    (int a) => 3 * a + 5
  NodeType                 = ExpressionType.Lambda
  Type                     = (TypeInfo): System.Func`2[[System.Int32, mscorlib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Int32, mscorlib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
  Name                     = <null>
  ReturnType               = (TypeInfo): System.Int32, mscorlib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
  Parameters               = TrueReadOnlyCollection<ParameterExpression>[1]: (System.Runtime.CompilerServices.TrueReadOnlyCollection`1[[System.Linq.Expressions.ParameterExpression, System.Core, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], System.Core, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089)
    PrimitiveParameterExpression<int> (System.Linq.Expressions.PrimitiveParameterExpression`1[[System.Int32, mscorlib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], System.Core, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089):
      NodeType                 = ExpressionType.Parameter
      Type                     = (TypeInfo): System.Int32, mscorlib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
      Name                     = a
      IsByRef                  = False
      NodeType                 = ExpressionType.Parameter
      Type                     = (TypeInfo): System.Int32, mscorlib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
      CanReduce                = False
  Body                     = SimpleBinaryExpression (System.Linq.Expressions.SimpleBinaryExpression, System.Core, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089):
    NodeType                 = ExpressionType.Add
    Type                     = (TypeInfo): System.Int32, mscorlib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
    Left                     = SimpleBinaryExpression (System.Linq.Expressions.SimpleBinaryExpression, System.Core, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089):
      NodeType                 = ExpressionType.Multiply
      Type                     = (TypeInfo): System.Int32, mscorlib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
      Left                     = ConstantExpression (System.Linq.Expressions.ConstantExpression, System.Core, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089):
        NodeType                 = ExpressionType.Constant
        Type                     = (TypeInfo): System.Int32, mscorlib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
        Value                    = 3
        NodeType                 = ExpressionType.Constant
        CanReduce                = False
      Right                    = PrimitiveParameterExpression<int> (see above)
      IsLiftedLogical          = False
      IsReferenceComparison    = False
      NodeType                 = ExpressionType.Multiply
      Type                     = (TypeInfo): System.Int32, mscorlib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
      Method                   = <null>
      Conversion               = <null>
      IsLifted                 = False
      IsLiftedToNull           = False
      CanReduce                = False
    Right                    = ConstantExpression (System.Linq.Expressions.ConstantExpression, System.Core, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089):
      NodeType                 = ExpressionType.Constant
      Type                     = (TypeInfo): System.Int32, mscorlib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
      Value                    = 5
      NodeType                 = ExpressionType.Constant
      CanReduce                = False
    IsLiftedLogical          = False
    IsReferenceComparison    = False
    NodeType                 = ExpressionType.Add
    Type                     = (TypeInfo): System.Int32, mscorlib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
    Method                   = <null>
    Conversion               = <null>
    IsLifted                 = False
    IsLiftedToNull           = False
    CanReduce                = False
  NodeType                 = ExpressionType.Lambda
  Type                     = (TypeInfo): System.Func`2[[System.Int32, mscorlib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Int32, mscorlib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
  TailCall                 = False
  CanReduce                = False
```

### Performance of the vm.Aspects.Diagnostics.ObjectTextDumper and the Dump Cache
You can imagine that the implementation of the object dumper is one huge exercise on .NET reflection. However this would make it not particularly good performer - on average an object is dumped in tens or even hundreds of milliseconds. While working on the [expression serialization](https://github.com/vmelamed/vm/tree/master/Aspects/Linq/Expressions/Serialization) I realized that all that reflection code can be used to generate expression trees that represent the dumping code for each class. Then all I need to do is compile these expression trees into delegates and cache them. So, the next time when I need to dump an object of the same type, instead of traversing the object graph with all its properties and nested objects using reflection again, all I need is really to pull the cached delegate and execute it with parameter the current object. The result was significant improvement in performance. If on average dumping of an average object for first time takes something in the order of 100-150 milliseconds, the second time (executing a delegate from the cache) takes say 0.2-0.35ms - a pretty good performance gain. 

If for some reason (memory concerns or bugs in the dumper) you want to suppress the expression cache, you can do so by setting the static property `DumpSettings.UseDumpScriptCache`, e.g.
```csharp
        ObjectTextDumper.DefaultDumpSettings.UseDumpScriptCache = false;
```
