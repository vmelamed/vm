﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;

using vm.Aspects.Diagnostics.Properties;

namespace vm.Aspects.Diagnostics.Implementation
{
    partial class DumpScript
    {
        static readonly MethodInfo _miIntToString1            = typeof(int).GetMethod(nameof(int.ToString), BindingFlags.Public|BindingFlags.Instance, null, new Type[] { typeof(IFormatProvider) }, null);

        static readonly MethodInfo _miReferenceEquals         = typeof(object).GetMethod(nameof(object.ReferenceEquals), BindingFlags.Public|BindingFlags.Static, null, new Type[] { typeof(object), typeof(object) }, null);
        static readonly MethodInfo _miGetType                 = typeof(object).GetMethod(nameof(object.GetType), BindingFlags.Public|BindingFlags.Instance, null, Array.Empty<Type>(), null);
        static readonly MethodInfo _miToString                = typeof(object).GetMethod(nameof(object.ToString), BindingFlags.Public|BindingFlags.Instance, null, Array.Empty<Type>(), null);
        static readonly MethodInfo _miEqualsObject            = typeof(object).GetMethod(nameof(object.Equals), BindingFlags.Public|BindingFlags.Instance, null, new Type[] { typeof(object) }, null);

        //static readonly MethodInfo _miDispose                 = typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose), BindingFlags.Public|BindingFlags.Instance);

        static readonly PropertyInfo _piArrayLength           = typeof(Array).GetProperty(nameof(Array.Length), BindingFlags.Public|BindingFlags.Instance);

        static readonly MethodInfo _miBitConverterToString    = typeof(BitConverter).GetMethod(nameof(BitConverter.ToString), BindingFlags.Public|BindingFlags.Static, null, new Type[] { typeof(byte[]), typeof(int), typeof(int) }, null);

        //static readonly PropertyInfo _piRecurseDump           = typeof(DumpAttribute).GetProperty(nameof(DumpAttribute.RecurseDump), BindingFlags.Public|BindingFlags.Instance);

        //static readonly MethodInfo _miDumpNullValues          = typeof(ClassDumpData).GetMethod(nameof(ClassDumpData.DumpNullValues), BindingFlags.Public|BindingFlags.Instance, null, new Type[] { typeof(DumpAttribute) }, null);

        static readonly PropertyInfo _piNamespace             = typeof(Type).GetProperty(nameof(Type.Namespace), BindingFlags.Public|BindingFlags.Instance);
        static readonly PropertyInfo _piAssemblyQualifiedName = typeof(Type).GetProperty(nameof(Type.AssemblyQualifiedName), BindingFlags.Public|BindingFlags.Instance);
        static readonly PropertyInfo _piIsArray               = typeof(Type).GetProperty(nameof(Type.IsArray), BindingFlags.Public|BindingFlags.Instance);
        static readonly PropertyInfo _piIsGenericType         = typeof(Type).GetProperty(nameof(Type.IsGenericType), BindingFlags.Public|BindingFlags.Instance);
        static readonly MethodInfo _miGetElementType          = typeof(Type).GetMethod(nameof(Type.GetElementType), BindingFlags.Public|BindingFlags.Instance, null, Array.Empty<Type>(), null);
        static readonly MethodInfo _miGetGenericArguments     = typeof(Type).GetMethod(nameof(Type.GetGenericArguments), BindingFlags.Public|BindingFlags.Instance, null, Array.Empty<Type>(), null);

        static readonly FieldInfo _fiDumperIndentLevel        = typeof(ObjectTextDumper).GetField(nameof(ObjectTextDumper._indentLevel), BindingFlags.NonPublic|BindingFlags.Instance);
        static readonly FieldInfo _fiDumperIndentLength       = typeof(ObjectTextDumper).GetField(nameof(ObjectTextDumper._indentSize), BindingFlags.NonPublic|BindingFlags.Instance);
        static readonly FieldInfo _fiDumperMaxDepth           = typeof(ObjectTextDumper).GetField(nameof(ObjectTextDumper._maxDepth), BindingFlags.NonPublic|BindingFlags.Instance);
        static readonly PropertyInfo _piDumperWriter          = typeof(ObjectTextDumper).GetProperty(nameof(ObjectTextDumper.Writer), BindingFlags.NonPublic|BindingFlags.Instance);
        static readonly MethodInfo _miIndent                  = typeof(ObjectTextDumper).GetMethod(nameof(ObjectTextDumper.Indent), BindingFlags.NonPublic|BindingFlags.Instance, null, Array.Empty<Type>(), null);
        static readonly MethodInfo _miUnindent                = typeof(ObjectTextDumper).GetMethod(nameof(ObjectTextDumper.Unindent), BindingFlags.NonPublic|BindingFlags.Instance, null, Array.Empty<Type>(), null);
        static readonly MethodInfo _miDumperDumpObject        = typeof(ObjectTextDumper).GetMethod(nameof(ObjectTextDumper.DumpObject), BindingFlags.NonPublic|BindingFlags.Instance, null, new Type[] { typeof(object), typeof(Type), typeof(DumpAttribute), typeof(DumpState) }, null);

        static readonly MethodInfo _miGetTypeName             = typeof(Extensions).GetMethod(nameof(Extensions.GetTypeName), BindingFlags.NonPublic|BindingFlags.Static, null, new[] { typeof(Type), typeof(bool) }, null);
        static readonly MethodInfo _miGetMaxToDump            = typeof(Extensions).GetMethod(nameof(Extensions.GetMaxToDump), BindingFlags.NonPublic|BindingFlags.Static, null, new[] { typeof(DumpAttribute), typeof(int) }, null);

        //static readonly MethodInfo _miIndent3                 = typeof(DumpUtilities).GetMethod(nameof(DumpUtilities.Indent), BindingFlags.Public|BindingFlags.Static, null, new Type[] { typeof(TextWriter), typeof(int), typeof(int) }, null);
        static readonly MethodInfo _miUnindent3               = typeof(DumpUtilities).GetMethod(nameof(DumpUtilities.Unindent), BindingFlags.Public|BindingFlags.Static, null, new Type[] { typeof(TextWriter), typeof(int), typeof(int) }, null);

        static readonly MethodInfo _miIsMatch                 = typeof(WriterExtensions).GetMethod(nameof(WriterExtensions.IsFromSystem), BindingFlags.Public|BindingFlags.Static, null, new[] { typeof(Type) }, null);
        static readonly MethodInfo _miDumpedBasicValue        = typeof(WriterExtensions).GetMethod(nameof(WriterExtensions.DumpedBasicValue), BindingFlags.Public|BindingFlags.Static, null, new Type[] { typeof(TextWriter), typeof(object), typeof(DumpAttribute) }, null);
        static readonly MethodInfo _miDumpedBasicNullable     = typeof(WriterExtensions).GetMethod(nameof(WriterExtensions.DumpedBasicNullable), BindingFlags.Public|BindingFlags.Static, null, new Type[] { typeof(TextWriter), typeof(object), typeof(DumpAttribute) }, null);
        static readonly MethodInfo _miDumpedDelegate          = typeof(WriterExtensions).GetMethod(nameof(WriterExtensions.Dumped), BindingFlags.Public|BindingFlags.Static, null, new Type[] { typeof(TextWriter), typeof(Delegate) }, null);
        static readonly MethodInfo _miDumpedMemberInfo        = typeof(WriterExtensions).GetMethod(nameof(WriterExtensions.Dumped), BindingFlags.Public|BindingFlags.Static, null, new Type[] { typeof(TextWriter), typeof(MemberInfo) }, null);
        //static readonly MethodInfo _miDumpedDictionary        = typeof(WriterExtensions).GetMethod(nameof(WriterExtensions.DumpedDictionary), BindingFlags.Public|BindingFlags.Static, null, new Type[] { typeof(TextWriter), typeof(IEnumerable), typeof(DumpAttribute), typeof(Action<object>), typeof(Action), typeof(Action) }, null);
        //static readonly MethodInfo _miDumpedSequence          = typeof(WriterExtensions).GetMethod(nameof(WriterExtensions.DumpedCollection), BindingFlags.Public|BindingFlags.Static, null, new Type[] { typeof(TextWriter), typeof(IEnumerable), typeof(DumpAttribute), typeof(bool), typeof(Action<object>), typeof(Action), typeof(Action) }, null);

        static readonly ConstantExpression _zero              = Expression.Constant(0, typeof(int));
        static readonly ConstantExpression _intMax            = Expression.Constant(int.MaxValue, typeof(int));
        static readonly ConstantExpression _null              = Expression.Constant(null);
        static readonly ConstantExpression _empty             = Expression.Constant(string.Empty);
        static readonly ConstantExpression _false             = Expression.Constant(false);
        static readonly ConstantExpression _stringNull        = Expression.Constant(Properties.Resources.StringNull);
        //static readonly ConstantExpression _true              = Expression.Constant(true);

        // parameters to the dump script:
        readonly ParameterExpression _instance;
        readonly ParameterExpression _instanceType            = Expression.Parameter(typeof(Type),             nameof(_instanceType));
        readonly ParameterExpression _instanceAsObject        = Expression.Parameter(typeof(object),           nameof(_instanceAsObject));
        readonly ParameterExpression _instanceDumpAttribute   = Expression.Parameter(typeof(DumpAttribute),    nameof(_instanceDumpAttribute));
        readonly ParameterExpression _classDumpData           = Expression.Parameter(typeof(ClassDumpData),    nameof(_classDumpData));
        readonly ParameterExpression _dumper                  = Expression.Parameter(typeof(ObjectTextDumper), nameof(_dumper));
        readonly ParameterExpression _dumpState               = Expression.Parameter(typeof(DumpState),        nameof(_dumpState));
        readonly ParameterExpression _tempBool                = Expression.Parameter(typeof(bool),             nameof(_tempBool));
        readonly ParameterExpression _tempDumpAttribute       = Expression.Parameter(typeof(DumpAttribute),    nameof(_tempDumpAttribute));

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
            if (instanceType == null)
                throw new ArgumentNullException(nameof(instanceType));

            _instance     = Expression.Parameter(instanceType, nameof(_instance));
            _dumpState    = Expression.Parameter(typeof(DumpState), nameof(_dumpState));
            _writer       = Expression.Property(_dumper, _piDumperWriter);
            _indentLevel  = Expression.Field(_dumper, _fiDumperIndentLevel);
            _indentLength = Expression.Field(_dumper, _fiDumperIndentLength);
            _maxDepth     = Expression.Field(_dumper, _fiDumperMaxDepth);

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
                ////_instanceDumpAttribute = _classDumpData.DumpAttribute;
                ////_dumpState             = dumpState;
                Expression.Assign(_instance, Expression.Convert(_instanceAsObject, instanceType)),
                Expression.Assign(_instanceType, Expression.Call(_instance, _miGetType)),
                Expression.Assign(_instanceDumpAttribute, Expression.PropertyOrField(_classDumpData, nameof(ClassDumpData.DumpAttribute))),
                Expression.Assign(_tempDumpAttribute, Expression.PropertyOrField(_classDumpData, nameof(ClassDumpData.DumpAttribute)))
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
                            new ParameterExpression[] { _instanceAsObject, _classDumpData, _dumper, _dumpState });

            Debug.WriteLine(lambda.DumpCSharpText());

            return lambda;
        }

        public Script GetScriptAction() =>
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

        Expression MemberValue(MemberInfo mi) =>
            mi is not null
                ? Expression.MakeMemberAccess(_instance, mi)
                : throw new ArgumentNullException(nameof(mi));

        //// Writer.Indent(++_dumper._indentLevel, _dumper._indentLength);
        Expression Indent() =>
            Expression.Call(_dumper, _miIndent);

        //// Writer.Indent(--_dumper._indentLevel, _dumper._indentLength);
        Expression Unindent() =>
            Expression.Call(_dumper, _miUnindent);

        //// _dumper._maxDepth--;
        Expression DecrementMaxDepth() =>
            Expression.PostDecrementAssign(_maxDepth);

        //// _dumper._maxDepth++;
        Expression IncrementMaxDepth() =>
            Expression.PostIncrementAssign(_maxDepth);

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
            type is not null
                ? Write(
                    DumpFormat.Type,
                    Expression.Call(_miGetTypeName, type, _false),
                    Expression.Property(type, _piNamespace),
                    Expression.Property(type, _piAssemblyQualifiedName))
                : throw new ArgumentNullException(nameof(type));

        ////_dumper.Writer.Write(
        ////    DumpFormat.SequenceType,
        ////    sequenceType.GetTypeName(),
        ////    sequenceType.Namespace,
        ////    sequenceType.AssemblyQualifiedName);
        Expression DumpSequenceType(
            Expression sequenceType) =>
            sequenceType is not null
                ? Write(
                    DumpFormat.SequenceType,
                    Expression.Call(_miGetTypeName, sequenceType, _false),
                    Expression.Property(sequenceType, _piNamespace),
                    Expression.Property(sequenceType, _piAssemblyQualifiedName))
                : throw new ArgumentNullException(nameof(sequenceType));

        Expression DumpSequenceTypeName(
            Expression sequence,
            Expression sequenceType) =>
            sequence is not null && sequenceType is not null
                ? Write(
                    DumpFormat.SequenceTypeName,
                    Expression.Call(_miGetTypeName, sequenceType, _false),
                    Expression.Call(
                        sequence.Type.IsArray
                            ? Expression.Property(sequence, _piArrayLength)
                            : Expression.Property(sequence, _piCollectionCount),
                        _miIntToString1,
                        Expression.Constant(CultureInfo.InvariantCulture)))
                : throw new ArgumentNullException(sequence is null ? nameof(sequence) : nameof(sequenceType));

        Expression DumpExpressionText(
            string cSharpText) =>
            cSharpText.IsNullOrWhiteSpace()
                ? throw new ArgumentException("The argument cannot be null, empty string or consist of whitespace characters only.", nameof(cSharpText))
                : Expression.Block(
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
            @delegate is not null
                ? Expression.IfThenElse(
                        Expression.Call(_miReferenceEquals, @delegate, _null),
                        Write(_stringNull),
                        Expression.Call(
                                _miDumpedDelegate,
                                _writer,
                                Expression.TypeAs(@delegate, typeof(Delegate)))
                    )
                : throw new ArgumentNullException(nameof(@delegate));

        //// _dumper.Writer.Dumped((Delegate)_instance.Property);
        Expression DumpedDelegate(
            MemberInfo mi) =>
            mi is not null
                ? DumpedDelegate(MemberValue(mi))
                : throw new ArgumentNullException(nameof(mi));

        //// _dumper.Writer.Dumped((Delegate)Instance);
        Expression DumpedDelegate() =>
            DumpedDelegate(_instance);

        // ==================== Dumping MemberInfo-s

        Expression DumpedMemberInfo(
            Expression mi) =>
            mi is not null
                ? Expression.IfThenElse(
                        Expression.Call(_miReferenceEquals, mi, _null),
                        Write(_stringNull),
                        Expression.Call(
                            _miDumpedMemberInfo,
                            _writer,
                            Expression.TypeAs(mi, typeof(MemberInfo))))
                : throw new ArgumentNullException(nameof(mi));

        //// _dumper.Writer.Dumped(Instance as MemberInfo);
        Expression DumpedMemberInfo(
            MemberInfo mi) =>
            mi is not null
                ? DumpedMemberInfo(MemberValue(mi))
                : throw new ArgumentNullException(nameof(mi));

        //// _dumper.Writer.Dumped(Instance as MemberInfo);
        Expression DumpedMemberInfo() =>
            DumpedMemberInfo(_instance);

        // ====================== Dumping basic values:

        Expression DumpedBasicValue(
            MemberInfo mi) =>
            mi is not null
                ? Expression.Call(
                        _miDumpedBasicValue,
                        _writer,
                        Expression.Convert(MemberValue(mi), typeof(object)),
                        _tempDumpAttribute)
                : throw new ArgumentNullException(nameof(mi));

        Expression DumpedBasicValue() =>
            Expression.Call(
                        _miDumpedBasicValue,
                        _writer,
                        Expression.Convert(_instance, typeof(object)),
                        _tempDumpAttribute);

        // ====================== Dumping basic values:

        Expression DumpedBasicNullable(
            MemberInfo mi) =>
            mi is not null
                ? Expression.Call(
                        _miDumpedBasicNullable,
                        _writer,
                        Expression.Convert(MemberValue(mi), typeof(object)),
                        _tempDumpAttribute)
                : throw new ArgumentNullException(nameof(mi));

        //Expression DumpedBasicNullable()
        //    => Expression.Call(
        //                _miDumpedBasicNullable,
        //                _writer,
        //                Expression.Convert(_instance, typeof(object)),
        //                _tempDumpAttribute);

        // ===================

        Expression CustomDumpPropertyOrField(
            MemberInfo mi,
            MethodInfo dumpMethod) =>
            mi is not null
                ? Expression.Call(
                    _writer,
                    _miWrite1,
                    dumpMethod is null
                        ? Expression.Call(MemberValue(mi), _miToString)
                        : dumpMethod.IsStatic
                            ? Expression.Call(dumpMethod, MemberValue(mi))
                            : Expression.Call(MemberValue(mi), dumpMethod))
                : throw new ArgumentNullException(nameof(mi));

        // ===============

        Expression DumpedDictionary(
            Expression dictionary,
            DumpAttribute dumpAttribute)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));
            if (dumpAttribute == null)
                throw new ArgumentNullException(nameof(dumpAttribute));

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

            ParameterExpression kv;     // the current key-value item
            ParameterExpression n;      // how many items left to be dumped?
            ParameterExpression max;    // max items to dump
            ParameterExpression count;  // count of items

            kv    = Expression.Parameter(typeof(DictionaryEntry), nameof(kv));
            n     = Expression.Parameter(typeof(int), nameof(n));
            max   = Expression.Parameter(typeof(int), nameof(max));
            count = Expression.Parameter(typeof(int), nameof(count));

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
                        //// var kv; var n=0; var max = dumpAttribute.GetMaxToDump(sequence.Count); n = 0; WriteLine(); Write("{"); Indent();
                        new[] { kv, n, max, count },
                        Expression.Assign(n, _zero),
                        Expression.Assign(count, Expression.Property(dictionary, _piCollectionCount)),
                        Expression.Assign(max, Expression.Call(_miGetMaxToDump, Expression.Constant(dumpAttribute, typeof(DumpAttribute)), count)),

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
                                    Expression.GreaterThanOrEqual(Expression.PostDecrementAssign(n), max),
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
                                Expression.Call(_dumper, _miDumperDumpObject, Expression.Property(kv, _piDictionaryEntryKey), Expression.Convert(_null, typeof(Type)), Expression.Convert(_null, typeof(DumpAttribute)), _dumpState),
                                // Writer.Write("] = ");
                                Write(Resources.DictionaryKeyEnd),

                                //// _dumper.DumpObject(kv.Value);
                                Expression.Call(_dumper, _miDumperDumpObject, Expression.Property(kv, _piDictionaryEntryValue), Expression.Convert(_null, typeof(Type)), Expression.Convert(_null, typeof(DumpAttribute)), _dumpState)
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
            DumpAttribute dumpAttribute) =>
            dumpAttribute is not null
                ? DumpedDictionary(_instance, dumpAttribute)
                //// _instance
                : throw new ArgumentNullException(nameof(dumpAttribute));

        Expression DumpedDictionary(
            MemberInfo mi,
            DumpAttribute dumpAttribute) =>
            mi is not null && dumpAttribute is not null
                ? DumpedDictionary(
                    //// _instance.Property
                    MemberValue(mi),
                    dumpAttribute)
                : throw new ArgumentNullException(mi is null ? nameof(mi) : nameof(dumpAttribute));

        // ==============================

        Expression DumpedCollection(
            Expression sequence,
            DumpAttribute dumpAttribute,
            Expression expressionCount = null)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));
            if (dumpAttribute == null)
                throw new ArgumentNullException(nameof(dumpAttribute));

            ParameterExpression n;              // how many items left to be dumped?
            ParameterExpression max;            // max items to dump
            ParameterExpression item;           // the iteration variable
            ParameterExpression bytes;          // collection as byte[];
            ParameterExpression isArray;        // flag that the sequence is an array of items
            ParameterExpression sequenceType;   // the type of the sequence

            n            = Expression.Parameter(typeof(int), nameof(n));
            max          = Expression.Parameter(typeof(int), nameof(max));
            item         = Expression.Parameter(typeof(object), nameof(item));
            bytes        = Expression.Parameter(typeof(byte[]), nameof(bytes));
            isArray      = Expression.Parameter(typeof(bool), nameof(isArray));
            sequenceType = Expression.Parameter(typeof(Type), nameof(sequenceType));

            var @break = Expression.Label();

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
            var truncatedCount = expressionCount!=null ? (Expression)Expression.Convert(expressionCount, typeof(object)) : Expression.Constant(Resources.StringUnknown);

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

            if (dumpAttribute.RecurseDump != ShouldDump.Skip)
                Add
                (
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
                        ForEachInEnumerable
                        (
                            item,
                            sequence,
                            Expression.Block
                            (
                                WriteLine(),
                                Expression.IfThen
                                (
                                    Expression.GreaterThanOrEqual(Expression.PostIncrementAssign(n), max),
                                    Expression.Block
                                    (
                                        Write(DumpFormat.SequenceDumpTruncated, Expression.Convert(max, typeof(object)), truncatedCount),
                                        Expression.Break(@break)
                                    )
                                ),
                                Expression.Call(_dumper, _miDumperDumpObject, item, Expression.Convert(_null, typeof(Type)), Expression.Convert(_null, typeof(DumpAttribute)), _dumpState)
                            ),
                            @break
                        ),
                        Unindent()
                    )
                );

            var dumpingLoop = EndScriptSegment();

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
                        new[] { sequenceType, n, max, bytes, isArray },

                        //// sequence.GetType()
                        Expression.Assign(sequenceType, Expression.Call(sequence, _miGetType)),
                        Expression.Assign(n, _zero),
                        Expression.Assign(isArray, Expression.Property(sequenceType, _piIsArray)),
                        Expression.Assign(max, Expression.Call(_miGetMaxToDump, _tempDumpAttribute, expressionCount ?? _intMax)),
                        Expression.Assign(bytes, Expression.TypeAs(sequence, typeof(byte[]))),

                        //// if (!(sequenceType.IsArray || sequenceType.IsFromSystem())) WriteLine();
                        Expression.IfThen
                        (
                            Expression.Not
                            (
                                Expression.OrElse
                                (
                                    Expression.Property(sequenceType, _piIsArray),
                                    Expression.Call(_miIsMatch, sequenceType)
                                )
                            ),
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
                            Expression.Block
                            (
                                dumpingLoop
                            )
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
            if (dumpAttribute == null)
                throw new ArgumentNullException(nameof(dumpAttribute));

            var piCount = _instance.Type.IsArray
                            ? _piArrayLength
                            : _instance.Type.GetProperty(nameof(ICollection.Count), BindingFlags.Public|BindingFlags.Instance);
            var count = piCount != null
                            ? Expression.Property(_instance, piCount)
                            : null;

            return DumpedCollection(
                        //// _instance
                        _instance,
                        dumpAttribute,
                        count);
        }

        Expression DumpedCollection(
            MemberInfo mi,
            DumpAttribute dumpAttribute)
        {
            if (mi == null)
                throw new ArgumentNullException(nameof(mi));
            if (dumpAttribute == null)
                throw new ArgumentNullException(nameof(dumpAttribute));

            Expression count = null;

            var type = (mi as PropertyInfo)?.PropertyType ?? (mi as FieldInfo)?.FieldType;

            if (type != null)
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

            return DumpedCollection(
                        //// _instance.Property
                        MemberValue(mi),
                        dumpAttribute,
                        count);
        }

        // ================================ dump the value of a property

        Expression DumpPropertyOrCollectionValue(
            MemberInfo mi,
            DumpAttribute dumpAttribute)
        {
            if (mi == null)
                throw new ArgumentNullException(nameof(mi));
            if (dumpAttribute == null)
                throw new ArgumentNullException(nameof(dumpAttribute));

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
                                ? type.DictionaryTypeArguments() is null
                                    ? DumpedCollection(mi, dumpAttribute)
                                    : DumpedDictionary(mi, dumpAttribute)
                                : DumpObject(mi, null, !dumpAttribute.IsDefaultAttribute()
                                    ? (Expression)_tempDumpAttribute
                                    : Expression.Convert(_null, typeof(DumpAttribute)));
        }

        internal Expression DumpObject(
            MemberInfo mi,
            Type dumpMetadata,
            Expression dumpAttribute) =>
            mi is not null
                ? Expression.Call(
                    _dumper,
                    _miDumperDumpObject,
                    Expression.Convert(
                        MemberValue(mi),
                        typeof(object)),
                    Expression.Constant(dumpMetadata, typeof(Type)),
                    dumpAttribute,
                    _dumpState)
                : throw new ArgumentNullException(nameof(mi));
    }
}
