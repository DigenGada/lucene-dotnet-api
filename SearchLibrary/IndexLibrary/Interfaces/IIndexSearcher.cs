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
namespace IndexLibrary.Interfaces
{
    /// <summary>
    /// Represents an object that can act as an index searching class
    /// </summary>
    /// <typeparam name="T">A type of <c>SearcherEventArgsAbstract</c></typeparam>
    /// <remarks>
    /// Deriving Classes:
    ///     There are two classes that currently confine to this interface, IndexSearcher and MultiIndexSearcher.
    /// 
    /// Deriving T:
    ///     There are two classes that derive from <c>SearcherEventArgsAbstract</c>, <see cref="IndexLibrary.IndexSearcher"/> and <see cref="IndexLibrary.MultiIndexSearcher"/>
    /// </remarks>
    [System.CLSCompliant(true)]
    public interface IIndexSearcher<T>
        where T : global::IndexLibrary.SearcherAbstractEventArgs
    {
        #region Events

        /// <summary>
        /// Occurs when the index searcher begins a search.
        /// </summary>
        event global::System.EventHandler<T> BeginSearch;

        /// <summary>
        /// Occurs when the index searcher ends a search.
        /// </summary>
        event global::System.EventHandler<T> EndSearch;

        /// <summary>
        /// Occurs when the index searcher finds a match in the index.
        /// </summary>
        event global::System.EventHandler<T> SearchResultFound;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this instance's index is open in read only mode or not.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is open in read only mode; otherwise, <c>false</c>.
        /// </value>
        bool IsReadOnly
        {
            get;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Performs a search on an index or set of indexes that creates a <see cref="IndexLibrary.SearchResultDataSet"/>
        /// </summary>
        /// <param name="builder">The <see cref="IndexLibrary.QueryBuilder"/> query to run against the index(es)</param>
        /// <param name="totalResultsRequested">The total number of results requested to be returned.</param>
        /// <returns>A <see cref="IndexLibrary.SearchResultDataSet"/> that contains the results of the search</returns>
        global::IndexLibrary.SearchResultDataSet FullSearch(global::IndexLibrary.QueryBuilder builder, int totalResultsRequested);

        /// <summary>
        /// Performs a search on an index or set of indexes that creates a <see cref="IndexLibrary.SearchResultDataSet"/>
        /// </summary>
        /// <param name="builder">The <see cref="IndexLibrary.QueryBuilder"/> query to run against the index(es)</param>
        /// <param name="fieldFilterNames">A string array of field names to use when building a list of SearchFilters for the returning dataset</param>
        /// <param name="totalResultsRequested">The total number of results requested to be returned.</param>
        /// <returns>A <see cref="IndexLibrary.SearchResultDataSet"/> that contains the results of the search</returns>
        global::IndexLibrary.SearchResultDataSet FullSearch(global::IndexLibrary.QueryBuilder builder, int totalResultsRequested, string[] fieldFilterNames);

        /// <summary>
        /// Performs a quick search on an index or set of indexes and creates a list of string results
        /// </summary>
        /// <param name="builder">The <see cref="IndexLibrary.QueryBuilder"/> query to run against the index(es)</param>
        /// <param name="fieldName">The field name to return values from</param>
        /// <param name="totalResultsRequested">The total number of results requested to be returned.</param>
        /// <returns>A list of strings from the specified column</returns>
        /// <remarks>
        /// When performing a QuickSearch you must specify a fieldName, this is because a QuickSearch operates similarly to the Search method, except
        /// that instead of returning all fields from all results, it only returns a single field from all results. Thus the returning value is a list
        /// of string rather than a list of <c>SearchResult</c>
        /// </remarks>
        global::System.Collections.Generic.IEnumerable<string> QuickSearch(global::IndexLibrary.QueryBuilder builder, string fieldName, int totalResultsRequested);

        /// <summary>
        /// Performs a search on an index or set of indexes and creates a list of results
        /// </summary>
        /// <param name="builder">The <see cref="IndexLibrary.QueryBuilder"/> query to run against the index(es)</param>
        /// <param name="totalResultsRequested">The total number of results requested to be returned.</param>
        /// <returns>A list of <see cref="IndexLibrary.SearchResult"/></returns>
        global::System.Collections.Generic.IEnumerable<global::IndexLibrary.SearchResult> Search(global::IndexLibrary.QueryBuilder builder, int totalResultsRequested);

        #endregion Methods
    }
}