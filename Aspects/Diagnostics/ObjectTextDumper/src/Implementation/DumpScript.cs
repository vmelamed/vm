using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

using vm.Aspects.Diagnostics.Properties;

namespace vm.Aspects.Diagnostics.Implementation
{
    partial class DumpScript
    {
        // parameters to the dump script:
        readonly ParameterExpression _instance;
        readonly ParameterExpression _instanceType            = Expression.Parameter(typeof(Type),              nameof(_instanceType));
        readonly ParameterExpression _instanceAsObject        = Expression.Parameter(typeof(object),            nameof(_instanceAsObject));
        readonly ParameterExpression _instanceDumpAttribute   = Expression.Parameter(typeof(DumpAttribute),     nameof(_instanceDumpAttribute));
        readonly ParameterExpression _classDumpMetadata       = Expression.Parameter(typeof(ClassDumpMetadata), nameof(_classDumpMetadata));
        readonly ParameterExpression _dumper                  = Expression.Parameter(typeof(ObjectTextDumper),  nameof(_dumper));
        readonly ParameterExpression _dumpState               = Expression.Parameter(typeof(DumpState),         nameof(_dumpState));
        readonly ParameterExpression _tempBool                = Expression.Parameter(typeof(bool),              nameof(_tempBool));
        readonly ParameterExpression _tempDumpAttribute       = Expression.Parameter(typeof(DumpAttribute),     nameof(_tempDumpAttribute));

        // helpful expressions inside the dump script:
        readonly Expression _writer;
        readonly Expression _indentLevel;
        readonly Expression _indentLength;
        readonly Expression _maxDepth;
        readonly LabelTarget _return = Expression.Label();

        /// <summary>
        /// The script's body expressions.
        /// </summary>
        ICollection<Expression> _script = new List<Expression>();

        /// <summary>
        /// Stack of temporarily saved script fragments
        /// </summary>
        readonly Stack<ICollection<Expression>> _scripts = new Stack<ICollection<Expression>>();

        /// <summary>
        /// The script is closed for adding more expressions to it.
        /// </summary>
        bool _isClosed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DumpScript" /> class.
        /// </summary>
        /// <param name="instanceType">Type of the instance.</param>
        public DumpScript(
            Type instanceType)
        {
            _instance     = Expression.Parameter(instanceType, nameof(_instance));
            _dumpState    = Expression.Parameter(typeof(DumpState), nameof(_dumpState));
            _writer       = Expression.Property(_dumper, _piDumperWriter);
            _indentLevel  = Expression.Field(_dumper, _fiDumperIndentLevel);
            _indentLength = Expression.Field(_dumper, _fiDumperIndentLength);
            _maxDepth     = Expression.Property(_dumper, _piDumperMaxDepth);

            Add
            (
                //// if (ReferenceEqual(_instance,null)) { Writer.Write("<null>"); return; }
                Expression.IfThen
                (
                    Expression.Call(_miReferenceEquals, _instanceAsObject, _null),
                    Expression.Block
                    (
                        Write(Expression.Constant(DumpUtilities.Null, typeof(string))),
                        Expression.Return(_return)
                    )
                ),
                ////_instance              = (<actual instance type>)_instanceAsObject;
                ////_instanceType          = _instance.GetType();
                ////_instanceDumpAttribute = _classDumpMetadata.DumpAttribute;
                ////_dumpState             = dumpState;
                Expression.Assign(_instance, Expression.Convert(_instanceAsObject, instanceType)),
                Expression.Assign(_instanceType, Expression.Call(_instance, _miGetType)),
                Expression.Assign(_instanceDumpAttribute, Expression.PropertyOrField(_classDumpMetadata, nameof(ClassDumpMetadata.DumpAttribute))),
                Expression.Assign(_tempDumpAttribute, Expression.PropertyOrField(_classDumpMetadata, nameof(ClassDumpMetadata.DumpAttribute)))
            );
        }

        public Expression<Script> GetScriptExpression()
        {
            Close();
            var lambda = Expression.Lambda<Script>(
                            Expression.Block
                            (
                                new ParameterExpression[] { _instance, _instanceType, _instanceDumpAttribute, _tempBool, _tempDumpAttribute },
                                _script
                            ),
                            new ParameterExpression[] { _instanceAsObject, _classDumpMetadata, _dumper, _dumpState });

            Debug.WriteLine(lambda.DumpCSharpText());

            return lambda;
        }

        public Script Compile() =>
            GetScriptExpression().Compile();

        void BeginScriptSegment()
        {
            _scripts.Push(_script);
            _script = new List<Expression>();
        }

        ICollection<Expression> EndScriptSegment()
        {
            var segment = _script;

            _script = _scripts.Pop();
            return segment;
        }

        Expression MemberValue(MemberInfo mi) => Expression.MakeMemberAccess(_instance, mi);

        //// Writer.Indent(++_dumper._indentLevel, _dumper._indentLength);
        Expression Indent() => Expression.Call(_dumper, _miIndent);

        //// Writer.Indent(--_dumper._indentLevel, _dumper._indentLength);
        Expression Unindent() => Expression.Call(_dumper, _miUnindent);

        //// _dumper._maxDepth--;
        Expression DecrementMaxDepth() => Expression.PostDecrementAssign(_maxDepth);

        //// _dumper._maxDepth++;
        Expression IncrementMaxDepth() => Expression.PostIncrementAssign(_maxDepth);

        ////_dumper.Writer.Write(
        ////    DumpFormat.CyclicalReference,
        ////    type.GetTypeName(),
        ////    type.Namespace,
        ////    type.AssemblyQualifiedName);
        Expression DumpSeenAlready() =>
            Write(
                DumpFormat.CyclicalReference,
                Expression.Call(_miGetTypeName, _instanceType, _false),
                Expression.Property(_instanceType, _piNamespace),
                Expression.Property(_instanceType, _piAssemblyQualifiedName));

        // ============= Dumping types:

        ////_dumper.Writer.Write(
        ////    DumpFormat.Type,
        ////    type.GetTypeName(),
        ////    type.Namespace,
        ////    type.AssemblyQualifiedName);
        Expression DumpType(
            Expression type) =>
            Write(
                DumpFormat.Type,
                Expression.Call(_miGetTypeName, type, _false),
                Expression.Property(type, _piNamespace),
                Expression.Property(type, _piAssemblyQualifiedName));

        ////_dumper.Writer.Write(
        ////    DumpFormat.SequenceType,
        ////    sequenceType.GetTypeName(),
        ////    sequenceType.Namespace,
        ////    sequenceType.AssemblyQualifiedName);
        Expression DumpSequenceType(
            Expression sequenceType) =>
            Write(
                DumpFormat.SequenceType,
                Expression.Call(_miGetTypeName, sequenceType, _false),
                Expression.Property(sequenceType, _piNamespace),
                Expression.Property(sequenceType, _piAssemblyQualifiedName));

        Expression DumpSequenceTypeName(
            Expression sequence,
            Expression sequenceType) =>
            Write(
                DumpFormat.SequenceTypeName,
                Expression.Call(_miGetTypeName, sequenceType, _false),
                Expression.Call(
                    sequence.Type.IsArray
                        ? Expression.Property(sequence, _piArrayLength)
                        : Expression.Property(sequence, _piCollectionCount),
                    _miIntToString1,
                    Expression.Constant(CultureInfo.InvariantCulture)));

        ////_dumper.Writer.Write(
        ////    DumpFormat.SequenceType,
        ////    typeof(Expando),
        ////    typeof(Expando).Namespace,
        ////    typeof(Expando).AssemblyQualifiedName);
        Expression DumpExpandoType() =>
            Write(
                DumpFormat.SequenceType,
                Expression.Call(_miGetTypeName, _typeofExpando, _false),
                Expression.Property(_typeofExpando, _piNamespace),
                Expression.Property(_typeofExpando, _piAssemblyQualifiedName));

        //// writer.Write(
        ////    DumpFormat.SequenceTypeName,
        ////    nameof(ExpandoObject),
        ////    expando.Count);
        Expression DumpExpandoTypeName(Expression expando) =>
            Write(
                DumpFormat.SequenceTypeName,
                Expression.Constant(nameof(ExpandoObject)),
                Expression.Call(
                    Expression.Property(
                        Expression.Convert(expando, ExpandoCollectionType),
                        _piExpandoCount),
                    _miIntToString1,
                    Expression.Constant(CultureInfo.InvariantCulture)));

        Expression DumpExpressionText(
            string cSharpText) =>
            Expression.Block(
                Indent(),
                WriteLine(),
                Write(DumpFormat.CSharpDumpLabel),
                Indent(),
                WriteLine(),
                Write(cSharpText),
                Unindent(),
                Unindent()
            );

        // ==================== Dumping delegates:

        //// _dumper.Writer.Dumped((Delegate)Instance);
        Expression DumpedDelegate(
            Expression @delegate) =>
            Expression.IfThenElse(
                    Expression.Call(_miReferenceEquals, @delegate, _null),
                    Write(_stringNull),
                    Expression.Call(
                            _miDumpedDelegate,
                            _writer,
                            Expression.TypeAs(@delegate, typeof(Delegate)))
                );

        //// _dumper.Writer.Dumped((Delegate)_instance.Property);
        Expression DumpedDelegate(MemberInfo mi) => DumpedDelegate(MemberValue(mi));

        //// _dumper.Writer.Dumped((Delegate)Instance);
        Expression DumpedDelegate() => DumpedDelegate(_instance);

        // ==================== Dumping MemberInfo-s

        Expression DumpedMemberInfo(
            Expression mi) =>
            Expression.IfThenElse(
                Expression.Call(_miReferenceEquals, mi, _null),
                Write(_stringNull),
                Expression.Call(
                    _miDumpedMemberInfo,
                    _writer,
                    Expression.TypeAs(mi, typeof(MemberInfo))));

        //// _dumper.Writer.Dumped(Instance as MemberInfo);
        Expression DumpedMemberInfo(
            MemberInfo mi) => DumpedMemberInfo(MemberValue(mi));

        //// _dumper.Writer.Dumped(Instance as MemberInfo);
        Expression DumpedMemberInfo() => DumpedMemberInfo(_instance);

        // ====================== Dumping basic values:

        Expression DumpedBasicValue(
            MemberInfo mi) =>
            Expression.Call(
                _miDumpedBasicValue,
                _writer,
                Expression.Convert(MemberValue(mi), typeof(object)),
                _tempDumpAttribute);

        Expression DumpedBasicValue() =>
            Expression.Call(
                _miDumpedBasicValue,
                _writer,
                Expression.Convert(_instance, typeof(object)),
                _tempDumpAttribute);

        // ====================== Dumping basic values:

        Expression DumpedBasicNullable(
            MemberInfo mi) =>
            Expression.Call(
                _miDumpedBasicNullable,
                _writer,
                Expression.Convert(MemberValue(mi), typeof(object)),
                _tempDumpAttribute);

        //Expression DumpedBasicNullable()
        //    => Expression.Call(
        //                _miDumpedBasicNullable,
        //                _writer,
        //                Expression.Convert(_instance, typeof(object)),
        //                _tempDumpAttribute);

        // ===================

        Expression CustomDumpPropertyOrField(
            MemberInfo mi,
            MethodInfo? dumpMethod) =>
            Expression.Call(
                _writer,
                _miWrite1,
                dumpMethod is null
                    ? Expression.Call(MemberValue(mi), _miToString)
                    : dumpMethod.IsStatic
                        ? Expression.Call(dumpMethod, MemberValue(mi))
                        : Expression.Call(MemberValue(mi), dumpMethod));

        // ===============

        Expression DumpedExpando(
            Expression expando,
            DumpAttribute dumpAttribute)
        {
            BeginScriptSegment();

            //// writer.Write(DumpFormat.SequenceTypeName, typeof(ExpandoObject), expando.Count);
            //// writer.Write(DumpFormat.SequenceType, expando.GetTypeName(), expando.Namespace!, expando.AssemblyQualifiedName!)
            AddDumpExpandoTypeName(expando);
            AddDumpExpandoType();

            if (dumpAttribute.RecurseDump==ShouldDump.Skip)
            {
                //// return true;
                _script.Add(Expression.Constant(true));
                return Expression.Block(EndScriptSegment());
            }

            ParameterExpression key         = Expression.Parameter(typeof(string), nameof(key));          // the current key-value item
            ParameterExpression value       = Expression.Parameter(typeof(object), nameof(value));        // the current key-value item
            ParameterExpression left        = Expression.Parameter(typeof(int), nameof(left));            // how many items left to be dumped?
            ParameterExpression max         = Expression.Parameter(typeof(int), nameof(max));             // max items to dump
            ParameterExpression count       = Expression.Parameter(typeof(int), nameof(count));           // count of items

            var @break = Expression.Label();

            _script.Add(
                Expression.Block(

                    //// string key; object? value; var left=0; var max = dumpAttribute.GetMaxToDump(sequence.Count);
                    new[] { key, value, left, max, count, },

                    Expression.Assign(left, _zero),
                    Expression.Assign(count, Expression.Property(
                                                            Expression.Convert(expando, ExpandoCollectionType),
                                                            _piExpandoCount)),
                    Expression.Assign(max, Expression.Call(_miGetMaxToDump, Expression.Constant(dumpAttribute, typeof(DumpAttribute)), count)),

                    //// WriteLine(); Write("{"); Indent();
                    WriteLine(),
                    Write(Resources.DictionaryBegin),
                    Indent(),

                    //// foreach (kv in expandoCollection)
                    ForEachInDictionary<string, object?>
                    (
                        key,
                        value,
                        expando,
                        //// {
                        Expression.Block
                        (
                            //// { Writer.WriteLine();
                            WriteLine(),

                            //// if (n++ >= max) {
                            Expression.IfThen
                            (
                                Expression.GreaterThanOrEqual(Expression.PostDecrementAssign(left), max),
                                Expression.Block
                                (
                                    //// Writer.Write(DumpFormat.SequenceDumpTruncated, max);
                                    Write(DumpFormat.SequenceDumpTruncated, Expression.Convert(max, typeof(object)), Expression.Convert(count, typeof(object))),
                                    //// break; }
                                    Expression.Break(@break)
                                )
                            ),

                            //// Writer.Write(kv.Key);
                            Write(Expression.Constant(dumpAttribute.LabelFormat), key),

                            //// _dumper.DumpObject(kv.Value);
                            Expression.Call(
                                _dumper,
                                _miDumperDumpObject,
                                value,
                                _nullType,
                                _nullDumpAttribute,
                                _dumpState)
                        //// }
                        ),
                        @break
                    ),

                    Unindent(),
                    WriteLine(),
                    Write(Resources.DictionaryEnd),

                    //// return true; }
                    Expression.Assign(_tempBool, Expression.Constant(true))
                )
            );

            return Expression.Block(EndScriptSegment());
        }

        // ===============

        Expression DumpedDictionary(
            Expression dictionary,
            DumpAttribute dumpAttribute)
        {
            //// dictionary.GetType()
            var dictionaryType = Expression.Call(dictionary, _miGetType);

            BeginScriptSegment();

            //// writer.Write(DumpFormat.SequenceTypeName, dictionaryType.GetTypeName(), dictionary.Count.ToString(CultureInfo.InvariantCulture));
            //// writer.Write(DumpFormat.SequenceType, dictionaryType.GetTypeName(), dictionaryType.Namespace, dictionaryType.AssemblyQualifiedName);
            AddDumpSequenceTypeName(dictionary, dictionaryType);
            AddDumpSequenceType(dictionaryType);

            if (dumpAttribute.RecurseDump==ShouldDump.Skip)
            {
                //// return true;
                _script.Add(Expression.Constant(true));
                return Expression.Block(EndScriptSegment());
            }

            ParameterExpression kv    = Expression.Parameter(typeof(DictionaryEntry), nameof(kv));  // the current key-value item
            ParameterExpression left  = Expression.Parameter(typeof(int), nameof(left));            // how many items left to be dumped?
            ParameterExpression max   = Expression.Parameter(typeof(int), nameof(max));             // max items to dump
            ParameterExpression count = Expression.Parameter(typeof(int), nameof(count));           // count of items

            var @break = Expression.Label();

            _script.Add
            (
                //// if (ReferenceEqual(dictionary,null)) return false; else {
                Expression.IfThenElse
                (
                    Expression.Call(_miReferenceEquals, dictionary, _null),
                    Expression.Block
                    (
                        Expression.Assign(_tempBool, _false),
                        Write(_stringNull)
                    ),
                    Expression.Block
                    (
                        //// var kv; var n=0; var max = dumpAttribute.GetMaxToDump(sequence.Count);
                        new[] { kv, left, max, count },
                        Expression.Assign(left, _zero),
                        Expression.Assign(count, Expression.Property(dictionary, _piCollectionCount)),
                        Expression.Assign(max, Expression.Call(_miGetMaxToDump, Expression.Constant(dumpAttribute, typeof(DumpAttribute)), count)),

                        //// n = 0; WriteLine(); Write("{"); Indent();
                        WriteLine(),
                        Write(Resources.DictionaryBegin),
                        Indent(),

                        //// foreach (kv in dictionary)
                        ForEachInDictionary
                        (
                            kv,
                            dictionary,
                            Expression.Block
                            (
                                //// { Writer.WriteLine();
                                WriteLine(),

                                //// if (n++ >= max) {
                                Expression.IfThen
                                (
                                    Expression.GreaterThanOrEqual(Expression.PostDecrementAssign(left), max),
                                    Expression.Block
                                    (
                                        //// Writer.Write(DumpFormat.SequenceDumpTruncated, max);
                                        Write(DumpFormat.SequenceDumpTruncated, Expression.Convert(max, typeof(object)), Expression.Convert(count, typeof(object))),
                                        //// break; }
                                        Expression.Break(@break)
                                    )
                                ),

                                //// Writer.Write("[");
                                Write(Resources.DictionaryKeyBegin),
                                //// _dumper.DumpObject(kv.Key);
                                Expression.Call(_dumper, _miDumperDumpObject, Expression.Property(kv, _piDictionaryEntryKey), _nullType, _nullDumpAttribute, _dumpState),
                                // Writer.Write("] = ");
                                Write(Resources.DictionaryKeyEnd),

                                //// _dumper.DumpObject(kv.Value);
                                Expression.Call(_dumper, _miDumperDumpObject, Expression.Property(kv, _piDictionaryEntryValue), _nullType, _nullDumpAttribute, _dumpState)
                            // }
                            ),
                            @break
                        ),

                        Unindent(),
                        WriteLine(),
                        Write(Resources.DictionaryEnd),
                        //// return true; }
                        Expression.Assign(_tempBool, Expression.Constant(true))
                    )
                )
            );

            return Expression.Block(EndScriptSegment());
        }

        Expression DumpedDictionary(
            DumpAttribute dumpAttribute) => DumpedDictionary(_instance, dumpAttribute);

        //// _instance.Property
        Expression DumpedDictionary(
            MemberInfo mi,
            DumpAttribute dumpAttribute) => DumpedDictionary(MemberValue(mi), dumpAttribute);

        // ==============================

        /// <summary>
        /// Dumpeds the collection.
        /// </summary>
        /// <param name="sequence">The sequence.</param>
        /// <param name="dumpAttribute">The dump attribute.</param>
        /// <param name="expressionCount">The expression count.</param>
        /// <returns></returns>
        Expression DumpedCollection(
            Expression sequence,
            DumpAttribute dumpAttribute,
            Expression? expressionCount = null)
        {
            ParameterExpression sequenceType = Expression.Parameter(typeof(Type), nameof(sequenceType));    // the type of the sequence

            ////var elementsType = sequenceType.IsArray
            ////                        ? new Type[] { sequenceType.GetElementType() }
            ////                        : sequenceType.IsGenericType
            ////                            ? sequenceType.GetGenericArguments()
            ////                            : new Type[] { typeof(object) };
            var elementsType = Expression.Condition(
                                            Expression.Property(sequenceType, _piIsArray),
                                            Expression.NewArrayInit(typeof(Type), Expression.Call(sequenceType, _miGetElementType)),
                                            Expression.Condition(
                                                Expression.Property(sequenceType, _piIsGenericType),
                                                Expression.Call(sequenceType, _miGetGenericArguments),
                                                Expression.NewArrayInit(typeof(Type), Expression.Constant(typeof(object)))));
            var truncatedCount = expressionCount is not null
                                    ? (Expression)Expression.Convert(expressionCount, typeof(object))
                                    : Expression.Constant(DumpUtilities.Unknown);

            BeginScriptSegment();

            Add
            (
                ////writer.Write(
                ////    DumpFormat.SequenceType,
                ////    sequenceType.GetTypeName(),
                ////    sequenceType.Namespace,
                ////    sequenceType.AssemblyQualifiedName);
                Write
                (
                    DumpFormat.SequenceType,
                    Expression.Call(_miGetTypeName, sequenceType, _false),
                    Expression.Property(sequenceType, _piNamespace),
                    Expression.Property(sequenceType, _piAssemblyQualifiedName)
                )
            );

            ParameterExpression left = Expression.Parameter(typeof(int), nameof(left));             // how many items are left to be dumped?
            ParameterExpression max  = Expression.Parameter(typeof(int), nameof(max));              // max items to dump

            if (dumpAttribute.RecurseDump != ShouldDump.Skip)
            {
                ParameterExpression item = Expression.Parameter(typeof(object), nameof(item));          // the iteration variable

                var @break = Expression.Label();

                Add
                (
                    ////indent();
                    Indent(),
                    ////foreach (var item in sequence)
                    ForEachInEnumerable
                    (
                        item,
                        sequence,
                        Expression.Block
                        (
                            //// writer.WriteLine();
                            WriteLine(),
                            //// if (n++ >= max)
                            Expression.IfThen
                            (
                                Expression.GreaterThanOrEqual(Expression.PostIncrementAssign(left), max),
                                Expression.Block
                                (
                                    //// {
                                    ////    writer.Write(DumpFormat.SequenceDumpTruncated, max);
                                    Write(DumpFormat.SequenceDumpTruncated, Expression.Convert(max, typeof(object)), truncatedCount),
                                    ////    break;
                                    Expression.Break(@break)
                                //// }
                                )
                            ),
                            //// dumpObject(item);
                            Expression.Call(_dumper, _miDumperDumpObject, item, Expression.Constant(null, typeof(Type)), Expression.Constant(null, typeof(DumpAttribute)), _dumpState)
                        ),
                        @break
                    ),
                    ////unindent();
                    Unindent()
                );
            }

            var dumpingLoop = EndScriptSegment();

            ParameterExpression isArray = Expression.Parameter(typeof(bool), nameof(isArray));         // flag that the sequence is an array of items not bytes
            ParameterExpression bytes   = Expression.Parameter(typeof(byte[]), nameof(bytes));         // collection as byte[];

            //// if (ReferenceEqual(collection,null)) Writer.Write("<null>"); else {
            return Expression.Block
            (
                Expression.IfThenElse
                (
                    Expression.Call(_miReferenceEquals, sequence, _null),
                    Expression.Block
                    (
                        Expression.Assign(_tempBool, _false),
                        Write(_stringNull)
                    ),
                    Expression.Block
                    (
                        //// var max = dumpAttribute.GetMaxToDump(sequence.Count); var n = 0; var bytes = collection as byte[]; var count = collection.Count();
                        new[] { sequenceType, left, max, bytes, isArray },

                        //// sequence.GetType()
                        Expression.Assign(sequenceType, Expression.Call(sequence, _miGetType)),
                        Expression.Assign(left, _zero),
                        Expression.Assign(isArray, Expression.Property(sequenceType, _piIsArray)),
                        Expression.Assign(max, Expression.Call(_miGetMaxToDump, _tempDumpAttribute, expressionCount ?? _intMax)),
                        Expression.Assign(bytes, Expression.TypeAs(sequence, typeof(byte[]))),

                        //// if (!(sequenceType.IsArray || sequenceType.IsFromSystem()))
                        Expression.IfThen
                        (
                            Expression.Not(Expression.OrElse
                                           (
                                                Expression.Property(sequenceType, _piIsArray),
                                                Expression.Call(_miIsMatch, sequenceType)
                                           )
                            ),
                            //// WriteLine();
                            WriteLine()
                        ),

                        ////writer.Write(
                        ////    DumpFormat.SequenceTypeName,
                        ////    sequenceType.IsArray
                        ////            ? elementsType[0].GetTypeName()
                        ////            : sequenceType.GetTypeName(),
                        ////    collection != null
                        ////            ? collection.Count.ToString(CultureInfo.InvariantCulture)
                        ////            : string.Empty);
                        Write
                        (
                            DumpFormat.SequenceTypeName,
                            Expression.Condition
                            (
                                isArray,
                                Expression.Call(_miGetTypeName, Expression.ArrayIndex(elementsType, _zero), _false),
                                Expression.Call(_miGetTypeName, sequenceType, _false)
                            ),
                            expressionCount != null
                              ? (Expression)Expression.Condition
                                (
                                    Expression.NotEqual(sequence, _null),
                                    Expression.Call(expressionCount, _miIntToString1, Expression.Constant(CultureInfo.InvariantCulture)),
                                    _empty
                                )
                              : _empty
                        ),

                        ////if (bytes != null)
                        ////{
                        ////    // dump no more than max elements from the sequence:
                        ////    writer.Write(BitConverter.ToString(bytes, 0, max));
                        ////    if (max < bytes.Length)
                        ////        writer.Write(DumpFormat.SequenceDumpTruncated, max);
                        ////}
                        Expression.IfThenElse
                        (
                            Expression.NotEqual(bytes, _null),
                            Expression.Block
                            (
                                Write(Expression.Call(_miBitConverterToString, bytes, _zero, max)),
                                Expression.IfThen
                                (
                                    Expression.LessThan(max, Expression.Property(bytes, _piArrayLength)),
                                    Write(DumpFormat.SequenceDumpTruncated, Expression.Convert(max, typeof(object)), truncatedCount)
                                )
                            ),

                            ////else {
                            Expression.Block(dumpingLoop)
                        //// }
                        ),
                        //// return true; }
                        Expression.Assign(_tempBool, Expression.Constant(true))
                    )
                )
            );
        }

        Expression DumpedCollection(
            DumpAttribute dumpAttribute)
        {
            var piCount = _instance.Type.IsArray
                            ? _piArrayLength
                            : _instance.Type.GetProperty(nameof(ICollection.Count), BindingFlags.Public|BindingFlags.Instance);
            var count = piCount != null
                            ? Expression.Property(_instance, piCount)
                            : null;

            //// _instance
            return DumpedCollection(_instance, dumpAttribute, count);
        }

        Expression DumpedCollection(
            MemberInfo mi,
            DumpAttribute dumpAttribute)
        {
            Expression? count = null;

            var type = (mi as PropertyInfo)?.PropertyType ?? (mi as FieldInfo)?.FieldType;

            if (type is not null)
            {
                if (type.IsArray)
                    count = Expression.Property(MemberValue(mi), _piArrayLength);
                else
                {
                    var piCount = type.GetProperty(nameof(ICollection.Count), BindingFlags.Public|BindingFlags.Instance);

                    if (piCount != null)
                        count = Expression.Property(MemberValue(mi), piCount);
                }
            }

            //// _instance.Property
            return DumpedCollection(MemberValue(mi), dumpAttribute, count);
        }

        // ================================ dump the value of a property

        Expression DumpPropertyOrCollectionValue(
            MemberInfo mi,
            DumpAttribute dumpAttribute)
        {
            var type = (mi as PropertyInfo)?.PropertyType ??
                       (mi as FieldInfo)?.FieldType ??
                       throw new ArgumentException("The parameter must be either field or property info object.", nameof(mi));

            return type.IsBasicType()
                ? DumpedBasicValue(mi)
                : type.IsGenericType  &&
                  type.GetGenericTypeDefinition() == typeof(Nullable<>)  &&
                  type.GetGenericArguments()[0].IsBasicType()
                    ? DumpedBasicNullable(mi)
                    : typeof(Delegate).IsAssignableFrom(type)
                        ? DumpedDelegate(mi)
                        : typeof(MemberInfo).IsAssignableFrom(type)
                            ? DumpedMemberInfo(mi)
                            : typeof(IEnumerable).IsAssignableFrom(type)  &&  (type.IsArray  ||  type.IsFromSystem())
                                ? type.DictionaryTypeArguments().keyType == typeof(void)
                                    ? DumpedCollection(mi, dumpAttribute)
                                    : DumpedDictionary(mi, dumpAttribute)
                                : DumpObject(mi, null, !dumpAttribute.IsDefaultAttribute()
                                                            ? (Expression)_tempDumpAttribute
                                                            : _nullDumpAttribute);
        }

        internal Expression DumpObject(
            MemberInfo mi,
            Type? dumpMetadata,
            Expression dumpAttribute) =>
            Expression.Call(
                _dumper,
                _miDumperDumpObject,
                Expression.Convert(
                    MemberValue(mi),
                    typeof(object)),
                Expression.Constant(dumpMetadata, typeof(Type)),
                dumpAttribute,
                _dumpState);
    }
}
