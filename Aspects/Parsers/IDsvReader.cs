using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;

namespace vm.Aspects.Parsers
{
    /// <summary>
    /// Defines the contract for objects that can read a text and produce a collection of objects from
    /// a stream of delimiter separated values, e.g. comma separated or tab separated value files.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dsv", Justification = "DSV is local acronym for delimiter separated values")]
    [ContractClass(typeof(IDsvReaderContract))]
    public interface IDsvReader
    {
        /// <summary>
        /// Reads from the specified <see cref="TextReader"/> and produces a sequence of objects.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>sequence of objects - arrays of strings.</returns>
        IEnumerable<string[]> Read(TextReader reader);
    }

    [ContractClassFor(typeof(IDsvReader))]
    abstract class IDsvReaderContract : IDsvReader
    {
        #region IDelimiterSeparatedValuesReader Members
        public IEnumerable<string[]> Read(
            TextReader reader)
        {
            Contract.Requires<ArgumentNullException>(reader != null, nameof(reader));
            Contract.Ensures(Contract.Result<IEnumerable<string[]>>() != null);

            throw new NotImplementedException();
        }
        #endregion
    }
}
