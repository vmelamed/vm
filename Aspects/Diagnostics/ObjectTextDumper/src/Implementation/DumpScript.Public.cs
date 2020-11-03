using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace vm.Aspects.Diagnostics.Implementation
{
    partial class DumpScript
    {
        #region Public methods helpers:
        DumpScript Add(
            Expression expression1)
        {
            if (expression1 == null)
                throw new ArgumentNullException(nameof(expression1));

            _script.Add(expression1);
            return this;
        }

        DumpScript Add(
            Expression expression1,
            Expression expression2)
        {
            if (expression1 == null)
                throw new ArgumentNullException(nameof(expression1));
            if (expression2 == null)
                throw new ArgumentNullException(nameof(expression2));

            _script.Add(expression1);
            _script.Add(expression2);
            return this;
        }

        DumpScript Add(
            Expression expression1,
            Expression expression2,
            Expression expression3)
        {
            if (expression1 == null)
                throw new ArgumentNullException(nameof(expression1));
            if (expression2 == null)
                throw new ArgumentNullException(nameof(expression2));
            if (expression3 == null)
                throw new ArgumentNullException(nameof(expression2));

            _script.Add(expression1);
            _script.Add(expression2);
            _script.Add(expression3);
            return this;
        }

        DumpScript Add(
            params Expression[] expressions)
        {
            if (expressions == null)
                throw new ArgumentNullException(nameof(expressions));
            if (_script == null)
                throw new InvalidOperationException(nameof(_script) + " cannot be null.");

            for (var i = 0; i<expressions.Length; i++)
                _script.Add(expressions[i]);

            return this;
        }

        DumpScript Close()
        {
            if (_scripts.Count > 0)
                throw new InvalidOperationException("Not all scripts were popped from the stack of scripts.");

            if (_isClosed)
                return this;

            _isClosed = true;
            Add
            (
                //// Writer.Unindent(_indentLevel, _indentLength); WriteLine();
                Expression.Call(_miUnindent3, _writer, _indentLevel, _indentLength),
                Expression.Label(_return)
            );

            return this;
        }
        #endregion

        //// Writer.Indent();
        public DumpScript AddIndent()
            => Add(Indent());

        //// Writer.Indent(--_dumper._indentLevel, _dumper._indentLength);
        public DumpScript AddUnindent()
            => Add(Unindent());

        public DumpScript AddDecrementMaxDepth()
            => Add(DecrementMaxDepth());

        public DumpScript AddIncrementMaxDepth()
            => Add(IncrementMaxDepth());

        public DumpScript AddDumpSeenAlready()
            => Add(DumpSeenAlready());

        public DumpScript BeginDumpProperty(
            MemberInfo mi,
            DumpAttribute dumpAttribute)
        {
            if (mi == null)
                throw new ArgumentNullException(nameof(mi));

            Add(Expression.Assign(_tempDumpAttribute, Expression.Constant(dumpAttribute)));
            BeginScriptSegment();
            AddWriteLine();
            AddWrite(
                Expression.Constant(dumpAttribute.LabelFormat),
                Expression.Constant(mi.Name));

            return this;
        }

        public DumpScript EndDumpProperty(
            MemberInfo mi,
            bool dontDumpNulls)
        {
            if (mi == null)
                throw new ArgumentNullException(nameof(mi));

            var blockBody = EndScriptSegment();
            var isReference = true;
            var isNullable = false;

            if (mi is PropertyInfo pi)
            {
                isReference = !pi.PropertyType.IsValueType;
                isNullable  = !isReference && pi.PropertyType.IsGenericType && pi.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) && pi.PropertyType.GetGenericArguments()[0].IsBasicType();
            }
            else
            {
                if (mi is FieldInfo fi)
                {
                    isReference = !fi.FieldType.IsValueType;
                    isNullable  = !isReference && fi.FieldType.IsGenericType && fi.FieldType.GetGenericTypeDefinition() == typeof(Nullable<>) && fi.FieldType.GetGenericArguments()[0].IsBasicType();
                }
            }

            if (isReference)
            {
                Add
                (
                    ////// should we dump a null value of the current property
                    ////if (!(value == null  &&  dontDumpNulls))
                    Expression.IfThen(
                        Expression.Not
                        (
                            Expression.AndAlso
                            (
                                Expression.Constant(dontDumpNulls),
                                Expression.Call(_miReferenceEquals, MemberValue(mi), _null)
                            )
                        ),
                        //// { dumpProperty }
                        Expression.Block
                        (
                            blockBody
                        )
                    )
                );
            }
            else
            if (isNullable)
            {
                Add
                (
                    ////// should we dump a null value of the current property
                    ////if (!(!value.HasValue  &&  dontDumpNulls))
                    Expression.IfThen(
                        Expression.Not
                        (
                            Expression.AndAlso
                            (
                                Expression.Constant(dontDumpNulls),
                                Expression.Call(MemberValue(mi), _miEqualsObject, _null)
                            )
                        ),
                        //// { dumpProperty }
                        Expression.Block
                        (
                            blockBody
                        )
                    )
                );
            }
            else
                Add
                (
                    //// { dumpProperty }
                    Expression.Block
                    (
                        blockBody
                    )
                );

            return this;
        }

        // =============== Add Dumping types:

        ////_dumper.Writer.Write(
        ////    DumpFormat.SequenceType,
        ////    sequenceType.GetTypeName(),
        ////    sequenceType.Namespace,
        ////    sequenceType.AssemblyQualifiedName);
        public DumpScript AddDumpSequenceType(
            Expression type)
            => Add(DumpSequenceType(type));

        public DumpScript AddDumpSequenceTypeName(
            Expression sequence,
            Expression sequenceType)
            => Add(DumpSequenceTypeName(sequence, sequenceType));

        ////_dumper.Writer.Write(
        ////    DumpFormat.Type,
        ////    type.GetTypeName(),
        ////    type.Namespace,
        ////    type.AssemblyQualifiedName);
        public DumpScript AddDumpType(
            Expression type)
            => Add(DumpType(type));

        ////_dumper.Writer.Write(
        ////    DumpFormat.Type,
        ////    _instanceType.GetTypeName(),
        ////    _instanceType.Namespace,
        ////    _instanceType.AssemblyQualifiedName);
        public DumpScript AddDumpType()
            => AddDumpType(_instanceType);

        // ============== Add expression's C# text dump

        public DumpScript AddDumpExpressionText(
            string cSharpText)
            => Add(DumpExpressionText(cSharpText));

        // ============== Add Dumping delegates:

        //// _dumper.Writer.Dumped((Delegate)Instance);
        public DumpScript AddDumpedDelegate()
            => Add(DumpedDelegate());

        //// _dumper.Writer.Dumped((Delegate)_instance.property);
        public DumpScript AddDumpedDelegate(MemberInfo mi) =>
            mi is not null
                ? Add(DumpedDelegate(mi))
                : throw new ArgumentNullException(nameof(mi));

        // ================ Add Dumping MemberInfo-s:

        public DumpScript AddDumpedMemberInfo(MemberInfo mi) =>
            mi is not null
                ? Add(DumpedMemberInfo(mi))
                : throw new ArgumentNullException(nameof(mi));

        public DumpScript AddDumpedMemberInfo() =>
            Add(DumpedMemberInfo());

        // ================ Add Dumping basic values

        public DumpScript AddDumpedBasicValue(MemberInfo mi) =>
            mi is not null
                ? Add(DumpedBasicValue(mi))
                : throw new ArgumentNullException(nameof(mi));

        public DumpScript AddDumpedBasicValue() =>
            Add(DumpedBasicValue());

        // =============== Add Dumping the a property or field with custom method

        public DumpScript AddCustomDumpPropertyOrField(
            MemberInfo mi,
            MethodInfo dumpMethod) =>
            mi is not null
                ? Add(CustomDumpPropertyOrField(mi, dumpMethod))
                : throw new ArgumentNullException(nameof(mi));

        // =========================

        public DumpScript AddDumpedDictionary(DumpAttribute dumpAttribute) =>
            dumpAttribute is not null
                ? Add(DumpedDictionary(Expression.Convert(_instance, typeof(IDictionary)), dumpAttribute))
                : throw new ArgumentNullException(nameof(dumpAttribute));

        public DumpScript AddDumpedDictionary(
            MemberInfo mi,
            DumpAttribute dumpAttribute) =>
            mi is not null && dumpAttribute is not null
                ? Add(DumpedDictionary(mi, dumpAttribute))
                : throw new ArgumentNullException(mi is null ? nameof(mi) : nameof(dumpAttribute));

        // ===================================

        public DumpScript AddDumpedCollection(DumpAttribute dumpAttribute) =>
            dumpAttribute is not null
                ? Add(DumpedCollection(dumpAttribute))
                : throw new ArgumentNullException(nameof(dumpAttribute));

        public DumpScript AddDumpedCollection(
            MemberInfo mi,
            DumpAttribute dumpAttribute) =>
            mi is not null  &&  dumpAttribute is not null
                ? Add(DumpedCollection(mi, dumpAttribute))
                : throw new ArgumentNullException(mi is null ? nameof(mi) : nameof(dumpAttribute));

        // ========================== Add dumping of a property

        public DumpScript AddDumpPropertyOrCollectionValue(
            MemberInfo mi,
            DumpAttribute dumpAttribute)
        {
            if (mi is null)
                throw new ArgumentNullException(nameof(mi));
            if (dumpAttribute is null)
                throw new ArgumentNullException(nameof(dumpAttribute));

            var ex = DumpPropertyOrCollectionValue(mi, dumpAttribute);

            return ex is not null ? Add(ex) : this;
        }

        // ============================

        public DumpScript AddDumpObject(
            MemberInfo mi,
            Type dumpMetadata) =>
            mi is not null  &&  dumpMetadata is not null
                ? Add(DumpObject(mi, dumpMetadata, _tempDumpAttribute))
                : throw new ArgumentNullException(mi is null ? nameof(mi) : nameof(dumpMetadata));
    }
}
