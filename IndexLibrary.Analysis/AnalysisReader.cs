namespace IndexLibrary.Analysis
{
    using System;
    using System.Data;
    using System.Data.SQLite;
    using System.IO;

    /// <summary>
    /// A reader class used to read and summerize analysis data output from the <see cref="IndexLibrary.Analysis.AnalysisWriter"/> class
    /// </summary>
    [CLSCompliant(true)]
    public static class AnalysisReader
    {
        #region Methods

        /// <summary>
        /// Reads all rows out of the local flat-files for the specified day and table
        /// </summary>
        /// <param name="day">The day to grab rows from.</param>
        /// <param name="tableName">Name of the table to grab data from.</param>
        /// <returns>Returns a DataTable that contains all rows from for a specified day starting at the specified time</returns>
        public static DataTable ReadDay(DateTime day, TableName tableName)
        {
            return ReadDay(day, tableName, true);
        }

        /// <summary>
        /// Reads all rows out of the local flat-files for the specified day and table
        /// </summary>
        /// <param name="day">The day to grab rows from.</param>
        /// <param name="tableName">Name of the table to grab data from.</param>
        /// <param name="getDataAfterTime">Determines if data should be pulled from all rows before the timestamp in the day parameter or after</param>
        /// <returns>Returns a DataTable that contains all rows from for a specified day starting at the specified time</returns>
        public static DataTable ReadDay(DateTime day, TableName tableName, bool getDataAfterTime)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), string.Format("analysis {0}{1}{2}.db", day.Year, day.Month.ToString().PadLeft(2, '0'), day.Day.ToString().PadLeft(2, '0')));
            if (!File.Exists(path)) {
                return null;
            }
            DataTable dataTable = new DataTable();
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + path + ";Version=3")) {
                string format = (((day.Hour == 0) && (day.Minute == 0)) && (day.Second == 0)) ? "Select * From {0}" : ("Select * From {0} Where CreateTime " + (getDataAfterTime ? "> " : "< ") + "@cDate"); //day.ToString("G"));
                using (SQLiteCommand command = new SQLiteCommand(string.Format(format, tableName.ToString()), connection)) {
                    var param = command.CreateParameter();
                    param.Value = day.ToString("G");
                    param.ParameterName = "@cDate";
                    command.Parameters.Add(param);

                    using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(command)) {
                        adapter.Fill(dataTable);
                    }
                }
            }
            return dataTable;
        }

        /// <summary>
        /// Reads all rows out of multiple local flat-files for the specified date range and table name
        /// </summary>
        /// <param name="startDay">The start day and time to grab rows from.</param>
        /// <param name="endDay">The end day and time to grab rows from.</param>
        /// <param name="name">Name of the table to grab data from.</param>
        /// <returns>Returns a DataSet that contains all rows that were created inbetween the start and end datetime stamps</returns>
        public static DataSet ReadRange(DateTime startDay, DateTime endDay, TableName name)
        {
            DataTable table;
            if (startDay > endDay) {
                throw new ArgumentOutOfRangeException("startDay", "startDay cannot be newer than endDay");
            }
            DataSet set = new DataSet();
            if (startDay.Date < endDay.Date) {
                table = ReadDay(startDay, name, true);
                if (table != null) {
                    set.Tables.Add(table);
                }
                startDay = startDay.Date.AddDays(1.0);
            }
            while (startDay < endDay) {
                table = ReadDay(startDay, name, true);
                if (table != null) {
                    set.Tables.Add(table);
                }
                startDay = startDay.AddDays(1.0);
            }
            if (startDay.Date == endDay.Date) {
                table = ReadDay(endDay, name, false);
                if (table != null) {
                    set.Tables.Add(table);
                }
                endDay = new DateTime(endDay.Year, endDay.Month, endDay.Day, 0x17, 0x3b, 0x3b);
            }
            return set;
        }

        /// <summary>
        /// Summarizes sets of ReadInfo instances that were created during the specified datetime range.
        /// </summary>
        /// <param name="startDay">The start day.</param>
        /// <param name="endDay">The end day.</param>
        /// <returns>A <see cref="IndexLibrary.Analysis.ReaderInfoSummary"/> that represents a summary of the data in that timespan</returns>
        public static ReaderInfoSummary SummarizeReadData(DateTime startDay, DateTime endDay)
        {
            DataTable table;
            if (startDay > endDay) throw new ArgumentOutOfRangeException("startDay", "startDay cannot be newer than endDay");

            ReaderInfoSummary summary = new ReaderInfoSummary();

            Action<DataTable> action = delegate(DataTable inputTable) {
                if (inputTable != null && inputTable.Columns.Contains("IndexName") && inputTable.Columns.Contains("TotalDocumentsRetrieved") && inputTable.Columns.Contains("TotalDocsInIndex") && inputTable.Columns.Contains("BuiltFiltersForAllData") && inputTable.Columns.Contains("Optimized") && inputTable.Columns.Contains("CreateTime")) {
                    int count = inputTable.Rows.Count;
                    for (int i = 0; i < count; i++) {
                        DataRow row = inputTable.Rows[i];
                        try {
                            summary.AddReadInfo(new ReadInfo(row["IndexName"].ToString(), int.Parse(row["TotalDocumentsRetrieved"].ToString()), int.Parse(row["TotalDocsInIndex"].ToString()), bool.Parse(row["BuiltFiltersForAllData"].ToString()), bool.Parse(row["Optimized"].ToString()), DateTime.Parse(row["CreateTime"].ToString())));
                        }
                        catch (Exception) {
                        }
                    }
                }
            };

            if (startDay.Date < endDay.Date) {
                table = ReadDay(startDay, TableName.ReadAnalysis, true);
                action(table);
                startDay = startDay.Date.AddDays(1.0);
            }

            while (startDay < endDay) {
                table = ReadDay(startDay, TableName.ReadAnalysis, true);
                action(table);
                startDay = startDay.AddDays(1.0);
            }

            if (startDay.Date == endDay.Date) {
                table = ReadDay(endDay, TableName.ReadAnalysis, false);
                action(table);
                endDay = new DateTime(endDay.Year, endDay.Month, endDay.Day, 23, 59, 59);
            }

            return summary;
        }

        /// <summary>
        /// Summarizes sets of SearchInfo instances that were created during the specified datetime range.
        /// </summary>
        /// <param name="startDay">The start day.</param>
        /// <param name="endDay">The end day.</param>
        /// <returns>A <see cref="IndexLibrary.Analysis.SearchInfoSummary"/> that represents a summary of the data in that timespan</returns>
        public static SearchInfoSummary SummarizeSearchData(DateTime startDay, DateTime endDay)
        {
            if (startDay > endDay) throw new ArgumentOutOfRangeException("startDay", "startDay cannot be newer than endDay");

            DataTable table;
            SearchInfoSummary summary = new SearchInfoSummary();

            Action<DataTable> action = delegate(DataTable inputTable) {
                if (inputTable != null && inputTable.Columns.Contains("IndexName") && inputTable.Columns.Contains("Query") && inputTable.Columns.Contains("SearchMethodType") && inputTable.Columns.Contains("TotalResults") && inputTable.Columns.Contains("Canceled") && inputTable.Columns.Contains("CreateTime")) {
                    int count = inputTable.Rows.Count;
                    for (int j = 0; j < count; j++) {
                        DataRow row = inputTable.Rows[j];
                        try {
                            summary.AddSearchInfo(new SearchInfo(row["IndexName"].ToString(), row["Query"].ToString(), (SearchMethodType)Enum.Parse(typeof(SearchMethodType), row["SearchMethodType"].ToString()), int.Parse(row["TotalResults"].ToString()), bool.Parse(row["Canceled"].ToString()), DateTime.Parse(row["CreateTime"].ToString())));
                        }
                        catch (Exception) {
                        }
                    }
                }
            };

            if (startDay.Date < endDay.Date) {
                table = ReadDay(startDay, TableName.SearchAnalysis, true);
                action(table);
                startDay = startDay.Date.AddDays(1.0);
            }

            while (startDay < endDay) {
                table = ReadDay(startDay, TableName.SearchAnalysis, true);
                action(table);
                startDay = startDay.AddDays(1.0);
            }

            if (startDay.Date == endDay.Date) {
                table = ReadDay(endDay, TableName.SearchAnalysis, false);
                action(table);
                endDay = new DateTime(endDay.Year, endDay.Month, endDay.Day, 23, 59, 59);
            }

            return summary;
        }

        /// <summary>
        /// Summarizes sets of IndexInfo instances that were created during the specified datetime range.
        /// </summary>
        /// <param name="startDay">The start day.</param>
        /// <param name="endDay">The end day.</param>
        /// <returns>A <see cref="IndexLibrary.Analysis.IndexInfoSummary"/> that represents a summary of the data in that timespan</returns>
        public static IndexInfoSummary SummarizeWriteData(DateTime startDay, DateTime endDay)
        {
            DataTable table;
            if (startDay > endDay) throw new ArgumentOutOfRangeException("startDay", "startDay cannot be newer than endDay");

            IndexInfoSummary summary = new IndexInfoSummary();

            Action<DataTable> action = delegate(DataTable inputTable) {
                if (inputTable != null) {
                    int totalRows = inputTable.Rows.Count;
                    for (int i = 0; i < totalRows; i++) {
                        DataRow row = inputTable.Rows[i];
                        try {
                            summary.AddIndexInfo(new IndexInfo(row["IndexDirectory"].ToString(), row["IndexName"].ToString(), int.Parse(row["TotalDocuments"].ToString()), Boolean.Parse(row["Optimized"].ToString()), DateTime.Parse(row["CreateTime"].ToString())));
                        }
                        catch (Exception) {
                        }
                    }
                }
            };

            if (startDay.Date < endDay.Date) {
                table = ReadDay(startDay, TableName.IndexAnalysis, true);
                action(table);
                startDay = startDay.Date.AddDays(1.0);
            }

            while (startDay < endDay) {
                table = ReadDay(startDay, TableName.IndexAnalysis, true);
                action(table);
                startDay = startDay.AddDays(1.0);
            }

            if (startDay.Date == endDay.Date) {
                table = ReadDay(endDay, TableName.IndexAnalysis, false);
                action(table);
                endDay = new DateTime(endDay.Year, endDay.Month, endDay.Day, 23, 59, 59);
            }

            return summary;
        }

        /// <summary>
        /// Gets the total number of rows that exist in the specified table for a specified day.
        /// </summary>
        /// <param name="day">The day (the time portion is not used).</param>
        /// <param name="tableName">Name of the table to read data from.</param>
        /// <returns>An integer representing the total number of rows in the table</returns>
        public static int TotalRows(DateTime day, TableName tableName)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), string.Format("analysis {0}{1}{2}.db", day.Year, day.Month.ToString().PadLeft(2, '0'), day.Day.ToString().PadLeft(2, '0')));
            if (!File.Exists(path)) {
                return -1;
            }
            DataTable dataTable = new DataTable();
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=" + path + ";Version=3")) {
                using (SQLiteCommand command = new SQLiteCommand("Select COUNT(*) From " + tableName.ToString(), connection)) {
                    using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(command)) {
                        adapter.Fill(dataTable);
                    }
                }
            }
            if ((dataTable != null) && (dataTable.Rows.Count > 0)) {
                return int.Parse(dataTable.Rows[0][0].ToString());
            }
            return 0;
        }

        #endregion Methods
    }
}