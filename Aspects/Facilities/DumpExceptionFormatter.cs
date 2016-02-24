using System;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.IO;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using vm.Aspects.Diagnostics;

namespace vm.Aspects.Facilities
{
    /// <summary>
    /// Class DumpExceptionFormatter. Represents exception formatter which uses the object dumper to format an exception as text.
    /// </summary>
    public class DumpExceptionFormatter : TextExceptionFormatter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DumpExceptionFormatter"/> class.
        /// </summary>
        /// <param name="writer">The text writer where to format the exception as text.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="handlingInstanceId">The handling instance identifier.</param>
        public DumpExceptionFormatter(
            TextWriter writer,
            Exception exception,
            Guid handlingInstanceId)
            : base(writer, exception, handlingInstanceId)
        {
            Contract.Requires<ArgumentNullException>(writer != null, nameof(writer));
            Contract.Requires<ArgumentNullException>(exception != null, nameof(exception));
        }

        /// <summary>
        /// Writes and formats the exception and all nested inner exceptions to the <see cref="T:System.IO.TextWriter" />.
        /// </summary>
        /// <param name="exceptionToFormat">The exception to format.</param>
        /// <param name="outerException">
        /// The outer exception. This value will be null when writing the outer-most exception.
        /// </param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="exceptionToFormat"/> is <see langword="null"/>.</exception>
        protected override void WriteException(
            Exception exceptionToFormat,
            Exception outerException)
        {
            if (exceptionToFormat == null)
                throw new ArgumentNullException(nameof(exceptionToFormat));

            exceptionToFormat.DumpText(Writer, 1);
            WriteAdditionalInfo(AdditionalInfo);
        }

        /// <summary>
        /// Writes the current date and time to the <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="utcNow">The current time.</param>
        protected override void WriteDateTime(
            DateTime utcNow)
        {
            Writer.WriteLine(
                    "{0} ({1:o})",
                    utcNow.ToLocalTime(),
                    utcNow);
        }

        /// <summary>
        /// Writes the additional properties to the <see cref="T:System.IO.TextWriter"/>.
        /// </summary>
        /// <param name="additionalInformation">Additional information to be included with the exception report</param>
        protected override void WriteAdditionalInfo(
            NameValueCollection additionalInformation)
        {
            if (additionalInformation == null ||
                additionalInformation.AllKeys.Length == 0)
                return;

            Writer.WriteLine();
            Writer.WriteLine();

            Writer.Write("Additional Information: ");
            Writer.Indent(1);
            foreach (string key in additionalInformation.AllKeys)
                Writer.Write(
                        "{0}{1,-16} : {2}",
                        Writer.NewLine,
                        key,
                        additionalInformation[key].ToString());
            Writer.Unindent(1);

            Writer.WriteLine();
            Writer.WriteLine();
        }
    }
}
