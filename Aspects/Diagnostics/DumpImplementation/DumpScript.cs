using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;

namespace vm.Aspects.Diagnostics.DumpImplementation
{
    partial class DumpScript
    {
        static readonly PropertyInfo _piNamespace             = typeof(Type).GetProperty(nameof(Type.Namespace), BindingFlags.Public|BindingFlags.Instance);
        static readonly PropertyInfo _piAssemblyQualifiedName = typeof(Type).GetProperty(nameof(Type.AssemblyQualifiedName), BindingFlags.Public|BindingFlags.Instance);
        static readonly PropertyInfo _piIsArray               = typeof(Type).GetProperty(nameof(Type.IsArray), BindingFlags.Public|BindingFlags.Instance);
        static readonly PropertyInfo _piIsGenericType         = typeof(Type).GetProperty(nameof(Type.IsGenericType), BindingFlags.Public|BindingFlags.Instance);

        static readonly PropertyInfo _piArrayLength           = typeof(Array).GetProperty(nameof(Array.Length), BindingFlags.Public|BindingFlags.Instance);

        static readonly PropertyInfo _piRecurseDump           = typeof(DumpAttribute).GetProperty(nameof(DumpAttribute.RecurseDump), BindingFlags.Public|BindingFlags.Instance);

        static readonly MethodInfo _miGetElementType          = typeof(Type).GetMethod(nameof(Type.GetElementType), BindingFlags.Public|BindingFlags.Instance, null, new Type[0], null);
        static readonly MethodInfo _miGetGenericArguments     = typeof(Type).GetMethod(nameof(Type.GetGenericArguments), BindingFlags.Public|BindingFlags.Instance, null, new Type[0], null);

        static readonly MethodInfo _miGetType                 = typeof(object).GetMethod(nameof(object.GetType), BindingFlags.Public|BindingFlags.Instance, null, new Type[0], null);
        static readonly MethodInfo _miToString                = typeof(object).GetMethod(nameof(object.ToString), BindingFlags.Public|BindingFlags.Instance, null, new Type[0], null);
        static readonly MethodInfo _miIntToString1            = typeof(int).GetMethod(nameof(int.ToString), BindingFlags.Public|BindingFlags.Instance, null, new Type[] { typeof(IFormatProvider) }, null);

        static readonly MethodInfo _miGetTypeName             = typeof(Extensions).GetMethod(nameof(Extensions.GetTypeName), BindingFlags.NonPublic|BindingFlags.Static, null, new[] { typeof(Type) }, null);
        static readonly MethodInfo _miGetMaxToDump            = typeof(Extensions).GetMethod(nameof(Extensions.GetMaxToDump), BindingFlags.NonPublic|BindingFlags.Static, null, new[] { typeof(DumpAttribute), typeof(int) }, null);

        static readonly MethodInfo _miDispose                 = typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose), BindingFlags.Public|BindingFlags.Instance);

        static readonly MethodInfo _miIndent                  = typeof(ObjectTextDumper).GetMethod(nameof(ObjectTextDumper.Indent), BindingFlags.NonPublic|BindingFlags.Instance, null, new Type[0], null);
        static readonly MethodInfo _miUnindent                = typeof(ObjectTextDumper).GetMethod(nameof(ObjectTextDumper.Unindent), BindingFlags.NonPublic|BindingFlags.Instance, null, new Type[0], null);

        static readonly MethodInfo _miDumperDumpObject        = typeof(ObjectTextDumper).GetMethod(nameof(ObjectTextDumper.DumpObject), BindingFlags.NonPublic|BindingFlags.Instance, null, new Type[] { typeof(object), typeof(Type), typeof(DumpAttribute), typeof(bool), }, null);

        static readonly MethodInfo _miIsMatch                 = typeof(WriterExtensions).GetMethod(nameof(WriterExtensions.IsFromSystem), BindingFlags.Public|BindingFlags.Static, null, new[] { typeof(Type) }, null);
        static readonly MethodInfo _miDumpedBasicValue        = typeof(WriterExtensions).GetMethod(nameof(WriterExtensions.DumpedBasicValue), BindingFlags.Public|BindingFlags.Static, null, new Type[] { typeof(TextWriter), typeof(object), typeof(DumpAttribute) }, null);
        static readonly MethodInfo _miDumpedDelegate          = typeof(WriterExtensions).GetMethod(nameof(WriterExtensions.Dumped), BindingFlags.Public|BindingFlags.Static, null, new Type[] { typeof(TextWriter), typeof(Delegate) }, null);
        static readonly MethodInfo _miDumpedMemberInfo        = typeof(WriterExtensions).GetMethod(nameof(WriterExtensions.Dumped), BindingFlags.Public|BindingFlags.Static, null, new Type[] { typeof(TextWriter), typeof(MemberInfo) }, null);
        static readonly MethodInfo _miDumpedDictionary        = typeof(WriterExtensions).GetMethod(nameof(WriterExtensions.DumpedDictionary), BindingFlags.Public|BindingFlags.Static, null, new Type[] { typeof(TextWriter), typeof(IEnumerable), typeof(DumpAttribute), typeof(Action<object>), typeof(Action), typeof(Action) }, null);
        static readonly MethodInfo _miDumpedSequence          = typeof(WriterExtensions).GetMethod(nameof(WriterExtensions.DumpedCollection), BindingFlags.Public|BindingFlags.Static, null, new Type[] { typeof(TextWriter), typeof(IEnumerable), typeof(DumpAttribute), typeof(bool), typeof(Action<object>), typeof(Action), typeof(Action) }, null);

        static readonly MethodInfo _miBitConverterToString    = typeof(BitConverter).GetMethod(nameof(BitConverter.ToString), BindingFlags.Public|BindingFlags.Static, null, new Type[] { typeof(byte[]), typeof(int), typeof(int) }, null);

        static readonly ConstantExpression _zero              = Expression.Constant(0, typeof(int));
        static readonly ConstantExpression _null              = Expression.Constant(null);
        static readonly ConstantExpression _empty             = Expression.Constant(string.Empty);

        // parameters to the dump script:
        readonly ParameterExpression _instanceBase;
        readonly ParameterExpression _instance;
        readonly ParameterExpression _instanceType;
        readonly ParameterExpression _classDumpData = Expression.Parameter(typeof(ClassDumpData),    nameof(_classDumpData));
        readonly ParameterExpression _dumper        = Expression.Parameter(typeof(ObjectTextDumper), nameof(_dumper));
        readonly ParameterExpression _dumpMetadata  = Expression.Parameter(typeof(Type),             nameof(_dumpMetadata));
        readonly ParameterExpression _dumpAttribute = Expression.Parameter(typeof(DumpAttribute),    nameof(_dumpAttribute));

        // Constants in the dump script. 
        // These constants have structural effect on the script and must be compared for equality before picking a script from the scripts cache.
        readonly ConstantExpression _propertiesBindingFlags;
        readonly ConstantExpression _fieldsBindingFlags;

        // helpful expressions
        readonly Expression _writer;
        readonly Expression _indentLevel;
        readonly Expression _indentLength;
        readonly Expression _maxDepth;

        /// <summary>
        /// The script's body expressions.
        /// </summary>
        readonly ICollection<Expression> _script = new List<Expression>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DumpScript" /> class.
        /// </summary>
        /// <param name="dumper">The dumper.</param>
        /// <param name="instanceType">Type of the instance.</param>
        public DumpScript(
            ObjectTextDumper dumper,
            Type instanceType)
        {
            Contract.Requires<ArgumentNullException>(dumper != null, nameof(dumper));
            Contract.Requires<ArgumentNullException>(instanceType != null, nameof(instanceType));

            _propertiesBindingFlags = Expression.Constant(dumper.PropertiesBindingFlags);
            _fieldsBindingFlags     = Expression.Constant(dumper.FieldsBindingFlags);
            _writer                 = Expression.Property(
                                                    _dumper,
                                                    typeof(ObjectTextDumper)
                                                        .GetProperty(nameof(ObjectTextDumper.Writer), BindingFlags.NonPublic|BindingFlags.Instance));
            _indentLevel            = Expression.Field(
                                                    _dumper,
                                                    typeof(ObjectTextDumper)
                                                        .GetField(nameof(ObjectTextDumper._indentLevel), BindingFlags.NonPublic|BindingFlags.Instance));
            _indentLength           = Expression.Field(
                                                    _dumper,
                                                    typeof(ObjectTextDumper)
                                                        .GetField(nameof(ObjectTextDumper._indentLength), BindingFlags.NonPublic|BindingFlags.Instance));
            _maxDepth               = Expression.Field(
                                                    _dumper,
                                                    typeof(ObjectTextDumper)
                                                        .GetField(nameof(ObjectTextDumper._maxDepth), BindingFlags.NonPublic|BindingFlags.Instance));
            _instanceBase           = Expression.Parameter(typeof(object), nameof(_instanceBase));
            _instance               = Expression.Parameter(instanceType, nameof(_instance));
            _instanceType           = Expression.Parameter(typeof(Type), nameof(instanceType));

            Add(
                ////_instance = (<actual type>)_instanceType; _valueType = _value.GetType();
                Expression.Assign(_instance, Expression.Convert(_instanceBase, instanceType)),
                Expression.Assign(_instanceType, Expression.Call(_instance, _miGetType)),

                //// Writer.Indent(_indentLevel, _indentLength); WriteLine();
                Indent(),
                WriteLine()
            );
        }

        DumpScript Add(
            Expression expression1)
        {
            _script.Add(expression1);
            return this;
        }

        DumpScript Add(
            Expression expression1,
            Expression expression2)
        {
            _script.Add(expression1);
            _script.Add(expression2);
            return this;
        }

        DumpScript Add(
            Expression expression1,
            Expression expression2,
            Expression expression3)
        {
            _script.Add(expression1);
            _script.Add(expression2);
            _script.Add(expression3);
            return this;
        }

        DumpScript Add(
            params Expression[] expressions)
        {
            for (var i = 0; i<expressions.Length; i++)
                _script.Add(expressions[i]);
            return this;
        }

        public Action<object, ClassDumpData, ObjectTextDumper> GetScript()
            => Expression.Lambda<Action<object, ClassDumpData, ObjectTextDumper>>(
                            Expression.Block(
                                new ParameterExpression[] { _instance, _instanceType },
                                _script),
                            new ParameterExpression[] { _instanceBase, _classDumpData, _dumper })
                         .Compile();

        public Expression MemberValue(
            MemberInfo mi)
            => Expression.MakeMemberAccess(_instance, mi);

        //// Writer.Indent(++_dumper._indentLevel, _dumper._indentLength);
        public Expression Indent()
            => Expression.Call(_dumper, _miIndent);

        public DumpScript AddIndent()
            => Add(Indent());

        //// Writer.Indent(--_dumper._indentLevel, _dumper._indentLength);
        public Expression Unindent()
            => Expression.Call(_dumper, _miUnindent);

        //// Writer.Indent(--_dumper._indentLevel, _dumper._indentLength);
        public DumpScript AddUnindent()
            => Add(Unindent());

        //// _dumper._maxDepth--;
        public Expression DecrementMaxDepth()
            => Expression.PostDecrementAssign(_maxDepth);

        public DumpScript AddDecrementMaxDepth()
            => Add(DecrementMaxDepth());

        //// _dumper._maxDepth++;
        public Expression IncrementMaxDepth()
            => Expression.PostIncrementAssign(_maxDepth);

        public DumpScript AddIncrementMaxDepth()
            => Add(IncrementMaxDepth());

        ////_dumper.Writer.Write(
        ////    DumpFormat.CyclicalReference,
        ////    type.GetTypeName(),
        ////    type.Namespace,
        ////    type.AssemblyQualifiedName);
        public DumpScript DumpSeenAlready()
            => AddWrite(
                    DumpFormat.CyclicalReference,
                    Expression.Call(
                        _miGetTypeName,
                        _instanceType),
                    Expression.Property(
                        _instanceType,
                        _piNamespace),
                    Expression.Property(
                        _instanceType,
                        _piAssemblyQualifiedName)
                    );

        ////_dumper.Writer.Write(
        ////    DumpFormat.Type,
        ////    type.GetTypeName(),
        ////    type.Namespace,
        ////    type.AssemblyQualifiedName);
        public DumpScript DumpType(
            Expression type)
            => AddWrite(
                    DumpFormat.Type,
                    Expression.Call(
                        _miGetTypeName,
                        type),
                    Expression.Property(
                        type,
                        _piNamespace),
                    Expression.Property(
                        type,
                        _piAssemblyQualifiedName)
                    );

        ////_dumper.Writer.Write(
        ////    DumpFormat.Type,
        ////    _instanceType.GetTypeName(),
        ////    _instanceType.Namespace,
        ////    _instanceType.AssemblyQualifiedName);
        public DumpScript DumpType() => DumpType(_instanceType);

        //// _dumper.Writer.Dumped((Delegate)Instance);
        public Expression DumpedDelegate(
            Expression expression)
            => Expression.Call(
                        _miDumpedDelegate,
                        _writer,
                        Expression.Convert(
                            expression,
                            typeof(Delegate)));

        public DumpScript AddDumpedDelegate(
            Expression expression)
            => Add(DumpedDelegate(expression));

        //// _dumper.Writer.Dumped((Delegate)Instance);
        public DumpScript DumpedDelegate()
            => Add(DumpedDelegate(_instance));

        //// _dumper.Writer.Dumped((Delegate)_instance.property);
        public DumpScript DumpedDelegate(
            MemberInfo mi)
            => Add(DumpedDelegate(MemberValue(mi)));

        //// _dumper.Writer.Dumped(Instance as MemberInfo);
        public DumpScript DumpedMemberInfo(
            Expression expression)
            => Add(Expression.Call(
                        _miDumpedMemberInfo,
                        _writer,
                        Expression.TypeAs(
                            expression,
                            typeof(MemberInfo))));

        //// _dumper.Writer.Dumped(Instance as MemberInfo);
        public DumpScript DumpedMemberInfo()
            => DumpedMemberInfo(_instance);

        //// _dumper.Writer.Dumped((MemberInfo)_instance.Property);
        public DumpScript DumpedMemberInfo(
            MemberInfo mi)
            => DumpedMemberInfo(MemberValue(mi));

        public DumpScript DumpedBasicValue(
            MemberInfo mi,
            DumpAttribute dumpAttribute)
            => Add(Expression.Call(
                        _miDumpedBasicValue,
                        _writer,
                        Expression.Convert(MemberValue(mi), typeof(object)),
                        Expression.Constant(dumpAttribute)));

        public Expression DumpedBasicValue(
            DumpAttribute dumpAttribute)
            => Expression.Call(
                        _miDumpedBasicValue,
                        _writer,
                        Expression.Convert(_instance, typeof(object)),
                        Expression.Constant(dumpAttribute));

        public DumpScript AddDumpedBasicValue(
            DumpAttribute dumpAttribute)
            => Add(DumpedBasicValue(dumpAttribute));

        public DumpScript WritePropertyOrField(
            MemberInfo mi,
            MethodInfo dumpMethod)
        {
            Contract.Requires<ArgumentNullException>(mi != null, nameof(mi));

            if (dumpMethod == null)
                //// Writer.Write(value.ToString());
                return Add(
                            Expression.Call(
                                _writer,
                                _miWrite1,
                                Expression.Call(
                                    MemberValue(mi),
                                    _miToString)));

            if (dumpMethod.IsStatic)
                //// Writer.Write(dumpMethod(value));
                return Add(Expression.Call(
                        _writer,
                        _miWrite1,
                        Expression.Call(
                            dumpMethod,
                            MemberValue(mi))));
            else
                //// Writer.Write(value.dumpMethod());
                return Add(Expression.Call(
                        _writer,
                        _miWrite1,
                        Expression.Call(
                            MemberValue(mi),
                            dumpMethod)));
        }

        internal DumpScript DumpedDictionary(
            DumpAttribute dumpAttribute)
            => DumpedDictionary(
                        Expression.Convert(_instance, typeof(IDictionary)),             //// (IDictionary)_instance
                        dumpAttribute);

        internal DumpScript DumpedDictionary(
            MemberInfo mi,
            DumpAttribute dumpAttribute)
            => DumpedDictionary(
                        Expression.Convert(MemberValue(mi), typeof(IDictionary)),       //// (IDictionary)_instance.Property
                        dumpAttribute);

        internal DumpScript DumpedDictionary(
            Expression dictionary,
            DumpAttribute dumpAttribute)
        {
            var dictionaryType = Expression.Call(dictionary, _miGetType);               //// dictionary.GetType()

            // Writer.Write(DumpFormat.SequenceTypeName, _instance.Property.GetType(), ((IDictionary)_instance.Property).Count);
            AddWrite(
                DumpFormat.SequenceTypeName,
                Expression.Call(_miGetTypeName, dictionaryType),
                Expression.Call(
                    Expression.Property(dictionary, _piCollectionCount),
                    _miIntToString1,
                    Expression.Constant(CultureInfo.InvariantCulture)));
            DumpType(dictionaryType);

            if (dumpAttribute.RecurseDump==ShouldDump.Skip)
                return this;

            ParameterExpression kv;     // the current key-value item            
            ParameterExpression n;      // how many items left to be dumped?            
            ParameterExpression max;    // max items to dump

            kv  = Expression.Parameter(typeof(DictionaryEntry), nameof(kv));
            n   = Expression.Parameter(typeof(int), nameof(n));
            max = Expression.Parameter(typeof(int), nameof(max));

            var @break = Expression.Label();

            Add(
                Expression.Block
                (
                    //// var kv; var n=0; var max = dumpAttribute.GetMaxToDump(sequence.Count); n = 0; WriteLine(); Write("{"); Indent();
                    new[] { kv, n, max },
                    Expression.Assign(n, _zero),
                    Expression.Assign(max, Expression.Call(_miGetMaxToDump, Expression.Constant(dumpAttribute, typeof(DumpAttribute)), Expression.Property(dictionary, _piCollectionCount))),

                    WriteLine(),
                    Write("{"),
                    Indent(),

                    // foreach (kv in dictionary)
                    ForEachInDictionary(
                        kv,
                        dictionary,
                        Expression.Block
                        (
                            // { Writer.WriteLine();
                            WriteLine(),

                            // if (n++ >= max) {
                            Expression.IfThen(
                                Expression.LessThanOrEqual(Expression.PostDecrementAssign(n), max),
                                Expression.Block(
                                    // Writer.Write(DumpFormat.SequenceDumpTruncated, max);
                                    Write(DumpFormat.SequenceDumpTruncated, Expression.Convert(max, typeof(object))),
                                    // break; }
                                    Expression.Break(@break))),

                            // Writer.Write("[");
                            Write("["),
                            // _dumper.DumpObject(kv.Key);
                            Expression.Call(_dumper, _miDumperDumpObject, Expression.Property(kv, _piDictionaryEntryKey), Expression.Convert(_null, typeof(Type)), Expression.Convert(_null, typeof(DumpAttribute)), Expression.Constant(false)),
                            // Writer.Write("] = ");
                            Write("] = "),

                            // _dumper.Indent();
                            Indent(),
                            // _dumper.DumpObject(kv.Value);
                            Expression.Call(_dumper, _miDumperDumpObject, Expression.Property(kv, _piDictionaryEntryValue), Expression.Convert(_null, typeof(Type)), Expression.Convert(_null, typeof(DumpAttribute)), Expression.Constant(false)),
                            // Writer.Write(";");
                            Write(";"),
                            // _dumper.Unindent(); }
                            Unindent()
                        ),
                        @break),

                        Unindent(),
                        WriteLine(),
                        Write("}")
                    )
            );

            return this;
        }

        internal DumpScript DumpedCollection(
            DumpAttribute dumpAttribute,
            bool enumerateCustom)
            => DumpedCollection(
                        Expression.Convert(_instance, typeof(ICollection)),             //// (ICollection)_instance
                        dumpAttribute,
                        enumerateCustom);

        internal DumpScript DumpedCollection(
            MemberInfo mi,
            DumpAttribute dumpAttribute,
            bool enumerateCustom)
            => DumpedCollection(
                        Expression.Convert(MemberValue(mi), typeof(ICollection)),       //// (IDictionary)_instance.Property
                        dumpAttribute,
                        enumerateCustom);

        internal DumpScript DumpedCollection(
            Expression collection,
            DumpAttribute dumpAttribute,
            bool enumerateCustom)
        {
            var collectionType = Expression.Call(collection, _miGetType);              //// _instance.GetType()

            ////var elementsType = sequenceType.IsArray
            ////                        ? new Type[] { sequenceType.GetElementType() }
            ////                        : sequenceType.IsGenericType
            ////                            ? sequenceType.GetGenericArguments()
            ////                            : new Type[] { typeof(object) };
            var elementsType = Expression.Condition(
                                            Expression.Property(collectionType, _piIsArray),
                                            Expression.NewArrayInit(typeof(Type), Expression.Call(collectionType, _miGetElementType)),
                                            Expression.Condition(
                                                Expression.Property(collectionType, _piIsGenericType),
                                                Expression.Call(collectionType, _miGetGenericArguments),
                                                Expression.NewArrayInit(typeof(Type), Expression.Constant(typeof(object)))));

            ParameterExpression n;      // how many items left to be dumped?            
            ParameterExpression max;    // max items to dump
            ParameterExpression bytes;  // collection as byte[];
            ParameterExpression item;   // the iteration variable

            n     = Expression.Parameter(typeof(int), nameof(n));
            max   = Expression.Parameter(typeof(int), nameof(max));
            bytes = Expression.Parameter(typeof(byte[]), nameof(bytes));
            item  = Expression.Parameter(typeof(object), nameof(item));

            var @break = Expression.Label();

            Add
            (
                Expression.Block
                (
                    //// var n; var max;
                    new[] { n, max, bytes },

                    //// if (!(sequenceType.IsArray || sequenceType.IsFromSystem())) WriteLine();
                    Expression.IfThen(
                        Expression.Not(
                            Expression.AndAlso(
                                Expression.Property(collectionType, _piIsArray),
                                Expression.Call(_miIsMatch, collectionType))),
                        WriteLine()),

                    ////writer.Write(
                    ////    DumpFormat.SequenceTypeName,
                    ////    sequenceType.IsArray
                    ////            ? elementsType[0].GetTypeName()
                    ////            : sequenceType.GetTypeName(),
                    ////    collection != null
                    ////            ? collection.Count.ToString(CultureInfo.InvariantCulture)
                    ////            : string.Empty);
                    Write(
                            DumpFormat.SequenceTypeName,
                            Expression.Condition(
                                Expression.Property(collectionType, _piIsArray),
                                Expression.Call(_miGetTypeName,
                                    Expression.ArrayIndex(elementsType, _zero)),
                                Expression.Call(_miGetTypeName, collectionType)),
                            Expression.Condition(
                                Expression.NotEqual(collection, _null),
                                Expression.Call(
                                    Expression.Property(collection, _piCollectionCount),
                                    _miIntToString1,
                                    Expression.Constant(CultureInfo.InvariantCulture)),
                                _empty)
                    ),

                    //// max = dumpAttribute.GetMaxToDump(sequence.Count); n = 0; bytes = collection as byte[];
                    Expression.Assign(n, _zero),
                    Expression.Assign(max, Expression.Call(_miGetMaxToDump, Expression.Constant(dumpAttribute, typeof(DumpAttribute)), Expression.Property(collection, _piCollectionCount))),
                    Expression.Assign(bytes, Expression.TypeAs(collection, typeof(byte[]))),

                    ////if (bytes != null)
                    ////{
                    ////    // dump no more than max elements from the sequence:
                    ////    writer.Write(BitConverter.ToString(bytes, 0, max));
                    ////    if (max < bytes.Length)
                    ////        writer.Write(DumpFormat.SequenceDumpTruncated, max);
                    ////    return true;
                    ////}
                    Expression.IfThen(
                        Expression.NotEqual(bytes, _null),
                        Expression.Block(
                            Write(Expression.Call(_miBitConverterToString, bytes, _zero, max)),
                            Expression.IfThen(
                                Expression.LessThan(max, Expression.Property(bytes, _piArrayLength)),
                                Write(DumpFormat.SequenceDumpTruncated, Expression.Convert(max, typeof(object)))))
                        ),

                    ////writer.Write(
                    ////    DumpFormat.SequenceType,
                    ////    sequenceType.GetTypeName(),
                    ////    sequenceType.Namespace,
                    ////    sequenceType.AssemblyQualifiedName);
                    Write(
                        DumpFormat.SequenceType,
                        Expression.Call(_miGetTypeName, collectionType),
                        Expression.Property(collectionType, _piNamespace),
                        Expression.Property(collectionType, _piAssemblyQualifiedName)),

                    ////if (dumpAttribute.RecurseDump!=ShouldDump.Skip) {
                    Expression.IfThen(
                        Expression.NotEqual(
                            Expression.Property(Expression.Constant(dumpAttribute), _piRecurseDump),
                            Expression.Constant(ShouldDump.Skip)),

                            ////indent();
                            ////foreach (var item in sequence)
                            ////{
                            ////    writer.WriteLine();
                            ////    if (n++ >= max)
                            ////    {
                            ////        writer.Write(DumpFormat.SequenceDumpTruncated, max);
                            ////        break;
                            ////    }
                            ////    dumpObject(item);
                            ////}
                            ////unindent();
                            Expression.Block
                            (
                                Indent(),
                                ForEachInEnumerable(
                                    item,
                                    collection,
                                    Expression.Block(
                                        WriteLine(),
                                        Expression.IfThen(
                                            Expression.GreaterThanOrEqual(
                                                Expression.PostIncrementAssign(n), max),
                                            Expression.Block(
                                                Write(DumpFormat.SequenceDumpTruncated, Expression.Convert(max, typeof(object))),
                                                Expression.Break(@break))
                                            ),
                                        Expression.Call(_dumper, _miDumperDumpObject, item, Expression.Convert(_null, typeof(Type)), Expression.Convert(_null, typeof(DumpAttribute)), Expression.Constant(false))
                                    ),
                                    @break),
                                Unindent()
                            )
                        )
                ////}
                )
            );

            return this;
        }
    }
}
