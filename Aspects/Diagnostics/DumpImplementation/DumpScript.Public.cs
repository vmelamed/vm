using System;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace vm.Aspects.Diagnostics.DumpImplementation
{
    partial class DumpScript
    {
        #region Public methods helpers:
        DumpScript AddDebugInfo(
            string callerFile,
            int callerLine)
        {
            Contract.Ensures(Contract.Result<DumpScript>() != null);

            if (!callerFile.IsNullOrWhiteSpace()  &&  callerLine > 0)
                _script.Add(Expression.DebugInfo(Expression.SymbolDocument(callerFile), callerLine, 1, callerLine, 1));
            return this;
        }

        DumpScript Add(
            Expression expression1,
            string callerFile,
            int callerLine)
        {
            Contract.Requires<ArgumentNullException>(expression1 != null, nameof(expression1));
            Contract.Ensures(Contract.Result<DumpScript>() != null);

            AddDebugInfo(callerFile, callerLine);
            _script.Add(expression1);
            return this;
        }

        DumpScript Add(
            Expression expression1,
            Expression expression2,
            string callerFile,
            int callerLine)
        {
            Contract.Requires<ArgumentNullException>(expression1 != null, nameof(expression1));
            Contract.Requires<ArgumentNullException>(expression2 != null, nameof(expression2));
            Contract.Ensures(Contract.Result<DumpScript>() != null);

            AddDebugInfo(callerFile, callerLine);
            _script.Add(expression1);
            AddDebugInfo(callerFile, callerLine);
            _script.Add(expression2);
            return this;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        DumpScript Add(
            Expression expression1,
            Expression expression2,
            Expression expression3,
            string callerFile,
            int callerLine)
        {
            Contract.Requires<ArgumentNullException>(expression1 != null, nameof(expression1));
            Contract.Requires<ArgumentNullException>(expression2 != null, nameof(expression2));
            Contract.Requires<ArgumentNullException>(expression3 != null, nameof(expression2));
            Contract.Ensures(Contract.Result<DumpScript>() != null);

            AddDebugInfo(callerFile, callerLine);
            _script.Add(expression1);
            AddDebugInfo(callerFile, callerLine);
            _script.Add(expression2);
            AddDebugInfo(callerFile, callerLine);
            _script.Add(expression3);
            return this;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        DumpScript Add(
            string callerFile,
            int callerLine,
            params Expression[] expressions)
        {
            Contract.Requires<ArgumentNullException>(expressions != null, nameof(expressions));
            Contract.Ensures(Contract.Result<DumpScript>() != null);

            for (var i = 0; i<expressions.Length; i++)
            {
                AddDebugInfo(callerFile, callerLine);
                _script.Add(expressions[i]);
            }
            return this;
        }

        DumpScript Close(
            string callerFile,
            int callerLine)
        {
            Contract.Ensures(Contract.Result<DumpScript>() != null);

            if (_scripts.Any())
                throw new InvalidOperationException($"Not all scripts were popped from the stack of scripts. {callerFile}: {callerLine}");

            if (_isClosed)
                return this;

            _isClosed = true;
            Add
            (
                //// Writer.Unindent(_indentLevel, _indentLength); WriteLine();
                Expression.Call(_miUnindent3, _writer, _indentLevel, _indentLength),
                Expression.Label(_return),
                callerFile,
                callerLine
            );

            return this;
        }
        #endregion

        public Expression<Script> GetScriptExpression(
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
        {
            Close(callerFile, callerLine);
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

        public Script GetScriptAction()
            => GetScriptExpression().Compile();

        //// Writer.Indent();
        public DumpScript AddIndent(
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
            => Add(Indent(), callerFile, callerLine);

        //// Writer.Indent(--_dumper._indentLevel, _dumper._indentLength);
        public DumpScript AddUnindent(
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
            => Add(Unindent(), callerFile, callerLine);

        public DumpScript AddDecrementMaxDepth(
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
            => Add(DecrementMaxDepth(), callerFile, callerLine);

        public DumpScript AddIncrementMaxDepth(
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
            => Add(IncrementMaxDepth(), callerFile, callerLine);

        public DumpScript AddDumpSeenAlready(
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
            => Add(DumpSeenAlready(), callerFile, callerLine);

        public DumpScript BeginDumpProperty(
            MemberInfo mi,
            DumpAttribute dumpAttribute,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
        {
            Contract.Requires<ArgumentNullException>(mi != null, nameof(mi));

            AddDebugInfo(callerFile, callerLine);
            Add(Expression.Assign(_tempDumpAttribute, Expression.Constant(dumpAttribute)), callerFile, callerLine);
            BeginScriptSegment();
            AddWriteLine();
            AddWrite(
                Expression.Constant(dumpAttribute.LabelFormat),
                Expression.Constant(mi.Name));

            return this;
        }

        public DumpScript EndDumpProperty(
            MemberInfo mi,
            DumpAttribute dumpAttribute,
            ClassDumpData classDumpData,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
        {
            Contract.Requires<ArgumentNullException>(mi != null, nameof(mi));

            var blockBody   = EndScriptSegment();
            var isReference = true;
            var pi          = mi as PropertyInfo;

            if (pi != null)
                isReference = !pi.PropertyType.IsValueType;
            else
            {
                var fi = mi as FieldInfo;

                if (fi != null)
                    isReference = !fi.FieldType.IsValueType;
            }

            if (isReference)
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
                                        Expression.Constant(dumpAttribute.DumpNullValues),
                                        Expression.Constant(ShouldDump.Skip)),

                                    Expression.AndAlso(
                                        Expression.Equal(
                                            Expression.Constant(dumpAttribute.DumpNullValues),
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
                            blockBody
                        )
                    ),
                    callerFile,
                    callerLine
                );
            else
                Add
                (
                    //// { dumpProperty }
                    Expression.Block
                    (
                        blockBody
                    ),
                    callerFile,
                    callerLine
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
            Expression type,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
            => Add(DumpSequenceType(type), callerFile, callerLine);

        public DumpScript AddDumpSequenceTypeName(
            Expression sequence,
            Expression sequenceType,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
            => Add(DumpSequenceTypeName(sequence, sequenceType), callerFile, callerLine);

        ////_dumper.Writer.Write(
        ////    DumpFormat.Type,
        ////    type.GetTypeName(),
        ////    type.Namespace,
        ////    type.AssemblyQualifiedName);
        public DumpScript AddDumpType(
            Expression type,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
            => Add(DumpType(type), callerFile, callerLine);

        ////_dumper.Writer.Write(
        ////    DumpFormat.Type,
        ////    _instanceType.GetTypeName(),
        ////    _instanceType.Namespace,
        ////    _instanceType.AssemblyQualifiedName);
        public DumpScript AddDumpType(
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
            => AddDumpType(_instanceType, callerFile, callerLine);

        // ============== Add Dumping delegates:

        //// _dumper.Writer.Dumped((Delegate)Instance);
        public DumpScript AddDumpedDelegate(
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
            => Add(DumpedDelegate(), callerFile, callerLine);

        //// _dumper.Writer.Dumped((Delegate)_instance.property);
        public DumpScript AddDumpedDelegate(
            MemberInfo mi,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
        {
            Contract.Requires<ArgumentNullException>(mi != null, nameof(mi));

            return Add(DumpedDelegate(mi), callerFile, callerLine);
        }

        // ================ Add Dumping MemberInfo-s:

        public DumpScript AddDumpedMemberInfo(
            MemberInfo mi,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
        {
            Contract.Requires<ArgumentNullException>(mi != null, nameof(mi));

            return Add(DumpedMemberInfo(mi), callerFile, callerLine);
        }

        public DumpScript AddDumpedMemberInfo(
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
            => Add(DumpedMemberInfo(), callerFile, callerLine);

        // ================ Add Dumping basic values

        public DumpScript AddDumpedBasicValue(
            MemberInfo mi,
            DumpAttribute dumpAttribute,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
        {
            Contract.Requires<ArgumentNullException>(mi            != null, nameof(mi));
            Contract.Requires<ArgumentNullException>(dumpAttribute != null, nameof(dumpAttribute));

            return Add(DumpedBasicValue(mi), callerFile, callerLine);
        }

        public DumpScript AddDumpedBasicValue(
            DumpAttribute dumpAttribute,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
        {
            Contract.Requires<ArgumentNullException>(dumpAttribute != null, nameof(dumpAttribute));

            return Add(DumpedBasicValue(), callerFile, callerLine);
        }

        // =============== Add Dumping the a property or field with custom method

        public DumpScript AddCustomDumpPropertyOrField(
            MemberInfo mi,
            MethodInfo dumpMethod,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
        {
            Contract.Requires<ArgumentNullException>(mi != null, nameof(mi));

            return Add(CustomDumpPropertyOrField(mi, dumpMethod), callerFile, callerLine);
        }

        // =========================

        public DumpScript AddDumpedDictionary(
            DumpAttribute dumpAttribute,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
        {
            Contract.Requires<ArgumentNullException>(dumpAttribute != null, nameof(dumpAttribute));

            return Add(DumpedDictionary(Expression.Convert(_instance, typeof(IDictionary)), dumpAttribute), callerFile, callerLine);
        }

        public DumpScript AddDumpedDictionary(
            MemberInfo mi,
            DumpAttribute dumpAttribute,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
        {
            Contract.Requires<ArgumentNullException>(mi != null, nameof(mi));
            Contract.Requires<ArgumentNullException>(dumpAttribute != null, nameof(dumpAttribute));

            return Add(DumpedDictionary(mi, dumpAttribute), callerFile, callerLine);
        }

        // ===================================

        public DumpScript AddDumpedCollection(
            DumpAttribute dumpAttribute,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
        {
            Contract.Requires<ArgumentNullException>(dumpAttribute != null, nameof(dumpAttribute));

            return Add(DumpedCollection(dumpAttribute), callerFile, callerLine);
        }

        public DumpScript AddDumpedCollection(
            MemberInfo mi,
            DumpAttribute dumpAttribute,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
        {
            Contract.Requires<ArgumentNullException>(mi            != null, nameof(mi));
            Contract.Requires<ArgumentNullException>(dumpAttribute != null, nameof(dumpAttribute));

            return Add(DumpedCollection(mi, dumpAttribute), callerFile, callerLine);
        }

        // ========================== Add dumping of a property

        public DumpScript AddDumpPropertyOrCollectionValue(
            MemberInfo mi,
            DumpAttribute dumpAttribute,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
        {
            Contract.Requires<ArgumentNullException>(mi            != null, nameof(mi));
            Contract.Requires<ArgumentNullException>(dumpAttribute != null, nameof(dumpAttribute));

            return Add(DumpPropertyOrCollectionValue(mi, dumpAttribute), callerFile, callerLine);
        }

        // ============================

        public DumpScript AddDumpObject(
            MemberInfo mi,
            Type dumpMetadata,
            [CallerFilePath] string callerFile = null,
            [CallerLineNumber] int callerLine = 0)
        {
            Contract.Requires<ArgumentNullException>(mi != null, nameof(mi));

            return Add(DumpObject(mi, dumpMetadata, _tempDumpAttribute), callerFile, callerLine);
        }
    }
}
