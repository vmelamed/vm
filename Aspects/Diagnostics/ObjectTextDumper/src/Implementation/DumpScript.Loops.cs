using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace vm.Aspects.Diagnostics.Implementation
{
    partial class DumpScript
    {
        internal static Expression ForEachInDictionary(
            ParameterExpression dictionaryEntry,
            Expression dictionary,
            Expression body,
            LabelTarget? @break = null)
        {
            @break ??= Expression.Label();

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
                        @break)
                    ));
        }

        internal static Expression ForEachInDictionary<TKey, TValue>(
            ParameterExpression key,
            ParameterExpression value,
            Expression dictionary,
            Expression body,
            LabelTarget? @break = null)
        {
            @break ??= Expression.Label();

            var miGetEnumerator = typeof(IEnumerable<KeyValuePair<TKey, TValue>>).GetMethod(nameof(IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator))!;
            var miMoveNext      = typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext))!;
            var piCurrent       = typeof(IEnumerator<KeyValuePair<TKey, TValue>>).GetProperty(nameof(IEnumerator<KeyValuePair<TKey, TValue>>.Current))!;
            var piKey           = typeof(KeyValuePair<TKey, TValue>).GetProperty(nameof(KeyValuePair<TKey, TValue>.Key))!;
            var piValue         = typeof(KeyValuePair<TKey, TValue>).GetProperty(nameof(KeyValuePair<TKey, TValue>.Value))!;

            ParameterExpression enumerator = Expression.Parameter(typeof(IEnumerator<KeyValuePair<TKey, TValue>>), nameof(enumerator));

            return Expression.Block(
                // IDictionaryEnumerator enumerator;
                new[] { enumerator, /*key, value*/ },

                // enumerator = sequence.GetEnumerator();
                Expression.Assign(enumerator, Expression.Call(dictionary, miGetEnumerator)),

                // while (enumerator.MoveNext()) {
                Expression.Loop(
                    Expression.Block(
                        Expression.IfThen(
                            Expression.Not(Expression.Call(enumerator, miMoveNext)),
                            Expression.Break(@break)),

                        // key = enumerator.Current.Key;
                        // value = enumerator.Current.Value;
                        Expression.Assign(key, Expression.Property(Expression.Property(enumerator, piCurrent), piKey)),
                        Expression.Assign(value, Expression.Property(Expression.Property(enumerator, piCurrent), piValue)),

                        // execute the body of the loop;
                        body
                    ),
                    @break)
                );
        }

        internal static Expression ForEachInEnumerable(
            ParameterExpression entry,
            Expression sequence,
            Expression body,
            LabelTarget? @break = null)
        {
            @break ??= Expression.Label();

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
                            @break)
                        );
        }

        internal static Expression ForEachInEnumerable<T>(
            ParameterExpression entry,
            Expression sequence,
            Expression body,
            LabelTarget? @break = null)
        {
            @break ??= Expression.Label();

            var miGetEnumerator = typeof(ICollection<T>).GetMethod(nameof(IEnumerable<T>.GetEnumerator))!;
            var miMoveNext      = typeof(IEnumerable<T>).GetMethod(nameof(IEnumerator<T>.MoveNext))!;
            var piCurrent       = typeof(IEnumerable<T>).GetProperty(nameof(IEnumerator<T>.Current))!;

            ParameterExpression enumerator = Expression.Parameter(typeof(IEnumerator<T>), nameof(enumerator));

            return Expression.Block(
                // IDictionaryEnumerator enumerator;
                new[] { enumerator, entry },

                // enumerator = sequence.GetEnumerator();
                Expression.Assign(enumerator, Expression.Call(sequence, miGetEnumerator)),

                // while (enumerator.MoveNext()) {
                Expression.Loop(
                    Expression.Block(
                        Expression.IfThen(
                            Expression.Not(Expression.Call(enumerator, miMoveNext)),
                            Expression.Break(@break)),

                        // item = enumerator.Current;
                        Expression.Assign(entry, Expression.Convert(Expression.Property(enumerator, piCurrent), typeof(T))),

                        // execute the body of the loop;
                        body
                    ),
                    @break)
                );
        }
    }
}