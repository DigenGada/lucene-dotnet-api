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

    /// <summary>
    /// Represents information about an IndexReader operation
    /// </summary>
    [System.CLSCompliant(true)]
    public sealed class ReadInfo
    {
        #region Fields

        /// <summary>
        /// Specifies whether the entire index was read to build the filters or not
        /// </summary>
        private bool builtAllFilters;

        /// <summary>
        /// The time this instance was created
        /// </summary>
        private DateTime createdTime;

        /// <summary>
        /// The name of the specified index
        /// </summary>
        private string indexName;

        /// <summary>
        /// Specifies whether the index is optimized or not
        /// </summary>
        private bool isOptimized;

        /// <summary>
        /// The total number of documents in the specified index
        /// </summary>
        private int totalDocumentsInIndex;

        /// <summary>
        /// The total number of documents retrieved
        /// </summary>
        private int totalDocumentsRetrieved;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadInfo"/> class.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="totalDocsRetrieved">The total docs retrieved.</param>
        /// <param name="totalDocsInIndex">Total index of the docs in.</param>
        /// <param name="builtAllFilters">if set to <c>true</c> filters were built off all data in the index.</param>
        /// <param name="optimized">if set to <c>true</c> [optimized].</param>
        public ReadInfo(string indexName, int totalDocsRetrieved, int totalDocsInIndex, bool builtAllFilters, bool optimized)
        {
            if (string.IsNullOrEmpty(indexName))
                throw new ArgumentNullException("indexName", "indexName cannot be null or empty");
            if (totalDocsRetrieved == -1)
                totalDocsRetrieved = totalDocsInIndex;
            this.indexName = indexName;
            this.totalDocumentsInIndex = totalDocsInIndex;
            this.totalDocumentsRetrieved = totalDocsRetrieved;
            this.builtAllFilters = builtAllFilters;
            this.isOptimized = optimized;
            this.createdTime = DateTime.Now;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadInfo"/> class.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="totalDocsRetrieved">The total docs retrieved.</param>
        /// <param name="totalDocsInIndex">Total index of the docs in.</param>
        /// <param name="builtAllFilters">if set to <c>true</c> filters were built off all data in the index.</param>
        /// <param name="optimized">if set to <c>true</c> [optimized].</param>
        /// <param name="createTime">The time this info was created</param>
        public ReadInfo(string indexName, int totalDocsRetrieved, int totalDocsInIndex, bool builtAllFilters, bool optimized, DateTime createTime)
        {
            if (string.IsNullOrEmpty(indexName))
                throw new ArgumentNullException("indexName", "indexName cannot be null or empty");
            if (totalDocsRetrieved == -1)
                totalDocsRetrieved = totalDocsInIndex;
            this.indexName = indexName;
            this.totalDocumentsInIndex = totalDocsInIndex;
            this.totalDocumentsRetrieved = totalDocsRetrieved;
            this.builtAllFilters = builtAllFilters;
            this.isOptimized = optimized;
            this.createdTime = createTime;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Creates an empty instance of this class.
        /// </summary>
        public static ReadInfo Empty
        {
            get { return new ReadInfo("null", -1, -1, false, false) { createdTime = DateTime.MinValue }; }
        }

        /// <summary>
        /// Gets a value indicating whether the filters in the resultant dataset were built
        /// off all data in the index or just the requested rows.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [built filters on all data]; otherwise, <c>false</c>.
        /// </value>
        public bool BuiltFiltersOnAllData
        {
            get { return this.builtAllFilters; }
        }

        /// <summary>
        /// Gets the created time of this instance.
        /// </summary>
        public DateTime CreatedTime
        {
            get { return createdTime; }
        }

        public string IndexName
        {
            get {
                System.IO.DirectoryInfo info = new System.IO.DirectoryInfo(indexName);
                if (info.Parent == null || info.ToString() == info.Name)
                    return info.Name;
                if (info.Parent.Name.Equals(StaticValues.DirectoryA) || info.Parent.Name.Equals(StaticValues.DirectoryB)) {
                    if (info.Parent.Parent != null)
                        return info.Parent.Parent.Name;
                    return info.Parent.Name;
                }
                return info.Parent.Name;
            }
        }

        /// <summary>
        /// Gets the name of the index.
        /// </summary>
        /// <value>
        /// The name of the index.
        /// </value>
        public string IndexPath
        {
            get { return indexName; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is empty; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty
        {
            get { return this.Equals(Empty); }
        }

        /// <summary>
        /// Gets a value indicating whether the index is optimized.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is optimized; otherwise, <c>false</c>.
        /// </value>
        public bool IsOptimized
        {
            get { return isOptimized; }
        }

        /// <summary>
        /// Gets the total number of the documents in the index.
        /// </summary>
        /// <value>
        /// The total number of documents.
        /// </value>
        public int TotalDocumentsInIndex
        {
            get { return totalDocumentsInIndex; }
        }

        /// <summary>
        /// Gets the total documents retrieved.
        /// </summary>
        public int TotalDocumentsRetrieved
        {
            get { return totalDocumentsRetrieved; }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        ///   </exception>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(ReadInfo))
                return false;
            ReadInfo temp = (ReadInfo)obj;
            return temp.createdTime == this.createdTime && temp.isOptimized == this.isOptimized && temp.totalDocumentsInIndex == this.totalDocumentsInIndex && temp.totalDocumentsRetrieved == this.totalDocumentsRetrieved && temp.indexName == this.indexName;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("At {0} reader opened to reader top {1} results", this.createdTime.ToShortTimeString(), this.totalDocumentsRetrieved);
        }

        #endregion Methods
    }
}