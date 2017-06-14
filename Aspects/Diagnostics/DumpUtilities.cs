using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using vm.Aspects.Diagnostics.Implementation;
using vm.Aspects.Diagnostics.Properties;

namespace vm.Aspects.Diagnostics
{
    /// <summary>
    /// DumpUtilities
    /// </summary>
    public static class DumpUtilities
    {
        /// <summary>
        /// The string representing a null value - &quot;&lt;null&gt;&quot;.
        /// </summary>
        public static readonly string Null = Resources.StringNull;
        /// <summary>
        /// The string representing an unknown value.
        /// </summary>
        public static readonly string Unknown = Resources.StringUnknown;

        /// <summary>
        /// Appends <paramref name="indentLevel"/> times <paramref name="indentSize"/> number of spaces to the current new line sequence of the <paramref name="writer" />.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="indentLevel">The indent level.</param>
        /// <param name="indentSize">Size of the indent.</param>
        /// <returns>The <paramref name="writer" /> object.</returns>
        /// <exception cref="System.ArgumentNullException">If the <paramref name="writer" /> is <c>null</c>.</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static TextWriter Indent(
            this TextWriter writer,
            int indentLevel,
            int indentSize = 2)
        {
            Contract.Requires<ArgumentNullException>(writer != null, nameof(writer));
            Contract.Ensures(Contract.Result<TextWriter>() != null);

            var dumpWriter = writer as DumpTextWriter;

            if (dumpWriter != null)
            {
                dumpWriter.Indent = indentLevel;
                if (indentSize > 0)
                    dumpWriter.IndentSize = indentSize;

                return dumpWriter;
            }

            var countSpaces = indentLevel * indentSize;

            if (countSpaces > 0)
                writer.NewLine += new string(' ', countSpaces);

            return writer;
        }

        /// <summary>
        /// Removes <paramref name="indentLevel"/> times <paramref name="indentSize"/> number of spaces from the end of the current new line sequence of the <paramref name="writer" />.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="indentLevel">The indent level.</param>
        /// <param name="indentSize">Size of the indent.</param>
        /// <returns>The <paramref name="writer" /> object.</returns>
        /// <exception cref="System.ArgumentNullException">If the <paramref name="writer" /> is <c>null</c>.</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static TextWriter Unindent(
            this TextWriter writer,
            int indentLevel,
            int indentSize = 2)
        {
            Contract.Requires<ArgumentNullException>(writer != null, nameof(writer));
            Contract.Ensures(Contract.Result<TextWriter>() != null);

            var dumpWriter = writer as DumpTextWriter;

            if (dumpWriter != null)
            {
                dumpWriter.Indent = indentLevel;
                if (indentSize > 0)
                    dumpWriter.IndentSize = indentSize;

                return dumpWriter;
            }

            var countSpaces = indentLevel * indentSize;

            if (countSpaces == 0)
                return writer;

            if (countSpaces < 0)
            {
                writer.NewLine = null;
                return writer;
            }

            if (writer.NewLine.Length - countSpaces <= 2)
                writer.NewLine = null;
            else
                writer.NewLine = writer.NewLine.Substring(0, writer.NewLine.Length-countSpaces);

            return writer;
        }
    }
}
