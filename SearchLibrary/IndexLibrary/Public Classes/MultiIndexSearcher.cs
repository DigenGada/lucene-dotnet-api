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
    public class MultiIndexSearcher : IIndexSearcher<MultiSearcherEventArgs>
    {
        #region Fields

        /// <summary>
        /// A list of index directories and their structures
        /// </summary>
        protected List<IIndex> indexes;
        protected string[] namesArray;
        protected string namesConcat;

        #endregion Fields

        #region Constructors

        public MultiIndexSearcher(IEnumerable<IIndex> indexes)
        {
            if (indexes == null)
                throw new ArgumentNullException("indexes", "indexes cannot be null");
            this.indexes = new List<IIndex>(indexes);
            int totalIndexes = this.indexes.Count;
            if (totalIndexes == 0)
                throw new ArgumentNullException("indexes", "indexes must contain at least one member");
            for (int i = 0; i < totalIndexes; i++) if (this.indexes[i].IndexStructure == IndexType.None)
                    throw new ArgumentException("index.IndexStructure cannot be None", "indexes");
            this.namesConcat = string.Join(", ", this.indexes.Select((x, y) => x.IndexDirectory.Name).ToArray());
            this.namesArray = this.indexes.Select((x, y) => x.IndexDirectory.Name).ToArray();
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Occurs when a search begins.
        /// </summary>
        public event EventHandler<MultiSearcherEventArgs> BeginSearch;

        /// <summary>
        /// Occurs when a search ends.
        /// </summary>
        public event EventHandler<MultiSearcherEventArgs> EndSearch;

        /// <summary>
        /// Occurs when a search result is found.
        /// </summary>
        public event EventHandler<MultiSearcherEventArgs> SearchResultFound;

        #endregion Events

        #region Properties

        public IEnumerable<IIndex> Indexes
        {
            get { return this.indexes; }
        }

        public string[] IndexNames
        {
            get { return this.namesArray; }
        }

        /// <summary>
        /// Gets the index names concatenated together by semicolons.
        /// </summary>
        public string IndexNamesConcatenated
        {
            get { return this.namesConcat; }
        }

        /// <summary>
        /// Gets a value indicating whether searchers are opened in readonly mode or not.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the searchers are opened in readonly mode; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly
        {
            get { return true; }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Performs a full search against a set of indexes.
        /// </summary>
        /// <param name="builder">The query to run against the indexes.</param>
        /// <param name="totalResultsRequested">The total number of results to return.</param>
        /// <param name="fieldFilterNames">An exclusive list of fields to retrieve from the indexes.</param>
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
            bool getAllFilters = (fieldFilterNames == null || fieldFilterNames.Length == 0);
            if (OnBeginSearch(new MultiSearcherEventArgs(this.IndexNames, SearchMethodType.Quick, SearchMethodLocation.Beginning, null)) || totalResultsRequested < -1 || totalResultsRequested == 0) {
                OnEndSearch(new MultiSearcherEventArgs(this.IndexNames, SearchMethodType.Quick, SearchMethodLocation.Ending, null));
                LibraryAnalysis.Fire(new SearchInfo(this.IndexNamesConcatenated, builder.ToString(), SearchMethodType.Full, 0, true));
                return dataSet;
            }

            Lucene29.Net.Store.Directory directory = null;
            Lucene29.Net.Search.MultiSearcher searcher = null;
            List<Lucene29.Net.Search.IndexSearcher> searchables = null;
            int hitsLength = 0;
            bool canceled = false;
            try {
                searchables = new List<Lucene29.Net.Search.IndexSearcher>();
                for (int i = 0; i < this.indexes.Count; i++) {
                    DirectoryInfo searchInfo = null;
                    if (!GetIndexReadDirectory(this.indexes[i], out searchInfo))
                        continue;
                    searchables.Add(new Lucene29.Net.Search.IndexSearcher(Lucene29.Net.Store.FSDirectory.Open(searchInfo), true));
                }

                if (searchables.Count == 0) {
                    OnEndSearch(new MultiSearcherEventArgs(this.IndexNames, SearchMethodType.Quick, SearchMethodLocation.Ending, null));
                    return dataSet;
                }
                searcher = new Lucene29.Net.Search.MultiSearcher(searchables.ToArray());
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
                        SearchResult result = new SearchResult(values, string.Join(", ", this.IndexNames), topDocs.scoreDocs[i].score);
                        dataSet.AddSearchResult(result);
                        resultsFound++;
                        if (OnSearchResultFound(new MultiSearcherEventArgs(this.IndexNames, SearchMethodType.Full, SearchMethodLocation.ResultFound, result))) {
                            canceled = true;
                            break;
                        }
                    }
                }
            }
            finally {
                if (searcher != null)
                    searcher.Close();
                if (searchables != null)
                    searchables.ForEach(x => x.Close());
                if (directory != null)
                    directory.Close();

                OnEndSearch(new MultiSearcherEventArgs(this.IndexNames, SearchMethodType.Quick, SearchMethodLocation.Ending, null));
                LibraryAnalysis.Fire(new SearchInfo(this.IndexNamesConcatenated, builder.ToString(), SearchMethodType.Full, hitsLength, canceled));
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
            return FullSearch(builder, -1);
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
        /// Performs a quick search against a set of indexes.
        /// </summary>
        /// <param name="builder">The query to run against the indexes.</param>
        /// <param name="fieldName">Name of the field to return from the indexes.</param>
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
            if (OnBeginSearch(new MultiSearcherEventArgs(this.IndexNames, SearchMethodType.Quick, SearchMethodLocation.Beginning, null)) || totalResultsRequested < -1 || totalResultsRequested == 0) {
                OnEndSearch(new MultiSearcherEventArgs(this.IndexNames, SearchMethodType.Quick, SearchMethodLocation.Ending, null));
                LibraryAnalysis.Fire(new SearchInfo(this.IndexNamesConcatenated, builder.ToString(), SearchMethodType.Quick, 0, true));
                return results;
            }

            Lucene29.Net.Store.Directory directory = null;
            Lucene29.Net.Search.MultiSearcher searcher = null;
            List<Lucene29.Net.Search.IndexSearcher> searchables = null;
            int hitsLength = 0;
            bool canceled = false;
            try {
                searchables = new List<Lucene29.Net.Search.IndexSearcher>();
                for (int i = 0; i < this.indexes.Count; i++) {
                    DirectoryInfo searchInfo = null;
                    if (!GetIndexReadDirectory(this.indexes[i], out searchInfo))
                        continue;
                    searchables.Add(new Lucene29.Net.Search.IndexSearcher(Lucene29.Net.Store.FSDirectory.Open(searchInfo), true));
                }

                if (searchables.Count == 0) {
                    OnEndSearch(new MultiSearcherEventArgs(this.IndexNames, SearchMethodType.Quick, SearchMethodLocation.Ending, null));
                    return results;
                }
                searcher = new Lucene29.Net.Search.MultiSearcher(searchables.ToArray());
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
                        if (OnSearchResultFound(new MultiSearcherEventArgs(this.IndexNames, SearchMethodType.Quick, SearchMethodLocation.ResultFound, collectedField.Name(), collectedField.StringValue(), topDocs.scoreDocs[i].score))) {
                            canceled = true;
                            break;
                        }
                    }
                }
            }
            finally {
                if (searcher != null)
                    searcher.Close();
                if (searchables != null)
                    searchables.ForEach(x => x.Close());
                if (directory != null)
                    directory.Close();

                OnEndSearch(new MultiSearcherEventArgs(this.IndexNames, SearchMethodType.Quick, SearchMethodLocation.Ending, null));
                LibraryAnalysis.Fire(new SearchInfo(this.IndexNamesConcatenated, builder.ToString(), SearchMethodType.Quick, hitsLength, canceled));

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
        /// Performs a search against a set of indexes.
        /// </summary>
        /// <param name="builder">The query to run against the indexes.</param>
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
            if (OnBeginSearch(new MultiSearcherEventArgs(this.IndexNames, SearchMethodType.Quick, SearchMethodLocation.Beginning, null)) || totalResultsRequested < -1 || totalResultsRequested == 0) {
                OnEndSearch(new MultiSearcherEventArgs(this.IndexNames, SearchMethodType.Quick, SearchMethodLocation.Ending, null));
                LibraryAnalysis.Fire(new SearchInfo(this.IndexNamesConcatenated, builder.ToString(), SearchMethodType.Normal, 0, true));
                return results;
            }

            Lucene29.Net.Store.Directory directory = null;
            Lucene29.Net.Search.MultiSearcher searcher = null;
            List<Lucene29.Net.Search.IndexSearcher> searchables = null;
            int hitsLength = 0;
            bool canceled = false;
            try {
                searchables = new List<Lucene29.Net.Search.IndexSearcher>();
                for (int i = 0; i < this.indexes.Count; i++) {
                    DirectoryInfo searchInfo = null;
                    if (!GetIndexReadDirectory(this.indexes[i], out searchInfo))
                        continue;
                    searchables.Add(new Lucene29.Net.Search.IndexSearcher(Lucene29.Net.Store.FSDirectory.Open(searchInfo), true));
                }
                if (searchables.Count == 0) {
                    OnEndSearch(new MultiSearcherEventArgs(this.IndexNames, SearchMethodType.Quick, SearchMethodLocation.Ending, null));
                    return results;
                }
                searcher = new Lucene29.Net.Search.MultiSearcher(searchables.ToArray());
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
                        values.Add(field.Name(), field.StringValue());
                        ++totalValues;
                    }
                    if (totalValues > 0) {
                        SearchResult result = new SearchResult(values, string.Join(", ", this.IndexNames), topDocs.scoreDocs[i].score);
                        results.Add(result);
                        resultsFound++;
                        if (OnSearchResultFound(new MultiSearcherEventArgs(this.IndexNames, SearchMethodType.Normal, SearchMethodLocation.ResultFound, result))) {
                            canceled = true;
                            break;
                        }
                    }
                }
            }
            finally {
                if (searcher != null)
                    searcher.Close();
                if (searchables != null)
                    searchables.ForEach(x => x.Close());
                if (directory != null)
                    directory.Close();

                OnEndSearch(new MultiSearcherEventArgs(this.IndexNames, SearchMethodType.Quick, SearchMethodLocation.Ending, null));
                LibraryAnalysis.Fire(new SearchInfo(this.IndexNamesConcatenated, builder.ToString(), SearchMethodType.Normal, hitsLength, canceled));
            }

            return results;
        }

        /// <summary>
        /// Outputs the directory of index files to read.
        /// </summary>
        /// <param name="index">The index to find the read directory for.</param>
        /// <param name="info">out of the discovered index directory.</param>
        /// <returns><c>true</c> if there are index files; otherwise, <c>false</c>.</returns>
        private static bool GetIndexReadDirectory(IIndex index, out DirectoryInfo info)
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
        /// <param name="e">The <see cref="IndexLibrary.MultiSearcherEventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        private bool OnBeginSearch(MultiSearcherEventArgs e)
        {
            EventHandler<MultiSearcherEventArgs> handler = BeginSearch;
            if (handler != null)
                handler(this, e);
            return e.Cancel;
        }

        /// <summary>
        /// Raises the <see cref="E:EndSearch"/> event.
        /// </summary>
        /// <param name="e">The <see cref="IndexLibrary.MultiSearcherEventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        private bool OnEndSearch(MultiSearcherEventArgs e)
        {
            EventHandler<MultiSearcherEventArgs> handler = EndSearch;
            if (handler != null)
                handler(this, e);
            return e.Cancel;
        }

        /// <summary>
        /// Raises the <see cref="E:SearchResultFound"/> event.
        /// </summary>
        /// <param name="e">The <see cref="IndexLibrary.MultiSearcherEventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        private bool OnSearchResultFound(MultiSearcherEventArgs e)
        {
            EventHandler<MultiSearcherEventArgs> handler = SearchResultFound;
            if (handler != null)
                handler(this, e);
            return e.Cancel;
        }

        #endregion Methods
    }
}