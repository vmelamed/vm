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
    /// Parses a list produced by an FTP site that uses Unix stile (ls) of listing. 
    /// </summary>
    /// <seealso cref="IFileListParser" />
    public class FtpParseUnixListStreams : IFileListParser
    {
        // matches a line like:
        // -r--r--r--   0 owner   group        22428 Feb 27 06:24 FDR_MQ0099_849320.txt
        const string RexStreamListLine = @"(?i)^(?<dir>(-|d))(?<access>(-|r)(-|w)(-|x)(-|r)(-|w)(-|x)(-|r)(-|w)(-|x))\s+"+
                                         @"(?<num>\d+)\s+(?<owner>\S+)\s+(?<group>\S+)\s+(?<size>\d+)\s+"+
                                         @"(?<date>(jan|feb|mar|apr|may|jun|jul|aug|sep|oct|nov|dec)\s+\d+\s+(\d?\d:\d\d|\d\d\d\d))\s+"+
                                         @"(?<name>.+)$";
        static readonly Regex _regex = new Regex(RexStreamListLine);
        static readonly string[] _dateTimeFormats = new[] { "MMM d H:mm", "MMM d yyyy" };

        #region IListParser Members

        /// <summary>
        /// Parses the specified stream returned by the FTP site to the command ls or dir
        /// and produces a sequence of <see cref="FileListEntry" />-s.
        /// </summary>
        /// <param name="fileListStream">The stream whose contents will be parsed to produce the list of entries.</param>
        /// <returns>A sequence of <see cref="FileListEntry" />-s</returns>
        public IEnumerable<FileListEntry> Parse(
            Stream fileListStream)
        {
            var reader = new StreamReader(fileListStream, Encoding.ASCII);

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var match = _regex.Match(line);

                if (!match.Success)
                    continue;

                yield return StreamDescriptorFactory(match);
            }
        }

        static FileListEntry StreamDescriptorFactory(Match match)
        {
            int num;
            DateTime dt;

            return new FileListEntry
            {
                IsFolder     = string.Compare(match.Groups["dir"].Value, "d", StringComparison.OrdinalIgnoreCase) == 0,
                AccessRights = match.Groups["access"].Value,
                Number       = int.TryParse(match.Groups["num"].Value, out num) ? num : 0,
                Owner        = match.Groups["owner"].Value,
                Group        = match.Groups["group"].Value,
                Name         = match.Groups["name"].Value,
                FileSize     = int.TryParse(match.Groups["size"].Value, out num) ? num : 0,
                Created      = DateTime.TryParseExact(match.Groups["date"].Value,
                                                      _dateTimeFormats,
                                                      CultureInfo.InvariantCulture,
                                                      DateTimeStyles.AllowInnerWhite | DateTimeStyles.AssumeLocal | DateTimeStyles.AdjustToUniversal,
                                                      out dt)
                                    ? dt
                                    : Facility.Clock.UtcNow,
            };
        }

        #endregion
    }
}
