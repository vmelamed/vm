using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace vm.Aspects
{
    /// <summary>
    /// Class IListExtensions.
    /// </summary>
    public static class IListExtensions
    {
        /// <summary>
        /// Inserts an item in a sorted list.
        /// </summary>
        /// <typeparam name="T">The type of the list items.</typeparam>
        /// <param name="list">The sorted list to be inserted into.</param>
        /// <param name="item">The item to be inserted.</param>
        /// <param name="comparer">The comparer used to sort the list.</param>
        /// <returns>The list.</returns>
        public static IList<T> InsertSorted<T>(
            this IList<T> list,
            T item,
            IComparer<T> comparer = null)
        {
            Contract.Requires<ArgumentNullException>(list != null, nameof(list));
            Contract.Ensures(Contract.Result<IList<T>>() != null);

            if (comparer == null)
                comparer = Comparer<T>.Default;

            int low = -1;
            int high = list.Count;
            int mid = (low + high) / 2;
            int compareResult = 1;

            // binary search:
            while (low+1 < high)
            {
                compareResult = comparer.Compare(list[mid], item);

                if (compareResult < 0)
                    low = mid;
                else
                    if (compareResult > 0)
                        high = mid;
                    else
                        break;

                mid = (low + high) / 2;
            }

            if (compareResult != 0)
                mid = high;
            else
                // if the list contains several elements with value equal to the value of list[mid],
                // advance mid to the first non-equal element
                while (++mid < list.Count && 
                       comparer.Compare(list[mid], item) == 0)
                    ;

            list.Insert(mid, item);
            return list;
        }
    }
}
