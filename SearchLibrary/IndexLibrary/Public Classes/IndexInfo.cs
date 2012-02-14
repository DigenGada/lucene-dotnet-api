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
    using System.IO;

    /// <summary>
    /// Represents information about an IndexWriter operation
    /// </summary>
    [CLSCompliant(true)]
    public sealed class IndexInfo
    {
        #region Fields

        /// <summary>
        /// The time this info was created
        /// </summary>
        private DateTime createdTime;

        /// <summary>
        /// The directory of the index used in this operation
        /// </summary>
        private DirectoryInfo directoryInfo;

        /// <summary>
        /// The name of the index used in this operation
        /// </summary>
        private string indexName;

        /// <summary>
        /// Indicates whether the index was optimized or not
        /// </summary>
        private bool optimized;

        /// <summary>
        /// Indicates the total number of documents in the indes
        /// </summary>
        private int totalDocuments;

        /// <summary>
        /// Indicates the total number of reads performed against this index
        /// </summary>
        private int totalReads;

        /// <summary>
        /// Indicates the total number of searches performed against this index
        /// </summary>
        private int totalSearches;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexInfo"/> class.
        /// </summary>
        /// <param name="indexDirectory">The index directory.</param>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="totalDocuments">The total documents.</param>
        /// <param name="optimized">if set to <c>true</c> [optimized].</param>
        /// <param name="createTime">The create time.</param>
        /// <exception cref="System.ArgumentNullException">
        /// The <paramref name="indexDirectory"/> parameter is null or empty
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// The <paramref name="indexName"/> parameter is null or empty
        /// </exception>
        public IndexInfo(string indexDirectory, string indexName, int totalDocuments, bool optimized, DateTime createTime)
        {
            this.totalSearches = 0;
            this.totalReads = 0;
            if (string.IsNullOrEmpty(indexDirectory)) {
                throw new ArgumentNullException("indexDirectory", "indexDirectory cannot be null or empty");
            }
            if (string.IsNullOrEmpty(indexName)) {
                throw new ArgumentNullException("indexName", "indexName cannot be null or empty");
            }
            this.directoryInfo = new DirectoryInfo(indexDirectory);
            if (!this.directoryInfo.Exists) {
                throw new DirectoryNotFoundException(indexDirectory);
            }
            this.totalDocuments = totalDocuments;
            this.createdTime = createTime;
            this.optimized = optimized;
            this.indexName = indexName;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="IndexInfo"/> class from being created.
        /// </summary>
        private IndexInfo()
        {
            this.totalSearches = 0;
            this.totalReads = 0;
            this.directoryInfo = new DirectoryInfo(@"C:\");
            this.totalDocuments = -1;
            this.createdTime = DateTime.MinValue;
            this.totalDocuments = 0;
            this.optimized = false;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Creates an empty version of this class
        /// </summary>
        public static IndexInfo Empty
        {
            get { return new IndexInfo(); }
        }

        /// <summary>
        /// Gets the DateTime of when this info was collected
        /// </summary>
        public DateTime CreatedTime
        {
            get { return this.createdTime; }
        }

        /// <summary>
        /// Gets the index directory.
        /// </summary>
        public string IndexDirectory
        {
            get { return this.directoryInfo.FullName; }
        }

        /// <summary>
        /// Gets the name of the index.
        /// </summary>
        /// <value>
        /// The name of the index.
        /// </value>
        public string IndexName
        {
            get { return this.indexName; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IndexInfo"/> is optimized.
        /// </summary>
        /// <value>
        ///   <c>true</c> if optimized; otherwise, <c>false</c>.
        /// </value>
        public bool Optimized
        {
            get { return this.optimized; }
        }

        /// <summary>
        /// Gets the time since this index was created
        /// </summary>
        public TimeSpan TimeSinceCreation
        {
            get { return DateTime.Now.Subtract(this.createdTime); }
        }

        /// <summary>
        /// Gets the total number of documents contained in this index
        /// </summary>
        public int TotalDocuments
        {
            get { return this.totalDocuments; }
        }

        /// <summary>
        /// Gets the total reads performed against this index
        /// </summary>
        public int TotalReads
        {
            get { return this.totalReads; }
            internal set { this.totalReads = value; }
        }

        /// <summary>
        /// Gets the total searches performed against this index
        /// </summary>
        public int TotalSearches
        {
            get { return this.totalSearches; }
            internal set { this.totalSearches = value; }
        }

        #endregion Properties
    }
}