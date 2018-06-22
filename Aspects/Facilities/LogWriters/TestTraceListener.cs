using System.Diagnostics.CodeAnalysis;
using System.IO;

using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging.Formatters;
using Microsoft.Practices.EnterpriseLibrary.Logging.TraceListeners;

namespace vm.Aspects.Facilities
{
    /// <summary>
    /// Class TestTraceListener. Stores the messages in <see cref="T:IList{string}"/>.
    /// </summary>
    [ConfigurationElementType(typeof(CustomTraceListenerData))]
    public class TestTraceListener : FormattedTextWriterTraceListener
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestTraceListener" /> class.
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public TestTraceListener()
            : base(new StringWriter(), new TextFormatter(@"
[{property(ActivityId)}]: {message}
{dictionary(    {key} - {value}{newline})}"))
        {
        }

        /// <summary>
        /// Resets the messages.
        /// </summary>
        public void Reset()
        {
            Writer.Dispose();
            Writer = new StringWriter();
        }

        /// <summary>
        /// Gets the text of the log so far.
        /// </summary>
        public string LogText => ((StringWriter)Writer).GetStringBuilder().ToString();

        /// <summary>
        /// Disposes this object.
        /// </summary>
        /// <param name="disposing"><see langword="true" /> to release managed resources; if <see langword="false" />, <see cref="M:System.Diagnostics.TextWriterTraceListener.Dispose(System.Boolean)" /> has no effect.</param>
        protected override void Dispose(
            bool disposing)
        {
            if (disposing)
            {
                Writer.Dispose();
                Formatter.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
