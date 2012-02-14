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
    /// A Lucene analyzer type
    /// </summary>
    [System.CLSCompliant(true)]
    public enum AnalyzerType
    {
        /// <summary>
        /// Uses the Standard analyzer
        /// </summary>
        Default,

        /// <summary>
        /// Special keywords are removed from input
        /// </summary>
        Keyword,

        /// <summary>
        /// Do not use an analyzer
        /// </summary>
        None,

        /// <summary>
        /// Special characters are tokenized from input
        /// </summary>
        Simple,

        /// <summary>
        /// Combination of Keyword, Stop, and Whitespace analyzers
        /// </summary>
        Standard,

        /// <summary>
        /// Articles, some pronouns, and other 'useless' words are removed from input
        /// </summary>
        Stop,

        /// <summary>
        /// Whitespace is removed and tokenized from input
        /// </summary>
        Whitespace,

        /// <summary>
        /// Used for unrecognized analyzer types
        /// </summary>
        Unknown
    }

    #endregion Enumerations
}