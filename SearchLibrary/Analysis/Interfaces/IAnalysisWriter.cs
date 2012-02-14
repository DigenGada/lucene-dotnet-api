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
namespace IndexLibrary.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using IndexLibrary;

    /// <summary>
    /// Represents any class that can receive Analysis events from the core classes.
    /// </summary>
    /// <remarks>
    /// If you want to subscribe to analytics from this assembly create a class that
    /// implements this interface and have it subscribe to the LibraryAnalysis singleton
    /// object. Any events that are throw from this assembly will be relayed to that
    /// class.
    /// </remarks>
    public interface IAnalysisWriter
    {
        #region Properties

        /// <summary>
        /// Gets or sets the unique identifier for this class.
        /// </summary>
        /// <remarks>
        /// I suggest you use the fully qualified name for this value
        /// such as 'IndexLibrary.Analysis.ClassThatImplmentsInterface'.
        /// </remarks>
        string AnalyticsId
        {
            get;
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not to [track reads].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [track reads]; otherwise, <c>false</c>.
        /// </value>
        bool TrackReads
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not to [track searches].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [track searches]; otherwise, <c>false</c>.
        /// </value>
        bool TrackSearches
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not to [track writes].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [track writes]; otherwise, <c>false</c>.
        /// </value>
        bool TrackWrites
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Adds a <see cref="IndexLibrary.IndexInfo"/> to the analysis stack.
        /// </summary>
        /// <param name="info">The information being reported.</param>
        void AddIndexInfo(IndexInfo info);

        /// <summary>
        /// Adds a <see cref="IndexLibrary.ReadInfo"/> to the analysis stack.
        /// </summary>
        /// <param name="info">The information being reported.</param>
        void AddReadInfo(ReadInfo info);

        /// <summary>
        /// Adds a <see cref="IndexLibrary.SearchInfo"/> to the analysis stack.
        /// </summary>
        /// <param name="info">The information being reported.</param>
        void AddSearchInfo(SearchInfo info);

        #endregion Methods
    }
}