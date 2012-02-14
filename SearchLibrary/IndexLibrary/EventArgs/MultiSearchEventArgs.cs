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
    /// Represents event arguments that are passed to MultiIndexSearcher events
    /// </summary>
    /// <remarks>
    /// Used specifically for BeginSearch, SearchResultFound, and EndSearch events
    /// </remarks>
    [System.CLSCompliant(true)]
    public sealed class MultiSearcherEventArgs : SearcherAbstractEventArgs
    {
        #region Fields

        /// <summary>
        /// The names of the indexes that were being searched when this event fired
        /// </summary>
        private string[] indexNames;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiSearcherEventArgs"/> class.
        /// </summary>
        /// <param name="indexNames">Names of the indexes that are being searched.</param>
        /// <param name="methodType">The search method type that is being used.</param>
        /// <param name="location">The location within the method body where this event was fired from.</param>
        public MultiSearcherEventArgs(string[] indexNames, SearchMethodType methodType, SearchMethodLocation location)
            : this(indexNames, methodType, location, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiSearcherEventArgs"/> class.
        /// </summary>
        /// <param name="indexNames">Names of the indexes that are being searched.</param>
        /// <param name="methodType">The search method type that is being used.</param>
        /// <param name="location">The location within the method body where this event was fired from.</param>
        /// <param name="result">The search result was found, otherwise null.</param>
        public MultiSearcherEventArgs(string[] indexNames, SearchMethodType methodType, SearchMethodLocation location, SearchResult result)
        {
            this.indexNames = indexNames;
            this.SearchMethodType = methodType;
            this.SearchMethodLocation = location;
            this.SearchResult = result;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiSearcherEventArgs"/> class.
        /// </summary>
        /// <param name="indexNames">Names of the indexes that are being searched.</param>
        /// <param name="methodType">The search method type that is being used.</param>
        /// <param name="location">The location within the method body where this event was fired from.</param>
        /// <param name="field">The field name to create a SearchResult instance from.</param>
        /// <param name="value">The value of the field to create a SearchResult instance from.</param>
        /// <param name="boost">The amount of relevance applied to the search result.</param>
        internal MultiSearcherEventArgs(string[] indexNames, SearchMethodType methodType, SearchMethodLocation location, string field, string value, float boost)
        {
            this.indexNames = indexNames;
            this.SearchMethodType = methodType;
            this.SearchMethodLocation = location;
            Dictionary<string, string> values = new Dictionary<string, string>();
            values.Add(field, value);
            this.SearchResult = new SearchResult(values, string.Join(", ", indexNames), boost);
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the names of the indexes that are being searched concatenated into a single string sperated by semicolons
        /// </summary>
        public string IndexNames
        {
            get { return string.Join("; ", this.indexNames); }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Gets all index names.
        /// </summary>
        /// <returns>A string array of index names</returns>
        public string[] GetAllIndexNames()
        {
            return this.indexNames;
        }

        #endregion Methods
    }
}