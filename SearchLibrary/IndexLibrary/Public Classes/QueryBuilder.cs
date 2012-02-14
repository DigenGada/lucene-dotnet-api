/// Copyright 2011 Timothy James
///
/// Licensed under the Apache License, Version 2.0 (the "License");
/// you may not use this file except in compliance with the License.
/// You may obtain a copy of the License at
///
///  http://www.apache.org/licenses/LICENSE-2.0
///
/// Unless required by applicable law or agreed to in writing, software
/// distributed under the License is distributed on an "AS IS" BASIS,
/// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/// See the License for the specific language governing permissions and
/// limitations under the License.
namespace IndexLibrary
{
    using System;

    using Lucene29.Net.Search;

    /// <summary>
    /// Class used to build search queries primarily for an index searcher
    /// </summary>
    /// <remarks>
    /// This class represents a wrapper around the Lucene BooleanQuery object. Each time
    /// a clause is added to an instance of this class it is parsed and added directly
    /// to the internal Lucene BooleanQuery. 
    ///
    /// There are two methods you can use to add numerical range queries to a QueryBuilder.
    /// First when you create a <see cref="IndexLibrary.SearchTerm"/> you can use one
    /// of the constructor overloads to create a range in the term itself; then you can
    /// add that query directly to a QueryBuilder using a normal add boolean clause method.
    /// Note that adding numerical range queries in this manner simply prebuilds the formatted
    /// string for numerical queries and thus, if tampered, could cause adverse or unexpected
    /// results.
    /// 
    /// The second method to add numerical queries to a QueryBuilder is to use either the 
    /// AddDoubleRangeClause, AddFloatRangeClause, AddIntRangeClause, or AddLongRangeClause
    /// methods of the QueryBuilder itself. Using this method to add numerical ranges to a
    /// query is more accurate (since there's no change of tampering) but you have to know
    /// the values you want to index when you have the builder. Thus if you need to prebuild
    /// your queries to fire at a later time you'll have to use the first method.
    /// </remarks>
    [System.CLSCompliant(true)]
    public class QueryBuilder
    {
        #region Fields

        /// <summary>
        /// Specifies whether any clause can begin with a wildcard character
        /// </summary>
        /// <remarks>
        /// Whenever the QueryBuilder class has to make a call to the Lucene query parser the
        /// SetAllowLeadingWildcard flag is set
        /// </remarks>
        private bool allowLeadingWildcard = false;

        /// <summary>
        /// The analyzer type declared inside the cachedParser.
        /// </summary>
        private AnalyzerType cachedAnalyzer = AnalyzerType.None;

        /// <summary>
        /// Static version of the query parser used in the Analyzer method
        /// </summary>
        /// <remarks>
        /// <para>
        /// Creating a query parser is a rather expensive operation so as to minimize the number of
        /// times that this operation has to happen some basic caching was implemented. 
        /// </para>
        /// <para>
        /// The caching was implemented to support the most common scenario when working with the 
        /// API, where all queries are run through an analyzer, of the same type, before being 
        /// executed against an index. Thus the caching will create an instance of the query parser
        /// and will only recycle the object if the requested analyzer is different than the cached one.
        /// </para>
        /// <para>
        /// For instance: Say we have a web site where 99% of all searches are performed directly from the
        /// page UI. Each query that is created needs to be run through the Standard analyzer, but the
        /// remaining 1% of the time a special search is used from the CMS that allows the user to specify
        /// the type of analyzer they want to use on their query. The QueryBuilder object will create a single
        /// instance of the query parser using the Standard analyzer; as soon as a request is made for an 
        /// analyzer other than the Standard one, the object is disposed and a new one is created for the 
        /// new analyzer. 
        /// </para>
        /// </remarks>
        private Lucene29.Net.QueryParsers.QueryParser cachedParser;

        /// <summary>
        /// A flag indicating whether or not similarity scoring is enabled
        /// </summary>
        /// <remarks>
        /// The Lucene similarity algorhithm "computes a score factor based on the fraction
        /// of all query terms that a document contains. This value is multiplied into scores.
        /// The presence of a large potion of the query terms indicates a better match with the
        /// query, so implementations of this method usually return larger values than when the 
        /// ratio between these parameters is large and smaller values when the ratio between
        /// them is small."
        /// (http://lucene.apache.org/java/2_9_0/api/all/org/apache/lucene/search/Similarity.html#coord%28int,%20int%29)
        /// </remarks>
        private bool disableCoord;

        /// <summary>
        /// Query object that this class wraps
        /// </summary>
        private BooleanQuery luceneQuery = null;

        /// <summary>
        /// Synchronization root used to maintain a static version of the query parser
        /// </summary>
        private volatile object syncRoot = new object();

        /// <summary>
        /// Keeps track of the total number of clauses in this instance
        /// </summary>
        /// <remarks>
        /// This is required as the Lucene library, by default, only allows
        /// up to 1024 clauses to be added to any query. You can manually
        /// up this number if necessary, but this class forces you to remain
        /// within an 1000 clause limit (StaticValues.TOTAL_ALLOWED_CLAUSES)
        /// </remarks>
        private int totalClauses = 0;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryBuilder"/> class.
        /// </summary>
        /// <remarks>
        /// Automatically sets the disableCoord property to true
        /// </remarks>
        public QueryBuilder()
            : this(true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryBuilder"/> class.
        /// </summary>
        /// <param name="disableCoord">if set to <c>true</c> [disable coordinate scoring].</param>
        public QueryBuilder(bool disableCoord)
        {
            this.disableCoord = disableCoord;
            this.luceneQuery = new BooleanQuery(disableCoord);
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether or not to allow leading wildcards in clauses.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [allow leading wildcards]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowLeadingWildcards
        {
            get { return this.allowLeadingWildcard; }
            set { this.allowLeadingWildcard = value; }
        }

        /// <summary>
        /// Gets a value indicating whether [coordinate scoring is disabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [coordinates disabled]; otherwise, <c>false</c>.
        /// </value>
        public bool CoordinatesDisabled
        {
            get { return this.disableCoord; }
        }

        /// <summary>
        /// Gets a value indicating the total number of clauses in this instance.
        /// </summary>
        /// <remarks>
        /// Limited to StaticValues.TOTAL_ALLOWED_CLAUSES which is 1000
        /// </remarks>
        public int TotalClauses
        {
            get { return this.totalClauses; }
        }

        /// <summary>
        /// Gets the get lucene query that this instance wraps
        /// </summary>
        internal BooleanQuery GetLuceneQuery
        {
            get { return this.luceneQuery; }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Adds a standard type clause to this instance
        /// </summary>
        /// <param name="term">Term to add to this query</param>
        public void AddBooleanClause(SearchTerm term)
        {
            this.AddBooleanClause(term, ClauseOccurrence.Default);
        }

        /// <summary>
        /// Adds a standard type clause to this instance
        /// </summary>
        /// <param name="term">Term to add to this query</param>
        /// <param name="occurrence">Defines how the term is added to this query</param>
        public void AddBooleanClause(SearchTerm term, ClauseOccurrence occurrence)
        {
            this.AddBooleanClause(term, occurrence, StaticValues.DefaultSlop);
        }

        /// <summary>
        /// Adds a standard type clause to this instance
        /// </summary>
        /// <param name="term">Term to add to this query.</param>
        /// <param name="occurrence">Defines how the term is added to this query.</param>
        /// <param name="slop">The amount of allowed slop in a phrase query.</param>
        /// <remarks>
        /// Slop is the amount of movement each word is allowed in a non-exact phrase query.
        /// For instance if you search for "Adobe Systems Incorporated" and the slop is set to 0 then
        /// only results with that term is allowed. If you set the slop to 2 then two movements can be
        /// made, max, for each word. In the same example with slop set to 2 results would be returned 
        /// for "Adobe Systems Incorporated", "Adobe Incorporated Systems", "Systems Adobe Incorporated",
        /// and "Systems Incorporated Adobe". 
        /// </remarks>
        public void AddBooleanClause(SearchTerm term, ClauseOccurrence occurrence, int slop)
        {
            if (term == null)
                throw new ArgumentNullException("term", "term cannot be null");
            IncrementTotalClauses(1);

            if (term.IsPhrase) {
                PhraseQuery phraseQuery = new PhraseQuery();
                phraseQuery.Add(term.GetLuceneTerm());
                phraseQuery.SetSlop(slop);
                phraseQuery.SetBoost(term.Boost);
                this.luceneQuery.Add(phraseQuery, TypeConverter.ConvertToLuceneClauseOccurrence(occurrence));
                phraseQuery = null;
            }
            else {
                TermQuery termQuery = new TermQuery(term.GetLuceneTerm());
                termQuery.SetBoost(term.Boost);
                this.luceneQuery.Add(termQuery, TypeConverter.ConvertToLuceneClauseOccurrence(occurrence));
                termQuery = null;
            }
        }

        /// <summary>
        /// Adds a double range clause to this instance.
        /// </summary>
        /// <param name="fieldName">Name of the field to add.</param>
        /// <param name="startValue">The start value of the range to search.</param>
        /// <param name="endValue">The end value of the range to search.</param>
        public void AddDoubleRangeClause(string fieldName, double startValue, double endValue)
        {
            this.AddDoubleRangeClause(fieldName, startValue, endValue, ClauseOccurrence.Default, true, true, StaticValues.DefaultBoost);
        }

        /// <summary>
        /// Adds a double range clause to this instance.
        /// </summary>
        /// <param name="fieldName">Name of the field to add.</param>
        /// <param name="startValue">The start value of the range to search.</param>
        /// <param name="endValue">The end value of the range to search.</param>
        /// <param name="occurrence">Defines how the term is added to this query</param>
        public void AddDoubleRangeClause(string fieldName, double startValue, double endValue, ClauseOccurrence occurrence)
        {
            this.AddDoubleRangeClause(fieldName, startValue, endValue, occurrence, true, true, StaticValues.DefaultBoost);
        }

        /// <summary>
        /// Adds a double range clause to this instance.
        /// </summary>
        /// <param name="fieldName">Name of the field to add.</param>
        /// <param name="startValue">The start value of the range to search.</param>
        /// <param name="endValue">The end value of the range to search.</param>
        /// <param name="occurrence">Defines how the term is added to this query</param>
        /// <param name="includeStartValue">Determines if the starting term is included or excluded in the search results</param>
        /// <param name="includeEndValue">Determines if the ending term is included or excluded in the search results</param>
        public void AddDoubleRangeClause(string fieldName, double startValue, double endValue, ClauseOccurrence occurrence, bool includeStartValue, bool includeEndValue)
        {
            AddDoubleRangeClause(fieldName, startValue, endValue, occurrence, includeStartValue, includeEndValue, StaticValues.DefaultBoost);
        }

        /// <summary>
        /// Adds a double range clause to this instance.
        /// </summary>
        /// <param name="fieldName">Name of the field to add.</param>
        /// <param name="startValue">The start value of the range to search.</param>
        /// <param name="endValue">The end value of the range to search.</param>
        /// <param name="occurrence">Defines how the term is added to this query</param>
        /// <param name="includeStartValue">Determines if the starting term is included or excluded in the search results</param>
        /// <param name="includeEndValue">Determines if the ending term is included or excluded in the search results</param>
        /// <param name="boost">The amount of boost to apply to this term</param>
        public void AddDoubleRangeClause(string fieldName, double startValue, double endValue, ClauseOccurrence occurrence, bool includeStartValue, bool includeEndValue, float boost)
        {
            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentNullException("fieldName", "fieldName cannot be null or empty");
            if (boost < StaticValues.MinimumAllowedBoost)
                throw new ArgumentOutOfRangeException("boost", "boost cannot be less than " + StaticValues.MinimumAllowedBoost);
            IncrementTotalClauses(1);
            NumericRangeQuery rangeQuery = NumericRangeQuery.NewDoubleRange(fieldName, startValue, endValue, includeStartValue, includeEndValue);
            rangeQuery.SetBoost(boost);
            this.luceneQuery.Add(rangeQuery, TypeConverter.ConvertToLuceneClauseOccurrence(occurrence));
            rangeQuery = null;
        }

        /// <summary>
        /// Adds a float range clause to this instance.
        /// </summary>
        /// <param name="fieldName">Name of the field to add.</param>
        /// <param name="startValue">The start value of the range to search.</param>
        /// <param name="endValue">The end value of the range to search.</param>
        public void AddFloatRangeClause(string fieldName, float startValue, float endValue)
        {
            this.AddFloatRangeClause(fieldName, startValue, endValue, ClauseOccurrence.Default, true, true, StaticValues.DefaultBoost);
        }

        /// <summary>
        /// Adds a float range clause to this instance.
        /// </summary>
        /// <param name="fieldName">Name of the field to add.</param>
        /// <param name="startValue">The start value of the range to search.</param>
        /// <param name="endValue">The end value of the range to search.</param>
        /// <param name="occurrence">Defines how the term is added to this query</param>
        public void AddFloatRangeClause(string fieldName, float startValue, float endValue, ClauseOccurrence occurrence)
        {
            this.AddFloatRangeClause(fieldName, startValue, endValue, occurrence, true, true, StaticValues.DefaultBoost);
        }

        /// <summary>
        /// Adds a float range clause to this instance.
        /// </summary>
        /// <param name="fieldName">Name of the field to add.</param>
        /// <param name="startValue">The start value of the range to search.</param>
        /// <param name="endValue">The end value of the range to search.</param>
        /// <param name="occurrence">Defines how the term is added to this query</param>
        /// <param name="includeStartValue">Determines if the starting term is included or excluded in the search results</param>
        /// <param name="includeEndValue">Determines if the ending term is included or excluded in the search results</param>
        public void AddFloatRangeClause(string fieldName, float startValue, float endValue, ClauseOccurrence occurrence, bool includeStartValue, bool includeEndValue)
        {
            AddFloatRangeClause(fieldName, startValue, endValue, occurrence, includeStartValue, includeEndValue, StaticValues.DefaultBoost);
        }

        /// <summary>
        /// Adds a float range clause to this instance.
        /// </summary>
        /// <param name="fieldName">Name of the field to add.</param>
        /// <param name="startValue">The start value of the range to search.</param>
        /// <param name="endValue">The end value of the range to search.</param>
        /// <param name="occurrence">Defines how the term is added to this query</param>
        /// <param name="includeStartValue">Determines if the starting term is included or excluded in the search results</param>
        /// <param name="includeEndValue">Determines if the ending term is included or excluded in the search results</param>
        /// <param name="boost">The amount of boost to apply to this term</param>
        public void AddFloatRangeClause(string fieldName, float startValue, float endValue, ClauseOccurrence occurrence, bool includeStartValue, bool includeEndValue, float boost)
        {
            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentNullException("fieldName", "fieldName cannot be null or empty");
            if (boost < StaticValues.MinimumAllowedBoost)
                throw new ArgumentOutOfRangeException("boost", "boost cannot be less than " + StaticValues.MinimumAllowedBoost);
            IncrementTotalClauses(1);
            NumericRangeQuery rangeQuery = NumericRangeQuery.NewFloatRange(fieldName, startValue, endValue, includeStartValue, includeEndValue);
            rangeQuery.SetBoost(boost);
            this.luceneQuery.Add(rangeQuery, TypeConverter.ConvertToLuceneClauseOccurrence(occurrence));
            rangeQuery = null;
        }

        /// <summary>
        /// Adds a fuzzy clause to this instance
        /// </summary>
        /// <remarks>Fuzzy clauses find results within a particular relevance distance of each hit</remarks>
        /// <param name="term">Term to add to this query</param>
        public void AddFuzzyClause(SearchTerm term)
        {
            this.AddFuzzyClause(term, ClauseOccurrence.Default, FuzzyQuery.defaultMinSimilarity);
        }

        /// <summary>
        /// Adds a fuzzy clause to this instance
        /// </summary>
        /// <remarks>Fuzzy clauses find results within a particular relevance distance of each hit</remarks>
        /// <param name="term">Term to add to this query</param>
        /// <param name="occurrence">Defines how the term is added to this query</param>
        public void AddFuzzyClause(SearchTerm term, ClauseOccurrence occurrence)
        {
            this.AddFuzzyClause(term, occurrence, FuzzyQuery.defaultMinSimilarity);
        }

        /// <summary>
        /// Adds a fuzzy clause to this instance
        /// </summary>
        /// <remarks>Fuzzy clauses find results within a particular relevance distance of each hit</remarks>
        /// <param name="term">Term to add to this query</param>
        /// <param name="occurrence">Defines how the term is added to this query</param>
        /// <param name="minimumSimilarity">Defines the amount of similarity that is allowed between matches</param>
        public void AddFuzzyClause(SearchTerm term, ClauseOccurrence occurrence, float minimumSimilarity)
        {
            if (term == null)
                throw new ArgumentNullException("term", "term cannot be null");
            IncrementTotalClauses(1);
            FuzzyQuery fuzzyQuery = new FuzzyQuery(term.GetLuceneTerm(), minimumSimilarity);
            fuzzyQuery.SetBoost(term.Boost);
            this.luceneQuery.Add(fuzzyQuery, TypeConverter.ConvertToLuceneClauseOccurrence(occurrence));
            fuzzyQuery = null;
        }

        /// <summary>
        /// Adds an integer range clause to this instance.
        /// </summary>
        /// <param name="fieldName">Name of the field to add.</param>
        /// <param name="startValue">The start value of the range to search.</param>
        /// <param name="endValue">The end value of the range to search.</param>
        public void AddIntRangeClause(string fieldName, int startValue, int endValue)
        {
            this.AddIntRangeClause(fieldName, startValue, endValue, ClauseOccurrence.Default, true, true, StaticValues.DefaultBoost);
        }

        /// <summary>
        /// Adds an integer range clause to this instance.
        /// </summary>
        /// <param name="fieldName">Name of the field to add.</param>
        /// <param name="startValue">The start value of the range to search.</param>
        /// <param name="endValue">The end value of the range to search.</param>
        /// <param name="occurrence">Defines how the term is added to this query</param>
        public void AddIntRangeClause(string fieldName, int startValue, int endValue, ClauseOccurrence occurrence)
        {
            this.AddIntRangeClause(fieldName, startValue, endValue, occurrence, true, true, StaticValues.DefaultBoost);
        }

        /// <summary>
        /// Adds an integer range clause to this instance.
        /// </summary>
        /// <param name="fieldName">Name of the field to add.</param>
        /// <param name="startValue">The start value of the range to search.</param>
        /// <param name="endValue">The end value of the range to search.</param>
        /// <param name="occurrence">Defines how the term is added to this query</param>
        /// <param name="includeStartValue">Determines if the starting term is included or excluded in the search results</param>
        /// <param name="includeEndValue">Determines if the ending term is included or excluded in the search results</param>
        public void AddIntRangeClause(string fieldName, int startValue, int endValue, ClauseOccurrence occurrence, bool includeStartValue, bool includeEndValue)
        {
            this.AddIntRangeClause(fieldName, startValue, endValue, occurrence, includeStartValue, includeEndValue, StaticValues.DefaultBoost);
        }

        /// <summary>
        /// Adds an integer range clause to this instance.
        /// </summary>
        /// <param name="fieldName">Name of the field to add.</param>
        /// <param name="startValue">The start value of the range to search.</param>
        /// <param name="endValue">The end value of the range to search.</param>
        /// <param name="occurrence">Defines how the term is added to this query</param>
        /// <param name="includeStartValue">Determines if the starting term is included or excluded in the search results</param>
        /// <param name="includeEndValue">Determines if the ending term is included or excluded in the search results</param>
        /// <param name="boost">The amount of boost to apply to this term</param>
        public void AddIntRangeClause(string fieldName, int startValue, int endValue, ClauseOccurrence occurrence, bool includeStartValue, bool includeEndValue, float boost)
        {
            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentNullException("fieldName", "fieldName cannot be null or empty");
            if (boost < StaticValues.MinimumAllowedBoost)
                throw new ArgumentOutOfRangeException("boost", "boost cannot be less than " + StaticValues.MinimumAllowedBoost);
            IncrementTotalClauses(1);
            NumericRangeQuery rangeQuery = NumericRangeQuery.NewIntRange(fieldName, startValue, endValue, includeStartValue, includeEndValue);
            rangeQuery.SetBoost(boost);
            this.luceneQuery.Add(rangeQuery, TypeConverter.ConvertToLuceneClauseOccurrence(occurrence));
            rangeQuery = null;
        }

        /// <summary>
        /// Adds a long range clause to this instance.
        /// </summary>
        /// <param name="fieldName">Name of the field to add.</param>
        /// <param name="startValue">The start value of the range to search.</param>
        /// <param name="endValue">The end value of the range to search.</param>
        public void AddLongRangeClause(string fieldName, long startValue, long endValue)
        {
            this.AddLongRangeClause(fieldName, startValue, endValue, ClauseOccurrence.Default, true, true, StaticValues.DefaultBoost);
        }

        /// <summary>
        /// Adds a long range clause to this instance.
        /// </summary>
        /// <param name="fieldName">Name of the field to add.</param>
        /// <param name="startValue">The start value of the range to search.</param>
        /// <param name="endValue">The end value of the range to search.</param>
        /// <param name="occurrence">Defines how the term is added to this query</param>
        public void AddLongRangeClause(string fieldName, long startValue, long endValue, ClauseOccurrence occurrence)
        {
            this.AddLongRangeClause(fieldName, startValue, endValue, occurrence, true, true, StaticValues.DefaultBoost);
        }

        /// <summary>
        /// Adds a long range clause to this instance.
        /// </summary>
        /// <param name="fieldName">Name of the field to add.</param>
        /// <param name="startValue">The start value of the range to search.</param>
        /// <param name="endValue">The end value of the range to search.</param>
        /// <param name="occurrence">Defines how the term is added to this query</param>
        /// <param name="includeStartValue">Determines if the starting term is included or excluded in the search results</param>
        /// <param name="includeEndValue">Determines if the ending term is included or excluded in the search results</param>
        public void AddLongRangeClause(string fieldName, long startValue, long endValue, ClauseOccurrence occurrence, bool includeStartValue, bool includeEndValue)
        {
            this.AddLongRangeClause(fieldName, startValue, endValue, occurrence, includeStartValue, includeEndValue, StaticValues.DefaultBoost);
        }

        /// <summary>
        /// Adds a long range clause to this instance.
        /// </summary>
        /// <param name="fieldName">Name of the field to add.</param>
        /// <param name="startValue">The start value of the range to search.</param>
        /// <param name="endValue">The end value of the range to search.</param>
        /// <param name="occurrence">Defines how the term is added to this query</param>
        /// <param name="includeStartValue">Determines if the starting term is included or excluded in the search results</param>
        /// <param name="includeEndValue">Determines if the ending term is included or excluded in the search results</param>
        /// <param name="boost">The amount of boost to apply to this term</param>
        public void AddLongRangeClause(string fieldName, long startValue, long endValue, ClauseOccurrence occurrence, bool includeStartValue, bool includeEndValue, float boost)
        {
            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentNullException("fieldName", "fieldName cannot be null or empty");
            if (boost < StaticValues.MinimumAllowedBoost)
                throw new ArgumentOutOfRangeException("boost", "boost cannot be less than " + StaticValues.MinimumAllowedBoost);
            IncrementTotalClauses(1);
            NumericRangeQuery rangeQuery = NumericRangeQuery.NewLongRange(fieldName, startValue, endValue, includeStartValue, includeEndValue);
            rangeQuery.SetBoost(boost);
            this.luceneQuery.Add(rangeQuery, TypeConverter.ConvertToLuceneClauseOccurrence(occurrence));
            rangeQuery = null;
        }

        /// <summary>
        /// Adds a custom Lucene query string to this builder by running it through an analyzer
        /// </summary>
        /// <param name="queryText">The Lucene query text.</param>
        /// <returns>True if query string is parsed and appended successfully</returns>
        public bool AddStringQuery(string queryText)
        {
            return AddStringQuery(queryText, ClauseOccurrence.Default, AnalyzerType.Default, this.totalClauses > 0);
        }

        /// <summary>
        /// Adds the string query.
        /// </summary>
        /// <param name="queryText">The query text.</param>
        /// <param name="occurrence">The occurrence.</param>
        /// <returns>True if query string is parsed and appended successfully</returns>
        public bool AddStringQuery(string queryText, ClauseOccurrence occurrence)
        {
            return AddStringQuery(queryText, occurrence, AnalyzerType.Default, this.totalClauses > 0);
        }

        /// <summary>
        /// Adds the string query.
        /// </summary>
        /// <param name="queryText">The query text.</param>
        /// <param name="occurrence">The occurrence.</param>
        /// <param name="analyzerType">Type of the analyzer.</param>
        /// <returns>True if query string is parsed and appended successfully</returns>
        public bool AddStringQuery(string queryText, ClauseOccurrence occurrence, AnalyzerType analyzerType)
        {
            return AddStringQuery(queryText, occurrence, analyzerType, this.totalClauses > 0);
        }

        /// <summary>
        /// Adds the string query.
        /// </summary>
        /// <param name="queryText">The query text.</param>
        /// <param name="occurrence">The occurrence.</param>
        /// <param name="analyzerType">Type of the analyzer.</param>
        /// <param name="merge">if set to <c>true</c> [merge].</param>
        /// <returns>True if query string is parsed and appended successfully</returns>
        public bool AddStringQuery(string queryText, ClauseOccurrence occurrence, AnalyzerType analyzerType, bool merge)
        {
            if (string.IsNullOrEmpty(queryText))
                throw new ArgumentNullException("queryText", "queryText cannot be null or empty");
            // this try catch is here to protect you from lucene specific exceptions
            bool success = true;
            IncrementTotalClauses(1);
            try {
                Lucene29.Net.QueryParsers.QueryParser parser = new Lucene29.Net.QueryParsers.QueryParser(StaticValues.LibraryVersion, "QueryParser", TypeConverter.GetAnalyzer(analyzerType));
                Query query = parser.Parse(queryText);
                if (query == null) {
                    success = false;
                }
                else {
                    if (merge)
                        this.luceneQuery.Combine(new Query[] { query });
                    else
                        this.luceneQuery.Add(query, TypeConverter.ConvertToLuceneClauseOccurrence(occurrence));
                }
            }
            catch (Exception) {
                //System.Diagnostics.Debug.WriteLine("Lucene exception -> " + ex.Message);
                success = false;
                this.totalClauses--;
            }
            return success;
        }

        /// <summary>
        /// Adds a range clause to this instance
        /// </summary>
        /// <remarks>Adds a range to search into the query</remarks>
        /// <remarks>All values specified in the startTerm are applied to this entire clause</remarks>
        /// <param name="startTerm">Starting half of the range to add to this query</param>
        /// <param name="endValue">Ending half of the range to add to this query</param>
        public void AddTermRangeClause(SearchTerm startTerm, string endValue)
        {
            this.AddTermRangeClause(startTerm, endValue, ClauseOccurrence.Default, true, true);
        }

        /// <summary>
        /// Adds a range clause to this instance
        /// </summary>
        /// <remarks>Adds a range to search into the query</remarks>
        /// <remarks>All values specified in the startTerm are applied to this entire clause</remarks>
        /// <param name="startTerm">Starting half of the range to add to this query</param>
        /// <param name="endValue">Ending half of the range to add to this query</param>
        /// <param name="occurrence">Defines how the term is added to this query</param>
        public void AddTermRangeClause(SearchTerm startTerm, string endValue, ClauseOccurrence occurrence)
        {
            this.AddTermRangeClause(startTerm, endValue, occurrence, true, true);
        }

        /// <summary>
        /// Adds a range clause to this instance
        /// </summary>
        /// <remarks>Adds a range to search into the query</remarks>
        /// <remarks>All values specified in the startTerm are applied to this entire clause</remarks>
        /// <param name="startTerm">Starting half of the range to add to this query</param>
        /// <param name="endValue">Ending half of the range to add to this query</param>
        /// <param name="occurrence">Defines how the term is added to this query</param>
        /// <param name="includeStartValue">Determines if the starting term is included or excluded in the search results</param>
        /// <param name="includeEndValue">Determines if the ending term is included or excluded in the search results</param>
        public void AddTermRangeClause(SearchTerm startTerm, string endValue, ClauseOccurrence occurrence, bool includeStartValue, bool includeEndValue)
        {
            if (startTerm == null)
                throw new ArgumentNullException("startTerm", "startTerm cannot be null");
            if (string.IsNullOrEmpty(endValue))
                throw new ArgumentNullException("endValue", "endValue cannot be null or empty");
            IncrementTotalClauses(1);
            TermRangeQuery rangeQuery = new TermRangeQuery(startTerm.FieldName, startTerm.FieldValue, endValue, includeStartValue, includeEndValue);
            rangeQuery.SetBoost(startTerm.Boost);
            this.luceneQuery.Add(rangeQuery, TypeConverter.ConvertToLuceneClauseOccurrence(occurrence));
            rangeQuery = null;
        }

        /// <summary>
        /// Adds a wildcard clause to this instance
        /// </summary>
        /// <remarks>Wildcard clauses include '*' and '?' character</remarks>
        /// <param name="term">Term to add to this query</param>
        public void AddWildcardClause(SearchTerm term)
        {
            this.AddWildcardClause(term, ClauseOccurrence.Default);
        }

        /// <summary>
        /// Adds a wildcard clause to this instance
        /// </summary>
        /// <remarks>Wildcard clauses include '*' and '?' character</remarks>
        /// <param name="term">Term to add to this query</param>
        /// <param name="occurrence">Defines how the term is added to this query</param>
        public void AddWildcardClause(SearchTerm term, ClauseOccurrence occurrence)
        {
            if (term == null)
                throw new ArgumentNullException("term", "term cannot be null");
            IncrementTotalClauses(1);
            WildcardQuery wildcardQuery = new WildcardQuery(term.GetLuceneTerm());
            wildcardQuery.SetBoost(term.Boost);
            this.luceneQuery.Add(wildcardQuery, TypeConverter.ConvertToLuceneClauseOccurrence(occurrence));
            wildcardQuery = null;
        }

        /// <summary>
        /// Takes the manaually generated query and runs it through a Lucene analyzer
        /// </summary>
        public void Analyze()
        {
            Analyze(AnalyzerType.Default, ClauseOccurrence.Default);
        }

        /// <summary>
        /// Takes the manaually generated query and runs it through a specified Lucene analyzer
        /// </summary>
        /// <param name="analyzerType">Lucene analyzer to run current query through</param>
        public void Analyze(AnalyzerType analyzerType)
        {
            Analyze(analyzerType, ClauseOccurrence.Default);
        }

        /// <summary>
        /// Takes the manaually generated query and runs it through a specified Lucene analyzer
        /// </summary>
        /// <param name="analyzerType">Lucene analyzer to run current query through</param>
        /// <param name="occurrence">Occurrence type of this query</param>
        public void Analyze(AnalyzerType analyzerType, ClauseOccurrence occurrence)
        {
            if (analyzerType == AnalyzerType.None)
                throw new ArgumentException("analyzerType cannot be set to None", "analyzerType");
            if (analyzerType == AnalyzerType.Unknown)
                throw new ArgumentException("analyzerType cannot be set to Unknown", "analyzerType");
            Analyze(TypeConverter.GetAnalyzer(analyzerType), occurrence);
        }

        /// <summary>
        /// Appends the clauses from the specified builder into this one.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <remarks>
        /// Append is simply a way of adding another query to this boolean query
        /// </remarks>
        public void Append(QueryBuilder builder)
        {
            Append(builder, ClauseOccurrence.Default);
        }

        /// <summary>
        /// Appends the clauses from the specified builder into this one.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="occurrence">The occurrence.</param>
        /// /// <remarks>
        /// Append is simply a way of adding another query to this boolean query
        /// </remarks>
        public void Append(QueryBuilder builder, ClauseOccurrence occurrence)
        {
            if (builder == null)
                throw new ArgumentNullException("builder", "QueryBuilder cannot be null");
            if (builder.luceneQuery == null)
                throw new ArgumentException("QueryBuilder is not valid", "builder");
            if (builder.TotalClauses == 0)
                return;
            IncrementTotalClauses(builder.totalClauses);
            this.luceneQuery.Add(builder.luceneQuery, TypeConverter.ConvertToLuceneClauseOccurrence(occurrence));
        }

        /// <summary>
        /// Appends the clauses from the specified builders into this one.
        /// </summary>
        /// <param name="builders">The builders.</param>
        /// /// <remarks>
        /// Append is simply a way of adding another query to this boolean query
        /// </remarks>
        public void Append(QueryBuilder[] builders)
        {
            Append(builders, ClauseOccurrence.Default);
        }

        /// <summary>
        /// Appends the clauses from the specified builders into this one.
        /// </summary>
        /// <param name="builders">The builders.</param>
        /// <param name="occurrence">The occurrence.</param>
        /// /// <remarks>
        /// Append is simply a way of adding another query to this boolean query
        /// </remarks>
        public void Append(QueryBuilder[] builders, ClauseOccurrence occurrence)
        {
            if (builders == null)
                throw new ArgumentNullException("builders", "builders cannot be null");
            int totalBuilders = builders.Length;
            for (int i = 0; i < totalBuilders; i++)
                Append(builders[i], occurrence);
        }

        /// <summary>
        /// Clears all clauses from this instance
        /// </summary>
        public void Clear()
        {
            this.luceneQuery = new BooleanQuery();
            this.totalClauses = 0;
        }

        /// <summary>
        /// Merges the specified builder with this one.
        /// </summary>
        /// <param name="builder">The QueryBuilders to merge into this one.</param>
        public void Merge(QueryBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException("builder", "builder cannot be null");
            if (builder.TotalClauses == 0)
                return;
            IncrementTotalClauses(builder.totalClauses);
            this.luceneQuery = (BooleanQuery)this.luceneQuery.Combine(new Query[] { this.luceneQuery, builder.luceneQuery });
        }

        /// <summary>
        /// Merges the specified builders with this one.
        /// </summary>
        /// <param name="builders">The QueryBuilders to merge into this one.</param>
        public void Merge(QueryBuilder[] builders)
        {
            if (builders == null)
                throw new ArgumentNullException("builders", "builders cannot be null");
            int totalBuilders = builders.Length;
            System.Collections.Generic.List<Query> queries = new System.Collections.Generic.List<Query>(totalBuilders);
            for (int i = 0; i < totalBuilders; i++) {
                if (builders[i].totalClauses == 0)
                    continue;
                IncrementTotalClauses(builders[i].totalClauses);
                queries.Add(builders[i].luceneQuery);
            }

            if (queries.Count > 0)
                this.luceneQuery.Combine(queries.ToArray());
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override sealed string ToString()
        {
            if (this.luceneQuery == null || this.totalClauses == 0)
                return "This QueryBuilder contains no clauses";
            return this.luceneQuery.ToString();
        }

        /// <summary>
        /// Takes the manaually generated query and runs it through a Lucene analyzer
        /// </summary>
        /// <param name="analyzer">Analyzer to use when parsing this query</param>
        /// <param name="occurrence">Occurrence type of this query</param>
        internal void Analyze(Lucene29.Net.Analysis.Analyzer analyzer, ClauseOccurrence occurrence)
        {
            if (analyzer == null)
                throw new ArgumentNullException("analyzer", "Analyzer cannot be null");

            try {
                AnalyzerType requestedType = TypeConverter.GetAnalyzerType(analyzer);
                if (cachedAnalyzer != requestedType) {
                    lock (syncRoot) {
                        if (cachedAnalyzer != requestedType) {
                            cachedParser = new Lucene29.Net.QueryParsers.QueryParser(StaticValues.LibraryVersion, "Analyzer", analyzer);
                            cachedAnalyzer = requestedType;
                            cachedParser.SetAllowLeadingWildcard(this.allowLeadingWildcard);
                        }
                    }
                }

                Query query = cachedParser.Parse(this.luceneQuery.ToString());
                this.luceneQuery = null;
                this.luceneQuery = new BooleanQuery(this.disableCoord);
                this.luceneQuery.Add(query, TypeConverter.ConvertToLuceneClauseOccurrence(occurrence));
            }
            catch (Exception ex) {
                throw new FormatException("There was an unexpected exception thrown during the analyzing process of the instance.", ex);
            }
        }

        /// <summary>
        /// Increments the total clauses count to ensure we're under the quota
        /// </summary>
        /// <param name="clausesAdded">The number of clauses to be added.</param>
        private void IncrementTotalClauses(int clausesAdded)
        {
            if (this.totalClauses > StaticValues.TotalAllowedClauses)
                throw new OverflowException("A QueryBuilder cannot contain more than " + StaticValues.TotalAllowedClauses.ToString() + " clauses");
            else if (this.totalClauses + clausesAdded > StaticValues.TotalAllowedClauses)
                throw new OverflowException("A QueryBuilder cannot contain more than " + StaticValues.TotalAllowedClauses.ToString() + " clauses");
            this.totalClauses += clausesAdded;
        }

        #endregion Methods
    }
}