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
    #region Enumerations

    /// <summary>
    /// Specifies the type of method where a SearcherEventArgsAbstract event was called from
    /// </summary>
    [System.CLSCompliant(true)]
    public enum SearchMethodType
    {
        /// <summary>
        /// Specifies a QuickSearch method called this event
        /// </summary>
        /// <remarks>
        /// QuickSearch methods always return an IEnumerable list of string
        /// </remarks>
        Quick,

        /// <summary>
        /// Specifies a Search method called this event
        /// </summary>
        /// <remarks>
        /// Search methods always return an IEnumerable list of SearchResult
        /// </remarks>
        Normal,

        /// <summary>
        /// Specifies a FullSearch method called this event
        /// </summary>
        /// <remarks>
        /// FullSearch methods always return a <c>SearchResultDataSet</c>
        /// </remarks>
        Full
    }

    #endregion Enumerations
}