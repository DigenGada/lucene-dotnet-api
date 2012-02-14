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

    /// <summary>
    /// Represents a resultant filter created by an IndexSearcher FullSearch operation
    /// </summary>
    [System.CLSCompliant(true)]
    public class SearchResultFilter
    {
        #region Fields

        /// <summary>
        /// The filter name
        /// </summary>
        private string resultName;

        /// <summary>
        /// A list of distinct values contained in this filter and a switch indicating whether
        /// they're active or not
        /// </summary>
        private Dictionary<string, bool> values;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchResultFilter"/> class.
        /// </summary>
        /// <param name="name">The filter name.</param>
        public SearchResultFilter(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name", "Name cannot be null or empty");
            this.resultName = name;
            this.values = new Dictionary<string, bool>();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the name of this filter.
        /// </summary>
        public string Name
        {
            get { return this.resultName; }
        }

        /// <summary>
        /// Gets the distinct values of this filter.
        /// </summary>
        public IDictionary<string, bool> Values
        {
            get { return this.values; }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Determines whether this instance contains the specified key.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        ///   <c>true</c> if the specified name contains key; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsKey(string name)
        {
            return this.values.ContainsKey(name);
        }

        /// <summary>
        /// Determines whether this instance contains the specified value
        /// </summary>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <returns>
        ///   <c>true</c> if the specified value contains value; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsValue(bool value)
        {
            return this.values.ContainsValue(value);
        }

        /// <summary>
        /// Sets the value of the specified key.
        /// </summary>
        /// <param name="key">The key to update.</param>
        /// <param name="value">The new value to assign to this key.</param>
        public void SetValue(string key, bool value)
        {
            if (this.values.ContainsKey(key))
                this.values[key] = value;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("Filter {0} has {1} unique values", this.resultName, this.values.Count);
        }

        /// <summary>
        /// Adds a value to this filter.
        /// </summary>
        /// <param name="pair">The key/value pair to add.</param>
        internal void AddValue(KeyValuePair<string, bool> pair)
        {
            if (string.IsNullOrEmpty(pair.Key))
                return;
            // if it exists, update it
            if (this.values.ContainsKey(pair.Key))
                this.values[pair.Key] = pair.Value;
            else
                this.values.Add(pair.Key, pair.Value);
        }

        /// <summary>
        /// Removes a value from this filter.
        /// </summary>
        /// <param name="key">The key of a pair to remove.</param>
        /// <returns>A boolean indicating success</returns>
        internal bool RemoveValue(string key)
        {
            return this.values.Remove(key);
        }

        #endregion Methods
    }
}