namespace IndexLibrary.Analysis
{
    using System;
    using System.Collections.Generic;

    using Lucene29.Net.Analysis.Standard;
    using Lucene29.Net.QueryParsers;
    using Lucene29.Net.Search;

    /// <summary>
    /// Represents the summarization of multiple <see cref="IndexLibrary.SearchInfo"/> instances
    /// </summary>
    [CLSCompliant(true)]
    public sealed class SearchInfoSummary
    {
        #region Fields

        /// <summary>
        /// The list of statistics this summary should build
        /// </summary>
        private SearchInfoSummaryFeature features;

        /// <summary>
        /// A Lucene parser used for query parsing
        /// </summary>
        private QueryParser parser;

        /// <summary>
        /// A list of the total number of searches performed by time
        /// </summary>
        private Dictionary<DateTime, int> searchSpread;

        /// <summary>
        /// The total number of canceled searches
        /// </summary>
        private long totalCanceledSearches;

        /// <summary>
        /// The total number of searches
        /// </summary>
        private long totalSearches;

        /// <summary>
        /// The total number of searches per index
        /// </summary>
        private Dictionary<string, int> totalSearchesFromEachIndex;

        /// <summary>
        /// The total number of searches by method
        /// </summary>
        private Dictionary<SearchMethodType, int> totalSearchesFromEachMethod;

        /// <summary>
        /// The total number of searches that returned results
        /// </summary>
        private long totalSearchesWithResults;

        /// <summary>
        /// The total number of unique clauses and the number of times used
        /// </summary>
        private Dictionary<string, int> totalUniqueClausesAndTotalTimesUsed;

        /// <summary>
        /// The total number of unique search queries and the number of times used
        /// </summary>
        private Dictionary<string, int> totalUniqueSearchesAndTotalTimesUsed;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchInfoSummary"/> class.
        /// </summary>
        public SearchInfoSummary()
            : this(SearchInfoSummaryFeature.All)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchInfoSummary"/> class.
        /// </summary>
        /// <param name="features">The features to enable when building the summary data.</param>
        public SearchInfoSummary(SearchInfoSummaryFeature features)
        {
            this.totalCanceledSearches = 0L;
            this.totalSearchesWithResults = 0L;
            this.totalSearches = 0L;
            this.totalSearchesFromEachMethod = new Dictionary<SearchMethodType, int>(3);
            this.totalSearchesFromEachMethod.Add(SearchMethodType.Full, 0);
            this.totalSearchesFromEachMethod.Add(SearchMethodType.Normal, 0);
            this.totalSearchesFromEachMethod.Add(SearchMethodType.Quick, 0);
            this.totalSearchesFromEachIndex = new Dictionary<string, int>(2);
            this.totalUniqueSearchesAndTotalTimesUsed = new Dictionary<string, int>();
            this.totalUniqueClausesAndTotalTimesUsed = new Dictionary<string, int>();
            this.searchSpread = new Dictionary<DateTime, int>();
            this.parser = new QueryParser(StaticValues.LibraryVersion, "Query Parser - SearchInfoSummary", new StandardAnalyzer(StaticValues.LibraryVersion));
            this.features = features;
        }

        #endregion Constructors

        #region Enumerations

        /// <summary>
        /// The list of features that can be applied to this summary object
        /// </summary>
        [Flags]
        public enum SearchInfoSummaryFeature
        {
            All = 0x7f,
            CanceledSearches = 1,
            SearchTimeSpread = 0x40,
            TotalSearches = 2,
            TotalSearchesByIndex = 8,
            TotalSearchesByMethod = 4,
            UniqueClauses = 0x20,
            UniqueQueries = 0x10
        }

        #endregion Enumerations

        #region Properties

        /// <summary>
        /// Gets or sets the list of statistics this summary should build.
        /// </summary>
        /// <value>
        /// The features.
        /// </value>
        public SearchInfoSummaryFeature Features
        {
            get {
                return this.features;
            }
            set {
                this.features = value;
            }
        }

        /// <summary>
        /// Gets a list of the total number of searches performed by time.
        /// </summary>
        public IDictionary<DateTime, int> SearchTimeSpread
        {
            get {
                return this.searchSpread;
            }
        }

        /// <summary>
        /// Gets the total number of canceled searches.
        /// </summary>
        public long TotalCanceledSearches
        {
            get {
                return this.totalCanceledSearches;
            }
        }

        /// <summary>
        /// Gets the total number of searches that were not canceled.
        /// </summary>
        public long TotalNonCanceledSearches
        {
            get { return this.totalSearches - this.totalCanceledSearches; }
        }

        /// <summary>
        /// Gets the total number of searches.
        /// </summary>
        public long TotalSearches
        {
            get {
                return this.totalSearches;
            }
        }

        /// <summary>
        /// Gets the total number of searches performed against each unique index.
        /// </summary>
        /// <value>
        /// The total number of the searches from each index.
        /// </value>
        public IDictionary<string, int> TotalSearchesFromEachIndex
        {
            get {
                return this.totalSearchesFromEachIndex;
            }
        }

        /// <summary>
        /// Gets the total number of searches performed against each method.
        /// </summary>
        public IDictionary<SearchMethodType, int> TotalSearchesFromEachMethod
        {
            get {
                return this.totalSearchesFromEachMethod;
            }
        }

        /// <summary>
        /// Gets the total number of searches that did not return results.
        /// </summary>
        public long TotalSearchesWithoutResults
        {
            get {
                return (this.totalSearches - this.totalSearchesWithResults);
            }
        }

        /// <summary>
        /// Gets the total number of searches that returned results
        /// </summary>
        public long TotalSearchesWithResults
        {
            get {
                return this.totalSearchesWithResults;
            }
        }

        /// <summary>
        /// Gets a list of each unique clause and the number of times it was used
        /// </summary>
        public IDictionary<string, int> TotalUniqueClausesAndTotalTimesUsed
        {
            get {
                return this.totalUniqueClausesAndTotalTimesUsed;
            }
        }

        /// <summary>
        /// Gets a list of the each unique search query and the number of times it was used
        /// </summary>
        public IDictionary<string, int> TotalUniqueSearchesAndTotalTimesUsed
        {
            get {
                return this.totalUniqueSearchesAndTotalTimesUsed;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Adds a <see cref="IndexLibrary.SearchInfo"/> to this summary
        /// </summary>
        /// <param name="info">The info to add.</param>
        public void AddSearchInfo(SearchInfo info)
        {
            if (info == null) throw new ArgumentNullException("info", "info cannot be null");
            if (info.IsEmpty) throw new ArgumentNullException("info", "info cannot be empty");

            this.totalSearches += 1L;

            if ((info.TotalResultsFound > 0) && (this.features & SearchInfoSummaryFeature.TotalSearches) == SearchInfoSummaryFeature.TotalSearches) this.totalSearchesWithResults += 1L;

            if (info.Canceled && (this.features & SearchInfoSummaryFeature.CanceledSearches) == SearchInfoSummaryFeature.CanceledSearches) this.totalCanceledSearches += 1L;

            if ((this.features & SearchInfoSummaryFeature.TotalSearchesByMethod) == SearchInfoSummaryFeature.TotalSearchesByMethod) {
                Dictionary<SearchMethodType, int> dictionary;
                SearchMethodType type;
                (dictionary = this.totalSearchesFromEachMethod)[type = info.SearchMethodType] = dictionary[type] + 1;
            }
            if ((this.features & SearchInfoSummaryFeature.TotalSearchesByIndex) == SearchInfoSummaryFeature.TotalSearchesByIndex) {
                if (!this.totalSearchesFromEachIndex.ContainsKey(info.IndexName)) this.totalSearchesFromEachIndex.Add(info.IndexName, 0);
                this.totalSearchesFromEachIndex[info.IndexName]++;
            }

            if ((this.features & SearchInfoSummaryFeature.SearchTimeSpread) == SearchInfoSummaryFeature.SearchTimeSpread) {
                DateTime key = new DateTime(info.CreatedTime.Year, info.CreatedTime.Month, info.CreatedTime.Day, info.CreatedTime.Hour, info.CreatedTime.Minute, 0);
                if (!this.searchSpread.ContainsKey(key)) this.searchSpread.Add(key, 0);
                this.searchSpread[key]++;
            }

            bool flag = (this.features & SearchInfoSummaryFeature.UniqueQueries) == SearchInfoSummaryFeature.UniqueQueries;
            bool flag2 = (this.features & SearchInfoSummaryFeature.UniqueClauses) == SearchInfoSummaryFeature.UniqueClauses;
            if (!string.IsNullOrEmpty(info.Query) && (flag || flag2)) {
                try {
                    BooleanQuery query = new BooleanQuery();
                    query.Add(this.parser.Parse(info.Query), BooleanClause.Occur.SHOULD);
                    if (flag) {
                        string str = query.ToString();
                        if (!this.totalUniqueSearchesAndTotalTimesUsed.ContainsKey(str)) this.totalUniqueSearchesAndTotalTimesUsed.Add(str, 0);
                        this.totalUniqueSearchesAndTotalTimesUsed[str]++;

                        str = null;
                    }
                    if (flag2) {
                        System.Collections.Hashtable terms = new System.Collections.Hashtable();
                        query.ExtractTerms(terms); // its okay to fail, if its a term with something like Field:Value~0.5 we don't want the primitives, the list would be too large
                        string value = null;
                        foreach (var term in terms) {
                            value = ((System.Collections.DictionaryEntry)term).Key.ToString();

                            if (!this.totalUniqueClausesAndTotalTimesUsed.ContainsKey(value)) this.totalUniqueClausesAndTotalTimesUsed.Add(value, 0);
                            this.totalUniqueClausesAndTotalTimesUsed[value]++;
                        }
                    }
                }
                catch {
                }
            }
        }

        #endregion Methods
    }
}