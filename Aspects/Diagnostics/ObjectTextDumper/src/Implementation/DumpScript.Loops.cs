using System;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace vm.Aspects.Diagnostics.Implementation
{
    partial class DumpScript
    {
        // Collection enumeration methods and properties:
        static readonly PropertyInfo _piCollectionCount       = typeof(ICollection).GetProperty(nameof(ICollection.Count), BindingFlags.Public|BindingFlags.Instance);
        static readonly MethodInfo _miGetEnumerator           = typeof(IEnumerable).GetMethod(nameof(IEnumerable.GetEnumerator), BindingFlags.Public|BindingFlags.Instance, null, Array.Empty<Type>(), null);
        static readonly MethodInfo _miEnumeratorMoveNext      = typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext), BindingFlags.Public|BindingFlags.Instance, null, Array.Empty<Type>(), null);
        static readonly PropertyInfo _piEnumeratorCurrent     = typeof(IEnumerator).GetProperty(nameof(IEnumerator.Current), BindingFlags.Public|BindingFlags.Instance);

        // Dictionary enumeration methods and properties:
        static readonly MethodInfo _miGetDEnumerator          = typeof(IDictionary).GetMethod(nameof(IDictionary.GetEnumerator), BindingFlags.Public|BindingFlags.Instance, null, Array.Empty<Type>(), null);
        static readonly PropertyInfo _piDictionaryEntryKey    = typeof(DictionaryEntry).GetProperty(nameof(DictionaryEntry.Key), BindingFlags.Public|BindingFlags.Instance);
        static readonly PropertyInfo _piDictionaryEntryValue  = typeof(DictionaryEntry).GetProperty(nameof(DictionaryEntry.Value), BindingFlags.Public|BindingFlags.Instance);

        internal static Expression ForEachInDictionary(
            ParameterExpression dictionaryEntry,
            Expression dictionary,
            Expression body,
            LabelTarget @break = null,
            LabelTarget @continue = null)
        {
            if (dictionaryEntry == null)
                throw new ArgumentNullException(nameof(dictionaryEntry));
            if (body == null)
                throw new ArgumentNullException(nameof(body));

            if (@break == null)
                @break = Expression.Label();
            if (@continue == null)
                @continue = Expression.Label();

            ParameterExpression enumerator = Expression.Parameter(typeof(IDictionaryEnumerator), nameof(enumerator));

            return Expression.Block(
                // IDictionaryEnumerator enumerator;
                new[] { enumerator, dictionaryEntry },

                Expression.Block(
                    // enumerator = sequence.GetEnumerator();
                    Expression.Assign(enumerator, Expression.Call(dictionary, _miGetDEnumerator)),

                    // while (enumerator.MoveNext()) {
                    Expression.Loop(
                        Expression.Block(
                            Expression.IfThen(
                                Expression.Not(Expression.Call(enumerator, _miEnumeratorMoveNext)),
                                Expression.Break(@break)),

                            // item = enumerator.Current;
                            Expression.Assign(dictionaryEntry, Expression.Convert(Expression.Property(enumerator, _piEnumeratorCurrent), typeof(DictionaryEntry))),

                            // execute the body of the loop;
                            body
                        ),
                        @break,
                        @continue)
                    ));
        }

        internal static Expression ForEachInEnumerable(
            ParameterExpression entry,
            Expression sequence,
            Expression body,
            LabelTarget @break = null,
            LabelTarget @continue = null)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));
            if (body == null)
                throw new ArgumentNullException(nameof(body));

            if (@break == null)
                @break = Expression.Label();
            if (@continue == null)
                @continue = Expression.Label();

            ParameterExpression enumerator = Expression.Parameter(typeof(IEnumerator), nameof(enumerator));

            return Expression.Block(
                        // IDictionaryEnumerator enumerator;
                        new[] { enumerator, entry },

                        // enumerator = sequence.GetEnumerator();
                        Expression.Assign(enumerator, Expression.Call(sequence, _miGetEnumerator)),
                        // while (enumerator.MoveNext()) {
                        Expression.Loop(
                            Expression.Block(
                                Expression.IfThen(
                                    Expression.Not(Expression.Call(enumerator, _miEnumeratorMoveNext)),
                                    Expression.Break(@break)),

                                // item = enumerator.Current;
                                Expression.Assign(entry, Expression.Property(enumerator, _piEnumeratorCurrent)),

                                // execute the body of the loop;
                                body
                            ),
                            @break,
                            @continue)
                        );
        }
    }
}