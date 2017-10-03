using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace vm.Aspects.Parsers
{
    /// <summary>
    /// Defines the contract for objects that can read a text and produce a collection of objects from
    /// a stream of delimiter separated values, e.g. comma separated or tab separated value files.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dsv", Justification = "DSV is local acronym for delimiter separated values")]
    public interface IDsvReader
    {
        /// <summary>
        /// Reads from the specified <see cref="TextReader"/> and produces a sequence of objects.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>sequence of objects - arrays of strings.</returns>
        IEnumerable<string[]> Read(TextReader reader);
    }
}
