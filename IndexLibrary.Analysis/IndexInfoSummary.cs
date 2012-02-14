namespace IndexLibrary.Analysis
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents the summarization of multiple <see cref="IndexLibrary.IndexInfo"/> instances
    /// </summary>
    [CLSCompliant(true)]
    public sealed class IndexInfoSummary
    {
        #region Fields

        /// <summary>
        /// The list of statistics this summary should build 
        /// </summary>
        private IndexInfoSummaryFeature features;

        /// <summary>
        /// A list of each index and the last IndexInfo that was emitted from the core library when it was written
        /// </summary>
        private Dictionary<string, IndexInfo> mostRecent;

        /// <summary>
        /// A list of each index writing at the time it was written
        /// </summary>
        private List<KeyValuePair<DateTime, string>> timeSpread;

        /// <summary>
        /// The total number of reads 
        /// </summary>
        private long totalReads;

        /// <summary>
        /// The total number of searches 
        /// </summary>
        private long totalSearches;

        /// <summary>
        /// The total number of writes 
        /// </summary>
        private long totalWrites;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexInfoSummary"/> class.
        /// </summary>
        public IndexInfoSummary()
            : this(IndexInfoSummaryFeature.All)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexInfoSummary"/> class.
        /// </summary>
        /// <param name="features">The features to enable when building the summary data.</param>
        public IndexInfoSummary(IndexInfoSummaryFeature features)
        {
            this.features = features;
            this.timeSpread = new List<KeyValuePair<DateTime, string>>();
            this.mostRecent = new Dictionary<string, IndexInfo>();
            this.totalReads = 0L;
            this.totalSearches = 0L;
            this.totalWrites = 0L;
        }

        #endregion Constructors

        #region Enumerations

        /// <summary>
        /// The list of features that can be applied to this summary object
        /// </summary>
        public enum IndexInfoSummaryFeature
        {
            WriteTimeSpread = 1,
            CurrentList = 2,
            LifetimeIndex = 4,
            TotalReads = 8,
            TotalSearches = 16,
            MostRecentIndexes = 32,
            All = 63
        }

        #endregion Enumerations

        #region Properties

        /// <summary>
        /// Gets or sets the list of statistics this summary should build.
        /// </summary>
        /// <value>
        /// The features.
        /// </value>
        public IndexInfoSummaryFeature Features
        {
            get { return this.features; }
            set { this.features = value; }
        }

        /// <summary>
        /// Gets the most recent <see cref="IndexLibrary.IndexInfo"/> that represent the most recent index operations for each index
        /// </summary>
        public IDictionary<string, IndexInfo> MostRecentIndexes
        {
            get { return this.mostRecent; }
        }

        /// <summary>
        /// Gets A list of each index writing at the time it was written.
        /// </summary>
        /// <value>
        /// The time spread.
        /// </value>
        public IEnumerable<KeyValuePair<DateTime, string>> TimeSpread
        {
            get { return this.timeSpread; }
        }

        /// <summary>
        /// Gets the total number of reads.
        /// </summary>
        public long TotalReads
        {
            get { return this.totalReads; }
        }

        /// <summary>
        /// Gets the total number of searches.
        /// </summary>
        public long TotalSearches
        {
            get { return this.totalSearches; }
        }

        /// <summary>
        /// Gets the total number of writes.
        /// </summary>
        public long TotalWrites
        {
            get { return this.totalWrites; }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Adds a <see cref="IndexLibrary.IndexInfo"/> to this summary
        /// </summary>
        /// <param name="info">The info to add.</param>
        public void AddIndexInfo(IndexInfo info)
        {
            if (info == null) throw new ArgumentNullException("info", "info cannot be null");

            this.totalWrites++;

            if ((this.features & IndexInfoSummaryFeature.WriteTimeSpread) == IndexInfoSummaryFeature.WriteTimeSpread) {
                DateTime key = new DateTime(info.CreatedTime.Year, info.CreatedTime.Month, info.CreatedTime.Day, info.CreatedTime.Hour, info.CreatedTime.Minute, 0);
                this.timeSpread.Add(new KeyValuePair<DateTime, string>(key, info.IndexName));
            }

            if ((this.features & IndexInfoSummaryFeature.TotalReads) == IndexInfoSummaryFeature.TotalReads) this.totalReads += info.TotalReads;
            if ((this.features & IndexInfoSummaryFeature.TotalSearches) == IndexInfoSummaryFeature.TotalSearches) this.totalSearches += info.TotalSearches;

            if ((this.features & IndexInfoSummaryFeature.MostRecentIndexes) == IndexInfoSummaryFeature.MostRecentIndexes) {
                if (this.mostRecent.ContainsKey(info.IndexName)) this.mostRecent.Add(info.IndexName, info);
                this.mostRecent[info.IndexName] = info;
            }
        }

        #endregion Methods
    }
}