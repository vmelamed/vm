using System;

namespace vm.Aspects.Diagnostics.ObjectTextDumperTests
{
    public partial class ObjectTextDumperTest
    {
        public const string ObjectTextDumperTestAssembly = "vm.Aspects.Diagnostics.ObjectTextDumperTests, Version=3.0.0.0, Culture=neutral, PublicKeyToken=1fb2eb0544466393";
        public const string ObjectTextDumperTestClass    = "vm.Aspects.Diagnostics."+nameof(ObjectTextDumperTests)+"."+nameof(ObjectTextDumperTest);
        public const string CSharpLambda                 = "Expression1<Func<";
        public const string LinqExpression               = "System.Linq.Expressions.Expression1";
        public const string DotNetAssembly               = "System.Private.CoreLib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e";
        public const string LinqAssembly                 = "System.Linq.Expressions, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
        public const string RuntimeExtensionsAssembly    = "System.Private.CoreLib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e";
        public const string RoCollectionNamespace        = "System.Collections.ObjectModel";
        public const string ReadOnlyCollection           = "ReadOnlyCollection";

        #region basic values and corresponding strings
        readonly object?[] _basicValues =
        {
            null,
            new int?(),
            true,
            'A',
            (byte)1,
            (sbyte)1,
            (short)1,
            (int)1,
            (long)1,
            (ushort)1,
            (uint)1,
            (ulong)1,
            1.0,
            (float)1.0,
            1M,
            "1M",
            Guid.Empty,
            new Uri("http://localhost"),
            new DateTime(2013, 1, 13),
            new TimeSpan(123L),
            new DateTimeOffset(new DateTime(2013, 1, 13, 0, 0, 0, DateTimeKind.Utc)),
        };

        readonly string[] _basicValuesStrings =
        {
            "<null>",
            "<null>",
            "True",
            "A",
            "1",
            "1",
            "1",
            "1",
            "1",
            "1",
            "1",
            "1",
            "1",
            "1",
            "1",
            "1M",
            "00000000-0000-0000-0000-000000000000",
            "http://localhost/",
            "2013-01-13T00:00:00.0000000",
            "00:00:00.0000123",
            "2013-01-13T00:00:00.0000000+00:00",
        };
        #endregion

        const string TestDumpObject1_1_expected = @"
Object1 ("+ObjectTextDumperTestClass+@"+Object1, "+ObjectTextDumperTestAssembly+@"):
  BoolField                = True
  ByteField                = 1
  CharField                = A
  DateTimeField            = 2013-01-13T00:00:00.0000000Z
  DateTimeOffsetField      = 2013-01-13T00:00:00.0000000+00:00
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
  DateTimeOffsetProperty   = 2013-01-13T00:00:00.0000000+00:00
  DateTimeProperty         = 2013-01-13T00:00:00.0000000Z
  DecimalProperty          = 1
  DoubleProperty           = 1
  FloatProperty            = 1
  GuidProperty             = 00000000-0000-0000-0000-000000000000
  IntProperty              = 1
  LongProperty             = 1
  NullIntProperty          = <null>
  NullLongProperty         = 1
  NullObjectProperty       = <null>
  ObjectProperty           = object (System.Object, "+DotNetAssembly+@"):
  SByteProperty            = 1
  ShortProperty            = 1
  TimeSpanProperty         = 00:00:00.0000123
  UIntProperty             = 1
  ULongProperty            = 1
  UShortProperty           = 1
  UriProperty              = http://localhost/";

        const string TestDumpObject1WithFields_1_expected = @"
Object1 ("+ObjectTextDumperTestClass+@"+Object1, "+ObjectTextDumperTestAssembly+@"):
  BoolField                = True
  ByteField                = 1
  CharField                = A
  DateTimeField            = 2013-01-13T00:00:00.0000000Z
  DateTimeOffsetField      = 2013-01-13T00:00:00.0000000+00:00
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
  DateTimeOffsetProperty   = 2013-01-13T00:00:00.0000000+00:00
  DateTimeProperty         = 2013-01-13T00:00:00.0000000Z
  DecimalProperty          = 1
  DoubleProperty           = 1
  FloatProperty            = 1
  GuidProperty             = 00000000-0000-0000-0000-000000000000
  IntProperty              = 1
  LongProperty             = 1
  NullIntProperty          = <null>
  NullLongProperty         = 1
  NullObjectProperty       = <null>
  ObjectProperty           = object (System.Object, "+DotNetAssembly+@"):
  SByteProperty            = 1
  ShortProperty            = 1
  TimeSpanProperty         = 00:00:00.0000123
  UIntProperty             = 1
  ULongProperty            = 1
  UShortProperty           = 1
  UriProperty              = http://localhost/";

        const string TestDumpObject1_1_Limited_expected = @"
Object1 ("+ObjectTextDumperTestClass+@"+Object1, "+ObjectTextDumperTestAssembly+ @"):
  BoolField                = True
  ByteField                = 1
  CharField                = A
  DateTimeField            = 2013-01-13T00:00:00.0000000Z
  DateTimeOffsetField      = 2013-01-13T00:00:00.0000000+00:00
  DecimalField             = 1
  DoubleField              = 1
  FloatField               = ...
The dump exceeded the maximum length of 500 characters. Either increase the value of the argument maxDumpLength of the constructor of the ObjectTextDumper class, or suppress the dump of some types and properties using DumpAttribute-s and metadata.";

        const string TestDumpObject1_2_expected = @"
Object1 ("+ObjectTextDumperTestClass+@"+Object1, "+ObjectTextDumperTestAssembly+@"):
  ObjectProperty           : object (System.Object, "+DotNetAssembly+@"):
  NullObjectProperty       = <null>
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
  DateTimeField            = 2013-01-13T00:00:00.0000000Z
  DateTimeOffsetField      = 2013-01-13T00:00:00.0000000+00:00
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
  DateTimeProperty         = 2013-01-13T00:00:00.0000000Z
  TimeSpanProperty         = 00:00:00.0000123";

        const string TestDumpObject1_3_expected = @"
Object1 ("+ObjectTextDumperTestClass+@"+Object1, "+ObjectTextDumperTestAssembly+@"):
  ObjectProperty           : object (System.Object, "+DotNetAssembly+@"):
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
  DateTimeField            = 2013-01-13T00:00:00.0000000Z
  DateTimeOffsetField      = 2013-01-13T00:00:00.0000000+00:00
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
  DateTimeProperty         = 2013-01-13T00:00:00.0000000Z
  TimeSpanProperty         = 00:00:00.0000123";

        const string TestDumpObject1WithFieldsMetadata_2_expected = @"
Object1 ("+ObjectTextDumperTestClass+@"+Object1, "+ObjectTextDumperTestAssembly+@"):
  ObjectProperty           : object (System.Object, "+DotNetAssembly+@"):
  NullIntProperty          = <null>
  NullObjectProperty       = object (System.Object, "+DotNetAssembly+@"):
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
  DateTimeField            = 2013-01-13T00:00:00.0000000Z
  DateTimeOffsetField      = 2013-01-13T00:00:00.0000000+00:00
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
  DateTimeProperty         = 2013-01-13T00:00:00.0000000Z
  TimeSpanProperty         = 00:00:00.0000123";

        const string TestDumpObject2_1_expected = @"
Object2 ("+ObjectTextDumperTestClass+@"+Object2, "+ObjectTextDumperTestAssembly+@"):
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
  TimeSpanProperty         = 00:00:00.0000123";

        const string TestDumpObject3_1_expected = @"
Object3 ("+ObjectTextDumperTestClass+@"+Object3, "+ObjectTextDumperTestAssembly+@"):
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
  DateTimeProperty         = 2013-01-13T00:00:00.0000000Z
  TimeSpanProperty         = 00:00:00.0000123";

        const string TestDumpObject5_1_expected = @"
Object5_1 ("+ObjectTextDumperTestClass+@"+Object5_1, "+ObjectTextDumperTestAssembly+@"):
  PropertyA                = PropertyA
  PropertyB                = PropertyB
  PropertyC                = ValueTuple<int, string> (System.ValueTuple`2[[System.Int32, "+DotNetAssembly+@"],[System.String, "+DotNetAssembly+@"]], "+DotNetAssembly+@"):
    Item1                    = 42
    Item2                    = Don't panic
    System.Runtime.CompilerServices.ITuple.Length = 2
  Associate                = Object4_1 ("+ObjectTextDumperTestClass+@"+Object4_1, "+ObjectTextDumperTestAssembly+@"):
    Property1                = Property1
  Associate2               = Object4_1 ("+ObjectTextDumperTestClass+@"+Object4_1, "+ObjectTextDumperTestAssembly+@"):";

        const string TestDumpExpression_expected = @"
"+CSharpLambda+@"int, int>> ("+LinqExpression+@"`1[[System.Func`2[[System.Int32, "+DotNetAssembly+@"],[System.Int32, "+DotNetAssembly+@"]], "+DotNetAssembly+@"]], "+LinqAssembly+@"):
  C#-like expression text:
    (int a) => 3 * a + 5
  NodeType                 = ExpressionType.Lambda
  Type                     = (TypeInfo): System.Func`2[[System.Int32, "+DotNetAssembly+@"],[System.Int32, "+DotNetAssembly+@"]], "+DotNetAssembly+@"
  Name                     = <null>
  ReturnType               = (TypeInfo): System.Int32, "+DotNetAssembly+@"
  Parameters               = "+ReadOnlyCollection+@"<ParameterExpression>[1]: ("+RoCollectionNamespace+"."+ReadOnlyCollection+@"`1[[System.Linq.Expressions.ParameterExpression, "+LinqAssembly+@"]], "+DotNetAssembly+@")
    PrimitiveParameterExpression<int> (System.Linq.Expressions.PrimitiveParameterExpression`1[[System.Int32, "+DotNetAssembly+@"]], "+LinqAssembly+@"):
      NodeType                 = ExpressionType.Parameter
      Type                     = (TypeInfo): System.Int32, "+DotNetAssembly+@"
      Name                     = a
      IsByRef                  = False
      NodeType                 = ExpressionType.Parameter
      Type                     = (TypeInfo): System.Int32, "+DotNetAssembly+@"
      CanReduce                = False
  Body                     = SimpleBinaryExpression (System.Linq.Expressions.SimpleBinaryExpression, "+LinqAssembly+@"):
    NodeType                 = ExpressionType.Add
    Type                     = (TypeInfo): System.Int32, "+DotNetAssembly+@"
    Left                     = SimpleBinaryExpression (System.Linq.Expressions.SimpleBinaryExpression, "+LinqAssembly+@"):
      NodeType                 = ExpressionType.Multiply
      Type                     = (TypeInfo): System.Int32, "+DotNetAssembly+@"
      Left                     = ConstantExpression (System.Linq.Expressions.ConstantExpression, "+LinqAssembly+@"):
        NodeType                 = ExpressionType.Constant
        Type                     = (TypeInfo): System.Int32, "+DotNetAssembly+@"
        Value                    = 3
        NodeType                 = ExpressionType.Constant
        CanReduce                = False
      Right                    = PrimitiveParameterExpression<int> (see ""PrimitiveParameterExpression`1--1"" above)
      IsLiftedLogical          = False
      IsReferenceComparison    = False
      NodeType                 = ExpressionType.Multiply
      Type                     = (TypeInfo): System.Int32, "+DotNetAssembly+@"
      Method                   = <null>
      Conversion               = <null>
      IsLifted                 = False
      IsLiftedToNull           = False
      CanReduce                = False
    Right                    = ConstantExpression (System.Linq.Expressions.ConstantExpression, "+LinqAssembly+@"):
      NodeType                 = ExpressionType.Constant
      Type                     = (TypeInfo): System.Int32, "+DotNetAssembly+@"
      Value                    = 5
      NodeType                 = ExpressionType.Constant
      CanReduce                = False
    IsLiftedLogical          = False
    IsReferenceComparison    = False
    NodeType                 = ExpressionType.Add
    Type                     = (TypeInfo): System.Int32, "+DotNetAssembly+@"
    Method                   = <null>
    Conversion               = <null>
    IsLifted                 = False
    IsLiftedToNull           = False
    CanReduce                = False
  NodeType                 = ExpressionType.Lambda";

        const string TestDumpObject6_1_expected = @"
Object6 ("+ObjectTextDumperTestClass+@"+Object6, "+ObjectTextDumperTestAssembly+@"):
  D                        = static ObjectTextDumperTest.TestMethod
  Ex                       = p => (p > 0)
  Property1                = Property1
  Property2                = Property2";

        const string TestDumpObject7_1_expected = @"
Object7 ("+ObjectTextDumperTestClass+@"+Object7, "+ObjectTextDumperTestAssembly+@"):
  Array                    = int[6]: (System.Int32[], "+DotNetAssembly+@")
    0
    1
    2
    3
    4
    5
  Property1                = Property1
  Property2                = Property2";

        const string TestDumpObject7_1_1_expected = @"
Object7_1 ("+ObjectTextDumperTestClass+@"+Object7_1, "+ObjectTextDumperTestAssembly+@"):
  List                     = List<int>[6]: (System.Collections.Generic.List`1[[System.Int32, "+DotNetAssembly+@"]], "+DotNetAssembly+@")
    0
    1
    2
    3
    4
    5
  Property1                = Property1
  Property2                = Property2";

        const string TestDumpObject7_1_2_expected = @"
Object7_1 ("+ObjectTextDumperTestClass+@"+Object7_1, "+ObjectTextDumperTestAssembly+@"):
  List                     = List<int>[18]: (System.Collections.Generic.List`1[[System.Int32, "+DotNetAssembly+@"]], "+DotNetAssembly+@")
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
  Property2                = Property2";

        const string TestDumpObject7_1_3_expected = @"
Object7_1 ("+ObjectTextDumperTestClass+@"+Object7_1, "+ObjectTextDumperTestAssembly+@"):
  List                     = List<int>[18]: (System.Collections.Generic.List`1[[System.Int32, "+DotNetAssembly+@"]], "+DotNetAssembly+@")
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
  Property2                = Property2";

        const string TestDumpObject7_2_expected = @"
Object7 ("+ObjectTextDumperTestClass+@"+Object7, "+ObjectTextDumperTestAssembly+@"):
  Array                    = int[18]: (System.Int32[], "+DotNetAssembly+@")
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
  Property2                = Property2";

        const string TestDumpObject8_1_expected = @"
Object8 ("+ObjectTextDumperTestClass+@"+Object8, "+ObjectTextDumperTestAssembly+@"):
  Array                    = int[6]: (System.Int32[], "+DotNetAssembly+@")
    0
    1
    2
    ... dumped the first 3/6 elements.
  Array2                   = int[6]: (System.Int32[], "+DotNetAssembly+@")
  Array3                   = byte[18]: 00-01-02-03-04-05-00-01-02-03-04-05-00-01-02-03-04-05
  Array4                   = byte[18]: 00-01-02-03-04-05-00-01-02-03... dumped the first 10/18 elements.
  Array5                   = byte[18]: 00-01-02... dumped the first 3/18 elements.
  Array6                   = ArrayList[18]: (System.Collections.ArrayList, "+RuntimeExtensionsAssembly+@")
    0
    1
    2
    ... dumped the first 3/18 elements.
  Property1                = Property1
  Property2                = Property2";

        const string TestDumpObject8_1_1_expected = @"
Object8_1 ("+ObjectTextDumperTestClass+@"+Object8_1, "+ObjectTextDumperTestAssembly+@"):
  List                     = int[6]: (System.Int32[], "+DotNetAssembly+@")
    0
    1
    2
    ... dumped the first 3/6 elements.
  List2                    = int[6]: (System.Int32[], "+DotNetAssembly+@")
  Property1                = Property1
  Property2                = Property2";

        const string TestDumpObject9_1_expected = @"
Object91 ("+ObjectTextDumperTestClass+@"+Object91, "+ObjectTextDumperTestAssembly+@"):
  Object90                 = Object90 ("+ObjectTextDumperTestClass+@"+Object90, "+ObjectTextDumperTestAssembly+@"):
    Flags                    = TestFlags (Two | Four)
    Prop                     = TestEnum.One
  Prop91                   = 0
  Prop92                   = 1
  Prop911                  = 2
  Prop912                  = 3
  Prop913                  = 6
  Prop914                  = 7
  InheritedObject90        = Object90 (see ""Object90--1"" above)
  Prop93                   = 4
  Prop94                   = 5";

        const string TestDumpObject9_1null_expected = @"
Object91 ("+ObjectTextDumperTestClass+@"+Object91, "+ObjectTextDumperTestAssembly+@"):
  Object90                 = <null>
  Prop91                   = 0
  Prop92                   = 1
  Prop911                  = 2
  Prop912                  = 3
  Prop913                  = 6
  Prop914                  = 7
  InheritedObject90        = <null>
  Prop93                   = 4
  Prop94                   = 5";

        const string TestDumpObjectWithDelegates_expected = @"
ObjectWithDelegates ("+ObjectTextDumperTestClass+@"+ObjectWithDelegates, "+ObjectTextDumperTestAssembly+@"):
  DelegateProp0            = <null>
  DelegateProp1            = static Object10.Static
  DelegateProp2            = Object10.Instance
  DelegateProp3            = static ObjectTextDumperTest.TestMethod
  Object10Prop             = Object10 ("+ObjectTextDumperTestClass+@"+Object10, "+ObjectTextDumperTestAssembly+@"):
    Offset                   = 23";

        const string TestDumpObjectWithMyEnumerable_expected = @"
ObjectWithMyEnumerable ("+ObjectTextDumperTestClass+@"+ObjectWithMyEnumerable, "+ObjectTextDumperTestAssembly+@"):
  MyEnumerable             = MyEnumerable ("+ObjectTextDumperTestClass+@"+MyEnumerable, "+ObjectTextDumperTestAssembly+@"):
    Property                 = foo
    MyEnumerable[]: ("+ObjectTextDumperTestClass+@"+MyEnumerable, "+ObjectTextDumperTestAssembly+@")
      0
      1
      3
  Stuff                    = stuff";

        const string TestDumpObjectWithMemberInfos_expected = @"
ObjectWithMemberInfos ("+ObjectTextDumperTestClass+@"+ObjectWithMemberInfos, "+ObjectTextDumperTestAssembly+@"):
  EventInfo                = (Event): EventHandler ObjectWithMembers.Event
  IndexerIntInfo           = (Property): String ObjectWithMembers.this[Int32] { get; }
  IndexerStringInfo        = (Property): String ObjectWithMembers.this[String, Int32] { get;set; }
  MemberInfo               = (Field): Int32 ObjectWithMembers.member
  MemberInfos              = MemberInfo[22]: (System.Reflection.MemberInfo[], "+DotNetAssembly+@")
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
  Type                     = (NestedType): "+ObjectTextDumperTestClass+@"+ObjectWithMembers, "+ObjectTextDumperTestAssembly+@"";

        const string TestDumpNestedObject_expected = @"
NestedItem ("+ObjectTextDumperTestClass+@"+NestedItem, "+ObjectTextDumperTestAssembly+@"):
  Next                     = NestedItem ("+ObjectTextDumperTestClass+@"+NestedItem, "+ObjectTextDumperTestAssembly+@"):
    Next                     = NestedItem ("+ObjectTextDumperTestClass+@"+NestedItem, "+ObjectTextDumperTestAssembly+@"):
      Next                     = NestedItem ("+ObjectTextDumperTestClass+@"+NestedItem, "+ObjectTextDumperTestAssembly+@"):
        Next                     = ...object dump reached the maximum depth level. Use the DumpAttribute.MaxDepth to increase the depth level if needed.
        Property                 = 3
      Property                 = 2
    Property                 = 1
  Property                 = 0";

        const string TestCollectionObject_expected = @"
List<string>[3]: (System.Collections.Generic.List`1[[System.String, "+DotNetAssembly+@"]], "+DotNetAssembly+@")
  one
  two
  three";

        const string TestDictionaryBaseTypes_expected = @"
Dictionary<string, int>[3]: (System.Collections.Generic.Dictionary`2[[System.String, "+DotNetAssembly+@"],[System.Int32, "+DotNetAssembly+@"]], "+DotNetAssembly+@")
{
  [one] = 1
  [two] = 2
  [three] = 3
}";

        const string TestDictionaryBaseTypeAndObject_expected = @"
Dictionary<int, Object4_1>[3]: (System.Collections.Generic.Dictionary`2[[System.Int32, "+DotNetAssembly+@"],["+ObjectTextDumperTestClass+@"+Object4_1, "+ObjectTextDumperTestAssembly+@"]], "+DotNetAssembly+@")
{
  [1] = Object4_1 ("+ObjectTextDumperTestClass+@"+Object4_1, "+ObjectTextDumperTestAssembly+@"):
    Property1                = one
    Property2                = Property2
  [2] = Object4_1 ("+ObjectTextDumperTestClass+@"+Object4_1, "+ObjectTextDumperTestAssembly+@"):
    Property1                = two
    Property2                = Property2
  [3] = Object4_1 ("+ObjectTextDumperTestClass+@"+Object4_1, "+ObjectTextDumperTestAssembly+@"):
    Property1                = three
    Property2                = Property2
}";

        const string TestVirtualProperties_expected = @"
Derived ("+ObjectTextDumperTestClass+@"+Derived, "+ObjectTextDumperTestAssembly+@"):
  IntProperty              = 0
  StringProperty           = StringProperty
  IntProperty              = 5";

        const string TestDumpOfDynamic_expected = @"
<>f__AnonymousType0<int, string, double> (<>f__AnonymousType0`3[[System.Int32, "+DotNetAssembly+@"],[System.String, "+DotNetAssembly+@"],[System.Double, "+DotNetAssembly+@"]], "+ObjectTextDumperTestAssembly+@"):
  DoubleProperty           = 3.141592653589793
  IntProperty              = 10
  StringProperty           = hello";

        const string TestDumpOfExpando_expected = @"
ExpandoObject[3]: (System.Dynamic.ExpandoObject, System.Linq.Expressions, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a)
{
  IntProperty              = 10
  StringProperty           = hello
  DoubleProperty           = 3.141592653589793
}";

        const string TestDumpClassDumpMethod_expected = @"
Object12 ("+ObjectTextDumperTestClass+@"+Object12, "+ObjectTextDumperTestAssembly+@"):
  Object11Property_1       = Dumped by Objec11Dumper.Dump1: string value
  Object11Property_11      = Dumped by Objec11Dumper.Dump1: string value
  Object11Property_2       = Dumped by Objec11Dumper.Dump: string value
  Object11Property_21      = Dumped by Objec11Dumper.Dump: string value
  Object11Property_3       = Dumped by Objec11.DumpMe: string value
  Object11Property_31      = Dumped by Objec11.DumpMe: string value
  Object11Property_4       = Dumped by Objec11.DumpMeStatic: string value
  Object11Property_41      = Dumped by Objec11.DumpMeStatic: string value
  Object11Property_51      = *** Could not find a public, static, method DumpMeNoneSuch, with return type of System.String, with a single parameter of type "+ObjectTextDumperTestClass+@"+Object11_1 in the class "+ObjectTextDumperTestClass+@"+Object11_1.";

        const string TestOptInDump_expected = @"
Object13 ("+ObjectTextDumperTestClass+@"+Object13, "+ObjectTextDumperTestAssembly+@"):
  Prop2                    = <null>
  Prop3                    = <null>";

        const string TestArrayIndentationCreep_expected = @"
DavidATest ("+ObjectTextDumperTestClass+@"+DavidATest, "+ObjectTextDumperTestAssembly+@"):
  A                        = 10
  Array                    = int[3]: (System.Int32[], "+DotNetAssembly+@")
    1
    2
    3
  B                        = 6";

        const string TestObjectWithNullCollection_expected = @"
Object14 ("+ObjectTextDumperTestClass+@"+Object14, "+ObjectTextDumperTestAssembly+@"):
  Collection               = <null>
  Property11               = 0
  Property12               = <null>";

        const string TestObjectWithNullCollection_expected2 = @"
Object14 ("+ObjectTextDumperTestClass+@"+Object14, "+ObjectTextDumperTestAssembly+@"):
  Collection               = <null>
  Property11               = 0
  Property12               = <null>";

        const string TestObjectWithNotNullCollection_expected = @"
Object14 ("+ObjectTextDumperTestClass+@"+Object14, "+ObjectTextDumperTestAssembly+@"):
  Collection               = List<Object14_1>[2]: (System.Collections.Generic.List`1[["+ObjectTextDumperTestClass+@"+Object14_1, "+ObjectTextDumperTestAssembly+@"]], "+DotNetAssembly+@")
    Object14_1 ("+ObjectTextDumperTestClass+@"+Object14_1, "+ObjectTextDumperTestAssembly+@"):
      Property1                = 0
      Property2                = zero
    Object14_1 ("+ObjectTextDumperTestClass+@"+Object14_1, "+ObjectTextDumperTestAssembly+@"):
      Property1                = 1
      Property2                = one
  Property11               = 1
  Property12               = one.two";

        const string TestObjectWithNotNullCollection_expected2 = @"
Object14 ("+ObjectTextDumperTestClass+@"+Object14, "+ObjectTextDumperTestAssembly+@"):
  Collection               = <null>
  Property11               = 0
  Property12               = <null>";

        const string TestVirtualPropertiesVariations_expected = @"
BaseClass ("+ObjectTextDumperTestClass+@"+BaseClass, "+ObjectTextDumperTestAssembly+@"):
  Property                 = 0
  VirtualProperty1         = 1
  VirtualProperty2         = 2";

        const string TestVirtualPropertiesVariations_expected2 = @"
Descendant1 ("+ObjectTextDumperTestClass+@"+Descendant1, "+ObjectTextDumperTestAssembly+@"):
  Property                 = 0
  VirtualProperty1         = 1
  VirtualProperty2         = 2";

        const string TestVirtualPropertiesVariations_expected3 = @"
Descendant2 ("+ObjectTextDumperTestClass+@"+Descendant2, "+ObjectTextDumperTestAssembly+@"):
  Property                 = 0
  VirtualProperty1         = 21
  VirtualProperty2         = 2";

        const string TestVirtualPropertiesVariations_expected4 = @"
Descendant3 ("+ObjectTextDumperTestClass+@"+Descendant3, "+ObjectTextDumperTestAssembly+@"):
  Property                 = 0
  VirtualProperty1         = 21
  VirtualProperty2         = 32";

        const string TestVirtualPropertiesVariations_expected5 = @"
BaseClass ("+ObjectTextDumperTestClass+@"+BaseClass, "+ObjectTextDumperTestAssembly+@"):
  Property                 = 0
  VirtualProperty1         = 1
  VirtualProperty2         = 2";

        const string TestVirtualPropertiesVariations_expected6 = @"
Descendant1 ("+ObjectTextDumperTestClass+@"+Descendant1, "+ObjectTextDumperTestAssembly+@"):
  Property                 = 10
  VirtualProperty1         = 11
  VirtualProperty2         = 12";

        const string TestVirtualPropertiesVariations_expected7 = @"
Descendant2 ("+ObjectTextDumperTestClass+@"+Descendant2, "+ObjectTextDumperTestAssembly+@"):
  Property                 = 20
  VirtualProperty1         = 21
  VirtualProperty2         = 22";

        const string TestVirtualPropertiesVariations_expected8 = @"
Descendant3 ("+ObjectTextDumperTestClass+@"+Descendant3, "+ObjectTextDumperTestAssembly+@"):
  Property                 = 30
  VirtualProperty1         = 31
  VirtualProperty2         = 32";

        const string TestWrappedByteArray_expected = @"
WrappedByteArray ("+ObjectTextDumperTestClass+@"+WrappedByteArray, "+ObjectTextDumperTestAssembly+@"):
  Bytes                    = byte[8]: 00-00-00-00-00-00-00-00";

        const string TestGenericWithBuddy_expected = @"
GenericWithBuddy<int> ("+ObjectTextDumperTestClass+@"+GenericWithBuddy`1[[System.Int32, "+DotNetAssembly+@"]], "+ObjectTextDumperTestAssembly+@"):
  Property2                = ******";
    }
}

