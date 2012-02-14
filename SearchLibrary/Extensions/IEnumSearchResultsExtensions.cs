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
namespace IndexLibrary.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    /// <summary>
    /// Extensions for IEnumerable of <see cref="IndexLibrary.SearchResult"/> instances.
    /// </summary>
    [System.CLSCompliant(true)]
    public static class IEnumSearchResultsExtensions
    {
        #region Methods

        /// <summary>
        /// Converts this IEnumerable of SearchResult into a DataTable by adding all values in the collection to a new DataTable.
        /// </summary>
        /// <param name="results">The values to add to the table.</param>
        /// <param name="table">The table to add this instance to.</param>
        public static void AddToDataTable(this IEnumerable<SearchResult> results, DataTable table)
        {
            AddToDataTable(results, table, true);
        }

        /// <summary>
        /// Converts this IEnumerable of SearchResult into a DataTable by adding all values in the collection to a new DataTable.
        /// </summary>
        /// <param name="results">The values to add to the table.</param>
        /// <param name="table">The table to add this instance to.</param>
        /// <param name="addColumnsThatDoNotExist">Specifies to create columns that don't exist in the table.</param>
        public static void AddToDataTable(this IEnumerable<SearchResult> results, DataTable table, bool addColumnsThatDoNotExist)
        {
            if (table == null)
                throw new ArgumentNullException("table", "table cannot be null");
            if (results != null) {
                DataRow row = null;
                int addedFields = 0;
                foreach (var result in results) {
                    row = table.NewRow();
                    foreach (string field in result.GetDistinctFields()) {
                        if (!table.Columns.Contains(field)) {
                            if (!addColumnsThatDoNotExist)
                                continue;
                            table.Columns.Add(field, typeof(string));
                        }
                        addedFields++;
                        row[field] = result.GetValue(field);
                    }
                    if (addedFields > 0)
                        table.Rows.Add(row);
                    addedFields = 0;
                }
            }
        }

        /// <summary>
        /// Converts this IEnumerable of SearchResult into a DataTable by adding all values in the collection to a new DataTable.
        /// </summary>
        /// <param name="results">The values to add to the table.</param>
        /// <returns>A DataTable that represents this IEnumerable of SearchResult instance.</returns>
        public static DataTable ToDataTable(this IEnumerable<SearchResult> results)
        {
            DataTable table = new DataTable();
            if (results != null) {
                DataRow row = null;
                foreach (var result in results) {
                    row = table.NewRow();
                    foreach (string field in result.GetDistinctFields()) {
                        if (!table.Columns.Contains(field))
                            table.Columns.Add(field);
                        row[field] = result.GetValue(field);
                    }
                    table.Rows.Add(row);
                }
                row = null;
            }

            return table;
        }

        #endregion Methods
    }
}