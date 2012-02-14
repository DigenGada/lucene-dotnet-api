namespace IndexLibrary.FileParsers.DelimitedText
{
    using System;
    using System.Data;
    using System.IO;

    /// <summary>
    /// Extensions for the System.IO.StreamReader class to allow easier reading access from
    /// delimited files
    /// </summary>
    [CLSCompliant(true)]
    public static class StreamReaderExtensions
    {
        #region Methods

        /// <summary>
        /// Reads a delimited line from a file and parses it into a string array.
        /// </summary>
        /// <param name="reader">The stream to read data from.</param>
        /// <param name="seperator">The delimiting character in the file.</param>
        /// <returns>A string array that represents a delimited line from the file.</returns>
        public static string[] ReadDelimitedLine(this StreamReader reader, char seperator)
        {
            if (reader == null) throw new ArgumentNullException("reader", "reader cannot be null");
            return reader.ReadLine().Split(new char[] { seperator });
        }

        /// <summary>
        /// Reads a delimited line from a file and parses it into a string array.
        /// </summary>
        /// <param name="reader">The stream to read data from.</param>
        /// <param name="seperator">The delimiting character in the file.</param>
        /// <returns>A string array that represents a delimited line from the file.</returns>
        public static string[] ReadDelimitedLine(this StreamReader reader, string seperator)
        {
            if (reader == null) throw new ArgumentNullException("reader", "reader cannot be null");
            if (seperator == null) return new string[] { reader.ReadLine() };
            return reader.ReadLine().Split(new string[] { seperator }, StringSplitOptions.None);
        }

        /// <summary>
        /// Reads an entire delimited text file and converts it into a System.Data.DataTable
        /// </summary>
        /// <param name="reader">The stream to read data from.</param>
        /// <param name="seperator">The delimiting character in the file.</param>
        /// <param name="firstLineIsHeader">if set to <c>true</c> the first line of the file is treated as column headers.</param>
        /// <returns>A DataTable that represents all data within a delimited text file.</returns>
        public static DataTable ReadToEndAsDataTable(this StreamReader reader, char seperator, bool firstLineIsHeader)
        {
            if (reader == null) throw new ArgumentNullException("reader", "reader cannot be null");
            if (reader.EndOfStream) return null;

            DataTable table = new DataTable();
            string[] parts = null;
            int totalParts = 0;
            int i = 0;

            if (firstLineIsHeader) {
                parts = ReadDelimitedLine(reader, seperator);
                if (parts != null) {
                    totalParts = parts.Length;
                    for (i = 0; i < totalParts; i++) {
                        if (string.IsNullOrEmpty(parts[i])) table.Columns.Add(parts[i]);
                        else table.Columns.Add(ConvertIntToColumnName(i + 1));
                    }
                }
            }

            while (!reader.EndOfStream) {
                parts = ReadDelimitedLine(reader, seperator);
                if (parts == null) continue;
                totalParts = parts.Length;
                var row = table.NewRow();
                for (i = 0; i < totalParts; i++) {
                    if (table.Columns.Count <= i) table.Columns.Add(ConvertIntToColumnName(i + 1));
                    row[i] = parts[i];
                }
                table.Rows.Add(row);
            }

            return table;
        }

        /// <summary>
        /// Converts an integer into the name of an excel column.
        /// </summary>
        /// <param name="columnNumber">The column number.</param>
        /// <returns>A string representing an Excel column name</returns>
        private static string ConvertIntToColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = string.Empty;
            int modulo;

            while (dividend > 0) {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }

        #endregion Methods
    }

    /// <summary>
    /// Extensions for the System.IO.StreamWRiter class to allow easier write access for delimited data
    /// </summary>
    [CLSCompliant(true)]
    public static class StreamWriterExtensions
    {
        #region Methods

        /// <summary>
        /// Output an array of objects as a delimited string to a file.
        /// </summary>
        /// <param name="writer">The stream to write data to.</param>
        /// <param name="objects">The objects to output as a delimited line.</param>
        public static void WriteDelimitedLine(this StreamWriter writer, object[] objects)
        {
            WriteDelimitedLine(writer, ',', objects);
        }

        /// <summary>
        /// Output an array of objects as a delimited string to a file.
        /// </summary>
        /// <param name="writer">The stream to write data to.</param>
        /// <param name="seperator">The seperator to place between each value.</param>
        /// <param name="objects">The objects to output as a delimited line.</param>
        public static void WriteDelimitedLine(this StreamWriter writer, char seperator, object[] objects)
        {
            WriteDelimitedLine(writer, seperator, objects, false);
        }

        /// <summary>
        /// Output an array of objects as a delimited string to a file.
        /// </summary>
        /// <param name="writer">The stream to write data to.</param>
        /// <param name="seperator">The seperator to place between each value.</param>
        /// <param name="objects">The objects to output as a delimited line.</param>
        /// <param name="outputNullAsString">if <c>true</c> output null values as "null"; otherwise as an empty string</param>
        public static void WriteDelimitedLine(this StreamWriter writer, char seperator, object[] objects, bool outputNullAsString)
        {
            if (objects == null) throw new ArgumentNullException("objects", "objects cannot be null");
            int totalObjects = objects.Length;
            string output = null;
            for (int i = 0; i < totalObjects; i++) {
                if (objects[i] == null) {
                    if (outputNullAsString) output = "null";
                    else output = string.Empty;
                }
                else {
                    output = objects[i].ToString();
                }
                if (i == totalObjects - 1) writer.Write(output);
                else writer.Write(output + seperator);
            }

            writer.Write(Environment.NewLine);
        }

        /// <summary>
        /// Output an array of objects as a delimited string to a file.
        /// </summary>
        /// <param name="writer">The stream to write data to.</param>
        /// <param name="seperator">The seperator to place between each value.</param>
        /// <param name="objects">The objects to output as a delimited line.</param>
        public static void WriteDelimitedLine(this StreamWriter writer, string seperator, object[] objects)
        {
            WriteDelimitedLine(writer, seperator, objects, false);
        }

        /// <summary>
        /// Output an array of objects as a delimited string to a file.
        /// </summary>
        /// <param name="writer">The stream to write data to.</param>
        /// <param name="seperator">The seperator to place between each value.</param>
        /// <param name="objects">The objects to output as a delimited line.</param>
        /// <param name="outputNullAsString">if <c>true</c> output null values as "null"; otherwise as an empty string</param>
        public static void WriteDelimitedLine(this StreamWriter writer, string seperator, object[] objects, bool outputNullAsString)
        {
            if (objects == null) throw new ArgumentNullException("objects", "objects cannot be null");
            int totalObjects = objects.Length;
            string output = null;
            for (int i = 0; i < totalObjects; i++) {
                if (objects[i] == null) {
                    if (outputNullAsString) output = "null";
                    else output = string.Empty;
                }
                else {
                    output = objects[i].ToString();
                }
                if (i == totalObjects - 1) writer.Write(output);
                else writer.Write(output + seperator);
            }

            writer.Write(Environment.NewLine);
        }

        #endregion Methods
    }
}