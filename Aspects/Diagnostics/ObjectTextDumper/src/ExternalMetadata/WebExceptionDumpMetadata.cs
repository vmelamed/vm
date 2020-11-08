using System;
using System.IO;
using System.Net;

#pragma warning disable 1591

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    public abstract class WebExceptionDumpMetadata
    {
        [Dump(0)]
        public object? Status { get; set; }

        [Dump(1, DumpClass = typeof(WebExceptionDumpMetadata), DumpMethod = nameof(DumpResponse))]
        public object? Response { get; set; }

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
