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

    /// <summary>
    /// Class that is used when returning results from searching a Lucene index
    /// </summary>
    /// <remarks>
    /// When performing a search on an index it is not known what specified fields will be returned
    /// in the result (each document in the index is not guaranteed to have the same fields in it).
    /// Thus either an anonymous type could be generated everytime a search is done or a class could
    /// be created to house the result and it's dynamic fields and values. The latter approach was
    /// taken and now all values are stored in a dictionary collection.
    /// </remarks>
    [System.CLSCompliant(true)]
    public class SearchResult
    {
        #region Fields

        /// <summary>
        /// The index being searched
        /// </summary>
        private string searchIndex;

        /// <summary>
        /// The boost value of this result document
        /// </summary>
        private float searchRelevance;

        /// <summary>
        /// The total number of values in the dictionary list
        /// </summary>
        private int totalValues = 0;

        /// <summary>
        /// The fieldname/fieldvalues list
        /// </summary>
        private Dictionary<string, string> values;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the LuceneSearchResult class
        /// </summary>
        /// <param name="values">Values to insert into this search result</param>
        /// <param name="indexName">Name of the index this result was generated from</param>
        /// <param name="relevance">The relevancy of the result</param>
        public SearchResult(Dictionary<string, string> values, string indexName, float relevance)
        {
            if (values == null)
                throw new ArgumentNullException("values", "Values cannot be null");
            this.totalValues = values.Count;
            if (this.totalValues == 0)
                throw new ArgumentException("Values must have at least one member", "values");
            this.values = values;
            this.searchIndex = indexName;
            this.searchRelevance = relevance;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets all key/value pairs contained in this search result
        /// </summary>
        public IDictionary<string, string> Entries
        {
            get { return this.values; }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Determines if this instance of the LuceneSearchResult class is equal to another
        /// </summary>
        /// <param name="obj">Instance of LuceneSearchResult to compare against this one</param>
        /// <returns>Returns true if the instances contain the same values</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(SearchResult))
                return false;
            SearchResult tempResult = obj as SearchResult;
            return (tempResult.searchIndex == this.searchIndex && tempResult.searchRelevance == this.searchRelevance && tempResult.totalValues == this.totalValues);
        }

        /// <summary>
        /// Gets a list of all distinct fields contained in this instance.
        /// </summary>
        /// <returns>Returns a string list of all distinct fields</returns>
        public IEnumerable<string> GetDistinctFields()
        {
            return this.values.Keys.Distinct();
        }

        /// <summary>
        /// Gets a list of all distinct values contained in this instance.
        /// </summary>
        /// <returns>Returns a string list of all distinct values</returns>
        public IEnumerable<string> GetDistinctValues()
        {
            return this.values.Values.Distinct();
        }

        /// <summary>
        /// Gets a list of all fields contained in this instance.
        /// </summary>
        /// <returns>Returns a string list of all fields</returns>
        public IEnumerable<string> GetFields()
        {
            return this.values.Keys;
        }

        /// <summary>
        /// Gets a non-unique integer hash code to identify this instance
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Attempts to get a value from this search result. If the key being searched is not
        /// contained in this result, null is returned.
        /// </summary>
        /// <param name="key">Field name to retrieve a value from</param>
        /// <returns>Returns the value corresponding to the specified fieldname (keyname). Otherwise, returns null.</returns>
        public string GetValue(string key)
        {
            if (this.values.ContainsKey(key))
                return this.values[key];
            return null;
        }

        /// <summary>
        /// Gets a list of all values contained in this instance.
        /// </summary>
        /// <returns>Returns a string list of all values</returns>
        public IEnumerable<string> GetValues()
        {
            return this.values.Values;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.Append("Index: " + this.searchIndex + Environment.NewLine);
            foreach (var pair in this.values)
                builder.Append(string.Format("<{0}: {1}>{2}", pair.Key, pair.Value, Environment.NewLine));
            builder.Append("Relevance: " + this.searchRelevance.ToString());
            return builder.ToString();
        }

        #endregion Methods
    }
}