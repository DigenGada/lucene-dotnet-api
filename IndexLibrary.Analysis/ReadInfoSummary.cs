namespace IndexLibrary.Analysis
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents the summarization of multiple <see cref="IndexLibrary.ReadInfo"/> instances
    /// </summary>
    [CLSCompliant(true)]
    public sealed class ReaderInfoSummary
    {
        #region Fields

        /// <summary>
        /// The list of statistics this summary should build
        /// </summary>
        private ReadInfoSummaryFeature features;

        /// <summary>
        /// A list of the number of reads by timestamp
        /// </summary>
        private Dictionary<DateTime, int> readSpread;

        /// <summary>
        /// The total number of reads
        /// </summary>
        private long totalReads;

        /// <summary>
        /// The total number of reads from each index
        /// </summary>
        private Dictionary<string, int> totalReadsFromEachIndex;

        /// <summary>
        /// The total number of reads that built all fitlers
        /// </summary>
        private long totalReadsThatBuiltAllFilters;

        /// <summary>
        /// The total number of reads that returned results
        /// </summary>
        private long totalReadsWithResults;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ReaderInfoSummary"/> class.
        /// </summary>
        public ReaderInfoSummary()
            : this(ReadInfoSummaryFeature.All)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReaderInfoSummary"/> class.
        /// </summary>
        /// <param name="features">The features to enable when building the summary data.</param>
        public ReaderInfoSummary(ReadInfoSummaryFeature features)
        {
            this.totalReads = 0L;
            this.totalReadsWithResults = 0L;
            this.totalReadsThatBuiltAllFilters = 0L;
            this.readSpread = new Dictionary<DateTime, int>();
            this.totalReadsFromEachIndex = new Dictionary<string, int>();
            this.features = features;
        }

        #endregion Constructors

        #region Enumerations

        /// <summary>
        /// The list of features that can be applied to this summary object
        /// </summary>
        [Flags]
        public enum ReadInfoSummaryFeature
        {
            All = 15,
            ReadTimeSpread = 8,
            TotalReads = 1,
            TotalReadsByIndex = 2,
            TotalReadsThatBuiltFiltersOnAllData = 4
        }

        #endregion Enumerations

        #region Properties

        /// <summary>
        /// Gets a list of the number of reads by timestamp.
        /// </summary>
        public IDictionary<DateTime, int> ReadTimeSpread
        {
            get {
                return this.readSpread;
            }
        }

        /// <summary>
        /// Gets the total number of reads reads.
        /// </summary>
        public long TotalReads
        {
            get {
                return this.totalReads;
            }
        }

        /// <summary>
        /// Gets the total number of reads from each index.
        /// </summary>
        /// <value>
        /// The total number of reads from each unique index.
        /// </value>
        public IDictionary<string, int> TotalReadsFromEachIndex
        {
            get {
                return this.totalReadsFromEachIndex;
            }
        }

        /// <summary>
        /// Gets the total number of reads that built all filters.
        /// </summary>
        public long TotalReadsThatBuiltAllFilters
        {
            get {
                return this.totalReadsThatBuiltAllFilters;
            }
        }

        /// <summary>
        /// Gets the total number of reads that did not build all filters.
        /// </summary>
        public long TotalReadsThatDidNotBuildAllFilters
        {
            get {
                return (this.totalReads - this.totalReadsThatBuiltAllFilters);
            }
        }

        /// <summary>
        /// Gets the total number of reads that did not return results.
        /// </summary>
        public long TotalReadsWithoutResults
        {
            get {
                return (this.totalReads - this.totalReadsWithResults);
            }
        }

        /// <summary>
        /// Gets the total number of reads that returned results.
        /// </summary>
        public long TotalReadsWithResults
        {
            get {
                return this.totalReadsWithResults;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Adds <see cref="IndexLibrary.ReadInfo"/> to this summary
        /// </summary>
        /// <param name="info">The info to add.</param>
        public void AddReadInfo(ReadInfo info)
        {
            if (info == null) throw new ArgumentNullException("info", "info cannot be null");
            if (info.IsEmpty) throw new ArgumentNullException("info", "info cannot be empty");

            this.totalReads++;

            if ((info.TotalDocumentsRetrieved > 0) && ((this.features & ReadInfoSummaryFeature.TotalReads) == ReadInfoSummaryFeature.TotalReads)) this.totalReadsWithResults++;
            if (info.BuiltFiltersOnAllData && ((this.features & ReadInfoSummaryFeature.TotalReadsThatBuiltFiltersOnAllData) == ReadInfoSummaryFeature.TotalReadsThatBuiltFiltersOnAllData)) this.totalReadsThatBuiltAllFilters++;

            if ((this.features & ReadInfoSummaryFeature.TotalReadsByIndex) == ReadInfoSummaryFeature.TotalReadsByIndex) {
                if (!this.totalReadsFromEachIndex.ContainsKey(info.IndexName)) this.totalReadsFromEachIndex.Add(info.IndexName, 0);
                this.totalReadsFromEachIndex[info.IndexName]++;
            }
            if ((this.features & ReadInfoSummaryFeature.ReadTimeSpread) == ReadInfoSummaryFeature.ReadTimeSpread) {
                DateTime key = new DateTime(info.CreatedTime.Year, info.CreatedTime.Month, info.CreatedTime.Day, info.CreatedTime.Hour, info.CreatedTime.Minute, 0);
                if (!this.readSpread.ContainsKey(key)) this.readSpread.Add(key, 0);
                this.readSpread[key]++;
            }
        }

        #endregion Methods
    }
}