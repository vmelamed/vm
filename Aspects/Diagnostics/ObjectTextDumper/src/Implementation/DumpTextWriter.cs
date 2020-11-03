using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;

using vm.Aspects.Diagnostics.Properties;

namespace vm.Aspects.Diagnostics.Implementation
{
    [DebuggerDisplay("{GetType().Name, nq}: {_writer,nq}")]
    class DumpTextWriter : TextWriter
    {
        public const string EndOfLine = "\r\n";

        // the default maximum length of the dump
        public const int DefaultMaxLength = 4 * 1024 * 1024;

        public const int DefaultIndentSize = 4;

        readonly StringWriter _writer;
        readonly bool _isOwnWriter;
        int _indent;
        int _indentSize = DefaultIndentSize;
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
            : base(writer?.FormatProvider  ??  throw new ArgumentNullException(nameof(writer)))
        {
            _writer    = writer;
            _maxLength = maxLength;
        }

        public DumpTextWriter(
            StringBuilder existing,
            int maxLength = 0)
            : base(CultureInfo.InvariantCulture)
        {
            _isOwnWriter = true;
            _maxLength   = maxLength <= 0 ? DefaultMaxLength : maxLength;
            _writer      = new StringWriter(
                                    existing ?? throw new ArgumentNullException(nameof(existing)),
                                    CultureInfo.InvariantCulture)
            { NewLine = EndOfLine };
        }

        public TextWriter UnderlyingWriter => _writer;

        public override Encoding Encoding => _writer.Encoding;

        public override string NewLine
        {
            get { return _writer.NewLine; }
            set
            {
                Debug.Assert(value == EndOfLine, "The DumpTextWriter works well only with the sequence \\r\\n as end of line marker.");

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
            if (value.IsNullOrEmpty())
                return;

            WriteCharBuffer(value.ToCharArray(), 0, value.Length);
        }

        public override void Write(char value)
        {
            WriteChar(value);
        }

        public override void Write(
            char[] buffer,
            int index,
            int count)
        {
            WriteCharBuffer(buffer, index, count);
        }

        void WriteCharBuffer(char[] buffer, int index, int count)
        {
            if (IsClosed)
                throw new InvalidOperationException("The writer is closed.");
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (index < 0)
                throw new ArgumentException("The parameter must be non-negative.", nameof(index));
            if (count < 0)
                throw new ArgumentException("The parameter must be non-negative.", nameof(count));
            if (buffer.Length - index < count)
                throw new ArgumentException("The parameters index or count have invalid values.");

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
            if (!_indents.TryGetValue(_indent, out var indent))
                indent = _indents[_indent] = new string(' ', _indent * _indentSize);

            return indent;
        }

        public DumpTextWriter Reset()
        {
            _mustIndent        = false;
            _currentLength     = 0;
            _maxLengthExceeded = false;

            return this;
        }

        protected override void Dispose(bool disposing)
        {
            IsClosed = true;
            if (disposing && _isOwnWriter)
                _writer.Dispose();
            base.Dispose(disposing);
        }

        public override string ToString() => _writer.ToString();
    }
}
