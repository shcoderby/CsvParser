using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace SimpleCsvParser
{
    /// <summary>
    /// Delimited files reader. Supports gzipped delimited files.
    /// File should contain one header row with column names.
    /// </summary>
    public class CsvReader : Reader
    {
        /// <summary>
        /// The name of an input file.
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// Whether input file is gzipped or not.
        /// </summary>
        public bool Gzipped { get; private set; }

        /// <summary>
        /// Number of lines CsvReader should skip before reading csv data.
        /// </summary>
        public int LinesToSkip { get; private set; }

        /// <summary>
        /// Indicates whether this reader should add empty values to target records.
        /// </summary>
        public bool SkipEmptyValues { get; private set; }

        /// <summary>
        /// Character used to delimit values in the input file.
        /// </summary>
        public char Delimiter
        {
            get
            {
                return delimiter;
            }
            private set
            {
                delimiter = value;
            }
        }

        private char delimiter;
        private char quoteChar = '"';

        private TextReader reader;
        private string[] header = null;
        private int headerSize;

        /// <summary>
        /// Creates a new instance of CsvReader.
        /// </summary>
        /// <param name="fileName">Input file name.</param>
        /// <param name="delimiter">Character used to delimit values in the input file.</param>
        /// <param name="gzipped">Whether input file is gzipped or not. 
        /// By default is false</param>
        /// <param name="linesToSkip">Lines to skip before reading the header.</param>
        /// <param name="skipEmptyValues">
        /// Indicates whether empty fields should be added to output records or not.
        /// </param>
        public CsvReader(string fileName, char delimiter = ',', bool gzipped = false,
            int linesToSkip = 0, bool skipEmptyValues = false, char quoteChar = '"')
        {
            FileName = fileName;
            Gzipped = gzipped;
            Delimiter = delimiter;
            LinesToSkip = linesToSkip;
            SkipEmptyValues = skipEmptyValues;
            this.quoteChar = quoteChar;
        }

        /// <summary>
        /// Creates a new instance of CsvReader.
        /// </summary>
        /// <param name="reader">StreamReader to read from.</param>
        /// <param name="delimiter">Character used to delimit values in the input file.</param>
        /// <param name="linesToSkip">Lines to skip before reading the header.</param>
        /// <param name="skipEmptyValues">Indicates whether empty fields should be added to output records or not.</param>
        public CsvReader(TextReader reader, char delimiter = ',', int linesToSkip = 0, bool skipEmptyValues = false)
        {
            this.reader = reader;

            Gzipped = false;
            Delimiter = delimiter;
            LinesToSkip = linesToSkip;
            SkipEmptyValues = skipEmptyValues;
        }

        /// <summary>
        /// Opens the input file <see cref="FileName"/> for reading.
        /// </summary>
        protected override void Open()
        {
            base.Open();

            if (reader == null)
            {
                Stream stream = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (Gzipped)
                {
                    stream = new GZipStream(stream, CompressionMode.Decompress, false);
                }
                reader = new System.IO.StreamReader(stream);
            }
        }

        /// <summary>
        /// Reads a header (if necessary) and a single data row from the input file
        /// and returns a <c>Record</c>.
        /// </summary>
        /// <returns>Resulting Record.</returns>
        protected override Record ReadInternal()
        {
            string currentLine;
            if (header == null)
            {
                for (var i = 0; i < LinesToSkip; i++)
                    currentLine = reader.ReadLine();

                currentLine = reader.ReadLine();
                header = currentLine.Split(delimiter);
                headerSize = header.Length;
            }

            currentLine = reader.ReadLine();
            if (currentLine == null)
                return null;

            var data = SplitCsvLine(currentLine);
            var result = ZipHeaderAndData(data);

            return result;
        }

        /// <summary>
        /// Zips arrays of field names and field values into a Record.
        /// </summary>
        /// <param name="data">Array of field values.</param>
        /// <returns>Zipped record.</returns>
        protected Record ZipHeaderAndData(List<string> data)
        {
            if (headerSize != data.Count)
            {
                throw new ArgumentException(
                    string.Format(
                        "Header and rows must be of the same length. " +
                        "Header size is {0}, row size is {1}. Row: {2}",
                        headerSize, data.Count, string.Join(Environment.NewLine, data)),
                        "data");
            }

            var result = new Record();
            for (int i = 0; i < headerSize; ++i)
            {
                string field = data[i];
                if (!SkipEmptyValues || !String.IsNullOrEmpty(field))
                    result[header[i]] = data[i];
            }

            return result;
        }

        /// <summary>
        /// Closes <see cref="reader"/> if necessary.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; 
        /// false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (reader != null)
                    reader.Dispose();
            }

            base.Dispose(disposing);
        }

        #region CSV State Machine implementation

        private delegate CsvStateHandler CsvStateHandler(char currentChar, StringBuilder currentValue, List<string> values);

        private List<string> SplitCsvLine(string line)
        {
            List<string> values = new List<string>();
            StringBuilder currentValue = new StringBuilder();

            CsvStateHandler currentState = HandleValueStart;

            for (int i = 0, lineLength = line.Length; i < lineLength; i++)
            {
                currentState = currentState(line[i], currentValue, values);
            }

            currentState(delimiter, currentValue, values);

            return values;
        }

        private CsvStateHandler HandleValueStart(char currentChar, StringBuilder currentValue, List<string> values)
        {
            currentValue.Clear();

            if (quoteChar != '\0' && currentChar == quoteChar)
            {
                return HandleQuotedValue;
            }

            if (currentChar == delimiter)
            {
                values.Add(String.Empty);
                return HandleValueStart;
            }

            currentValue.Append(currentChar);

            return HandleSimpleValue;
        }

        private CsvStateHandler HandleQuotedValue(char currentChar, StringBuilder currentValue, List<string> values)
        {
            if (currentChar == quoteChar)
            {
                return HandleQuoteInQuotedValue;
            }

            currentValue.Append(currentChar);

            return HandleQuotedValue;
        }

        private CsvStateHandler HandleQuoteInQuotedValue(char currentChar, StringBuilder currentValue, List<string> values)
        {
            if (currentChar == quoteChar)
            {
                currentValue.Append(currentChar);
                return HandleQuotedValue;
            }

            if (currentChar == delimiter)
            {
                values.Add(currentValue.ToString());
                return HandleValueStart;
            }

            throw new FormatException("Quoted value contains unescaped quote character");
        }

        private CsvStateHandler HandleSimpleValue(char currentChar, StringBuilder currentValue, List<string> values)
        {
            if (currentChar == delimiter)
            {
                values.Add(currentValue.ToString());
                return HandleValueStart;
            }

            currentValue.Append(currentChar);

            return HandleSimpleValue;
        }

        #endregion
    }
}
