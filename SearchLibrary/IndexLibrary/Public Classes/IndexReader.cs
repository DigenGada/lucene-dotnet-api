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
    using System.Linq;

    using IndexLibrary.Analysis;
    using IndexLibrary.Interfaces;

    // needs path directly to index files, to folder with *.gen files in it
    /// <summary>
    /// Similar to StreamReader, represents a reader object that allows you
    /// to sequentially read through items within an index.
    /// </summary>
    /// <remarks>
    /// This class requires an IndexDirectory parameter because the IndexReader
    /// is created regardless of the index structure. Point to the index files
    /// you want to open and this class will open them, no questions asked.
    /// Use the <see cref="IndexLibrary.IndexHelper"/> to help you open indexes
    /// so you don't have to traverse the directory structure yourself.
    /// </remarks>
    [System.CLSCompliant(true)]
    public class IndexReader : IDisposable
    {
        #region Fields

        protected IIndex index;
        protected bool isDisposed = false;
        protected bool isReadOnly = false;
        protected Lucene29.Net.Store.Directory luceneDirectory;
        protected Lucene29.Net.Index.IndexReader luceneReader;

        #endregion Fields

        #region Constructors

        public IndexReader(IIndex index)
            : this(index, true)
        {
        }

        public IndexReader(IIndex index, bool openReadOnly)
        {
            if (index == null)
                throw new ArgumentNullException("index", "index cannot be null");
            this.index = index;
            this.isReadOnly = openReadOnly;
            this.isDisposed = false;
            switch (index.IndexStructure) {
                case IndexType.SingleIndex:
                    var singleIndex = (IIndex)index;
                    if (!singleIndex.HasIndexFiles())
                        throw new InvalidOperationException("There are no index files in the specified directory " + index.IndexDirectory.FullName);
                    this.luceneDirectory = singleIndex.GetLuceneDirectory();
                    break;
                case IndexType.DoubleIndex:
                case IndexType.CyclicalIndex:
                    var doubleIndex = (DoubleIndex)index;
                    if (!doubleIndex.HasIndexFiles())
                        throw new InvalidOperationException("There are no index files in the specified directory " + index.IndexDirectory.FullName);
                    this.luceneDirectory = doubleIndex.GetLuceneDirectory();
                    break;
                default:
                    throw new NotSupportedException(index.IndexStructure.ToString() + " not supported");
            }

            this.luceneReader = Lucene29.Net.Index.IndexReader.Open(this.luceneDirectory, openReadOnly);
        }

        #endregion Constructors

        #region Events

        public event EventHandler<ReaderEventArgs> BeginRead;

        public event EventHandler<ReaderEventArgs> EndRead;

        public event EventHandler<ReaderEventArgs> ReadResultFound;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets a value indicating whether the index has deletions.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has deletions; otherwise, <c>false</c>.
        /// </value>
        public bool HasDeletions
        {
            get {
                if (this.isDisposed)
                    throw new ObjectDisposedException("IndexReader", "You cannot access HasDeletions from a disposed IndexReader");
                return this.luceneReader.HasDeletions();
            }
        }

        /// <summary>
        /// The index this instance is reading from
        /// </summary>
        public IIndex Index
        {
            get { return this.index; }
        }

        /// <summary>
        /// Gets a value indicating whether the open index is current.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is current; otherwise, <c>false</c>.
        /// </value>
        public bool IsCurrent
        {
            get {
                if (this.isDisposed)
                    throw new ObjectDisposedException("IndexReader", "You cannot access IsCurrent from a disposed IndexReader");
                return this.luceneReader.IsCurrent();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the open index is optimized.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is optimized; otherwise, <c>false</c>.
        /// </value>
        public bool IsOptimized
        {
            get {
                if (this.isDisposed)
                    throw new ObjectDisposedException("IndexReader", "You cannot access IsOptimized from a disposed IndexReader");
                return this.luceneReader.IsOptimized();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is open in readonly mode or not.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is open in readonly mode; otherwise, <c>false</c>.
        /// </value>
        public bool ReadOnly
        {
            get { return this.isReadOnly; }
        }

        /// <summary>
        /// Gets the total number of documents in the open index.
        /// </summary>
        public int TotalDocuments
        {
            get {
                if (this.isDisposed)
                    throw new ObjectDisposedException("IndexReader", "You cannot access TotalDocuments from a disposed IndexReader");
                return this.luceneReader.MaxDoc();
            }
        }

        /// <summary>
        /// Gets the number of unique terms (across all fields) in this reader.
        /// </summary>
        /// <returns>Returns the number of unique terms</returns>
        public long UniqueTermCount
        {
            get {
                if (this.isDisposed)
                    throw new ObjectDisposedException("IndexReader", "You cannot call GetUniqueTermCount from a disposed IndexReader");
                return this.luceneReader.GetUniqueTermCount();
            }
        }

        /// <summary>
        /// Gets the version of the open index.
        /// </summary>
        public long Version
        {
            get {
                if (this.isDisposed)
                    throw new ObjectDisposedException("Version", "You cannot access a property from a disposed instance");
                return this.luceneReader.GetVersion();
            }
        }

        #endregion Properties

        #region Indexers

        public IndexDocument this[int docNumber]
        {
            get {
                if (this.isDisposed)
                    throw new ObjectDisposedException("indexer", "You cannot access a document from a disposed IndexReader");
                return this.ReadDocument(docNumber);
            }
        }

        #endregion Indexers

        #region Methods

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public void Close()
        {
            if (!this.isDisposed) {
                if (this.luceneReader != null)
                    this.luceneReader.Close();
                if (this.luceneDirectory != null)
                    this.luceneDirectory.Close();
            }
        }

        /// <summary>
        /// [experimental] Commits any unsaved data to the index.
        /// </summary>
        public void Commit()
        {
            if (this.isDisposed)
                throw new ObjectDisposedException("IndexReader", "You cannot call Commit from a disposed IndexReader");
            if (this.isReadOnly)
                throw new InvalidOperationException("You cannot commit data to an index that is opened in readonly mode");
            this.luceneReader.Commit();
        }

        /// <summary>
        /// [experimental] Deletes the document at the index documentID.
        /// </summary>
        /// <param name="documentId">The index of the document to delete.</param>
        public void DeleteDocument(int documentId)
        {
            if (this.isDisposed)
                throw new ObjectDisposedException("IndexReader", "You cannot call DeleteDocument(int) from a disposed IndexReader");
            if (this.isReadOnly)
                throw new InvalidOperationException("You cannot delete documents from an index that is opened in readonly mode");
            if (documentId < 0)
                throw new ArgumentOutOfRangeException("documentId", "documentId cannot be less than 0");
            if (documentId >= this.TotalDocuments)
                throw new ArgumentOutOfRangeException("documentId", "documentId cannot be greater than the total number of documents in this index");
            this.luceneReader.DeleteDocument(documentId);
        }

        /// <summary>
        /// [experimental] Deletes all documents that have a given term indexed.
        /// </summary>
        /// <param name="indexTerm">The index term to match against existing documents.</param>
        /// <returns>The number of documents deleted</returns>
        /// <remarks>
        /// This is useful if one uses a document field to hold a unique ID string for the document. Then to delete such a document, one merely constructs a term with the
        /// appropriate field and the unique ID string as it's text and passes it to this method. 
        /// </remarks>
        public int DeleteDocuments(SearchTerm indexTerm)
        {
            if (this.isDisposed)
                throw new ObjectDisposedException("IndexReader", "You cannot call DeleteDocuments(FieldAbstract) from a disposed IndexReader");
            if (this.isReadOnly)
                throw new InvalidOperationException("You cannot delete documents from an index that is opened in readonly mode");
            if (indexTerm == null)
                throw new ArgumentNullException("indexTerm", "indexTerm cannot be null");
            return this.luceneReader.DeleteDocuments(indexTerm.GetLuceneTerm());
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this.isDisposed)
                return;
            if (this.luceneReader != null) {
                this.luceneReader.Close();
                this.luceneReader = null;
            }
            if (this.luceneDirectory != null) {
                this.luceneDirectory.Close();
                this.luceneDirectory = null;
            }
            this.isDisposed = true;
        }

        /// <summary>
        /// [experimental] Returns the number of documents containing the indexTerm
        /// </summary>
        /// <param name="indexTerm">The index term.</param>
        /// <returns></returns>
        public int DocumentFrequency(SearchTerm indexTerm)
        {
            if (this.isDisposed)
                throw new ObjectDisposedException("IndexReader", "You cannot call DocumentFrequency(FieldAbstract) from a disposed IndexReader");
            if (indexTerm == null)
                throw new ArgumentNullException("indexTerm", "indexTerm cannot be null");
            return this.luceneReader.DocFreq(indexTerm.GetLuceneTerm());
        }

        /// <summary>
        /// Flushes pending changes in this index to the actual file.
        /// </summary>
        public void Flush()
        {
            if (this.isDisposed)
                throw new ObjectDisposedException("IndexReader", "You cannot call Flush from a disposed IndexReader");
            if (this.isReadOnly)
                throw new InvalidOperationException("You cannot flush data to an index that is opened in readonly mode");
            this.luceneReader.Flush();
        }

        /// <summary>
        /// Gets a list of unique field names that exist in this index.
        /// </summary>
        /// <returns>A string list of field names</returns>
        public ICollection<string> GetFieldNames()
        {
            if (this.isDisposed)
                throw new ObjectDisposedException("IndexReader", "You cannot call GetFieldNames from a disposed IndexReader");
            return this.GetFieldNames(FieldOption.All);
        }

        /// <summary>
        /// [experimental] Gets a list of unique field names taht exist in this index and have the specified field option information.
        /// </summary>
        /// <param name="options">The options that a field must have applied to it.</param>
        /// <returns>A string list of field names</returns>
        public ICollection<string> GetFieldNames(FieldOption options)
        {
            if (this.isDisposed)
                throw new ObjectDisposedException("IndexReader", "You cannot call GetFieldNames(FieldOption) from a disposed IndexReader");
            return this.luceneReader.GetFieldNames(TypeConverter.ConvertToLuceneFieldOption(options));
        }

        /// <summary>
        /// Retrieves all data from this index.
        /// </summary>
        /// <returns>A <see cref="IndexLibrary.SearchResultDataSet"/> that contains all data from this index</returns>
        public virtual SearchResultDataSet ReadAllDocuments()
        {
            if (this.isDisposed)
                throw new ObjectDisposedException("IndexReader", "You cannot call RetrieveIndex from a disposed IndexReader");
            return ReadDocuments(-1, true);
        }

        /// <summary>
        /// Gets an individual document from the index by it's internal document number.
        /// </summary>
        /// <param name="documentId">The document id.</param>
        /// <returns>An <see cref="IndexLibrary.IndexDocument"/> that represents the specified document</returns>
        public virtual IndexDocument ReadDocument(int documentId)
        {
            // not necessarily true since we now allow index editing
            //if (documentId >= this.TotalDocuments) throw new ArgumentOutOfRangeException("documentId", "documentId cannot be greater than the number of items in the collection");
            try {
                var result = this.luceneReader.Document(documentId);
                if (result == null)
                    return null;
                return (IndexDocument)result;
            }
            catch {
                return null;
            }
        }

        /// <summary>
        /// Retrieves data from this index.
        /// </summary>
        /// <param name="topNResults">The number of results to return from this index</param>
        /// <returns>A <see cref="IndexLibrary.SearchResultDataSet"/> that contains all data from this index</returns>
        public virtual SearchResultDataSet ReadDocuments(int topNResults)
        {
            if (this.isDisposed)
                throw new ObjectDisposedException("IndexReader", "You cannot call RetrieveIndex(int) from a disposed IndexReader");
            return ReadDocuments(topNResults, false);
        }

        /// <summary>
        /// Retrieves data from this index.
        /// </summary>
        /// <param name="topNResults">The number of results to return from this index</param>
        /// <param name="buildFieldFiltersFromAllData">Specifies whether distinct values from all fields should be stored, even if all actual results are not returned from the index</param>
        /// <returns>A <see cref="IndexLibrary.SearchResultDataSet"/> that contains all data from this index</returns>
        public virtual SearchResultDataSet ReadDocuments(int topNResults, bool buildFieldFiltersFromAllData)
        {
            if (this.isDisposed)
                throw new ObjectDisposedException("IndexReader", "You cannot call RetrieveIndex(int, bool) from a disposed IndexReader");
            if (topNResults == -1)
                topNResults = this.TotalDocuments;
            if (topNResults <= 0)
                return new SearchResultDataSet();

            SearchResultDataSet dataSet = new SearchResultDataSet();
            int resultsToCollect = this.TotalDocuments;

            if (OnBeginRead(new ReaderEventArgs(this.index.IndexDirectory.FullName, topNResults, buildFieldFiltersFromAllData, null))) {
                OnEndRead(new ReaderEventArgs(this.index.IndexDirectory.FullName, topNResults, buildFieldFiltersFromAllData, null));
                LibraryAnalysis.Fire(new ReadInfo(this.index.IndexDirectory.FullName, resultsToCollect, this.TotalDocuments, buildFieldFiltersFromAllData, this.IsOptimized));
                return dataSet;
            }

            if (!buildFieldFiltersFromAllData && topNResults < this.TotalDocuments)
                resultsToCollect = topNResults;
            LibraryAnalysis.Fire(new ReadInfo(this.index.IndexDirectory.FullName, resultsToCollect, this.TotalDocuments, buildFieldFiltersFromAllData, this.IsOptimized));
            for (int i = 0; i < resultsToCollect; i++) {
                bool pastWantedResults = (buildFieldFiltersFromAllData && i >= topNResults);

                Lucene29.Net.Documents.Document document = this.luceneReader.Document(i);
                System.Collections.IList fieldList = document.GetFields();
                Dictionary<string, string> values = new Dictionary<string, string>();
                int totalValues = 0;
                int totalFields = fieldList.Count;
                for (int j = 0; j < totalFields; j++) {
                    if (fieldList[j] == null)
                        continue;
                    Lucene29.Net.Documents.Field field = fieldList[j] as Lucene29.Net.Documents.Field;
                    string name = field.Name();
                    string value = field.StringValue();

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

                    if (values.ContainsKey(name)) {
                        int revision = 1;
                        while (values.ContainsKey(name + "(" + revision.ToString() + ")"))
                            revision++;
                        name += "(" + revision.ToString() + ")";
                    }
                    values.Add(name, value);
                    if (OnReadResultFound(new ReaderEventArgs(this.index.IndexDirectory.FullName, topNResults, buildFieldFiltersFromAllData, null))) {
                        OnEndRead(new ReaderEventArgs(this.index.IndexDirectory.FullName, topNResults, buildFieldFiltersFromAllData, null));
                        LibraryAnalysis.Fire(new ReadInfo(this.index.IndexDirectory.FullName, resultsToCollect, this.TotalDocuments, buildFieldFiltersFromAllData, this.IsOptimized));
                        return dataSet;
                    }
                    ++totalValues;
                }
                if (totalValues > 0 && !pastWantedResults)
                    dataSet.AddSearchResult(new SearchResult(values, this.index.IndexDirectory.FullName, 1.0f));
            }

            OnEndRead(new ReaderEventArgs(this.index.IndexDirectory.FullName, topNResults, buildFieldFiltersFromAllData, null));

            return dataSet;
        }

        /// <summary>
        /// Retrieves data from this index.
        /// </summary>
        /// <param name="topNResults">The number of results to return from this index</param>
        /// <param name="buildFieldFiltersFromAllData">Specifies whether distinct values from all fields should be stored, even if all actual results are not returned from the index</param>
        /// <param name="filterFieldNames">The field names to build filters from</param>
        /// <returns>A <see cref="IndexLibrary.SearchResultDataSet"/> that contains all data from this index</returns>
        /// <remarks>
        /// filterFieldNames was added as a parameter to allow you to only pull data from specific fields into the filters,
        /// this can significantly increase the performance and the total amount of time required by this method.
        /// </remarks>
        public virtual SearchResultDataSet ReadDocuments(int topNResults, bool buildFieldFiltersFromAllData, string[] filterFieldNames)
        {
            if (this.isDisposed)
                throw new ObjectDisposedException("IndexReader", "You cannot call RetrieveIndex(int, string[], bool) from a disposed IndexReader");
            if (topNResults == -1)
                topNResults = this.TotalDocuments;
            if (topNResults <= 0)
                return new SearchResultDataSet();

            bool filterAll = (filterFieldNames == null || filterFieldNames.Length == 0);
            SearchResultDataSet dataSet = new SearchResultDataSet();
            int resultsToCollect = this.TotalDocuments;

            if (OnBeginRead(new ReaderEventArgs(this.index.IndexDirectory.FullName, topNResults, buildFieldFiltersFromAllData, filterFieldNames))) {
                OnEndRead(new ReaderEventArgs(this.index.IndexDirectory.FullName, topNResults, buildFieldFiltersFromAllData, filterFieldNames));
                LibraryAnalysis.Fire(new ReadInfo(this.index.IndexDirectory.FullName, resultsToCollect, this.TotalDocuments, buildFieldFiltersFromAllData, this.IsOptimized));
                return dataSet;
            }

            if (!buildFieldFiltersFromAllData && topNResults < this.TotalDocuments)
                resultsToCollect = topNResults;
            LibraryAnalysis.Fire(new ReadInfo(this.index.IndexDirectory.FullName, topNResults, this.TotalDocuments, buildFieldFiltersFromAllData, this.isDisposed));

            for (int i = 0; i < resultsToCollect; i++) {
                bool pastWantedResults = (buildFieldFiltersFromAllData && i >= topNResults);

                Lucene29.Net.Documents.Document document = this.luceneReader.Document(i);
                System.Collections.IList fieldList = document.GetFields();
                Dictionary<string, string> values = new Dictionary<string, string>();
                int totalValues = 0;
                int totalFields = fieldList.Count;
                for (int j = 0; j < totalFields; j++) {
                    if (fieldList[j] == null)
                        continue;
                    Lucene29.Net.Documents.Field field = fieldList[j] as Lucene29.Net.Documents.Field;
                    string name = field.Name();
                    string value = field.StringValue();

                    if (filterAll || filterFieldNames.Contains(name)) {
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
                    if (OnReadResultFound(new ReaderEventArgs(this.index.IndexDirectory.FullName, topNResults, buildFieldFiltersFromAllData, filterFieldNames))) {
                        OnEndRead(new ReaderEventArgs(this.index.IndexDirectory.FullName, topNResults, buildFieldFiltersFromAllData, filterFieldNames));
                        LibraryAnalysis.Fire(new ReadInfo(this.index.IndexDirectory.FullName, resultsToCollect, this.TotalDocuments, buildFieldFiltersFromAllData, this.IsOptimized));
                        return dataSet;
                    }
                    ++totalValues;
                }
                if (totalValues > 0 && !pastWantedResults) {
                    dataSet.AddSearchResult(new SearchResult(values, this.index.IndexDirectory.FullName, 1f));
                }
            }

            OnEndRead(new ReaderEventArgs(this.index.IndexDirectory.FullName, topNResults, buildFieldFiltersFromAllData, filterFieldNames));

            return dataSet;
        }

        /// <summary>
        /// Gets the total number the deleted documents in this session.
        /// </summary>
        /// <returns></returns>
        public int TotalDeletedDocuments()
        {
            if (this.isDisposed)
                throw new ObjectDisposedException("IndexReader", "You cannot call TotalDeletedDocuments from a disposed IndexReader");
            return this.luceneReader.NumDeletedDocs();
        }

        /// <summary>
        /// Undeletes all documents currently marked as deleted in this index.
        /// </summary>
        public void UndeleteAll()
        {
            if (this.isDisposed)
                throw new ObjectDisposedException("IndexReader", "You cannot call UndeleteAll from a disposed IndexReader");
            this.luceneReader.UndeleteAll();
        }

        private bool OnBeginRead(ReaderEventArgs e)
        {
            EventHandler<ReaderEventArgs> handler = BeginRead;
            if (handler != null)
                handler(this, e);
            return e.Cancel;
        }

        private bool OnEndRead(ReaderEventArgs e)
        {
            EventHandler<ReaderEventArgs> handler = EndRead;
            if (handler != null)
                handler(this, e);
            return e.Cancel;
        }

        private bool OnReadResultFound(ReaderEventArgs e)
        {
            EventHandler<ReaderEventArgs> handler = ReadResultFound;
            if (handler != null)
                handler(this, e);
            return e.Cancel;
        }

        #endregion Methods
    }
}