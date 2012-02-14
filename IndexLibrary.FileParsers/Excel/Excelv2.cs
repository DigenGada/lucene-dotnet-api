namespace IndexLibrary.FileParsers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Xml;

    using ICSharpCode.SharpZipLib.Zip;

    /// <summary>
    /// Abstract for ExcelV2 readers and streams
    /// </summary>
    [CLSCompliant(true)]
    public abstract class ExcelBaseV2
    {
        #region Fields

        /// <summary>
        /// The xlsx file provided by the user
        /// </summary>
        protected string file;

        /// <summary>
        /// Determines if values read in as 'null' should be interpreted as empty strings or null
        /// </summary>
        protected bool readNullValuesAsEmpty;

        /// <summary>
        /// The full path to the SharedStrings.xml file
        /// </summary>
        protected string sharedStringsFile;

        /// <summary>
        /// A list of spreadsheet ids/names
        /// </summary>
        protected Dictionary<string, string> spreadsheets = new Dictionary<string, string>();

        /// <summary>
        /// The temporary path where the unzipped xlsx file is placed
        /// </summary>
        protected string temporaryPath;

        /// <summary>
        /// The full path to the workbook.xml definition file
        /// </summary>
        protected string workbookFile;

        /// <summary>
        /// The full path to the worksheets directory 
        /// </summary>
        protected string worksheetsPath;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcelBaseV2"/> class.
        /// </summary>
        /// <param name="xlsxFile">The XLSX file to open.</param>
        public ExcelBaseV2(string xlsxFile)
        {
            if (string.IsNullOrEmpty(xlsxFile)) throw new ArgumentNullException("xlsxFile", "xlsxFile cannot be null or empty");
            if (!File.Exists(xlsxFile)) throw new FileNotFoundException("The specified file does not exist", xlsxFile);
            if (!Path.GetExtension(xlsxFile).Equals(".xlsx", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("xlsxFile is not an xlsx", "xlsxFile");

            this.file = xlsxFile;
            this.temporaryPath = Path.Combine(Path.GetTempPath(), DateTime.Now.Ticks.ToString());
            if (!Directory.Exists(this.temporaryPath)) Directory.CreateDirectory(this.temporaryPath);
            UnzipFile();
            this.worksheetsPath = Path.Combine(this.temporaryPath, "xl\\worksheets");
            this.sharedStringsFile = Path.Combine(this.temporaryPath, "xl\\sharedStrings.xml");
            this.workbookFile = Path.Combine(this.temporaryPath, "xl\\workbook.xml");
            GetSpreadsheetNames();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the natural excel file provided by the user.
        /// </summary>
        public string ExcelFile
        {
            get {
                return this.file;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether null values are converted into empty strings when reading.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [read null as empty]; otherwise, <c>false</c>.
        /// </value>
        public bool ReadNullAsEmpty
        {
            get {
                return this.readNullValuesAsEmpty;
            }
            set {
                this.readNullValuesAsEmpty = value;
            }
        }

        /// <summary>
        /// Gets the list of spreadsheet ids.
        /// </summary>
        public IEnumerable<string> SpreadsheetIds
        {
            get {
                return this.spreadsheets.Values;
            }
        }

        /// <summary>
        /// Gets the list of spreadsheet names.
        /// </summary>
        public IEnumerable<string> SpreadsheetNames
        {
            get {
                return this.spreadsheets.Keys;
            }
        }

        /// <summary>
        /// Gets the collection of spreadsheet names and ids.
        /// </summary>
        public IDictionary<string, string> SpreadsheetNamesAndIds
        {
            get {
                return this.spreadsheets;
            }
        }

        /// <summary>
        /// Gets the total number of spreadsheets in the excel file.
        /// </summary>
        public int TotalSpreadsheets
        {
            get {
                return this.spreadsheets.Count;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Converts an excel date, time, or datetime value into a .Net DateTime instance.
        /// </summary>
        /// <param name="dateTime">The excel date time value to convert.</param>
        /// <returns>A .Net DateTime value representing the value from excel</returns>
        /// <remarks>
        /// This method is not perfect! Due to conversions from excel to .NET the date will
        /// always come out correct but the time is not guaranteed to be the same. The standard
        /// deviation in the time should be around 1 minute. If exact values are absolutely 
        /// needed consider using a driver to grab the values from the spreadsheet instead.
        /// </remarks>
        protected DateTime ConvertExcelToNETDateTime(string dateTime)
        {
            if (string.IsNullOrEmpty(dateTime)) return DateTime.Now;
            string datePortion = string.Empty;
            string timePortion = string.Empty;

            if (dateTime.Contains(".")) {
                datePortion = dateTime.Substring(0, dateTime.IndexOf('.'));
                timePortion = "0" + dateTime.Substring(dateTime.IndexOf('.'));
            }
            else {
                datePortion = dateTime;
                timePortion = "0";
            }

            double dblDate = -1;
            double dblTime = -1;

            double.TryParse(datePortion, out dblDate);
            double.TryParse(timePortion, out dblTime);

            if (dblDate == -1 && dblTime == -1) return DateTime.Now;
            if (dblDate == -1 && dblTime != -1) {
                return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddSeconds(86400 * dblTime);
            }
            else {
                DateTime epoch = new DateTime(1899, 12, 30);
                DateTime parsedDate = epoch.AddDays(dblDate);

                if (dblTime <= 0.0) return parsedDate;
                return parsedDate.AddSeconds(86400.0 * dblTime);
            }
        }

        /// <summary>
        /// Converts an integer into the name of an excel column.
        /// </summary>
        /// <param name="columnNumber">The column number.</param>
        /// <returns>A string representing an Excel column name</returns>
        protected string ConvertIntToColumnName(int columnNumber)
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

        /// <summary>
        /// Gets all values from the SharedStrings.xml file.
        /// </summary>
        /// <param name="sharedStrings">The collection to populate with the shared strings values.</param>
        protected void GetSharedStrings(Dictionary<int, string> sharedStrings)
        {
            if (sharedStrings == null) throw new ArgumentNullException("sharedStrings", "sharedStrings cannot be null");
            if (sharedStrings.Count > 0) sharedStrings.Clear();
            int key = 0;
            string innerXml = string.Empty;
            using (XmlReader reader = XmlReader.Create(File.OpenRead(this.sharedStringsFile))) {
                reader.IsStartElement();
                while (!reader.EOF) {
                    reader.Read();
                    if (reader.Name.Equals("t", StringComparison.OrdinalIgnoreCase)) {
                        innerXml = reader.ReadInnerXml();
                        if (readNullValuesAsEmpty && innerXml.Equals("null", StringComparison.OrdinalIgnoreCase)) innerXml = string.Empty;
                        sharedStrings.Add(key++, innerXml);
                    }
                }
                reader.Close();
            }
        }

        /// <summary>
        /// Trims all non-digit characters from the input string
        /// </summary>
        /// <param name="value">The string to remove non-digit characters from</param>
        /// <returns>A integer that represents the numbers from the input string, zero if none were found</returns>
        protected int TrimNonDigit(string value)
        {
            string result = string.Empty;
            int valueLength = value.Length;
            for (int i = 0; i < valueLength; i++) if (char.IsDigit(value[i])) result += value[i];
            if (string.IsNullOrEmpty(result)) return 0;
            return int.Parse(result);
        }

        /// <summary>
        /// Trims all non-letter characters from the input string
        /// </summary>
        /// <param name="value">The string to remove non-letter characters from.</param>
        /// <returns>A string that does not include any non-letters</returns>
        protected string TrimNonLetter(string value)
        {
            string result = string.Empty;
            int valueLength = value.Length;
            for (int i = 0; i < valueLength; i++) if (char.IsLetter(value[i])) result += value[i];
            return result;
        }

        /// <summary>
        /// Gets a list of all spreadsheet ids/names
        /// </summary>
        private void GetSpreadsheetNames()
        {
            using (XmlReader reader = XmlReader.Create(File.OpenRead(this.workbookFile))) {
                reader.IsStartElement();
                while (!reader.EOF && !reader.Name.Equals("sheet", StringComparison.OrdinalIgnoreCase)) reader.Read();
                while (reader.Name.Equals("sheet", StringComparison.OrdinalIgnoreCase)) {
                    this.spreadsheets.Add(reader.GetAttribute("sheetId"), reader.GetAttribute("name"));
                    reader.Read();
                }
                reader.Close();
            }
        }

        /// <summary>
        /// Unzips the user specified xlsx file
        /// </summary>
        private void UnzipFile()
        {
            string fileName = Path.GetFileNameWithoutExtension(this.file);
            using (ZipInputStream zipStream = new ZipInputStream(File.OpenRead(this.file))) {
                ZipEntry entry;
                while ((entry = zipStream.GetNextEntry()) != null) {
                    if (entry.IsFile && !string.IsNullOrEmpty(entry.Name)) {
                        string newFileName = Path.Combine(this.temporaryPath, entry.Name);
                        if (File.Exists(newFileName)) continue;

                        string directoryName = Path.GetDirectoryName(newFileName);
                        if (!directoryName.Equals(this.temporaryPath)) if (!Directory.Exists(directoryName)) Directory.CreateDirectory(directoryName);

                        using (FileStream streamWriter = File.Create(newFileName)) {
                            int size = 4096;
                            byte[] data = new byte[4096];
                            while (true) {
                                size = zipStream.Read(data, 0, data.Length);
                                if (size < 1) break;
                                streamWriter.Write(data, 0, size);
                            }
                            streamWriter.Close();
                        }
                    }
                    else if (entry.IsDirectory) {
                        string strNewDirectory = Path.Combine(this.temporaryPath, entry.Name);

                        if (!Directory.Exists(strNewDirectory)) Directory.CreateDirectory(strNewDirectory);
                    }
                }
                zipStream.Close();
            }
        }

        #endregion Methods
    }

    /// <summary>
    /// ExcelReader for opening xlsx files without using a Windows or Third Party driver
    /// </summary>
    [CLSCompliant(true)]
    public sealed class ExcelReaderV2 : ExcelBaseV2, IDisposable
    {
        #region Fields

        /// <summary>
        /// Specifies if dispose has been called on this instance or not
        /// </summary>
        private bool isDisposed = false;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcelReaderV2"/> class.
        /// </summary>
        /// <param name="xlsxFile">The XLSX file to open.</param>
        public ExcelReaderV2(string xlsxFile)
            : base(xlsxFile)
        {
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this.isDisposed) return;
            // try to dump everything out of memory and attempt to delete temp stuff
            try {
                if (!Directory.Exists(temporaryPath)) Directory.Delete(this.temporaryPath, true);
            }
            catch { }

            this.file = null;
            this.temporaryPath = null;
            this.worksheetsPath = null;
            this.sharedStringsFile = null;
            this.workbookFile = null;
            if (this.spreadsheets != null) this.spreadsheets.Clear();
            this.spreadsheets = null;

            // we're clean :)
            this.isDisposed = true;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        ///   </exception>
        public override bool Equals(object obj)
        {
            if (this.isDisposed) throw new ObjectDisposedException("Equals", "You cannot access this method as its class instance has been disposed of");
            return base.Equals(obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            if (this.isDisposed) throw new ObjectDisposedException("GetHashCode", "You cannot access this method as its class instance has been disposed of");
            return base.GetHashCode();
        }

        /// <summary>
        /// Retrieves data out of a spreadsheet by it's numerical id
        /// </summary>
        /// <param name="name">The name of the spreadsheet to read.</param>
        /// <returns>Returns a DataTable containing all requested data from a spreadsheet</returns>
        public DataTable GetSpreadsheetById(string id)
        {
            return GetSpreadsheetById(id, -1, null);
        }

        /// <summary>
        /// Retrieves data out of a spreadsheet by it's numerical id
        /// </summary>
        /// <param name="name">The name of the spreadsheet to read.</param>
        /// <param name="topNRows">The top number of rows to read out of the spreadsheet.</param>
        /// <returns>Returns a DataTable containing all requested data from a spreadsheet</returns>
        public DataTable GetSpreadsheetById(string id, int topNRows)
        {
            return GetSpreadsheetById(id, topNRows, null);
        }

        /// <summary>
        /// Retrieves data out of a spreadsheet by it's numerical id
        /// </summary>
        /// <param name="name">The name of the spreadsheet to read.</param>
        /// <param name="topNRows">The top number of rows to read out of the spreadsheet.</param>
        /// <param name="columnsToRead">The columns to read. Use null to read all columns.</param>
        /// <returns>Returns a DataTable containing all requested data from a spreadsheet</returns>
        public DataTable GetSpreadsheetById(string id, int topNRows, string[] columnsToRead)
        {
            if (this.isDisposed) throw new ObjectDisposedException("GetSpreadsheetById", "You cannot access this method as its class instance has been disposed of");
            if (string.IsNullOrEmpty(id)) throw new ArgumentOutOfRangeException("id", "The specified spreadsheet id was not found.");
            if (!this.spreadsheets.ContainsKey(id)) throw new KeyNotFoundException("The specified id was not found in the spreasheet dictionary");

            return ReadDataToTable(Path.Combine(this.worksheetsPath, "sheet" + id + ".xml"), topNRows, columnsToRead);
        }

        /// <summary>
        /// Retrieves data out of a spreadsheet by it's full qualified name
        /// </summary>
        /// <param name="name">The name of the spreadsheet to read.</param>
        /// <returns>Returns a DataTable containing all requested data from a spreadsheet</returns>
        public DataTable GetSpreadsheetByName(string name)
        {
            return GetSpreadsheetByName(name, -1, null);
        }

        /// <summary>
        /// Retrieves data out of a spreadsheet by it's full qualified name
        /// </summary>
        /// <param name="name">The name of the spreadsheet to read.</param>
        /// <param name="topNRows">The top number of rows to read out of the spreadsheet.</param>
        /// <returns>Returns a DataTable containing all requested data from a spreadsheet</returns>
        public DataTable GetSpreadsheetByName(string name, int topNRows)
        {
            return GetSpreadsheetByName(name, topNRows, null);
        }

        /// <summary>
        /// Retrieves data out of a spreadsheet by it's full qualified name
        /// </summary>
        /// <param name="name">The name of the spreadsheet to read.</param>
        /// <param name="topNRows">The top number of rows to read out of the spreadsheet.</param>
        /// <param name="columnsToRead">The columns to read. Use null to read all columns.</param>
        /// <returns>Returns a DataTable containing all requested data from a spreadsheet</returns>
        public DataTable GetSpreadsheetByName(string name, int topNRows, string[] columnsToRead)
        {
            if (this.isDisposed) throw new ObjectDisposedException("GetSpreadsheetByName", "You cannot access this method as its class instance has been disposed of");
            if (string.IsNullOrEmpty(name)) throw new ArgumentOutOfRangeException("name", "The specified spreadsheet name was not found.");
            string id = null;
            foreach (KeyValuePair<string, string> pair in spreadsheets) if (pair.Value.Equals(name, StringComparison.OrdinalIgnoreCase)) { id = pair.Key; break; }
            return GetSpreadsheetById(id, topNRows, columnsToRead);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (this.isDisposed) throw new ObjectDisposedException("ToString", "You cannot access this method as its class instance has been disposed of");
            return base.ToString();
        }

        /// <summary>
        /// Reads the data out of an excel xml file and loads it into a new DataTable.
        /// </summary>
        /// <param name="file">The full path to the file to open.</param>
        /// <param name="topNRows">The top N rows to read out of the file. Use -1 to read all rows.</param>
        /// <param name="columnsToRead">The columns to read. Use null to read all columns.</param>
        /// <returns>Returns a DataTable containing all requested data from a spreadsheet</returns>
        private DataTable ReadDataToTable(string file, int topNRows, string[] columnsToRead)
        {
            if (string.IsNullOrEmpty(file)) throw new ArgumentNullException("file", "file cannot be null or empty");
            if (!File.Exists(file)) throw new FileNotFoundException("file does not exist", file);

            int currentColumn = 1;
            if (topNRows == -1) topNRows = int.MaxValue;
            Dictionary<int, string> sharedStrings = new Dictionary<int, string>();
            List<string> readableColumns = new List<string>();
            DataTable table = new DataTable();
            bool end = false;
            bool specificColumns = (columnsToRead != null && columnsToRead.Length > 0);
            if (specificColumns) readableColumns.AddRange(columnsToRead);
            GetSharedStrings(sharedStrings);

            using (XmlReader reader = XmlReader.Create(File.OpenRead(file))) {
                reader.IsStartElement();
                // columns first

                while (!reader.EOF) {
                    reader.Read();
                    if (reader.Name.Equals("col", StringComparison.OrdinalIgnoreCase)) {
                        // add the column
                        table.Columns.Add(new DataColumn(ConvertIntToColumnName(currentColumn++), typeof(string)));
                    }
                    else if (reader.Name.Equals("row", StringComparison.OrdinalIgnoreCase)) {
                        DataRow row = table.NewRow();
                        reader.Read(); // now we're at c, get the R attribute
                        while (reader.Name.Equals("c", StringComparison.OrdinalIgnoreCase)) {
                            int rowNumber = TrimNonDigit(reader.GetAttribute("r"));
                            if (rowNumber > topNRows) { end = true; break; }

                            string name = TrimNonLetter(reader.GetAttribute("r"));
                            if (specificColumns && !readableColumns.Contains(name)) break;

                            string target = reader.GetAttribute("t");
                            string source = reader.GetAttribute("s");
                            // now we have the column name
                            reader.Read(); // now we're at v
                            string strKey = reader.ReadInnerXml();
                            if (!table.Columns.Contains(name)) table.Columns.Add(new DataColumn(name, typeof(string)));

                            // if the target is s (sharedStrings) look it up, if its not then take the value raw
                            if (target != null && target.Equals("s", StringComparison.OrdinalIgnoreCase)) {
                                int key;
                                if (int.TryParse(strKey, out key) && sharedStrings.ContainsKey(key)) {
                                    if (!table.Columns.Contains(name)) table.Columns.Add(new DataColumn(name, typeof(string)));
                                    row[name] = sharedStrings[key];
                                }
                            }
                            else {
                                int intSource = -1;
                                if (int.TryParse(source, out intSource)) {
                                    // TODO: Find other source string values!
                                    if (intSource == 1 || intSource == 2) {
                                        // datetime = 1
                                        // date = 2
                                        row[name] = ConvertExcelToNETDateTime(strKey);
                                    }
                                    else {
                                        row[name] = strKey;
                                    }
                                }
                                else {
                                    row[name] = strKey;
                                }
                            }
                            reader.Read(); // closing c
                        }
                        if (end) break;
                        table.Rows.Add(row);
                    }
                }
            }

            return table;
        }

        #endregion Methods
    }
}