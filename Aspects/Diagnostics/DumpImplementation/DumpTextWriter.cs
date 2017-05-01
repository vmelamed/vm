using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Text;
using vm.Aspects.Diagnostics.Properties;

namespace vm.Aspects.Diagnostics.DumpImplementation
{
    [DebuggerDisplay("{GetType().Name, nq}: {_writer,nq}")]
    class DumpTextWriter : TextWriter
    {
        public const string EndOfLine = "\r\n";

        // the default maximum length of the dump
        public const int DefaultMaxLength = 4 * 1024 * 1024;

        readonly StringWriter _writer;
        readonly bool _isOwnWriter;
        int _indent;
        int _indentSize = 4;
        bool _mustIndent;
        int _maxLength;
        int _currentLength;
        bool _maxLengthExceeded;
        readonly IDictionary<int, string> _indents = new Dictionary<int, string>(37);

        public DumpTextWriter()
            : base(CultureInfo.InvariantCulture)
        {
            _writer      = new StringWriter(CultureInfo.InvariantCulture);
            _isOwnWriter = true;
            _maxLength   = DefaultMaxLength;
        }

        public DumpTextWriter(
            StringWriter writer,
            int maxLength = DefaultMaxLength)
            : base(writer.FormatProvider)
        {
            Contract.Requires<ArgumentNullException>(writer != null, nameof(writer));

            _writer    = writer;
            _maxLength = maxLength;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "N/A")]
        public DumpTextWriter(
            StringBuilder existing,
            int maxLength = 0)
            : base(CultureInfo.InvariantCulture)
        {
            Contract.Requires<ArgumentNullException>(existing != null, nameof(existing));

            _writer      = new StringWriter(existing, CultureInfo.InvariantCulture) { NewLine = EndOfLine };
            _isOwnWriter = true;
            _maxLength   = maxLength <= 0 ? DefaultMaxLength : maxLength;
        }

        public override Encoding Encoding => _writer.Encoding;

        public override string NewLine
        {
            get { return _writer.NewLine; }
            set
            {
                Contract.Assume(value == EndOfLine, "The DumpTextWriter works well only with the sequence \\r\\n as end of line marker.");

                _writer.NewLine = value;
            }
        }

        public int MaxLength
        {
            get { return _maxLength; }
            set { _maxLength = value <= 0 ? DefaultMaxLength : value; }
        }

        public int Indent
        {
            get { return _indent; }
            set { _indent = value < 0 ? 0 : value; }
        }

        public int IndentSize
        {
            get { return _indentSize; }
            set { _indentSize = value < 0 ? 0 : value; }
        }

        public bool IsClosed { get; private set; }

        public override void Close()
        {
            IsClosed = true;
            _writer.Close();
        }

        public override void Write(string value)
        {
            if (string.IsNullOrEmpty(value))
                return;

            WriteCharBuffer(value.ToCharArray(), 0, value.Length);
        }

        public override void Write(char value)
        {
            WriteChar(value);
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public override void Write(
            char[] buffer,
            int index,
            int count)
        {
            WriteCharBuffer(buffer, index, count);
        }

        void WriteCharBuffer(char[] buffer, int index, int count)
        {
            Contract.Requires<InvalidOperationException>(!IsClosed, "The writer is closed.");
            Contract.Requires(buffer != null, "buffer");
            Contract.Requires(index >= 0, "The parameter index must be non-negative.");
            Contract.Requires(count >= 0, "The parameter count must be non-negative.");
            Contract.Requires((buffer.Length - index) >= count, "The parameters index or count have invalid values.");

            for (int i = index; i < index+count; i++)
                WriteChar(buffer[i]);
        }

        void WriteChar(char value)
        {
            if (_maxLengthExceeded)
                return;

            if (++_currentLength > MaxLength)
            {
                _maxLengthExceeded = true;
                _writer.Write(Resources.MaxLengthExceeded, MaxLength);
                return;
            }

            if (value == '\n')
                _mustIndent = true;
            else
            if (_mustIndent && value != '\r')   // we don't want to insert indentation blank strings on empty lines                
            {
                _writer.Write(GetIndent());
                _mustIndent = false;
            }

            _writer.Write(value);
        }

        string GetIndent()
        {
            string indent;

            if (!_indents.TryGetValue(_indent, out indent))
                indent = _indents[_indent] = new string(' ', _indent*_indentSize);

            return indent;
        }

        protected override void Dispose(bool disposing)
        {
            IsClosed = true;
            if (disposing && _isOwnWriter)
                _writer.Dispose();
            base.Dispose(disposing);
        }

        public override string ToString() => _writer.ToString();

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [ContractInvariantMethod]
        void Invariant()
        {
            Contract.Invariant(_writer!=null, "The underlying writer is null.");
        }
    }
}
