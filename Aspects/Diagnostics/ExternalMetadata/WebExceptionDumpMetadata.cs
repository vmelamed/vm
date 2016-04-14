using System;
using System.IO;
using System.Net;

namespace vm.Aspects.Diagnostics.ExternalMetadata
{
    /// <remarks/>
    public abstract class WebExceptionDumpMetadata
    {
        /// <remarks/>
        [Dump(0)]
        public object Status;

        /// <remarks/>
        [Dump(1, DumpClass = typeof(WebExceptionDumpMetadata), DumpMethod = nameof(DumpResponse))]
        public object Response;

        /// <remarks/>
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
