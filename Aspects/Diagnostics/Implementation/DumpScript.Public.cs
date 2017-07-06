using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
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
            Contract.Requires<ArgumentNullException>(expression1 != null, nameof(expression1));
            Contract.Ensures(Contract.Result<DumpScript>() != null);

            _script.Add(expression1);
            return this;
        }

        DumpScript Add(
            Expression expression1,
            Expression expression2)
        {
            Contract.Requires<ArgumentNullException>(expression1 != null, nameof(expression1));
            Contract.Requires<ArgumentNullException>(expression2 != null, nameof(expression2));
            Contract.Ensures(Contract.Result<DumpScript>() != null);

            _script.Add(expression1);
            _script.Add(expression2);
            return this;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        DumpScript Add(
            Expression expression1,
            Expression expression2,
            Expression expression3)
        {
            Contract.Requires<ArgumentNullException>(expression1 != null, nameof(expression1));
            Contract.Requires<ArgumentNullException>(expression2 != null, nameof(expression2));
            Contract.Requires<ArgumentNullException>(expression3 != null, nameof(expression2));
            Contract.Ensures(Contract.Result<DumpScript>() != null);

            _script.Add(expression1);
            _script.Add(expression2);
            _script.Add(expression3);
            return this;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        DumpScript Add(
            params Expression[] expressions)
        {
            Contract.Requires<ArgumentNullException>(expressions != null, nameof(expressions));
            Contract.Requires<InvalidOperationException>(_script != null, nameof(_script) + " cannot be null.");
            Contract.Ensures(Contract.Result<DumpScript>() != null);

            for (var i = 0; i<expressions.Length; i++)
            {
                _script.Add(expressions[i]);
            }
            return this;
        }

        DumpScript Close()
        {
            Contract.Ensures(Contract.Result<DumpScript>() != null);

            if (_scripts.Any())
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
            Contract.Requires<ArgumentNullException>(mi != null, nameof(mi));

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
            Contract.Requires<ArgumentNullException>(mi != null, nameof(mi));

            var blockBody   = EndScriptSegment();
            var isReference = true;
            var isNullable  = false;
            var pi          = mi as PropertyInfo;

            if (pi != null)
            {
                isReference = !pi.PropertyType.IsValueType;
                isNullable  = !isReference  &&  pi.PropertyType.IsGenericType  &&  pi.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)  &&  pi.PropertyType.GetGenericArguments()[0].IsBasicType();
            }
            else
            {
                var fi = mi as FieldInfo;

                if (fi != null)
                {
                    isReference = !fi.FieldType.IsValueType;
                    isNullable  = !isReference  &&  fi.FieldType.IsGenericType  &&  fi.FieldType.GetGenericTypeDefinition() == typeof(Nullable<>)  &&  fi.FieldType.GetGenericArguments()[0].IsBasicType();
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
        public DumpScript AddDumpedDelegate(
            MemberInfo mi)
        {
            Contract.Requires<ArgumentNullException>(mi != null, nameof(mi));

            return Add(DumpedDelegate(mi));
        }

        // ================ Add Dumping MemberInfo-s:

        public DumpScript AddDumpedMemberInfo(
            MemberInfo mi)
        {
            Contract.Requires<ArgumentNullException>(mi != null, nameof(mi));

            return Add(DumpedMemberInfo(mi));
        }

        public DumpScript AddDumpedMemberInfo()
            => Add(DumpedMemberInfo());

        // ================ Add Dumping basic values

        public DumpScript AddDumpedBasicValue(
            MemberInfo mi,
            DumpAttribute dumpAttribute)
        {
            Contract.Requires<ArgumentNullException>(mi            != null, nameof(mi));
            Contract.Requires<ArgumentNullException>(dumpAttribute != null, nameof(dumpAttribute));

            return Add(DumpedBasicValue(mi));
        }

        public DumpScript AddDumpedBasicValue(
            DumpAttribute dumpAttribute)
        {
            Contract.Requires<ArgumentNullException>(dumpAttribute != null, nameof(dumpAttribute));

            return Add(DumpedBasicValue());
        }

        // =============== Add Dumping the a property or field with custom method

        public DumpScript AddCustomDumpPropertyOrField(
            MemberInfo mi,
            MethodInfo dumpMethod)
        {
            Contract.Requires<ArgumentNullException>(mi != null, nameof(mi));

            return Add(CustomDumpPropertyOrField(mi, dumpMethod));
        }

        // =========================

        public DumpScript AddDumpedDictionary(
            DumpAttribute dumpAttribute)
        {
            Contract.Requires<ArgumentNullException>(dumpAttribute != null, nameof(dumpAttribute));

            return Add(DumpedDictionary(Expression.Convert(_instance, typeof(IDictionary)), dumpAttribute));
        }

        public DumpScript AddDumpedDictionary(
            MemberInfo mi,
            DumpAttribute dumpAttribute)
        {
            Contract.Requires<ArgumentNullException>(mi != null, nameof(mi));
            Contract.Requires<ArgumentNullException>(dumpAttribute != null, nameof(dumpAttribute));

            return Add(DumpedDictionary(mi, dumpAttribute));
        }

        // ===================================

        public DumpScript AddDumpedCollection(
            DumpAttribute dumpAttribute)
        {
            Contract.Requires<ArgumentNullException>(dumpAttribute != null, nameof(dumpAttribute));

            return Add(DumpedCollection(dumpAttribute));
        }

        public DumpScript AddDumpedCollection(
            MemberInfo mi,
            DumpAttribute dumpAttribute)
        {
            Contract.Requires<ArgumentNullException>(mi            != null, nameof(mi));
            Contract.Requires<ArgumentNullException>(dumpAttribute != null, nameof(dumpAttribute));

            return Add(DumpedCollection(mi, dumpAttribute));
        }

        // ========================== Add dumping of a property

        public DumpScript AddDumpPropertyOrCollectionValue(
            MemberInfo mi,
            DumpAttribute dumpAttribute)
        {
            Contract.Requires<ArgumentNullException>(mi            != null, nameof(mi));
            Contract.Requires<ArgumentNullException>(dumpAttribute != null, nameof(dumpAttribute));

            var ex = DumpPropertyOrCollectionValue(mi, dumpAttribute);

            if (ex != null)
                return Add(ex);

            return this;
        }

        // ============================

        public DumpScript AddDumpObject(
            MemberInfo mi,
            Type dumpMetadata)
        {
            Contract.Requires<ArgumentNullException>(mi != null, nameof(mi));

            return Add(DumpObject(mi, dumpMetadata, _tempDumpAttribute));
        }
    }
}
