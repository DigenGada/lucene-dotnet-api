namespace IndexLibrary.FileParsers
{
    using System;
    using System.Data;
    using System.Data.OleDb;
    using System.IO;

    /// <summary>
    /// Represents a class that can read from xls files
    /// </summary>
    [CLSCompliant(true)]
    public sealed class ExcelReader
    {
        #region Fields

        private const string connectionString = "Provider=Microsoft.{0}.OLEDB.{1};Data Source={2};Extended Properties=\"Excel {3};HDR={4};IMEX={5};READONLY=TRUE\"";

        private WorkbookType excelType;
        private string fileName;
        private bool header = false;
        private bool imex = true;

        #endregion Fields

        #region Constructors

        public ExcelReader(string fileToOpen)
            : this(fileToOpen, false)
        {
        }

        public ExcelReader(string fileToOpen, bool header)
            : this(fileToOpen, header, true)
        {
        }

        public ExcelReader(string fileToOpen, bool header, bool imex)
        {
            if (string.IsNullOrEmpty(fileToOpen)) throw new ArgumentNullException("filename", "filename cannot be null or empty");
            FileInfo info = new FileInfo(fileToOpen);
            if (!info.Exists) throw new FileNotFoundException("fileName must exist", fileToOpen);
            if (!info.Extension.EndsWith(".xls", StringComparison.OrdinalIgnoreCase) && !info.Extension.EndsWith(".xlsx")) throw new InvalidOperationException("The excel file must be an xls or xlsx");
            if (info.Extension.EndsWith(".xls")) this.excelType = WorkbookType.XLS;
            else this.excelType = WorkbookType.XLSX;
            this.fileName = fileToOpen;
            this.header = header;
            this.imex = imex;
        }

        #endregion Constructors

        #region Enumerations

        public enum WorkbookType
        {
            XLS,
            XLSX
        }

        #endregion Enumerations

        #region Properties

        public WorkbookType ExcelFileType
        {
            get { return this.excelType; }
        }

        public string File
        {
            get { return this.fileName; }
        }

        public FileInfo FileInfo
        {
            get { return new FileInfo(this.fileName); }
        }

        public bool Header
        {
            get { return this.header; }
            set { this.header = value; }
        }

        public bool IMEX
        {
            get { return this.imex; }
            set { this.imex = value; }
        }

        #endregion Properties

        #region Methods

        public DataTable GetSchema()
        {
            DataTable dtSchema = null;
            using (var connection = new OleDbConnection(GetConnectionString())) {
                connection.Open();
                dtSchema = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                connection.Close();
            }
            return dtSchema;
        }

        public DataTable GetSpreadsheet(string sheetName)
        {
            if (string.IsNullOrEmpty(sheetName)) throw new ArgumentNullException("sheetName", "sheetName cannot be null or empty");
            DataTable dtResult = new DataTable();
            using (var connection = new OleDbConnection(GetConnectionString())) {
                connection.Open();
                using (var command = new OleDbCommand("Select * From [" + sheetName + "]", connection)) {
                    using (var adapter = new OleDbDataAdapter(command)) {
                        using (DataSet ds = new DataSet()) {
                            adapter.Fill(ds);
                            if (ds.Tables.Count > 0) dtResult = ds.Tables[0];
                            ds.Dispose();
                        }
                    }
                }
                connection.Close();
            }
            dtResult.TableName = sheetName;
            return dtResult;
        }

        public DataTable GetSpreadsheet(string sheetName, ExcelCell startingCell, ExcelCell endingCell)
        {
            if (string.IsNullOrEmpty(sheetName)) throw new ArgumentNullException("sheetName", "sheetName cannot be null or empty");
            DataTable dtResult = new DataTable();
            using (var connection = new OleDbConnection(GetConnectionString())) {
                connection.Open();
                if (!sheetName.EndsWith("$")) sheetName += "$";
                using (var command = new OleDbCommand("Select * From [" + sheetName + startingCell.Cell + ":" + endingCell.Cell + "]", connection)) {
                    using (var adapter = new OleDbDataAdapter(command)) {
                        using (DataSet ds = new DataSet()) {
                            adapter.Fill(ds);
                            if (ds.Tables.Count > 0) dtResult = ds.Tables[0];
                            ds.Dispose();
                        }
                    }
                }
                connection.Close();
            }
            return dtResult;
        }

        public DataSet GetSpreadsheets()
        {
            DataSet dtSheets = new DataSet();
            //using (var connection = new OleDbConnection(GetConnectionString())) {
            //    connection.Open();
            //    using (var command = new OleDbCommand("Select * From [w1$]", connection)) {
            //        using (var adapter = new OleDbDataAdapter(command)) {
            //            adapter.Fill(dtSheets);
            //        }
            //    }
            //    connection.Close();
            //}
            using (DataTable schema = GetSchema()) {
                foreach (DataRow sheet in schema.Rows) {
                    DataTable table = GetSpreadsheet(sheet["TABLE_NAME"].ToString());
                    if (table.DataSet != null) {
                        if (!table.DataSet.Tables.CanRemove(table)) continue;
                        table.DataSet.Tables.Remove(table);
                    }
                    if (!dtSheets.Tables.Contains(table.TableName)) dtSheets.Tables.Add(table);
                    table = null;
                }
            }

            return dtSheets;
        }

        private string GetConnectionString()
        {
            if (this.excelType == WorkbookType.XLS) return string.Format(connectionString, "Jet", "4.0", this.fileName, "8.0", (header) ? "YES" : "NO", (imex) ? 1 : 0);

            return string.Format(connectionString, "Ace", "12.0", this.fileName, "12.0", (header) ? "YES" : "NO", (imex) ? 1 : 0);
        }

        #endregion Methods

        #region Nested Types

        public sealed class ExcelCell
        {
            #region Fields

            private string column;
            private int row;

            #endregion Fields

            #region Constructors

            public ExcelCell(string column, int row)
            {
                if (string.IsNullOrEmpty(column)) throw new ArgumentNullException("column", "column cannot be null or empty");
                if (row <= 0) throw new ArgumentOutOfRangeException("row", "row cannot be less than one");
                foreach (char c in column) if (!char.IsLetter(c)) throw new ArgumentException("column is in an invalid format and must only contain alpha characters", "column");
                this.column = column;
                this.row = row;
            }

            #endregion Constructors

            #region Properties

            public string Cell
            {
                get { return this.column + this.row.ToString(); }
            }

            public string Column
            {
                get { return this.column; }
            }

            public ExcelCell FirstCell
            {
                get { return new ExcelCell("A", 1); }
            }

            public int Row
            {
                get { return this.row; }
            }

            #endregion Properties
        }

        #endregion Nested Types
    }
}