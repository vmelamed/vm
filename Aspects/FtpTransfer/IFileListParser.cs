using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;

namespace vm.Aspects.FtpTransfer
{
    /// <summary>
    /// Interface IFileListParser
    /// </summary>
    [ContractClass(typeof(IFileListParserContract))]
    public interface IFileListParser
    {
        /// <summary>
        /// Parses the specified stream returned by the FTP site to the command ls or dir
        /// and produces a sequence of <see cref="FtpFileListEntry"/>-s.
        /// </summary>
        /// <param name="fileListStream">The stream whose contents will be parsed to produce the list of entries.</param>
        /// <returns>A sequence of <see cref="FtpFileListEntry"/>-s</returns>
        IEnumerable<FtpFileListEntry> Parse(Stream fileListStream);
    }

    #region IFileListParser contracts
    [ContractClassFor(typeof(IFileListParser))]
    abstract class IFileListParserContract : IFileListParser
    {
        public IEnumerable<FtpFileListEntry> Parse(Stream fileListStream)
        {
            Contract.Requires<ArgumentNullException>(fileListStream != null, nameof(fileListStream));
            Contract.Ensures(Contract.Result<IEnumerable<FtpFileListEntry>>() != null);

            throw new NotImplementedException();
        }
    }
    #endregion

}
