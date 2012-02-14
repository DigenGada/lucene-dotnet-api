namespace IndexLibrary.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.IO;
    using System.Threading;

    using IndexLibrary.Interfaces;

    /// <summary>
    /// Represents an <see cref="IndexLibrary.Analysis.IAnalysisWriter"/> 
    /// </summary>
    /// <remarks>
    /// Yes, this is a sealed singleton class that inherits IAnalysisWriter and IDisposable.
    /// Yes, this singleton creates its own thread upon initialization. Be careful with it
    /// please.
    /// </remarks>
    [CLSCompliant(true)]
    public sealed class AnalysisWriter : IAnalysisWriter, IDisposable
    {
        #region Fields

        /// <summary>
        /// The static instance for this singleton
        /// </summary>
        private static readonly AnalysisWriter analysis = new AnalysisWriter();

        /// <summary>
        /// Determines if this has been disposed of or not
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// Determines if the singleton is attempting to unload or not
        /// </summary>
        private bool ending = false;

        /// <summary>
        /// Determines if the user has forced an in-memory flush
        /// </summary>
        private bool flush = false;

        /// <summary>
        /// Specifies the time this singleton was created
        /// </summary>
        private DateTime initializationTime = DateTime.Now;

        /// <summary>
        /// Specifies the number of events that can be held in-memory before a flush occurs
        /// </summary>
        private int outputThresholdCount = 1000;

        /// <summary>
        /// Specifies the maximum amount of time that can pass before a flush occurs
        /// </summary>
        private TimeSpan outputThresholdTime = new TimeSpan(0, 15, 0);

        /// <summary>
        /// A list of all read events
        /// </summary>
        private Queue<ReadInfo> reads = new Queue<ReadInfo>();

        /// <summary>
        /// A list of all search events
        /// </summary>
        private Queue<SearchInfo> searches = new Queue<SearchInfo>();

        /// <summary>
        /// The synchronization thread that handles flushing
        /// </summary>
        private Thread syncThread;

        /// <summary>
        /// Determines if index writes should be tracked
        /// </summary>
        private bool trackIndexes = false;

        /// <summary>
        /// Determines if index reads should be tracked
        /// </summary>
        private bool trackReads = false;

        /// <summary>
        /// Determines if index searches should be tracked
        /// </summary>
        private bool trackSearches = false;

        /// <summary>
        /// A list of all write events
        /// </summary>
        private Queue<IndexInfo> writes = new Queue<IndexInfo>();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Prevents a default instance of the <see cref="AnalysisWriter"/> class from being created.
        /// </summary>
        private AnalysisWriter()
        {
            this.syncThread = new Thread(new ThreadStart(this.syncThread_DoWork));
            this.syncThread.IsBackground = false;
            this.syncThread.Name = "Search Analysis Synchronization Thread";
            this.syncThread.Start();
            LibraryAnalysis.Subscribe(this);
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the single instance of the <see cref="IndexLibrary.Analysis.AnalysisWriter"/> class.
        /// </summary>
        public static AnalysisWriter Instance
        {
            get {
                if (analysis.disposed) {
                    throw new ObjectDisposedException("analysis", "You cannot access a disposed object");
                }
                return analysis;
            }
        }

        /// <summary>
        /// Gets or sets the unique identifier for this class
        /// </summary>
        public string AnalyticsId
        {
            get { return "IndexLibrary.Analysis.AnalysisWriter"; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="AnalysisWriter"/> has been flushed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if flushed; otherwise, <c>false</c>.
        /// </value>
        public bool Flushed
        {
            get { return !this.flush; }
        }

        /// <summary>
        /// Gets the initialization time of this singleton
        /// </summary>
        public DateTime InitializationTime
        {
            get {
                return this.initializationTime;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not to [track reads].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [track reads]; otherwise, <c>false</c>.
        /// </value>
        public bool TrackReads
        {
            get {
                return this.trackReads;
            }
            set {
                this.trackReads = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not to [track searches].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [track searches]; otherwise, <c>false</c>.
        /// </value>
        public bool TrackSearches
        {
            get {
                return this.trackSearches;
            }
            set {
                this.trackSearches = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not to [track writes].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [track writes]; otherwise, <c>false</c>.
        /// </value>
        public bool TrackWrites
        {
            get {
                return this.trackIndexes;
            }
            set {
                this.trackIndexes = value;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Adds a <see cref="IndexLibrary.IndexInfo"/> to the analysis stack
        /// </summary>
        /// <param name="info">The information being reported.</param>
        public void AddIndexInfo(IndexInfo info)
        {
            if (this.trackIndexes) this.writes.Enqueue(info);
        }

        /// <summary>
        /// Adds a <see cref="IndexLibrary.ReadInfo"/> to the analysis stack
        /// </summary>
        /// <param name="info">The information being reported.</param>
        public void AddReadInfo(ReadInfo info)
        {
            if (this.trackReads) this.reads.Enqueue(info);
        }

        /// <summary>
        /// Adds a <see cref="IndexLibrary.SearchInfo"/> to the analysis stack
        /// </summary>
        /// <param name="info">The information being reported.</param>
        public void AddSearchInfo(SearchInfo info)
        {
            if (this.trackSearches) this.searches.Enqueue(info);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (!this.disposed) {
                LibraryAnalysis.Unsubscribe(this);
                this.ending = true;
                if (this.syncThread != null) {
                    this.syncThread.Join();
                    this.syncThread = null;
                }
                if (this.searches != null) {
                    this.searches.Clear();
                    this.searches = null;
                }
                if (this.reads != null) {
                    this.reads.Clear();
                    this.reads = null;
                }
                this.disposed = true;
            }
        }

        /// <summary>
        /// Enables this instance.
        /// </summary>
        public void Enable()
        {
            this.Enable(true, true, true);
        }

        /// <summary>
        /// Enables the tracking of specified features.
        /// </summary>
        /// <param name="trackSearches">if set to <c>true</c> [track searches].</param>
        /// <param name="trackReads">if set to <c>true</c> [track reads].</param>
        /// <param name="trackWrites">if set to <c>true</c> [track writes].</param>
        public void Enable(bool trackSearches, bool trackReads, bool trackWrites)
        {
            this.trackSearches = trackSearches;
            this.trackReads = trackReads;
            this.trackIndexes = trackWrites;
        }

        /// <summary>
        /// Flushes this instance.
        /// </summary>
        public void Flush()
        {
            this.flush = true;
        }

        /// <summary>
        /// Executes a query against a Sqlite database
        /// </summary>
        /// <param name="query">The query to execute.</param>
        /// <param name="connection">The connection to execute the query on.</param>
        private void ExecuteQuery(string query, SQLiteConnection connection)
        {
            if (!string.IsNullOrEmpty(query) && connection != null) new SQLiteCommand(query, connection).ExecuteNonQuery();
        }

        /// <summary>
        /// The background thread that handles all events and flushing to the database.
        /// </summary>
        private void syncThread_DoWork()
        {
            int partTime = (int)(this.outputThresholdTime.TotalMilliseconds / 5.0);
            int timeIterations = 0;
            int totalSearches = this.searches.Count;
            int totalReads = this.reads.Count;
            int totalWrites = this.writes.Count;
            while (!this.ending) {
                int partTimeCounter = 0;
                while ((partTimeCounter < partTime) && !this.ending) {
                    partTimeCounter += 1000;
                    Thread.Sleep(1000);
                    if (this.flush) break;
                }
                timeIterations++;

                totalSearches = this.searches.Count;
                totalReads = this.reads.Count;
                totalWrites = this.writes.Count;

                if (!this.flush) if ((timeIterations != 5 && totalSearches < this.outputThresholdCount && totalReads < this.outputThresholdCount && totalWrites < this.outputThresholdCount) || this.ending) continue;
                timeIterations = 0;
                DateTime now = DateTime.Now;
                string path = Path.Combine(Directory.GetCurrentDirectory(), string.Format("analysis {0}{1}{2}.db", now.Year, now.Month.ToString().PadLeft(2, '0'), now.Day.ToString().PadLeft(2, '0')));
                bool fileExists = File.Exists(path);
                using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3", path))) {
                    connection.Open();
                    if (!fileExists) {
                        this.ExecuteQuery("CREATE TABLE SearchAnalysis (Canceled bit, CreateTime varchar(24), IndexName varchar(128), Query varchar(8000), SearchMethodType varchar(6), TotalResults int)", connection);
                        this.ExecuteQuery("CREATE TABLE ReadAnalysis (BuiltFiltersForAllData bit, CreateTime varchar(24), IndexName varchar(128), TotalDocumentsRetrieved int, TotalDocsInIndex int, Optimized bit)", connection);
                        this.ExecuteQuery("CREATE TABLE IndexAnalysis (CreateTime varchar(24), IndexName varchar(512), IndexDirectory varchar(512), Optimized bit, TotalDocuments int)", connection);
                    }

                    System.Text.StringBuilder builder = new System.Text.StringBuilder();

                    // I know it looks weird but its a HUGE performance increase
                    // its 83 so that there are less than 500 parameters input to a command at any given time
                    for (int i = 0; i < totalSearches; i += 83) {
                        int totalToUpdate = totalSearches - i;
                        if (totalToUpdate > 83) totalToUpdate = 83;

                        using (SQLiteCommand command = new SQLiteCommand(connection)) {
                            builder.Append("INSERT INTO SearchAnalysis SELECT @canceled, @createdTime, @indexName, @query, @searchMethod, @totalResults ");
                            SearchInfo initialInfo = this.searches.Dequeue();
                            command.Parameters.Add(new SQLiteParameter("@canceled", initialInfo.Canceled));
                            command.Parameters.Add(new SQLiteParameter("@createdTime", initialInfo.CreatedTime.ToString("G")));
                            command.Parameters.Add(new SQLiteParameter("@indexName", initialInfo.IndexName));
                            command.Parameters.Add(new SQLiteParameter("@query", initialInfo.Query));
                            command.Parameters.Add(new SQLiteParameter("@searchMethod", initialInfo.SearchMethodType.ToString()));
                            command.Parameters.Add(new SQLiteParameter("@totalResults", initialInfo.TotalResultsFound));
                            totalToUpdate--;

                            for (int j = 0; j < totalToUpdate; j++) {
                                builder.Append(string.Format("UNION SELECT @canceled{0}, @createdTime{0}, @indexName{0}, @query{0}, @searchMethod{0}, @totalResults{0} ", j.ToString()));
                                SearchInfo info = this.searches.Dequeue();
                                command.Parameters.Add(new SQLiteParameter("@canceled" + j.ToString(), info.Canceled));
                                command.Parameters.Add(new SQLiteParameter("@createdTime" + j.ToString(), info.CreatedTime.ToString("G")));
                                command.Parameters.Add(new SQLiteParameter("@indexName" + j.ToString(), info.IndexName));
                                command.Parameters.Add(new SQLiteParameter("@query" + j.ToString(), info.Query));
                                command.Parameters.Add(new SQLiteParameter("@searchMethod" + j.ToString(), info.SearchMethodType.ToString()));
                                command.Parameters.Add(new SQLiteParameter("@totalResults" + j.ToString(), info.TotalResultsFound));
                            }

                            command.CommandText = builder.ToString();
                            command.ExecuteNonQuery();
                        }
                        builder.Remove(0, builder.Length);
                    }

                    // Again, crazy but lots of performance increase
                    for (int i = 0; i < totalReads; i += 83) {
                        int totalToUpdate = totalReads - i;
                        if (totalToUpdate > 83) totalToUpdate = 83;

                        using (SQLiteCommand command = new SQLiteCommand(connection)) {
                            builder.Append("INSERT INTO ReadAnalysis SELECT @builtAllData, @createdTime, @indexName, @totalDocsRetrieved, @totalDocsInIndex, @optimized ");
                            ReadInfo initialInfo = this.reads.Dequeue();
                            command.Parameters.Add(new SQLiteParameter("@builtAllData", initialInfo.BuiltFiltersOnAllData));
                            command.Parameters.Add(new SQLiteParameter("@createdTime", initialInfo.CreatedTime.ToString("G")));
                            command.Parameters.Add(new SQLiteParameter("@indexName", initialInfo.IndexName));
                            command.Parameters.Add(new SQLiteParameter("@totalDocsretrieved", initialInfo.TotalDocumentsRetrieved));
                            command.Parameters.Add(new SQLiteParameter("@totalDocsInIndex", initialInfo.TotalDocumentsInIndex));
                            command.Parameters.Add(new SQLiteParameter("@optimized", initialInfo.IsOptimized));
                            totalToUpdate--;

                            for (int j = 0; j < totalToUpdate; j++) {
                                builder.Append(string.Format("UNION SELECT @builtAllData{0}, @createdTime{0}, @indexName{0}, @totalDocsRetrieved{0}, @totalDocsInIndex{0}, @optimized{0} ", j.ToString()));
                                ReadInfo info = this.reads.Dequeue();
                                command.Parameters.Add(new SQLiteParameter("@builtAllData" + j.ToString(), info.BuiltFiltersOnAllData));
                                command.Parameters.Add(new SQLiteParameter("@createdTime" + j.ToString(), info.CreatedTime.ToString("G")));
                                command.Parameters.Add(new SQLiteParameter("@indexName" + j.ToString(), info.IndexName));
                                command.Parameters.Add(new SQLiteParameter("@totalDocsretrieved" + j.ToString(), info.TotalDocumentsRetrieved));
                                command.Parameters.Add(new SQLiteParameter("@totalDocsInIndex" + j.ToString(), info.TotalDocumentsInIndex));
                                command.Parameters.Add(new SQLiteParameter("@optimized" + j.ToString(), info.IsOptimized));
                            }

                            command.CommandText = builder.ToString();
                            command.ExecuteNonQuery();
                        }

                        builder.Remove(0, builder.Length);
                    }

                    //125
                    for (int i = 0; i < totalWrites; i += 125) {
                        int totalToUpdate = totalWrites - i;
                        if (totalToUpdate > 125) totalToUpdate = 125;

                        using (SQLiteCommand command = new SQLiteCommand(connection)) {
                            builder.Append("INSERT INTO IndexAnalysis SELECT @createTime, @indexName, @indexDir, @optimized, @totalDocs ");
                            IndexInfo initialInfo = this.writes.Dequeue();
                            command.Parameters.Add(new SQLiteParameter("@createTime", initialInfo.CreatedTime.ToString("G")));
                            command.Parameters.Add(new SQLiteParameter("@indexName", initialInfo.IndexName));
                            command.Parameters.Add(new SQLiteParameter("@indexDir", initialInfo.IndexDirectory));
                            command.Parameters.Add(new SQLiteParameter("@optimized", initialInfo.Optimized));
                            command.Parameters.Add(new SQLiteParameter("@totalDocs", initialInfo.TotalDocuments));
                            totalToUpdate--;

                            for (int j = 0; j < totalToUpdate; j++) {
                                builder.Append(string.Format("UNION SELECT @createTime{0}, @indexName{0}, @indexDir{0}, @optimized{0}, @totalDocs{0}  ", j.ToString()));
                                IndexInfo info = this.writes.Dequeue();
                                command.Parameters.Add(new SQLiteParameter("@createTime" + j.ToString(), info.CreatedTime.ToString("G")));
                                command.Parameters.Add(new SQLiteParameter("@indexName" + j.ToString(), info.IndexName));
                                command.Parameters.Add(new SQLiteParameter("@indexDir" + j.ToString(), info.IndexDirectory));
                                command.Parameters.Add(new SQLiteParameter("@optimized" + j.ToString(), info.Optimized));
                                command.Parameters.Add(new SQLiteParameter("@totalDocs" + j.ToString(), info.TotalDocuments));
                            }

                            command.CommandText = builder.ToString();
                            command.ExecuteNonQuery();
                        }

                        builder.Remove(0, builder.Length);
                    }// end for
                } // end using

                this.flush = false;
            }
        }

        #endregion Methods
    }
}