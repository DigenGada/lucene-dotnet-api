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
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using IndexLibrary.Analysis;
    using IndexLibrary.Interfaces;

    using Lucene29.Net.Documents;

    /// <summary>
    /// Represents a searcher object that opens an index, executes a query returns results, and closes.
    /// </summary>
    [System.CLSCompliant(true)]
    public class IndexSearcher : IIndexSearcher<SearcherEventArgs>
    {
        #region Fields

        protected IIndex index;

        #endregion Fields

        #region Constructors

        public IndexSearcher(IIndex index)
        {
            if (index == null)
                throw new ArgumentNullException("index", "index cannot be null");
            if (index.IndexStructure == IndexType.None)
                throw new ArgumentException("index.IndexStructure cannot be None", "index");
            this.index = index;
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Occurs when a search begins.
        /// </summary>
        public event EventHandler<SearcherEventArgs> BeginSearch;

        /// <summary>
        /// Occurs when a search ends.
        /// </summary>
        public event EventHandler<SearcherEventArgs> EndSearch;

        /// <summary>
        /// Occurs when a search result is found.
        /// </summary>
        public event EventHandler<SearcherEventArgs> SearchResultFound;

        #endregion Events

        #region Properties

        public IIndex Index
        {
            get { return this.index; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Performs a full search against an index.
        /// </summary>
        /// <param name="builder">The query to run against the index.</param>
        /// <param name="totalResultsRequested">The total number of results to return.</param>
        /// <param name="fieldFilterNames">An exclusive list of fields to retrieve from the index.</param>
        /// <returns>A <see cref="IndexLibrary.SearchResultDataSet"/> that contains distinct values for each specified field and all search results</returns>
        /// <remarks>
        /// Used when you want to create filter type results such as http://www.kayak.com when you perform
        /// a search for an airline. A list of distinct values for all fields is displayed so you can filter down
        /// your results. This method performs the same functionality.
        /// </remarks>
        public virtual SearchResultDataSet FullSearch(QueryBuilder builder, int totalResultsRequested, string[] fieldFilterNames)
        {
            if (builder == null)
                throw new ArgumentNullException("builder", "builder cannot be null");
            SearchResultDataSet dataSet = new SearchResultDataSet();
            if (OnBeginSearch(new SearcherEventArgs(this.index.IndexDirectory.Name, this.index.IndexStructure, SearchMethodType.Full, SearchMethodLocation.Beginning, null))) {
                OnEndSearch(new SearcherEventArgs(this.index.IndexDirectory.Name, this.index.IndexStructure, SearchMethodType.Full, SearchMethodLocation.Ending, null));
                LibraryAnalysis.Fire(new SearchInfo(this.index.IndexDirectory.Name, builder.ToString(), SearchMethodType.Full, 0, true));
                return dataSet;
            }

            this.index.Refresh();
            DirectoryInfo searchDirectory = null;
            bool hasIndexFiles = GetIndexReadDirectory(out searchDirectory);

            bool getAllFilters = (fieldFilterNames == null || fieldFilterNames.Length == 0);
            if (fieldFilterNames == null)
                fieldFilterNames = new string[] { };
            if ((totalResultsRequested < -1 || totalResultsRequested == 0) || (!hasIndexFiles)) {
                OnEndSearch(new SearcherEventArgs(this.index.IndexDirectory.Name, this.index.IndexStructure, SearchMethodType.Full, SearchMethodLocation.Ending, null));
                LibraryAnalysis.Fire(new SearchInfo(this.index.IndexDirectory.Name, builder.ToString(), SearchMethodType.Full, 0, true));
                return dataSet;
            }

            Lucene29.Net.Search.IndexSearcher searcher = null;
            Lucene29.Net.Store.Directory directory = null;
            int hitsLength = 0;
            bool canceled = false;
            try {
                directory = Lucene29.Net.Store.FSDirectory.Open(searchDirectory);
                searcher = new Lucene29.Net.Search.IndexSearcher(directory, true);
                Lucene29.Net.Search.TopDocs topDocs = searcher.Search(builder.GetLuceneQuery, (totalResultsRequested == -1) ? StaticValues.DefaultDocsToReturn : totalResultsRequested);

                hitsLength = (totalResultsRequested == -1) ? topDocs.totalHits : Math.Min(topDocs.totalHits, totalResultsRequested);
                if (hitsLength > 5000)
                    hitsLength = 5000;
                int resultsFound = 0;
                for (int i = 0; i < hitsLength; i++) {
                    int docID = topDocs.scoreDocs[i].doc;
                    Document document = searcher.Doc(docID);
                    System.Collections.IList fieldList = document.GetFields();
                    Dictionary<string, string> values = new Dictionary<string, string>();
                    int totalValues = 0;
                    int totalFields = fieldList.Count;
                    for (int j = 0; j < totalFields; j++) {
                        if (fieldList[j] == null)
                            continue;
                        Field field = fieldList[j] as Field;
                        string name = field.Name();
                        string value = field.StringValue();
                        field = null;

                        if (getAllFilters || fieldFilterNames.Contains(name)) {
                            SearchResultFilter filter = null;
                            if (!dataSet.ContainsFilter(name)) {
                                filter = new SearchResultFilter(name);
                                dataSet.AddFilter(filter);
                            }
                            else {
                                filter = dataSet.GetFilter(name);
                            }

                            if (!filter.ContainsKey(value))
                                filter.AddValue(new KeyValuePair<string, bool>(value, true));
                        }

                        if (values.ContainsKey(name)) {
                            int revision = 1;
                            while (values.ContainsKey(name + "(" + revision.ToString() + ")"))
                                revision++;
                            name += "(" + revision.ToString() + ")";
                        }
                        values.Add(name, value);
                        ++totalValues;
                    }
                    if (totalValues > 0) {
                        SearchResult result = new SearchResult(values, this.index.IndexDirectory.Name, topDocs.scoreDocs[i].score);
                        dataSet.AddSearchResult(result);
                        resultsFound++;
                        if (OnSearchResultFound(new SearcherEventArgs(this.index.IndexDirectory.Name, this.index.IndexStructure, SearchMethodType.Full, SearchMethodLocation.ResultFound, result))) {
                            canceled = true;
                            break;
                        }
                    }
                }
            }
            finally {
                if (searcher != null)
                    searcher.Close();
                if (directory != null)
                    directory.Close();

                OnEndSearch(new SearcherEventArgs(this.index.IndexDirectory.Name, this.index.IndexStructure, SearchMethodType.Full, SearchMethodLocation.Ending, null));
                LibraryAnalysis.Fire(new SearchInfo(this.index.IndexDirectory.Name, builder.ToString(), SearchMethodType.Full, hitsLength, canceled));
            }

            return dataSet;
        }

        /// <summary>
        /// Performs a full search against an index.
        /// </summary>
        /// <param name="builder">The query to run against the index.</param>
        /// <param name="totalResultsRequested">The total number of results to return.</param>
        /// <returns>A <see cref="IndexLibrary.SearchResultDataSet"/> that contains distinct values for each specified field and all search results</returns>
        /// <remarks>
        /// Used when you want to create filter type results such as http://www.kayak.com when you perform
        /// a search for an airline. A list of distinct values for all fields is displayed so you can filter down
        /// your results. This method performs the same functionality.
        /// </remarks>
        public virtual SearchResultDataSet FullSearch(QueryBuilder builder, int totalResultsRequested)
        {
            return FullSearch(builder, totalResultsRequested, null);
        }

        /// <summary>
        /// Performs a full search against an index.
        /// </summary>
        /// <param name="builder">The query to run against the index.</param>
        /// <returns>A <see cref="IndexLibrary.SearchResultDataSet"/> that contains distinct values for each specified field and all search results</returns>
        /// <remarks>
        /// Used when you want to create filter type results such as http://www.kayak.com when you perform
        /// a search for an airline. A list of distinct values for all fields is displayed so you can filter down
        /// your results. This method performs the same functionality.
        /// </remarks>
        public virtual SearchResultDataSet FullSearch(QueryBuilder builder)
        {
            return FullSearch(builder, -1, null);
        }

        public long GetTotalDocumentsInIndex()
        {
            // okay, open reader or searcher and get max docs
            DirectoryInfo searchDirectory = null;
            bool hasIndexFiles = false;
            switch (index.IndexStructure) {
                case IndexType.SingleIndex:
                    IIndex singleIndex = (IIndex)index;
                    searchDirectory = singleIndex.IndexDirectory;
                    hasIndexFiles = singleIndex.HasIndexFiles();
                    break;
                case IndexType.DoubleIndex:
                case IndexType.CyclicalIndex:
                    IDoubleIndex doubleIndex = (IDoubleIndex)index;
                    searchDirectory = doubleIndex.GetReadDirectory();
                    hasIndexFiles = doubleIndex.HasIndexFiles();
                    break;
                default:
                    throw new NotImplementedException(index.IndexStructure.ToString() + " not supported");
            }

            if (!hasIndexFiles)
                return -1L;

            Lucene29.Net.Store.Directory directory = null;
            Lucene29.Net.Search.IndexSearcher searcher = null;
            try {
                directory = Lucene29.Net.Store.FSDirectory.Open(searchDirectory);
                searcher = new Lucene29.Net.Search.IndexSearcher(directory, true);
                return searcher.MaxDoc();
            }
            catch {
                return -2L;
            }
            finally {
                if (searcher != null)
                    searcher.Close();
                if (directory != null)
                    directory.Close();
            }
        }

        /// <summary>
        /// Performs a quick search against an index.
        /// </summary>
        /// <param name="builder">The query to run against the index.</param>
        /// <param name="fieldName">Name of the field to return from the index.</param>
        /// <param name="totalResultsRequested">The total number of results to return.</param>
        /// <returns>A list of strings, one for each result found</returns>
        /// <remarks>
        /// Designed to be used for 'quick' type searches such as those used for type-ahead features or 
        /// searches where only a single column needs to be returned.
        /// </remarks>
        public IEnumerable<string> QuickSearch(QueryBuilder builder, string fieldName, int totalResultsRequested)
        {
            if (builder == null)
                throw new ArgumentNullException("builder", "builder cannot be null");
            List<string> results = new List<string>();
            if (OnBeginSearch(new SearcherEventArgs(this.index.IndexDirectory.Name, this.index.IndexStructure, SearchMethodType.Quick, SearchMethodLocation.Beginning, null))) {
                OnEndSearch(new SearcherEventArgs(this.index.IndexDirectory.Name, this.index.IndexStructure, SearchMethodType.Quick, SearchMethodLocation.Ending, null));
                LibraryAnalysis.Fire(new SearchInfo(this.index.IndexDirectory.Name, builder.ToString(), SearchMethodType.Quick, 0, true));
                return results;
            }
            this.index.Refresh();

            DirectoryInfo searchDirectory = null;
            bool hasIndexFiles = GetIndexReadDirectory(out searchDirectory);

            if ((totalResultsRequested < -1 || totalResultsRequested == 0) || (!hasIndexFiles)) {
                OnEndSearch(new SearcherEventArgs(this.index.IndexDirectory.Name, this.index.IndexStructure, SearchMethodType.Quick, SearchMethodLocation.Ending, null));
                LibraryAnalysis.Fire(new SearchInfo(this.index.IndexDirectory.Name, builder.ToString(), SearchMethodType.Quick, 0, true));
                return results;
            }

            Lucene29.Net.Store.Directory directory = null;
            Lucene29.Net.Search.IndexSearcher searcher = null;
            int hitsLength = 0;
            bool canceled = false;
            try {
                directory = Lucene29.Net.Store.FSDirectory.Open(searchDirectory);
                searcher = new Lucene29.Net.Search.IndexSearcher(directory, true);
                Lucene29.Net.Search.TopDocs topDocs = searcher.Search(builder.GetLuceneQuery, (totalResultsRequested == -1) ? StaticValues.DefaultDocsToReturn : totalResultsRequested);

                hitsLength = (totalResultsRequested == -1) ? topDocs.totalHits : Math.Min(topDocs.totalHits, totalResultsRequested);
                if (hitsLength > 5000)
                    hitsLength = 5000;
                int resultsFound = 0;
                for (int i = 0; i < hitsLength; i++) {
                    int docID = topDocs.scoreDocs[i].doc;
                    Document document = searcher.Doc(docID);
                    System.Collections.IList fieldList = document.GetFields();
                    //Dictionary<string, string> values = new Dictionary<string, string>();
                    int totalFields = fieldList.Count;
                    Field collectedField = null;
                    for (int j = 0; j < totalFields; j++) {
                        if (fieldList[j] == null)
                            continue;
                        collectedField = fieldList[j] as Field;
                        if (collectedField.Name().Equals(fieldName, StringComparison.CurrentCultureIgnoreCase))
                            break;
                        collectedField = null;
                    }
                    if (collectedField != null) {
                        results.Add(collectedField.StringValue());
                        resultsFound++;
                        if (OnSearchResultFound(new SearcherEventArgs(this.index.IndexDirectory.Name, this.index.IndexStructure, SearchMethodType.Quick, SearchMethodLocation.ResultFound, collectedField.Name(), collectedField.StringValue(), topDocs.scoreDocs[i].score))) {
                            canceled = true;
                            break;
                        }
                    }
                }
            }
            finally {
                if (searcher != null)
                    searcher.Close();
                if (directory != null)
                    directory.Close();

                OnEndSearch(new SearcherEventArgs(this.index.IndexDirectory.Name, this.index.IndexStructure, SearchMethodType.Quick, SearchMethodLocation.Ending, null));
                LibraryAnalysis.Fire(new SearchInfo(this.index.IndexDirectory.Name, builder.ToString(), SearchMethodType.Quick, hitsLength, canceled));
            }

            return results;
        }

        /// <summary>
        /// Performs a quick search against an index.
        /// </summary>
        /// <param name="builder">The query to run against the index.</param>
        /// <param name="fieldName">Name of the field to return from the index.</param>
        /// <returns>A list of strings, one for each result found</returns>
        /// <remarks>
        /// Designed to be used for 'quick' type searches such as those used for type-ahead features or 
        /// searches where only a single column needs to be returned.
        /// </remarks>
        public IEnumerable<string> QuickSearch(QueryBuilder builder, string fieldName)
        {
            return QuickSearch(builder, fieldName, -1);
        }

        /// <summary>
        /// Performs a search against an index.
        /// </summary>
        /// <param name="builder">The query to run against the index.</param>
        /// <param name="totalResultsRequested">The total number of results to return.</param>
        /// <returns>a list of <see cref="IndexLibrary.SearchResult"/>, one for each result found.</returns>
        /// <remarks>
        /// Designed to be used for the majority of searches. The functionality contained in this 
        /// method is used for a normal search, i.e. enter a term, search, get results. 
        /// </remarks>
        public virtual IEnumerable<SearchResult> Search(QueryBuilder builder, int totalResultsRequested)
        {
            if (builder == null)
                throw new ArgumentNullException("builder", "builder cannot be null");
            List<SearchResult> results = new List<SearchResult>();
            if (OnBeginSearch(new SearcherEventArgs(this.index.IndexDirectory.Name, this.index.IndexStructure, SearchMethodType.Normal, SearchMethodLocation.Beginning, null))) {
                OnEndSearch(new SearcherEventArgs(this.index.IndexDirectory.Name, this.index.IndexStructure, SearchMethodType.Normal, SearchMethodLocation.Ending, null));
                LibraryAnalysis.Fire(new SearchInfo(this.index.IndexDirectory.Name, builder.ToString(), SearchMethodType.Normal, 0, true));
                return results;
            }

            this.index.Refresh();
            DirectoryInfo searchDirectory = null;
            bool hasIndexFiles = GetIndexReadDirectory(out searchDirectory);

            if ((totalResultsRequested < -1 || totalResultsRequested == 0) || (!hasIndexFiles)) {
                OnEndSearch(new SearcherEventArgs(this.index.IndexDirectory.Name, this.index.IndexStructure, SearchMethodType.Normal, SearchMethodLocation.Ending, null));
                LibraryAnalysis.Fire(new SearchInfo(this.index.IndexDirectory.Name, builder.ToString(), SearchMethodType.Normal, 0, true));
                return results;
            }

            Lucene29.Net.Store.Directory directory = null;
            Lucene29.Net.Search.IndexSearcher searcher = null;
            int hitsLength = 0;
            bool canceled = false;
            try {
                directory = Lucene29.Net.Store.FSDirectory.Open(searchDirectory);
                searcher = new Lucene29.Net.Search.IndexSearcher(directory, true);
                Lucene29.Net.Search.TopDocs topDocs = searcher.Search(builder.GetLuceneQuery, (totalResultsRequested == -1) ? StaticValues.DefaultDocsToReturn : totalResultsRequested);

                hitsLength = (totalResultsRequested == -1) ? topDocs.totalHits : Math.Min(topDocs.totalHits, totalResultsRequested);
                if (hitsLength > 5000)
                    hitsLength = 5000;
                int resultsFound = 0;
                for (int i = 0; i < hitsLength; i++) {
                    int docID = topDocs.scoreDocs[i].doc;
                    Document document = searcher.Doc(docID);
                    System.Collections.IList fieldList = document.GetFields();
                    Dictionary<string, string> values = new Dictionary<string, string>();
                    int totalValues = 0;
                    int totalFields = fieldList.Count;
                    for (int j = 0; j < totalFields; j++) {
                        if (fieldList[j] == null)
                            continue;
                        Field field = fieldList[j] as Field;
                        string name = field.Name();
                        int renameCount = 1;
                        while (values.ContainsKey(name)) {
                            name = field.Name() + "(" + renameCount.ToString() + ")";
                            renameCount++;
                        }
                        values.Add(name, field.StringValue());
                        ++totalValues;
                    }
                    if (totalValues > 0) {
                        SearchResult result = new SearchResult(values, this.index.IndexDirectory.Name, topDocs.scoreDocs[i].score);
                        results.Add(result);
                        resultsFound++;
                        if (OnSearchResultFound(new SearcherEventArgs(this.index.IndexDirectory.Name, this.index.IndexStructure, SearchMethodType.Normal, SearchMethodLocation.ResultFound, result))) {
                            canceled = true;
                            break;
                        }
                    }
                }
            }
            finally {
                if (searcher != null)
                    searcher.Close();
                if (directory != null)
                    directory.Close();

                OnEndSearch(new SearcherEventArgs(this.index.IndexDirectory.Name, this.index.IndexStructure, SearchMethodType.Normal, SearchMethodLocation.Ending, null));
                LibraryAnalysis.Fire(new SearchInfo(this.index.IndexDirectory.Name, builder.ToString(), SearchMethodType.Normal, hitsLength, canceled));
            }

            return results;
        }

        /// <summary>
        /// Performs a search against an index.
        /// </summary>
        /// <param name="builder">The query to run against the index.</param>
        /// <returns>a list of <see cref="IndexLibrary.SearchResult"/>, one for each result found.</returns>
        /// <remarks>
        /// Designed to be used for the majority of searches. The functionality contained in this 
        /// method is used for a normal search, i.e. enter a term, search, get results. 
        /// </remarks>
        public virtual IEnumerable<SearchResult> Search(QueryBuilder builder)
        {
            return Search(builder, -1);
        }

        /// <summary>
        /// Outputs the directory of index files to read
        /// </summary>
        /// <param name="info">out of the discovered index directory</param>
        /// <returns>True if there are index files; false if not</returns>
        private bool GetIndexReadDirectory(out DirectoryInfo info)
        {
            switch (index.IndexStructure) {
                case IndexType.SingleIndex:
                    IIndex singleIndex = (IIndex)index;
                    info = singleIndex.IndexDirectory;
                    return singleIndex.HasIndexFiles();
                case IndexType.DoubleIndex:
                case IndexType.CyclicalIndex:
                    IDoubleIndex doubleIndex = (IDoubleIndex)index;
                    info = doubleIndex.GetReadDirectory();
                    return doubleIndex.HasIndexFiles();
                default:
                    throw new NotImplementedException(index.IndexStructure.ToString() + " not supported");
            }
        }

        /// <summary>
        /// Raises the <see cref="E:BeginSearch"/> event.
        /// </summary>
        /// <param name="e">The <see cref="IndexLibrary.SearcherEventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        private bool OnBeginSearch(SearcherEventArgs e)
        {
            EventHandler<SearcherEventArgs> handler = BeginSearch;
            if (handler != null)
                handler(this, e);
            return e.Cancel;
        }

        /// <summary>
        /// Raises the <see cref="E:EndSearch"/> event.
        /// </summary>
        /// <param name="e">The <see cref="IndexLibrary.SearcherEventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        private bool OnEndSearch(SearcherEventArgs e)
        {
            EventHandler<SearcherEventArgs> handler = EndSearch;
            if (handler != null)
                handler(this, e);
            return e.Cancel;
        }

        /// <summary>
        /// Raises the <see cref="E:SearchResultFound"/> event.
        /// </summary>
        /// <param name="e">The <see cref="IndexLibrary.SearcherEventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        private bool OnSearchResultFound(SearcherEventArgs e)
        {
            EventHandler<SearcherEventArgs> handler = SearchResultFound;
            if (handler != null)
                handler(this, e);
            return e.Cancel;
        }

        #endregion Methods
    }
}