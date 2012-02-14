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
    using System.Linq;

    /// <summary>
    /// Represents a set of data returned from an index search including the search results, distinct fields, and
    /// distinct values returned from those fields.
    /// </summary>
    [CLSCompliant(true)]
    public class SearchResultDataSet
    {
        #region Fields

        /// <summary>
        /// The search filters returned from the index search
        /// </summary>
        private List<SearchResultFilter> filters = null;

        /// <summary>
        /// The search results returned from the index search
        /// </summary>
        private List<SearchResult> searchResults = null;

        #endregion Fields

        #region Constructors

        public SearchResultDataSet(IEnumerable<SearchResult> results, IEnumerable<SearchResultFilter> filters)
        {
            if (results == null)
                throw new ArgumentNullException("results", "results cannot be null");
            if (filters == null)
                throw new ArgumentNullException("filters", "filters cannot be null");
            this.searchResults = new List<SearchResult>(results);
            this.filters = new List<SearchResultFilter>(filters);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchResultDataSet"/> class.
        /// </summary>
        internal SearchResultDataSet()
        {
            this.searchResults = new List<SearchResult>();
            this.filters = new List<SearchResultFilter>();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the search result filters in this instance.
        /// </summary>
        public IEnumerable<SearchResultFilter> ResultFilters
        {
            get { return this.filters; }
        }

        /// <summary>
        /// Gets the search results in this instance.
        /// </summary>
        public IEnumerable<SearchResult> SearchResults
        {
            get { return this.searchResults; }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Determines whether this instance's filter list contains the specified filter name
        /// </summary>
        /// <param name="filterName">Name of the filter.</param>
        /// <returns>
        ///   <c>true</c> if the specified filter name contains filter; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsFilter(string filterName)
        {
            return (this.filters.FirstOrDefault(x => x.Name.Equals(filterName, StringComparison.CurrentCultureIgnoreCase)) != null);
        }

        /// <summary>
        /// Gets a filter based on its filter name
        /// </summary>
        /// <param name="filterName">Name of the filter.</param>
        /// <returns></returns>
        public SearchResultFilter GetFilter(string filterName)
        {
            return this.filters.FirstOrDefault(x => x.Name.Equals(filterName));
        }

        /// <summary>
        /// Gets a subset of this instance based on the filter switches
        /// </summary>
        /// <returns></returns>
        public SearchResultDataSet GetSubsetBasedOnFilters()
        {
            return GetSubsetBasedOnFilters(false);
        }

        /// <summary>
        /// Gets a subset of this instance based on the filter switches
        /// </summary>
        /// <param name="resetFilters">if set to <c>true</c> [reset filters] switches after retrieving subset.</param>
        /// <returns></returns>
        public SearchResultDataSet GetSubsetBasedOnFilters(bool resetFilters)
        {
            int totalFilters = this.filters.Count;
            int totalResults = this.searchResults.Count;
            if (totalFilters == 0 || totalResults == 0)
                return null;
            SearchResultDataSet dataSet = new SearchResultDataSet();
            dataSet.AddSearchResultRange(this.searchResults);
            dataSet.AddFilterRange(this.filters);

            List<string> usedFilterValues = new List<string>();
            for (int i = 0; i < totalFilters; i++) {
                SearchResultFilter filter = dataSet.filters[i];
                foreach (KeyValuePair<string, bool> filterValue in filter.Values) {
                    if (!filterValue.Value) {
                        dataSet.searchResults.RemoveAll(x => !string.IsNullOrEmpty(x.GetValue(filter.Name)) && x.GetValue(filter.Name) == filterValue.Key);
                        usedFilterValues.Add(filterValue.Key);
                        if (resetFilters)
                            this.filters[i].SetValue(filterValue.Key, true);
                    }
                }
                foreach (string usedValues in usedFilterValues)
                    filter.RemoveValue(usedValues);
                usedFilterValues.Clear();
            }
            usedFilterValues = null;
            return dataSet;
        }

        /// <summary>
        /// Converts this instance of a <see cref="IndexLibrary.SearchResultDataSet"/> into a DataTable. 
        /// </summary>
        /// <returns>A DataTable that represents all results contained within this instance</returns>
        public DataTable ToDataTable()
        {
            DataTable table = new DataTable("SearchResultDataTable");
            int totalResults = this.searchResults.Count;
            for (int i = 0; i < totalResults; i++) {
                DataRow row = table.NewRow();
                foreach (string field in this.searchResults[i].GetFields()) {
                    if (!table.Columns.Contains(field))
                        table.Columns.Add(field);
                    row[field] = this.searchResults[i].GetValue(field);
                }
                table.Rows.Add(row);
            }
            return table;
        }

        /// <summary>
        /// [experimental] Updates the results based on filters.
        /// </summary>
        [Experimental("This will remove data from this dataset!")]
        public void UpdateResultsBasedOnFilters()
        {
            int totalFilters = this.filters.Count;
            int totalResults = this.searchResults.Count;
            if (totalFilters == 0 || totalResults == 0)
                return;

            List<string> usedFilterValues = new List<string>();
            for (int i = 0; i < totalFilters; i++) {
                SearchResultFilter filter = this.filters[i];
                foreach (KeyValuePair<string, bool> filterValue in filter.Values) {
                    if (!filterValue.Value) {
                        this.searchResults.RemoveAll(x => !string.IsNullOrEmpty(x.GetValue(filter.Name)) && x.GetValue(filter.Name) == filterValue.Key);
                        usedFilterValues.Add(filterValue.Key);
                    }
                }
                foreach (string usedValue in usedFilterValues)
                    filter.RemoveValue(usedValue);
                usedFilterValues.Clear();
            }
        }

        /// <summary>
        /// Adds a filter to this instance.
        /// </summary>
        /// <param name="filter">The filter to add.</param>
        internal void AddFilter(SearchResultFilter filter)
        {
            this.filters.Add(filter);
        }

        /// <summary>
        /// Adds a filter range to this instance.
        /// </summary>
        /// <param name="filterRange">The filters to add.</param>
        internal void AddFilterRange(IEnumerable<SearchResultFilter> filterRange)
        {
            this.filters.AddRange(filterRange);
        }

        /// <summary>
        /// Adds a search result to this instance
        /// </summary>
        /// <param name="result">The result.</param>
        internal void AddSearchResult(SearchResult result)
        {
            this.searchResults.Add(result);
        }

        /// <summary>
        /// Adds a search result range to this instance
        /// </summary>
        /// <param name="results">The results.</param>
        internal void AddSearchResultRange(IEnumerable<SearchResult> results)
        {
            this.searchResults.AddRange(results);
        }

        #endregion Methods
    }
}