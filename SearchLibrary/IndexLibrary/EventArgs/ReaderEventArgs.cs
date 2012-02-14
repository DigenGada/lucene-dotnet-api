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
    /// Represents event arguments that are passed to <see cref="IndexLibrary.IndexReader"/> events
    /// </summary>
    [System.CLSCompliant(true)]
    public sealed class ReaderEventArgs : EventArgs
    {
        #region Fields

        /// <summary>
        /// Indicates whether or not all data should be read so filters can be built
        /// </summary>
        private bool buildFieldFiltersFromAllData;

        /// <summary>
        /// Indicates if the read operation was canceled or not
        /// </summary>
        private bool cancel;

        /// <summary>
        /// Indicates the field names that should be read from the index (null means all)
        /// </summary>
        private string[] fieldNames;

        /// <summary>
        /// The index name
        /// </summary>
        private string indexName;

        /// <summary>
        /// The total number of results read out of the index
        /// </summary>
        private int topNResults;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ReaderEventArgs"/> class.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="topNResults">The top N results.</param>
        /// <param name="buildFieldFiltersFromAllData">if set to <c>true</c> [build field filters from all data].</param>
        /// <param name="filterFieldNames">The filter field names.</param>
        public ReaderEventArgs(string indexName, int topNResults, bool buildFieldFiltersFromAllData, string[] filterFieldNames)
        {
            if (string.IsNullOrEmpty(indexName))
                throw new ArgumentNullException("indexName", "indexName cannot be null or empty");
            if (topNResults < -1)
                throw new ArgumentOutOfRangeException("topNResults", "topNResults cannot be less than -1");
            this.indexName = indexName;
            this.topNResults = topNResults;
            this.buildFieldFiltersFromAllData = buildFieldFiltersFromAllData;
            this.fieldNames = filterFieldNames;
            this.cancel = false;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether to [build field filters from all data].
        /// </summary>
        /// <value>
        /// <c>true</c> if [build field filters from all data]; otherwise, <c>false</c>.
        /// </value>
        public bool BuildFieldFiltersFromAllData
        {
            get { return this.buildFieldFiltersFromAllData; }
            set { this.buildFieldFiltersFromAllData = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the read operation was canceled or not
        /// </summary>
        /// <value>
        ///   <c>true</c> if cancel; otherwise, <c>false</c>.
        /// </value>
        public bool Cancel
        {
            get { return this.cancel; }
            set { this.cancel = value; }
        }

        /// <summary>
        /// Gets the field names that were read out of the index (null means all)
        /// </summary>
        public string[] FieldNames
        {
            get { return this.fieldNames; }
        }

        /// <summary>
        /// Gets a value indicating whether the index has field names.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has field names; otherwise, <c>false</c>.
        /// </value>
        public bool HasFieldNames
        {
            get { return this.fieldNames != null && this.fieldNames.Length > 0; }
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
        /// Gets the total number of results that were read from the index
        /// </summary>
        public int TopNResults
        {
            get { return this.topNResults; }
        }

        #endregion Properties
    }
}