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
    /// Abstract class that represents event arguments that can be thrown by index searching classes
    /// </summary>
    [System.CLSCompliant(true)]
    public abstract class SearcherAbstractEventArgs : EventArgs
    {
        #region Fields

        /// <summary>
        /// Specifies whether or not the user wants to cancel the current searching operation.
        /// </summary>
        /// <remarks>
        /// Similar to OnClosing events, by marking an instance of <c>SearcherEventArgs</c> so
        /// that Cancel is <c>true</c> the searching method that this event was called from
        /// will stop searching, close all connections, and close; acting as though the method
        /// ended normally.
        /// </remarks>
        private bool cancel = false;

        /// <summary>
        /// Specifies the section of the method body this event was fired from.
        /// </summary>
        private SearchMethodLocation location;

        /// <summary>
        /// The type of search method that was being used when this event fired.
        /// </summary>
        private SearchMethodType methodType;

        /// <summary>
        /// If a search result was found, it is referenced here.
        /// </summary>
        private SearchResult searchResult;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="SearcherAbstractEventArgs"/> should send a cancellation message back to the caller.
        /// </summary>
        /// <value>
        ///   <c>true</c> if cancel the caller; otherwise, <c>false</c>.
        /// </value>
        public bool Cancel
        {
            get { return this.cancel; }
            set { this.cancel = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has <c>SearchResult</c>.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has <c>SearchResult</c>; otherwise, <c>false</c>.
        /// </value>
        public bool HasSearchResult
        {
            get { return this.searchResult != null; }
        }

        /// <summary>
        /// Gets the section of the method body this event was fired from.
        /// </summary>
        public SearchMethodLocation SearchMethodLocation
        {
            get { return this.location; }
            protected set { this.location = value; }
        }

        /// <summary>
        /// Gets the type of search method is being used.
        /// </summary>
        /// <value>
        /// The type of search method used.
        /// </value>
        public SearchMethodType SearchMethodType
        {
            get { return this.methodType; }
            protected set { this.methodType = value; }
        }

        /// <summary>
        /// Gets the search result that was found (if a result was found).
        /// </summary>
        public SearchResult SearchResult
        {
            get { return this.searchResult; }
            protected set { this.searchResult = value; }
        }

        #endregion Properties
    }
}