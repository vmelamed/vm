using System;
using System.Collections.Generic;

namespace vm.Aspects.Wcf
{
    /// <summary>
    /// Compares URI-s by speed (schema and host being this host)
    /// </summary>
    /// <seealso cref="IComparable{Uri}" />
    public class UriComparer : IComparer<Uri>
    {
        /// <summary>
        /// Gets the default instance of this class
        /// </summary>
        public static UriComparer Default { get; } = new UriComparer();

        /// <summary>
        /// The speed ratings of the different schemes
        /// </summary>
        static List<string> _schemaSpeedRating = new List<string>
        {
            "net.pipe",
            "net.tcp",
            "net.msmq",
            "http",
            "https",
            "sb",
        };


        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />:
        /// <list type="bullet">
        /// <item>
        /// Less than zero: <paramref name="x" /> is less than <paramref name="y" />.
        /// </item>
        /// <item>
        /// Zero<paramref name="x" /> equals <paramref name="y" />.
        /// </item>
        /// <item>
        /// Greater than zero<paramref name="x" /> is greater than <paramref name="y" />.
        /// </item>
        /// </list>
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// x
        /// or
        /// y
        /// </exception>
        public int Compare(Uri x, Uri y)
        {
            var speedX = _schemaSpeedRating.FindIndex(s => s.Equals(x.Scheme, StringComparison.OrdinalIgnoreCase));

            if (speedX == -1)
                throw new ArgumentException($"Invalid or unsupported schema {x.Scheme}", nameof(x));

            var speedY = _schemaSpeedRating.FindIndex(s => s.Equals(y.Scheme, StringComparison.OrdinalIgnoreCase));

            if (speedY == -1)
                throw new ArgumentException($"Invalid or unsupported schema {y.Scheme}", nameof(y));

            if (speedX == speedY)
            {
                // if same scheme:
                // * same host, same schema speed
                if (x.IsOnSameHostAs(y))
                    return 0;

                // * if x is on this host - x is faster
                if (x.IsOnThisHost())
                    return -1;

                // * if y is on this host - y is faster
                if (y.IsOnThisHost())
                    return 1;

                // * both not on this host - equal
                return 0;
            }
            else
            {
                // if different schemes:
                // * if x is named pipe: if it is on this host - x is faster, otherwise y is
                if (speedX == 0)
                    return x.IsOnThisHost() ? -1 : 1;
                // * if y is named pipe: if it is on this host - y is faster, otherwise x is
                if (speedY == 0)
                    return y.IsOnThisHost() ? 1 : -1;

                // * if x is on this host - if y is also on this host return the faster scheme, otherwise x is faster
                if (x.IsOnThisHost())
                    return y.IsOnThisHost() ? speedX-speedY : -1;
                // * if y is on this host - it is faster
                if (y.IsOnThisHost())
                    return 1;

                // both not on this host - return the faster scheme
                return speedX - speedY;
            }
        }
    }
}
