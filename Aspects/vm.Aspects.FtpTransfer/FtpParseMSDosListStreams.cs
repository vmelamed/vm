using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using vm.Aspects.Facilities;

namespace vm.Aspects.FtpTransfer
{
    /// <summary>
    /// Parses a list produced by an FTP site that uses MS-DOS stile (dir) of listing. 
    /// </summary>
    /// <seealso cref="IFileListParser" />
    public class FtpParseMSDosListStreams : IFileListParser
    {
        // matches a line like:
        // 09-07-12  09:17PM                14177 fdmq0099.06304520120907
        // 09-13-12  06:43PM                 1534 another.PM_2012-09-13_18-42-42.xml.ack.xml
        // 09-14-2012  04:11PM       <DIR>          GOLDB016_PMGR_1
        const string RexStreamListLine = @"(?i)^"+
                                         @"(?<date>\d\d(?:-|/)\d\d(?:-|/)\d\d(?:\d\d)?\s+\d\d:\d\d\s*(?:PM|AM))\s+"+
                                         @"(?:(?<size>\d+)|(?<isDir><DIR>))\s+"+
                                         @"(?<name>.+)$";

        static readonly Regex _regex = new Regex(RexStreamListLine);
        static readonly string[] _dateTimeFormats = new[] { "MM-dd-yy hh:mmtt", "MM-dd-yyyy hh:mmtt" };

        #region IListParser Members
        /// <summary>
        /// Parses the specified list streams.
        /// </summary>
        /// <param name="fileListStream">The list streams.</param>
        /// <returns>Sequence FileListEntry.</returns>
        public IEnumerable<FileListEntry> Parse(
            Stream fileListStream)
        {
            var reader = new StreamReader(fileListStream, Encoding.Unicode);

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var match = _regex.Match(line);

                if (!match.Success)
                    continue;

                yield return StreamDescriptorFactory(match);
            }
        }
        #endregion

        static FileListEntry StreamDescriptorFactory(Match match)
        {
            int num;
            DateTime dt;

            return new FileListEntry
            {
                IsFolder = !string.IsNullOrWhiteSpace(match.Groups["isDir"].Value),
                Name     = match.Groups["name"].Value,
                FileSize = int.TryParse(match.Groups["size"].Value, out num) ? num : 0,
                Created  = DateTime.TryParseExact(match.Groups["date"].Value,
                                                  _dateTimeFormats,
                                                  CultureInfo.InvariantCulture,
                                                  DateTimeStyles.AllowInnerWhite | DateTimeStyles.AssumeLocal | DateTimeStyles.AdjustToUniversal,
                                                  out dt)
                                ? dt
                                : Facility.Clock.UtcNow,
            };
        }
    }
}
