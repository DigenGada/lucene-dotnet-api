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
    using System.Collections.Generic;

    /// <summary>
    /// Represents event arguments that are passed to <see cref="IndexLibrary.IndexSearcher"/> events
    /// </summary>
    /// <remarks>
    /// Used specifically for BeginSearch, SearchResultFound, and EndSearch events
    /// </remarks>
    [System.CLSCompliant(true)]
    public sealed class SearcherEventArgs : SearcherAbstractEventArgs
    {
        #region Fields

        /// <summary>
        /// The name of the index that was being searched when this event fired.
        /// </summary>
        private string indexName;

        /// <summary>
        /// The structure of the index that was being searched when this event fired.
        /// </summary>
        private IndexType structure;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SearcherEventArgs"/> class.
        /// </summary>
        /// <param name="indexName">Name of the index that is being searched.</param>
        /// <param name="structure">The structure of the index that is being searched.</param>
        /// <param name="methodType">The search method type that is being used.</param>
        /// <param name="location">The location within the method body where this event was fired from.</param>
        public SearcherEventArgs(string indexName, IndexType structure, SearchMethodType methodType, SearchMethodLocation location)
            : this(indexName, structure, methodType, location, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearcherEventArgs"/> class.
        /// </summary>
        /// <param name="indexName">Name of the index that is being searched.</param>
        /// <param name="structure">The structure of the index that is being searched.</param>
        /// <param name="methodType">The search method type that is being used.</param>
        /// <param name="location">The location within the method body where this event was fired from.</param>
        /// <param name="result">The search result was found.</param>
        public SearcherEventArgs(string indexName, IndexType structure, SearchMethodType methodType, SearchMethodLocation location, SearchResult result)
        {
            this.indexName = indexName;
            this.structure = structure;
            this.SearchMethodType = methodType;
            this.SearchMethodLocation = location;
            this.SearchResult = result;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearcherEventArgs"/> class.
        /// </summary>
        /// <param name="indexName">Name of the index that is being searched.</param>
        /// <param name="structure">The structure of the index that is being searched.</param>
        /// <param name="methodType">The search method type that is being used.</param>
        /// <param name="location">The location within the method body where this event was fired from.</param>
        /// <param name="field">The field name to create a SearchResult instance from.</param>
        /// <param name="value">The value of the field to create a SearchResult instance from.</param>
        /// <param name="boost">The amount of relevance applied to the search result.</param>
        internal SearcherEventArgs(string indexName, IndexType structure, SearchMethodType methodType, SearchMethodLocation location, string field, string value, float boost)
        {
            this.indexName = indexName;
            this.structure = structure;
            this.SearchMethodType = methodType;
            this.SearchMethodLocation = location;
            Dictionary<string, string> values = new Dictionary<string, string>();
            values.Add(field, value);
            this.SearchResult = new SearchResult(values, this.indexName, boost);
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the name of the index that is being searched.
        /// </summary>
        /// <value>
        /// The name of the index.
        /// </value>
        public string IndexName
        {
            get { return this.indexName; }
        }

        /// <summary>
        /// Gets the structure of the index that is being searched.
        /// </summary>
        public IndexType IndexStructure
        {
            get { return this.structure; }
        }

        #endregion Properties
    }
}