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
    /// Represents information about an IndexSearcher operation
    /// </summary>
    [System.CLSCompliant(true)]
    public sealed class SearchInfo
    {
        #region Fields

        /// <summary>
        /// Specifies whether the searching operation was cancelled or not
        /// </summary>
        private bool canceled;

        /// <summary>
        /// The time this instance was created
        /// </summary>
        private DateTime createdTime;

        /// <summary>
        /// The name of the specified index
        /// </summary>
        private string indexName;

        /// <summary>
        /// The query executed
        /// </summary>
        private string query;

        /// <summary>
        /// The type of method that triggered this instance
        /// </summary>
        private SearchMethodType searchMethodType;

        /// <summary>
        /// The total number of results found
        /// </summary>
        private int totalResultsFound;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchInfo"/> class.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="methodType">Type of the method.</param>
        /// <param name="totalResultsFound">The total results found.</param>
        /// <param name="wasCanceled">if set to <c>true</c> [was canceled].</param>
        public SearchInfo(string indexName, string query, SearchMethodType methodType, int totalResultsFound, bool wasCanceled)
            : this(indexName, query, methodType, totalResultsFound, wasCanceled, DateTime.Now)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchInfo"/> class.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="query">The query.</param>
        /// <param name="methodType">Type of the method.</param>
        /// <param name="totalResultsFound">The total results found.</param>
        /// <param name="wasCanceled">if set to <c>true</c> [was canceled].</param>
        /// <param name="createTime">The create time.</param>
        public SearchInfo(string indexName, string query, SearchMethodType methodType, int totalResultsFound, bool wasCanceled, DateTime createTime)
        {
            this.searchMethodType = methodType;
            this.totalResultsFound = totalResultsFound;
            this.query = query;
            this.canceled = wasCanceled;
            this.createdTime = createTime;
            this.indexName = indexName;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Creates an empty instance of this class.
        /// </summary>
        public static SearchInfo Empty
        {
            get {
                return new SearchInfo(null, null, SearchMethodType.Normal, -1, true) { createdTime = DateTime.MinValue };
            }
        }

        /// <summary>
        /// Gets a value indicating whether the search method was canceled or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if canceled; otherwise, <c>false</c>.
        /// </value>
        public bool Canceled
        {
            get { return canceled; }
        }

        /// <summary>
        /// Gets the created time of this instance.
        /// </summary>
        public DateTime CreatedTime
        {
            get { return createdTime; }
        }

        /// <summary>
        /// Gets the name of the index.
        /// </summary>
        /// <value>
        /// The name of the index.
        /// </value>
        public string IndexName
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
        /// Gets the query that triggered this instance.
        /// </summary>
        public string Query
        {
            get { return query; }
        }

        /// <summary>
        /// Gets the type of the search method used to create this instance.
        /// </summary>
        /// <value>
        /// The type of the search method.
        /// </value>
        public SearchMethodType SearchMethodType
        {
            get { return searchMethodType; }
        }

        /// <summary>
        /// Gets the total results found.
        /// </summary>
        public int TotalResultsFound
        {
            get { return totalResultsFound; }
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
        public override sealed bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(SearchInfo))
                return false;
            SearchInfo temp = (SearchInfo)obj;
            return temp.canceled == this.canceled && temp.totalResultsFound == this.totalResultsFound && temp.searchMethodType == this.searchMethodType && temp.createdTime == this.createdTime && temp.indexName == this.indexName && temp.query == this.query;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override sealed int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override sealed string ToString()
        {
            return string.Format("At {0} query: {1}", this.createdTime.ToShortTimeString(), (string.IsNullOrEmpty(this.query)) ? "No Query" : this.query);
        }

        #endregion Methods
    }
}