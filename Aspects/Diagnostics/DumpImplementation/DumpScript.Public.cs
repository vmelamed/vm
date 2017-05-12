using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;

namespace vm.Aspects.Diagnostics.DumpImplementation
{
    partial class DumpScript
    {
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

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
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

        DumpScript Close()
        {
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

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        DumpScript Add(
            params Expression[] expressions)
        {
            Contract.Requires<ArgumentNullException>(expressions != null, nameof(expressions));
            Contract.Requires<ArgumentNullException>(_script     != null, nameof(_script));

            for (var i = 0; i<expressions.Length; i++)
                _script.Add(expressions[i]);
            return this;
        }

        public Expression<Action<object, ClassDumpData, ObjectTextDumper>> GetScriptExpression()
        {
            Close();
            var lambda = Expression.Lambda<Action<object, ClassDumpData, ObjectTextDumper>>(
                            Expression.Block
                            (
                                new ParameterExpression[] { _instance, _instanceType, _instanceDumpAttribute, _tempBool },
                                _script
                            ),
                            new ParameterExpression[] { _instanceAsObject, _classDumpData, _dumper });

            return lambda;
        }

        public Action<object, ClassDumpData, ObjectTextDumper> GetScriptAction()
            => GetScriptExpression().Compile();

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

            BeginScriptSegment();

            AddWriteLine();
            AddWrite(
                Expression.PropertyOrField(Expression.Constant(dumpAttribute), nameof(DumpAttribute.LabelFormat)),
                Expression.Constant(mi.Name));

            return this;
        }

        public DumpScript EndDumpProperty(
            MemberInfo mi,
            DumpAttribute dumpAttribute,
            ClassDumpData classDumpData)
        {
            Contract.Requires<ArgumentNullException>(mi != null, nameof(mi));

            var da = Expression.Constant(dumpAttribute);

            Add
            (
                ////// should we dump a null value of the current property
                ////if (!(value == null  &&
                ////      (CurrentPropertyDumpAttribute.DumpNullValues==ShouldDump.Skip  ||
                ////      CurrentPropertyDumpAttribute.DumpNullValues==ShouldDump.Default && DumpNullValues==ShouldDump.Skip)))
                Expression.IfThen(
                    Expression.Not
                    (
                        Expression.AndAlso
                        (
                            Expression.Call(_miReferenceEquals, MemberValue(mi), _null),
                            Expression.OrElse
                            (
                                Expression.Equal(
                                    Expression.PropertyOrField(da, nameof(DumpAttribute.DumpNullValues)),
                                    Expression.Constant(ShouldDump.Skip)),

                                Expression.AndAlso(
                                    Expression.Equal(
                                        Expression.PropertyOrField(da, nameof(DumpAttribute.DumpNullValues)),
                                        Expression.Constant(ShouldDump.Default)),
                                    Expression.Equal(
                                        Expression.Call(Expression.Constant(classDumpData), _miDumpNullValues, _instanceDumpAttribute),
                                        Expression.Constant(ShouldDump.Default)))
                            )
                        )
                    ),
                    //// { dumpProperty }
                    Expression.Block
                    (
                        EndScriptSegment()
                    )
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

            return Add(DumpedBasicValue(mi, dumpAttribute));
        }

        public DumpScript AddDumpedBasicValue(
            DumpAttribute dumpAttribute)
        {
            Contract.Requires<ArgumentNullException>(dumpAttribute != null, nameof(dumpAttribute));

            return Add(DumpedBasicValue(dumpAttribute));
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

            return Add(DumpedCollection(
                        dumpAttribute));
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

            return Add(DumpPropertyOrCollectionValue(mi, dumpAttribute));
        }

        // ============================

        public DumpScript AddDumpObject(
            MemberInfo mi,
            Type dumpMetadata,
            DumpAttribute dumpAttribute)
        {
            Contract.Requires<ArgumentNullException>(mi != null, nameof(mi));

            return Add(DumpObject(mi, dumpMetadata, dumpAttribute));
        }
    }
}
