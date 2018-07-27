using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;

#pragma warning disable 1591

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    /// <remarks/>
    public abstract class WebExceptionDumpMetadata
    {
        /// <remarks/>
        [Dump(0)]
        public object Status { get; set; }

        /// <remarks/>
        [Dump(1, DumpClass = typeof(WebExceptionDumpMetadata), DumpMethod = nameof(DumpResponse))]
        public object Response { get; set; }

        /// <remarks/>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static string DumpResponse(
            WebResponse response)
        {
            var stream = response?.GetResponseStream();

            if (stream == null)
                return "<null>";

            try
            {
                var reader = new StreamReader(stream, true);

                var result = reader.ReadToEnd();
                stream.Seek(0, SeekOrigin.Begin);

                return result;
            }
            catch (Exception)
            {
                return "<null>";
            }
        }
    }
}
