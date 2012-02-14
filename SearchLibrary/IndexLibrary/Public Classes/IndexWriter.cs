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
    using System.Data;
    using System.IO;
    using System.Linq;

    using IndexLibrary.Analysis;
    using IndexLibrary.Interfaces;

    using Lucene29.Net.Analysis;

    /// <summary>
    /// Creates a writer object that writes values to a set of index file
    /// </summary>
    [System.CLSCompliant(true)]
    public class IndexWriter : IDisposable
    {
        #region Fields

        public const string NoValue = "{null}";

        protected Analyzer analyzer = null;
        protected IIndex index = null;
        protected bool isDisposed = false;
        protected Lucene29.Net.Store.Directory openDirectory;
        protected bool optimized = false;
        protected int totalWrites = 0;
        protected Lucene29.Net.Index.IndexWriter writer = null;

        #endregion Fields

        #region Constructors

        public IndexWriter(IIndex index, AnalyzerType analyzer)
            : this(index, analyzer, true)
        {
        }

        public IndexWriter(IIndex index, AnalyzerType analyzer, bool create)
            : this(index, analyzer, create, false)
        {
        }

        public IndexWriter(IIndex index, AnalyzerType analyzer, bool create, bool allowUnlimitedFieldLength)
        {
            if (index == null) {
                throw new ArgumentNullException("index", "index cannot be null");
            }
            if (index.IndexStructure == IndexType.None) {
                throw new ArgumentException("The specified index structure cannot be None", "index");
            }
            if (analyzer == AnalyzerType.None || analyzer == AnalyzerType.Unknown) {
                throw new ArgumentException("The specified analyzer cannot be None or Unknown", "analyzer");
            }
            this.analyzer = TypeConverter.GetAnalyzer(analyzer);
            this.index = index;

            DirectoryInfo writeDirectory = null;
            bool hasIndexfiles = GetIndexWriteDirectory(out writeDirectory);
            // you said append but there's no index files
            if (!create && !hasIndexfiles) {
                create = true;
            }

            this.openDirectory = Lucene29.Net.Store.FSDirectory.Open(writeDirectory);
            this.writer = new Lucene29.Net.Index.IndexWriter(this.openDirectory, this.analyzer, create, (allowUnlimitedFieldLength) ? Lucene29.Net.Index.IndexWriter.MaxFieldLength.UNLIMITED : Lucene29.Net.Index.IndexWriter.MaxFieldLength.LIMITED);
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Occurs when [writing] a <see cref="IndexLibrary.IndexDocument"/> to the index
        /// </summary>
        public event EventHandler<WriterEventArgs> Writing;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets or sets the type of the analyzer.
        /// </summary>
        /// <value>
        /// The type of the analyzer.
        /// </value>
        public AnalyzerType AnalyzerType
        {
            get {
                if (this.isDisposed) {
                    throw new ObjectDisposedException("IndexWriter", "You cannot access this property from a disposed IndexWriter");
                }
                return TypeConverter.GetAnalyzerType(this.analyzer);
            }
            set {
                if (this.isDisposed) {
                    throw new ObjectDisposedException("IndexWriter", "You cannot access this property from a disposed IndexWriter");
                }
                if (value == IndexLibrary.AnalyzerType.None || value == IndexLibrary.AnalyzerType.Unknown) {
                    throw new ArgumentException("An analyzer is required for an IndexWriter");
                }
                this.analyzer = TypeConverter.GetAnalyzer(value);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has deletions.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has deletions; otherwise, <c>false</c>.
        /// </value>
        public bool HasDeletions
        {
            get {
                if (this.isDisposed) {
                    throw new ObjectDisposedException("IndexWriter", "You cannot access this property from a disposed IndexWriter");
                }
                return this.writer.HasDeletions();
            }
        }

        public bool IsOpen
        {
            get {
                return !this.isDisposed;
            }
        }

        /// <summary>
        /// Gets or sets the max buffered delete terms.
        /// </summary>
        /// <value>
        /// The max buffered delete terms.
        /// </value>
        public int MaxBufferedDeleteTerms
        {
            get {
                if (this.isDisposed) {
                    throw new ObjectDisposedException("IndexWriter", "You cannot access this property from a disposed IndexWriter");
                }
                return this.writer.GetMaxBufferedDeleteTerms();
            }
            set {
                if (this.isDisposed) {
                    throw new ObjectDisposedException("IndexWriter", "You cannot access this property from a disposed IndexWriter");
                }
                this.writer.SetMaxBufferedDeleteTerms(value);
            }
        }

        /// <summary>
        /// Gets or sets the max buffered docs.
        /// </summary>
        /// <value>
        /// The max buffered docs.
        /// </value>
        public int MaxBufferedDocs
        {
            get {
                if (this.isDisposed)
                    throw new ObjectDisposedException("IndexWriter", "You cannot access this property from a disposed IndexWriter");
                return this.writer.GetMaxBufferedDocs();
            }
            set {
                if (this.isDisposed)
                    throw new ObjectDisposedException("IndexWriter", "You cannot access this property from a disposed IndexWriter");
                this.writer.SetMaxBufferedDocs(value);
            }
        }

        /// <summary>
        /// Gets the max documents.
        /// </summary>
        public int MaxDocuments
        {
            get {
                if (this.isDisposed)
                    throw new ObjectDisposedException("IndexWriter", "You cannot access this property from a disposed IndexWriter");
                return this.writer.MaxDoc();
            }
        }

        /// <summary>
        /// Gets or sets the length of the max field.
        /// </summary>
        /// <value>
        /// The length of the max field.
        /// </value>
        public int MaxFieldLength
        {
            get {
                if (this.isDisposed)
                    throw new ObjectDisposedException("IndexWriter", "You cannot access this property from a disposed IndexWriter");
                return this.writer.GetMaxFieldLength();
            }
            set {
                if (this.isDisposed)
                    throw new ObjectDisposedException("IndexWriter", "You cannot access this property from a disposed IndexWriter");
                this.writer.SetMaxFieldLength(value);
            }
        }

        /// <summary>
        /// Gets or sets the max merged docs.
        /// </summary>
        /// <value>
        /// The max merged docs.
        /// </value>
        public int MaxMergedDocs
        {
            get {
                if (this.isDisposed)
                    throw new ObjectDisposedException("IndexWriter", "You cannot access this property from a disposed IndexWriter");
                return this.writer.GetMaxMergeDocs();
            }
            set {
                if (this.isDisposed)
                    throw new ObjectDisposedException("IndexWriter", "You cannot access this property from a disposed IndexWriter");
                this.writer.SetMaxMergeDocs(value);
            }
        }

        /// <summary>
        /// Gets or sets the merge factor.
        /// </summary>
        /// <value>
        /// The merge factor.
        /// </value>
        public int MergeFactor
        {
            get {
                if (this.isDisposed)
                    throw new ObjectDisposedException("IndexWriter", "You cannot access this property from a disposed IndexWriter");
                return this.writer.GetMergeFactor();
            }
            set {
                if (this.isDisposed)
                    throw new ObjectDisposedException("IndexWriter", "You cannot access this property from a disposed IndexWriter");
                this.writer.SetMergeFactor(value);
            }
        }

        /// <summary>
        /// Gets or sets the term index interval.
        /// </summary>
        /// <value>
        /// The term index interval.
        /// </value>
        public int TermIndexInterval
        {
            get {
                if (this.isDisposed)
                    throw new ObjectDisposedException("IndexWriter", "You cannot access this property from a disposed IndexWriter");
                return this.writer.GetTermIndexInterval();
            }
            set {
                if (this.isDisposed)
                    throw new ObjectDisposedException("IndexWriter", "You cannot access this property from a disposed IndexWriter");
                this.writer.SetTermIndexInterval(value);
            }
        }

        /// <summary>
        /// Gets the total documents.
        /// </summary>
        public int TotalDocuments
        {
            get {
                if (this.isDisposed)
                    throw new ObjectDisposedException("IndexWriter", "You cannot access this property from a disposed IndexWriter");
                return this.writer.NumDocs();
            }
        }

        /// <summary>
        /// Gets the total RAM documents.
        /// </summary>
        public int TotalRamDocuments
        {
            get {
                if (this.isDisposed)
                    throw new ObjectDisposedException("IndexWriter", "You cannot access this property from a disposed IndexWriter");
                return this.writer.NumRamDocs();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [uses compound file].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [uses compound file]; otherwise, <c>false</c>.
        /// </value>
        public bool UsesCompoundFile
        {
            get {
                if (this.isDisposed)
                    throw new ObjectDisposedException("IndexWriter", "You cannot access this property from a disposed IndexWriter");
                return this.writer.GetUseCompoundFile();
            }
            set {
                if (this.isDisposed)
                    throw new ObjectDisposedException("IndexWriter", "You cannot access this property from a disposed IndexWriter");
                this.writer.SetUseCompoundFile(value);
            }
        }

        /// <summary>
        /// Gets or sets the write lock timeout.
        /// </summary>
        /// <value>
        /// The write lock timeout.
        /// </value>
        public long WriteLockTimeout
        {
            get {
                if (this.isDisposed)
                    throw new ObjectDisposedException("IndexWriter", "You cannot access this property from a disposed IndexWriter");
                return this.writer.GetWriteLockTimeout();
            }
            set {
                if (this.isDisposed)
                    throw new ObjectDisposedException("IndexWriter", "You cannot access this property from a disposed IndexWriter");
                this.writer.SetWriteLockTimeout(value);
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Closes this index.
        /// </summary>
        public void Close()
        {
            // Flip that toggle switch
            //if (!string.IsNullOrEmpty(this.toggleFile)) IndexMap.HitTheJackalSwitch(this.toggleFile);
            if (this.writer != null)
                this.writer.Close();
            if (this.openDirectory != null)
                this.openDirectory.Close();
            this.writer = null;
            this.openDirectory = null;

            this.index.Refresh();
            if (this.index.IndexStructure == IndexType.DoubleIndex) {
                ((IDoubleIndex)this.index).FlipToggleSwitch();
            }
            else if (this.index.IndexStructure == IndexType.CyclicalIndex) {
                ICyclicalIndex ci = (ICyclicalIndex)this.index;
                ci.CopyMirror();
                ci.FlipToggleSwitch();

            }

            this.isDisposed = true;
            LibraryAnalysis.Fire(new IndexInfo(this.index.IndexDirectory.Parent.FullName, this.index.IndexDirectory.Name, this.totalWrites, this.optimized, DateTime.Now));
        }

        /// <summary>
        /// Commits this instance.
        /// </summary>
        public void Commit()
        {
            if (this.isDisposed)
                throw new ObjectDisposedException("IndexWriter", "You cannot access this method from a disposed IndexWriter");
            // Commits all pending changes (added & deleted, optimizations, segment merges, etc) to the index, and syncs all referenced index files,
            // such that a reader will see the changes and the index updates will survive an OS or machine crash or power loss
            this.writer.Commit();
        }

        /// <summary>
        /// Deletes all documents in this writer.
        /// </summary>
        public void DeleteAll()
        {
            if (this.isDisposed)
                throw new ObjectDisposedException("IndexWriter", "You cannot access this method from a disposed IndexWriter");
            this.writer.DeleteAll();
        }

        /// <summary>
        /// Deletes all documents that match the specified query.
        /// </summary>
        /// <param name="query">The query to run against this index.</param>
        public void DeleteDocuments(QueryBuilder query)
        {
            if (this.isDisposed)
                throw new ObjectDisposedException("IndexWriter", "You cannot access this method from a disposed IndexWriter");
            if (query == null)
                throw new ArgumentNullException("query", "query cannot be null");
            this.writer.DeleteDocuments(query.GetLuceneQuery);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Close();
        }

        /// <summary>
        /// Optimizes this index.
        /// </summary>
        public void Optimize()
        {
            if (this.isDisposed)
                throw new ObjectDisposedException("IndexWriter", "You cannot access this method from a disposed IndexWriter");
            this.writer.Optimize();
            this.optimized = true;
        }

        /// <summary>
        /// Optimizes this index.
        /// </summary>
        /// <param name="doWait">if set to <c>true</c> this call will block until the optimize completes.</param>
        [Experimental("Requires a MergeScheduler that isn't implemented yet")]
        public void Optimize(bool doWait)
        {
            if (this.isDisposed)
                throw new ObjectDisposedException("IndexWriter", "You cannot access this method from a disposed IndexWriter");
            this.writer.Optimize(doWait);
            this.optimized = true;
        }

        /// <summary>
        /// Optimizes this index down to less than or equal to maxNumSegments. If maxNumSegments == 1 then this is the same as Optimize().
        /// </summary>
        /// <param name="maxSegments">The max number segments.</param>
        public void Optimize(int maxSegments)
        {
            if (this.isDisposed)
                throw new ObjectDisposedException("IndexWriter", "You cannot access this method from a disposed IndexWriter");
            this.writer.Optimize(maxSegments);
            this.optimized = true;
        }

        /// <summary>
        /// Optimizes this index down to less than or equal to maxNumSegments. If maxNumSegments == 1 then this is the same as Optimize().
        /// </summary>
        /// <param name="maxSegments">The max number segments.</param>
        /// <param name="doWait">if set to <c>true</c> this call will block until the optimize completes.</param>
        [Experimental("Requires a MergeSchedule that isn't implemented yet")]
        public void Optimize(int maxSegments, bool doWait)
        {
            if (this.isDisposed)
                throw new ObjectDisposedException("IndexWriter", "You cannot access this method from a disposed IndexWriter");
            this.writer.Optimize(maxSegments, doWait);
            this.optimized = true;
        }

        /// <summary>
        /// Prepares the commit operation.
        /// </summary>
        public void PrepareCommit()
        {
            if (this.isDisposed)
                throw new ObjectDisposedException("IndexWriter", "You cannot access this method from a disposed IndexWriter");
            this.writer.PrepareCommit();
        }

        /// <summary>
        /// Rollbacks this instance.
        /// </summary>
        public void Rollback()
        {
            if (this.isDisposed)
                throw new ObjectDisposedException("IndexWriter", "You cannot access this method from a disposed IndexWriter");
            this.writer.Rollback();
        }

        /// <summary>
        /// Writes the specified document out to the index.
        /// </summary>
        /// <param name="document">The document to output.</param>
        public void Write(IndexDocument document)
        {
            this.Write(document, IndexLibrary.AnalyzerType.None);
        }

        /// <summary>
        /// Writes the specified document.
        /// </summary>
        /// <param name="document">The document to out.</param>
        /// <param name="appliedAnalyzer">An analyzer to use against specifically this field.</param>
        /// <remarks>
        /// This is useful if you have a specific type of field or document that needs special analysis rules;
        /// not normally helpful if the analyzer is more complex, usually useful if the analyzer is less complex.
        /// </remarks>
        public void Write(IndexDocument document, AnalyzerType appliedAnalyzer)
        {
            if (this.isDisposed)
                throw new ObjectDisposedException("IndexWriter", "You cannot access this method from a disposed IndexWriter");
            if (document == null)
                throw new ArgumentNullException("document", "document cannot be null");

            OnWriting(new WriterEventArgs(document, (appliedAnalyzer == IndexLibrary.AnalyzerType.None || appliedAnalyzer == IndexLibrary.AnalyzerType.Unknown) ? this.AnalyzerType : appliedAnalyzer, this.index.IndexDirectory.Name));
            if (appliedAnalyzer != IndexLibrary.AnalyzerType.None && appliedAnalyzer != IndexLibrary.AnalyzerType.Unknown) {
                this.writer.AddDocument(document.GetLuceneDocument, TypeConverter.GetAnalyzer(appliedAnalyzer));
            }
            else {
                this.writer.AddDocument(document.GetLuceneDocument);
            }
            this.totalWrites++;
        }

        public void Write<TKey, TValue>(IDictionary<TKey, TValue> values, FieldStorage storage)
        {
            if (!this.IsOpen)
                throw new ObjectDisposedException("IndexWriter", "You cannot access this method from a disposed IndexWriter");
            if (values == null)
                throw new ArgumentNullException("values", "values cannot be null");
            if (storage == null)
                throw new ArgumentNullException("storage", "storage cannot be null");

            IndexDocument document = new IndexDocument();
            foreach (KeyValuePair<TKey, TValue> pair in values) {
                if (pair.Key == null || pair.Value == null)
                    continue;
                string fieldName = pair.Key.ToString();
                string fieldValue = pair.Value.ToString();
                if (string.IsNullOrEmpty(fieldName) || string.IsNullOrEmpty(fieldValue))
                    continue;
                document.Add(new FieldNormal(fieldName, fieldValue, storage.Store, storage.SearchRule, storage.VectorRule));
            }
            this.Write(document);
        }

        public void Write<TKey, TValue>(ILookup<TKey, TValue> values, FieldStorage storage)
        {
            Write<TKey, TValue>(values, storage, false);
        }

        public void Write<TKey, TValue>(ILookup<TKey, TValue> values, FieldStorage storage, bool makeSingleDocument)
        {
            if (!this.IsOpen)
                throw new ObjectDisposedException("IndexWriter", "You cannot access this method from a disposed IndexWriter");
            if (values == null)
                throw new ArgumentNullException("values", "values cannot be null");
            if (storage == null)
                throw new ArgumentNullException("storage", "storage cannot be null");

            if (makeSingleDocument) {
                IndexDocument document = new IndexDocument();
                foreach (TKey key in values) {
                    if (key == null)
                        continue;
                    string fieldName = key.ToString();
                    if (string.IsNullOrEmpty(fieldName))
                        continue;
                    foreach (TValue value in values[key]) {
                        if (value == null)
                            continue;
                        string fieldValue = value.ToString();
                        if (string.IsNullOrEmpty(fieldValue))
                            continue;
                        document.Add(new FieldNormal(fieldName, fieldValue, storage.Store, storage.SearchRule, storage.VectorRule));
                    }
                }
                if (document.TotalFields > 0)
                    this.Write(document);
            }
            else {
                foreach (TKey key in values) {
                    if (key == null)
                        continue;
                    IndexDocument document = new IndexDocument();
                    string fieldName = key.ToString();
                    if (string.IsNullOrEmpty(fieldName))
                        continue;
                    foreach (TValue value in values[key]) {
                        if (value == null)
                            continue;
                        string fieldValue = value.ToString();
                        if (string.IsNullOrEmpty(fieldValue))
                            continue;
                        document.Add(new FieldNormal(fieldName, fieldValue, storage.Store, storage.SearchRule, storage.VectorRule));
                    }
                    if (document.TotalFields > 0)
                        this.Write(document);
                }
            }
        }

        public void Write<TKey>(string fieldName, IEnumerable<TKey> values, FieldStorage storage)
        {
            Write<TKey>(fieldName, values, storage, false);
        }

        public void Write<TKey>(string fieldName, IEnumerable<TKey> values, FieldStorage storage, bool makeSingleDocument)
        {
            if (!this.IsOpen)
                throw new ObjectDisposedException("IndexWriter", "You cannot access this method from a disposed IndexWriter");
            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentNullException("fieldName", "fieldName cannot be null");
            if (values == null)
                throw new ArgumentNullException("values", "values cannot be null");
            if (storage == null)
                throw new ArgumentNullException("storage", "storage cannot be null");

            if (makeSingleDocument) {
                IndexDocument document = new IndexDocument();
                foreach (TKey value in values) {
                    if (value == null)
                        continue;
                    string fieldValue = value.ToString();
                    if (string.IsNullOrEmpty(fieldValue))
                        continue;
                    document.Add(new FieldNormal(fieldName, fieldValue, storage.Store, storage.SearchRule, storage.VectorRule));
                }
                if (document.TotalFields > 0)
                    this.Write(document);
            }
            else {
                foreach (TKey value in values) {
                    if (value == null)
                        continue;
                    string fieldValue = value.ToString();
                    if (string.IsNullOrEmpty(fieldValue))
                        continue;
                    IndexDocument document = new IndexDocument();
                    document.Add(new FieldNormal(fieldName, fieldValue, storage.Store, storage.SearchRule, storage.VectorRule));
                    this.Write(document);
                }
            }
        }

        public void WriteDataRow(DataRow dataRow, IndexWriterRuleCollection ruleCollection)
        {
            WriteDataRow(dataRow, ruleCollection, AnalyzerType.None);
        }

        public void WriteDataRow(DataRow dataRow, IndexWriterRuleCollection ruleCollection, AnalyzerType analyzerType)
        {
            if (dataRow == null)
                throw new ArgumentNullException("dataRow", "dataRow cannot be null");
            if (ruleCollection == null)
                throw new ArgumentNullException("ruleCollection", "ruleCollection cannot be null");
            DataTable dataTable = dataRow.Table;
            int totalColumns = dataTable.Columns.Count;

            IndexDocument document = new IndexDocument();

            for (int j = 0; j < totalColumns; j++) {
                DataColumn column = dataTable.Columns[j];
                if (string.IsNullOrEmpty(column.ColumnName)) {
                    continue;
                }
                IndexWriterRule rule = ruleCollection.GetRuleFromType(column.DataType);
                if (rule.SkipField(column.DataType, column.ColumnName)) {
                    continue;
                }
                FieldStorage storageRule = rule.GetStorageRule(column.ColumnName);

                // that's all the field data, now lets get the value data
                object rowValue = dataRow[j];
                bool isRowNull = rowValue == null || rowValue == DBNull.Value;
                if (rule.SkipColumnIfNull && isRowNull) {
                    continue;
                }
                else if (!rule.SkipColumnIfNull && string.IsNullOrEmpty(rule.DefaultValueIfNull)) {
                    continue;
                }

                string fieldValue = (isRowNull) ? rule.DefaultValueIfNull : rowValue.ToString();
                rowValue = null;
                document.Add(new FieldNormal(column.ColumnName, fieldValue, storageRule.Store, storageRule.SearchRule, storageRule.VectorRule));
            }

            if (document.TotalFields > 0) {
                if (analyzerType == AnalyzerType.None || analyzerType == AnalyzerType.Unknown) {
                    this.Write(document);
                }
                else {
                    this.Write(document, analyzerType);
                }
            }
        }

        public void WriteDataSet(DataSet dataSet, IndexWriterRuleCollection ruleCollection)
        {
            WriteDataSet(dataSet, ruleCollection, AnalyzerType.None);
        }

        public void WriteDataSet(DataSet dataSet, IndexWriterRuleCollection ruleCollection, AnalyzerType analyzerType)
        {
            if (!this.IsOpen)
                throw new ObjectDisposedException("IndexWriter", "You cannot access this method from a disposed IndexWriter");
            if (dataSet == null || dataSet.Tables.Count == 0)
                return;
            if (ruleCollection == null)
                throw new ArgumentNullException("ruleCollection", "ruleCollection cannot be null");

            int totalTables = dataSet.Tables.Count;
            for (int i = 0; i < totalTables; i++)
                WriteDataTable(dataSet.Tables[i], ruleCollection, analyzerType);
        }

        public void WriteDataTable(DataTable dataTable, IndexWriterRuleCollection ruleCollection)
        {
            WriteDataTable(dataTable, ruleCollection, AnalyzerType.None);
        }

        public void WriteDataTable(DataTable dataTable, IndexWriterRuleCollection ruleCollection, AnalyzerType analyzerType)
        {
            if (!this.IsOpen)
                throw new ObjectDisposedException("IndexWriter", "You cannot access this method from a disposed IndexWriter");
            if (dataTable == null)
                throw new ArgumentNullException("dataTable", "dataTable cannot be null");
            if (ruleCollection == null)
                throw new ArgumentNullException("ruleCollection", "ruleCollection cannot be null");
            int totalColumns = dataTable.Columns.Count;
            int totalRows = dataTable.Rows.Count;
            if (totalColumns == 0 || totalRows == 0)
                return;

            for (int i = 0; i < totalRows; i++) {
                DataRow row = dataTable.Rows[i];

                IndexDocument document = new IndexDocument();

                for (int j = 0; j < totalColumns; j++) {
                    DataColumn column = dataTable.Columns[j];
                    if (string.IsNullOrEmpty(column.ColumnName))
                        continue;
                    IndexWriterRule rule = ruleCollection.GetRuleFromType(column.DataType);
                    if (rule.SkipField(column.DataType, column.ColumnName))
                        continue;
                    FieldStorage storageRule = rule.GetStorageRule(column.ColumnName);

                    // that's all the field data, now lets get the value data
                    object rowValue = row[j];
                    bool isRowNull = rowValue == null || rowValue == DBNull.Value || string.IsNullOrEmpty(rowValue.ToString());
                    if (rule.SkipColumnIfNull && isRowNull)
                        continue;
                    else if (!rule.SkipColumnIfNull && string.IsNullOrEmpty(rule.DefaultValueIfNull) && isRowNull)
                        continue;

                    string fieldValue = (isRowNull) ? rule.DefaultValueIfNull : rowValue.ToString();
                    rowValue = null;
                    document.Add(new FieldNormal(column.ColumnName, fieldValue, storageRule.Store, storageRule.SearchRule, storageRule.VectorRule));
                }

                if (document.TotalFields > 0) {
                    if (analyzerType == AnalyzerType.None || analyzerType == AnalyzerType.Unknown)
                        this.Write(document);
                    else
                        this.Write(document, analyzerType);
                }
            }
        }

        public void WriteDataView(DataView view, IndexWriterRuleCollection ruleCollection)
        {
            WriteDataView(view, ruleCollection, AnalyzerType.None);
        }

        public void WriteDataView(DataView view, IndexWriterRuleCollection ruleCollection, AnalyzerType analyzerType)
        {
            DataTable table = null;
            if (view != null)
                table = view.Table == null ? view.ToTable() : view.Table;
            WriteDataTable(table, ruleCollection, analyzerType);
        }

        public void WriteDirectories(FileSystemInfo[] directories)
        {
            WriteDirectories(directories, FieldStorage.Default);
        }

        public void WriteDirectories(FileSystemInfo[] directories, FieldStorage storage)
        {
            if (directories == null || directories.Length == 0)
                return;
            if (storage == null)
                throw new ArgumentNullException("storage", "storage cannot be null");
            int totalDirectories = directories.Length;
            for (int i = 0; i < totalDirectories; i++)
                WriteDirectory(directories[i], storage);
        }

        public void WriteDirectory(FileSystemInfo info)
        {
            WriteDirectory(info, FieldStorage.Default);
        }

        public void WriteDirectory(FileSystemInfo info, FieldStorage storage)
        {
            if (info == null)
                throw new ArgumentNullException("info", "info cannot be null");
            if (storage == null)
                throw new ArgumentNullException("storage", "storage cannot be null");
            IndexDocument document = new IndexDocument();
            if (!string.IsNullOrEmpty(info.Attributes.ToString()))
                document.Add(new FieldNormal("Attributes", info.Attributes.ToString(), storage.Store, storage.SearchRule, storage.VectorRule));
            document.Add(new FieldNormal("CreationTime", info.CreationTime.ToString("G"), storage.Store, storage.SearchRule, storage.VectorRule));
            document.Add(new FieldNormal("Exists", info.Exists.ToString(), storage.Store, storage.SearchRule, storage.VectorRule));
            document.Add(new FieldNormal("Extension", info.Extension, storage.Store, storage.SearchRule, storage.VectorRule));
            document.Add(new FieldNormal("FullName", info.FullName, storage.Store, storage.SearchRule, storage.VectorRule));
            document.Add(new FieldNormal("LastAccessTime", info.LastAccessTime.ToString("G"), storage.Store, storage.SearchRule, storage.VectorRule));
            document.Add(new FieldNormal("LastWriteTime", info.LastWriteTime.ToString("G"), storage.Store, storage.SearchRule, storage.VectorRule));
            document.Add(new FieldNormal("Name", info.Name, storage.Store, storage.SearchRule, storage.VectorRule));
            this.Write(document);

            // this.WriteInstance<DirectoryInfo>(info, storage); // could simpify the whole thing
        }

        public void WriteDriveInfo(DriveInfo info)
        {
            WriteDriveInfo(info, FieldStorage.Default);
        }

        public void WriteDriveInfo(DriveInfo info, FieldStorage storage)
        {
            if (info == null)
                throw new ArgumentNullException("info", "info cannot be null");
            if (storage == null)
                throw new ArgumentNullException("storage", "storage cannot be null");
            IndexDocument document = new IndexDocument();
            document.Add(new FieldNormal("AvailableFreeSpace", info.AvailableFreeSpace.ToString(), storage.Store, storage.SearchRule, storage.VectorRule));
            document.Add(new FieldNormal("DriveFormat", info.DriveFormat, storage.Store, storage.SearchRule, storage.VectorRule));
            document.Add(new FieldNormal("DriveType", info.DriveType.ToString(), storage.Store, storage.SearchRule, storage.VectorRule));
            document.Add(new FieldNormal("IsReady", info.IsReady.ToString(), storage.Store, storage.SearchRule, storage.VectorRule));
            document.Add(new FieldNormal("Name", info.Name, storage.Store, storage.SearchRule, storage.VectorRule));
            if (info.RootDirectory != null && !string.IsNullOrEmpty(info.RootDirectory.FullName))
                document.Add(new FieldNormal("", info.RootDirectory.FullName, storage.Store, storage.SearchRule, storage.VectorRule));
            document.Add(new FieldNormal("TotalFreeSpace", info.TotalFreeSpace.ToString(), storage.Store, storage.SearchRule, storage.VectorRule));
            document.Add(new FieldNormal("TotalSize", info.TotalSize.ToString(), storage.Store, storage.SearchRule, storage.VectorRule));
            document.Add(new FieldNormal("VolumeLabel", info.VolumeLabel, storage.Store, storage.SearchRule, storage.VectorRule));
            this.Write(document);
        }

        public void WriteDriveInfos(DriveInfo[] driveInfos)
        {
            WriteDriveInfos(driveInfos, FieldStorage.Default);
        }

        public void WriteDriveInfos(DriveInfo[] driveInfos, FieldStorage storage)
        {
            if (driveInfos == null || driveInfos.Length == 0)
                return;
            if (storage == null)
                throw new ArgumentNullException("storage", "storage cannot be null");
            int totalDrives = driveInfos.Length;
            for (int i = 0; i < totalDrives; i++)
                WriteDriveInfo(driveInfos[i], storage);
        }

        public void WriteFile(FileInfo info)
        {
            WriteFile(info, FieldStorage.Default);
        }

        public void WriteFile(FileInfo info, FieldStorage storage)
        {
            if (info == null)
                throw new ArgumentNullException("info", "info cannot be null");
            if (storage == null)
                throw new ArgumentNullException("storage", "storage cannot be null");
            IndexDocument document = new IndexDocument();
            if (!string.IsNullOrEmpty(info.Attributes.ToString()))
                document.Add(new FieldNormal("Attributes", info.Attributes.ToString(), storage.Store, storage.SearchRule, storage.VectorRule));
            document.Add(new FieldNormal("CreationTime", info.CreationTime.ToString("G"), storage.Store, storage.SearchRule, storage.VectorRule));
            if (info.Directory != null)
                document.Add(new FieldNormal("Directory", info.Directory.FullName, storage.Store, storage.SearchRule, storage.VectorRule));
            if (!string.IsNullOrEmpty(info.DirectoryName))
                document.Add(new FieldNormal("DirectoryName", info.DirectoryName, storage.Store, storage.SearchRule, storage.VectorRule));
            document.Add(new FieldNormal("Exists", info.Exists.ToString(), storage.Store, storage.SearchRule, storage.VectorRule));
            document.Add(new FieldNormal("Extension", info.Extension, storage.Store, storage.SearchRule, storage.VectorRule));
            document.Add(new FieldNormal("FullName", info.FullName, storage.Store, storage.SearchRule, storage.VectorRule));
            document.Add(new FieldNormal("IsReadOnly", info.IsReadOnly.ToString(), storage.Store, storage.SearchRule, storage.VectorRule));
            document.Add(new FieldNormal("LastAccessTime", info.LastAccessTime.ToString("G"), storage.Store, storage.SearchRule, storage.VectorRule));
            document.Add(new FieldNormal("LastWriteTime", info.LastWriteTime.ToString("G"), storage.Store, storage.SearchRule, storage.VectorRule));
            document.Add(new FieldNormal("Length", info.Length.ToString(), storage.Store, storage.SearchRule, storage.VectorRule));
            document.Add(new FieldNormal("Name", info.Name, storage.Store, storage.SearchRule, storage.VectorRule));
            this.Write(document);

            // writer.WriteInstance<FileInfo>(info, storage); // could simplfy the whole thing
        }

        public void WriteFiles(FileInfo[] files)
        {
            WriteFiles(files, FieldStorage.Default);
        }

        public void WriteFiles(FileInfo[] files, FieldStorage storage)
        {
            if (files == null || files.Length == 0)
                return;
            if (storage == null)
                throw new ArgumentNullException("storage", "storage cannot be null");
            int totalFiles = files.Length;
            for (int i = 0; i < totalFiles; i++)
                WriteFile(files[i]);
        }

        /// <summary>
        /// Outputs the directory of index files to read
        /// </summary>
        /// <param name="info">out of the discovered index directory</param>
        /// <returns>True if there are index files; false if not</returns>
        private bool GetIndexWriteDirectory(out DirectoryInfo info)
        {
            switch (index.IndexStructure) {
                case IndexType.SingleIndex:
                    IIndex singleIndex = (IIndex)index;
                    info = singleIndex.IndexDirectory;
                    return singleIndex.HasIndexFiles();
                case IndexType.DoubleIndex:
                case IndexType.CyclicalIndex:
                    IDoubleIndex doubleIndex = (IDoubleIndex)index;
                    info = doubleIndex.GetWriteDirectory();
                    return doubleIndex.HasWriteIndexFiles();
                default:
                    throw new NotImplementedException(index.IndexStructure.ToString() + " not supported");
            }
        }

        /// <summary>
        /// Raises the <see cref="E:Writing"/> event.
        /// </summary>
        /// <param name="e">The <see cref="IndexLibrary.WriterEventArgs"/> instance containing the event data.</param>
        private void OnWriting(WriterEventArgs e)
        {
            EventHandler<WriterEventArgs> handler = Writing;
            if (handler != null)
                handler(this, e);
        }

        #endregion Methods
    }
}