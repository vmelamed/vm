using System.Collections.Generic;
using System.IO;

namespace vm.Aspects.FtpTransfer
{
    /// <summary>
    /// Interface IFileListParser
    /// </summary>
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
}
