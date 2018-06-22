using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace vm.Aspects.Cache.Associative.NWaySet
{
    public partial class NWaySetAssociativeCache<TKey, TValue>
    {
        /// <summary>
        /// Class Enumerator implements the <see cref="IEnumerator{KeyValuePair}"/>.
        /// </summary>
        /// <seealso cref="IEnumerator{KeyValuePair}" />
        class Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDictionaryEnumerator
        {
            NWaySetAssociativeCache<TKey, TValue> _cache;
            int _current = -1;

            /// <summary>
            /// Initializes a new instance of the <see cref="Enumerator"/> class.
            /// </summary>
            /// <param name="cache">The cache that this enumerator is associated with.</param>
            public Enumerator(NWaySetAssociativeCache<TKey, TValue> cache)
            {
                _cache = cache;

                foreach (var set in cache.Sets)
                    set.Lock.EnterReadLock();
            }

            /// <summary>
            /// Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <exception cref="InvalidOperationException">
            /// The enumerator is not at a valid element, call MoveNext or Reset and MoveNext first.
            /// </exception>
            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    if (_current < 0  ||  _current >= _cache.Size)
                        throw new InvalidOperationException("The enumerator is not at a valid element, call MoveNext or Reset and MoveNext first.");

                    Debug.Assert(_cache.Entries[_current].IsUsed);
                    return new KeyValuePair<TKey, TValue>(_cache.Entries[_current].Key, _cache.Entries[_current].Value);
                }
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                foreach (var set in _cache.Sets)
                    set.Lock.ExitReadLock();
            }

            /// <summary>
            /// Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>
            /// <see langword="true" /> if the enumerator was successfully advanced to the next element;
            /// <see langword="false" /> if the enumerator has passed the end of the collection.
            /// </returns>
            public bool MoveNext()
            {
                for (_current++; _current < _cache.Size; ++_current)
                    if (_cache.Entries[_current].IsUsed)
                        return true;

                return false;
            }

            public void Reset()
            {
                _current = -1;
            }

            #region IEnumerator and IDictionaryEnumerator
            object IDictionaryEnumerator.Key => Current.Key;

            object IDictionaryEnumerator.Value => Current.Value;

            DictionaryEntry IDictionaryEnumerator.Entry => new DictionaryEntry(Current.Key, Current.Value);

            bool IEnumerator.MoveNext() => MoveNext();
            void IEnumerator.Reset() => Reset();

            object IEnumerator.Current => Current;
            #endregion
        }
    }
}
