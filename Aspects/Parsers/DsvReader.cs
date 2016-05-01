using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace vm.Aspects.Parsers
{
    /// <summary>
    /// Class DsvReader reads text lines of values, that are separated with predefined delimiter (e.g. comma separated value or tab separated value files).
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dsv", Justification = "DSV is local acronym for delimiter separated values")]
    public sealed class DsvReader : IDsvReader
    {
        /// <summary>
        /// The default field separator characters - ','.
        /// </summary>
        public const string DefaultFieldSeparators = ",";
        /// <summary>
        /// The default record separator characters - '\r' or '\n'.
        /// </summary>
        public const string DefaultRecordSeparators = "\r\n";
        /// <summary>
        /// The default end of file separator characters - '\u001A' (Ctrl+Z) and '\uFFFF'.
        /// </summary>
        public const string DefaultFileSeparators = "\u001A\uFFFF";
        /// <summary>
        /// The default encapsulating field mark - '&quot;'.
        /// </summary>
        public const string DefaultFieldMark = "\"";

        /// <summary>
        /// The carriage return character - '\r'.
        /// </summary>
        public const char CR = '\r';
        /// <summary>
        /// The line feed character - '\n'.
        /// </summary>
        public const char LF = '\n';
        /// <summary>
        /// The end of file mark - '\uFFFF'.
        /// </summary>
        public const char Eof = unchecked((char)-1);
        /// <summary>
        /// The end of file character - '\u001A' (Ctrl+Z).
        /// </summary>
        public const char EofChar = '\u001A';
        /// <summary>
        /// The null character - '\0'.
        /// </summary>
        public const char Null = '\0';
        /// <summary>
        /// The tab character - '\r'.
        /// </summary>
        public const char Tab = '\t';
        /// <summary>
        /// The vertical tab character - '\v'.
        /// </summary>
        public const char VTab = '\v';
        /// <summary>
        /// The form feed character - '\f'.
        /// </summary>
        public const char FF = '\f';
        /// <summary>
        /// The space character - ' '.
        /// </summary>
        public const char Space = ' ';
        /// <summary>
        /// The comma character - ','.
        /// </summary>
        public const char Comma = ',';
        /// <summary>
        /// The vertical bar character - '|'.
        /// </summary>
        public const char VBar = '|';
        /// <summary>
        /// The single quot character - '\''.
        /// </summary>
        public const char Quot = '\'';
        /// <summary>
        /// The double quot character - '&quot;'.
        /// </summary>
        public const char DoubleQuot = '"';
        /// <summary>
        /// The 'at' character - '@'.
        /// </summary>
        public const char AtChar = '@';
        /// <summary>
        /// The underscore character - '_'.
        /// </summary>
        public const char Underscore = '_';

        // the defaults are for CSV files
        readonly string _fieldSeparators;
        readonly string _recordSeparators;
        readonly string _fileSeparators;
        readonly string _fieldMark;

        TextReader _reader;
        int _line = 1;
        int _column = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="DsvReader"/> class.
        /// </summary>
        /// <param name="fieldSeparators">The field separators.</param>
        /// <param name="recordSeparators">The record separators.</param>
        /// <param name="fileSeparators">The file separators.</param>
        /// <param name="fieldMark">The record mark.</param>
        public DsvReader(
            string fieldSeparators = DefaultFieldSeparators,
            string recordSeparators = DefaultRecordSeparators,
            string fileSeparators = DefaultFileSeparators,
            string fieldMark = DefaultFieldMark)
        {
            Contract.Requires<ArgumentNullException>(fieldSeparators!=null, nameof(fieldSeparators));
            Contract.Requires<ArgumentException>(fieldSeparators.Length > 0, "The argument "+nameof(fieldSeparators)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(fieldSeparators.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(fieldSeparators)+" cannot be empty or consist of whitespace characters only.");

            Contract.Requires<ArgumentNullException>(recordSeparators!=null, nameof(recordSeparators));
            Contract.Requires<ArgumentException>(recordSeparators.Length > 0, "The argument "+nameof(recordSeparators)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(recordSeparators.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(recordSeparators)+" cannot be empty or consist of whitespace characters only.");

            Contract.Requires<ArgumentNullException>(fieldMark!=null, nameof(fieldMark));
            Contract.Requires<ArgumentException>(fieldMark.Length > 0, "The argument "+nameof(fieldMark)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(fieldMark.Any(c => !char.IsWhiteSpace(c)), "The argument "+nameof(fieldMark)+" cannot be empty or consist of whitespace characters only.");
            Contract.Requires<ArgumentException>(fieldMark.Length == 1, "The "+nameof(fieldMark)+" parameter must specify a single, non-whitespace character.");

            // the record separators always include CR/LF
            if (recordSeparators.IndexOf(CR) < 0)
                recordSeparators += CR;
            if (recordSeparators.IndexOf(LF) < 0)
                recordSeparators += LF;

            if (string.IsNullOrWhiteSpace(fileSeparators))
                fileSeparators = DefaultFileSeparators;
            else
            {
                // the EOF-s must always include ^Z and -1
                if (fileSeparators.LastIndexOf(EofChar) < 0)
                    fileSeparators += EofChar;
                if (fileSeparators.LastIndexOf(Eof) < 0)
                    fileSeparators += Eof;
            }

            if (string.IsNullOrWhiteSpace(fieldMark))
                fieldMark = string.Empty;

            string all;

            all = VerifyNoCommonCharacters(fieldSeparators, recordSeparators);
            all = VerifyNoCommonCharacters(all, fileSeparators);
            all = VerifyNoCommonCharacters(all, fieldMark);

            _fieldSeparators  = fieldSeparators;
            _recordSeparators = recordSeparators;
            _fileSeparators   = fileSeparators;
            _fieldMark        = fieldMark;
        }

        /// <summary>
        /// Helpers in verifying that the various classes of characters do not have common characters.
        /// </summary>
        /// <param name="target">The target string to check against the unchecked class.</param>
        /// <param name="newClass">The unchecked class of characters.</param>
        /// <returns>Concatenation of both sets.</returns>
        /// <exception cref="ArgumentException">Thrown if any characters in <paramref name="newClass"/> appear in <paramref name="target"/>.</exception>
        static string VerifyNoCommonCharacters(
            string target,
            string newClass)
        {
            Contract.Requires<ArgumentNullException>(target != null, nameof(target));
            Contract.Requires<ArgumentNullException>(newClass != null, nameof(newClass));

            var repeatedIndex = target.IndexOfAny(newClass.ToCharArray());

            if (repeatedIndex >= 0)
                throw new ArgumentException(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "The character-set definitions cannot have common characters."+
                                "The character '{0}' appears in more than one of the parameters.",
                                target[repeatedIndex]));

            return target + newClass;
        }

        #region IDelimiterSeparatedValuesReader Members
        /// <summary>
        /// Reads from the specified <see cref="TextReader"/>, treats it as a comma separated text stream 
        /// and produces a sequence of objects. For the syntax definition of the text stream see the file 
        /// "Delimiter Separated Values - BNF Syntax.txt" in this directory.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>sequence of objects.</returns>
        public IEnumerable<string[]> Read(
            TextReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            _reader = reader;

            while (!IsFileSeparator(PeekNextChar()))
                yield return GetRecord();
        }
        #endregion

        /// <summary>
        /// Parses a line of text and converts it to an array of string values.
        /// </summary>
        /// <returns>Array of string values.</returns>
        string[] GetRecord()
        {
            var values = new List<string>();

            // get the values from a single line:
            values.Add(GetField());
            while (!IsEndOfRecordOrFile(PeekNextChar()))
            {
                SkipFieldsSeparator();
                values.Add(GetField());
            }

            // consume the characters to the end of the record, if any.
            SkipEndOfRecord();

            return values.ToArray();
        }

        /// <summary>
        /// Gets a single comma separated value (field).
        /// </summary>
        /// <returns>The string value of the field.</returns>
        string GetField()
        {
            char nextChar = PeekNextChar();

            if (IsFieldMark(nextChar))
                return GetMarkedField();

            var value = new StringBuilder();

            while (!IsFieldSeparator(nextChar) &&
                   !IsEndOfRecordOrFile(nextChar))
            {
                value.Append(ReadNextChar());
                nextChar = PeekNextChar();
            }

            return value.ToString().Trim();
        }

        /// <summary>
        /// Gets a marked field (quoted value) allowing for special characters in the value, e.g. record and field separators, new line characters, quotes, etc.
        /// </summary>
        /// <returns>The string value of the field.</returns>
        string GetMarkedField()
        {
            var value = new StringBuilder();
            char c = SkipFieldMark();

            while (!IsFileSeparator(c))
            {
                if (IsFieldMark(c))
                {
                    c = SkipFieldMark();
                    if (IsFileSeparator(c))
                        // forgive: throw new InvalidDataException("Quoted value was not closed.");
                        break;

                    // if single quote - end of value - get out of the loop, otherwise continue
                    if (!IsFieldMark(c))
                        break;
                }

                value.Append(ReadNextChar());
                c = PeekNextChar();
            }

            SkipWhiteSpaces();
            return value.ToString();
        }

        /// <summary>
        /// Consumes all end of record characters to the next record.
        /// </summary>
        char SkipEndOfRecord()
        {
            char c = ReadNextChar();

            if (!IsEndOfRecordOrFile(c))
                throw new InvalidDataException(
                                string.Format(
                                    CultureInfo.InvariantCulture,
                                    "Expected records separating character at line {0}, column {1}. Got '{2}'->{3}",
                                    _line.ToString(CultureInfo.InvariantCulture),
                                    _column.ToString(CultureInfo.InvariantCulture),
                                    c,
                                    char.GetNumericValue(c)));

            while (!IsFileSeparator(c) && c!=LF)
                c = ReadNextChar();

            return PeekNextChar();
        }

        /// <summary>
        /// Consumes the next character in the reader (advancing the reader's pointer) 
        /// asserting its value to be a field-separator (e.g. comma), otherwise it throws an exception.
        /// </summary>
        /// <returns>The value of the character after the comma.</returns>
        /// <exception cref="InvalidDataException">If the next character is not comma.</exception>
        char SkipFieldsSeparator()
        {
            char c = ReadNextChar();

            if (!IsFieldSeparator(c))
                throw new InvalidDataException(
                                string.Format(
                                    CultureInfo.InvariantCulture,
                                    "Expected fields separating character at line {0}, column {1}. Got '{2}'->{3}",
                                    _line.ToString(CultureInfo.InvariantCulture),
                                    _column.ToString(CultureInfo.InvariantCulture),
                                    c,
                                    char.GetNumericValue(c)));

            return PeekNextChar();
        }

        /// <summary>
        /// Consumes the white-spaces following the current stream position.
        /// </summary>
        /// <returns>The following first non-whitespace character in the stream (not consumed).</returns>
        char SkipWhiteSpaces()
        {
            do
            {
                char c = PeekNextChar();

                if (char.IsWhiteSpace(c) && !IsRecordSeparator(c))
                    ReadNextChar();
                else
                    return c;
            }
            while (true);
        }

        /// <summary>
        /// Consumes the next character in the reader (advancing the reader's pointer) asserting its value to be '"' (double quote), otherwise it throws an exception.
        /// </summary>
        /// <returns>The value of the character after the comma.</returns>
        /// <exception cref="InvalidDataException">If the next character is not double quote.</exception>
        char SkipFieldMark()
        {
            char c = ReadNextChar();

            if (!IsFieldMark(c))
                throw new InvalidDataException(
                                string.Format(
                                    CultureInfo.InvariantCulture,
                                    "Expected field marking character at line {0}, column {1}. Got '{2}'->{3}",
                                    _line.ToString(CultureInfo.InvariantCulture),
                                    _column.ToString(CultureInfo.InvariantCulture),
                                    c,
                                    char.GetNumericValue(c)));

            return PeekNextChar();
        }

        /// <summary>
        /// Gets the value of the next character in the reader without advancing the reader to the next character (without consuming it).
        /// </summary>
        /// <returns>The value of the next character in the reader.</returns>
        char PeekNextChar()
        {
            var c = _reader.Peek();

            if (c != -1)
                return unchecked((char)c);

            var streamReader = _reader as StreamReader;

            if (streamReader == null)
                return unchecked((char)c);

            while (!streamReader.EndOfStream && c == -1)
                c = _reader.Peek();

            return unchecked((char)c);
        }

        /// <summary>
        /// Gets the value of the next character in the reader and advances the reader to the next character, i.e. consumes the next character.
        /// </summary>
        /// <returns>The value of the next character in the reader.</returns>
        char ReadNextChar()
        {
            var c = unchecked((char)_reader.Read());

            if (c==LF)
            {
                _line++;
                _column = 1;
            }
            else
                _column++;
            return c;
        }

        /// <summary>
        /// Determines whether the specified character is field separator.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <returns>
        ///   <c>true</c> if the specified character is a field separator; otherwise, <c>false</c>.
        /// </returns>
        bool IsFieldSeparator(
            char character) => _fieldSeparators.IndexOf(character) >= 0;

        /// <summary>
        /// Determines whether the specified character is record separator.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <returns>
        ///   <c>true</c> if the specified character is a record separator; otherwise, <c>false</c>.
        /// </returns>
        bool IsRecordSeparator(
            char character) => IsQualifiedCharacter(_recordSeparators, character);

        /// <summary>
        /// Determines whether the specified character is file separator.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <returns>
        ///   <c>true</c> if the specified character is a file separator; otherwise, <c>false</c>.
        /// </returns>
        bool IsFileSeparator(
            char character) => IsQualifiedCharacter(_fileSeparators, character);

        /// <summary>
        /// Determines whether the specified character is a field marking character.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <returns>
        ///   <c>true</c> if the specified character is a record mark; otherwise, <c>false</c>.
        /// </returns>
        bool IsFieldMark(
            char character) => IsQualifiedCharacter(_fieldMark, character);

        /// <summary>
        /// Determines whether the specified character is end of record or file.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <returns>
        ///   <c>true</c> if the specified character is end of record or file; otherwise, <c>false</c>.
        /// </returns>
        bool IsEndOfRecordOrFile(
            char character) => IsRecordSeparator(character) ||
                               IsFileSeparator(character);

        /// <summary>
        /// Determines whether a character is one of the specified qualifying characters.
        /// </summary>
        /// <param name="qualifications">The qualifying characters.</param>
        /// <param name="character">The character to be tested.</param>
        /// <returns>
        /// <see langword="true" /> if the character is one of the specified qualifying characters; otherwise, <see langword="false" />.
        /// </returns>
        static bool IsQualifiedCharacter(
            string qualifications,
            char character)
        {
            Contract.Requires<ArgumentNullException>(qualifications != null, nameof(qualifications));

            for (var i = 0; i<qualifications.Length; i++)
                if (qualifications[i] == character)
                    return true;

            return false;
        }
    }
}
