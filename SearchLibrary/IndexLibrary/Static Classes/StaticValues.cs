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
    /// <summary>
    /// Internal class used to store any static values that are required by the API
    /// </summary>
    /// <remarks>
    /// CLS compliance marking is not required for classes that are not 
    /// visible from outside this assembly
    /// </remarks>
    public static class StaticValues
    {
        #region Fields

        /// <summary>
        /// The default amount of boost to add to any field or document if one is not supplied
        /// </summary>
        public const float DefaultBoost = 1.0f;

        /// <summary>
        /// The default number of documents to return when searching an index if -1 is supplied
        /// </summary>
        public const int DefaultDocsToReturn = 5000;

        /// <summary>
        /// The default amount of slop to add to phrase queries if one is not supplied
        /// </summary>
        public const int DefaultSlop = 2;

        /// <summary>
        /// The static name of the first-half index in index structures
        /// </summary>
        public const string DirectoryA = "A";

        /// <summary>
        /// The static name of the second-half index in index structures
        /// </summary>
        public const string DirectoryB = "B";

        /// <summary>
        /// The static name 
        /// </summary>
        public const string DirectoryMirror = "Mirror";

        /// <summary>
        /// The static name of the toggle file for index structures
        /// </summary>
        public const string DirectoryToggleFile = "Toggle.txt";

        /// <summary>
        /// The maximum allowed boost value that can be assigned to any field or document
        /// </summary>
        public const float MaximumAllowedBoost = 100.0f;

        /// <summary>
        /// The maximum allowed slop that a user can attempt to assign to a phrase query
        /// </summary>
        public const int MaximumAllowedSlop = 10;

        /// <summary>
        /// The minimum allowed boost value that can be assigned to any field or document
        /// </summary>
        public const float MinimumAllowedBoost = 0.000000001f;

        /// <summary>
        /// The minimum allowed slop that a user can attempt to assign to a phrase query
        /// </summary>
        public const int MinimumAllowedSlop = 0;

        /// <summary>
        /// The maximum number of clauses that can be added to any <see cref="IndexLibrary.QueryBuilder"/> instance
        /// </summary>
        public const int TotalAllowedClauses = 1000;

        /// <summary>
        /// The version of Lucene in use by this API.
        /// </summary>
        /// <remarks>
        /// This exists to control the version of specific Lucene objects that are being used through
        /// both the API and the actually Lucene library. This makes for an easy upgrade when Lucene.NET
        /// 3.0 is released.
        /// </remarks>
        public static Lucene29.Net.Util.Version LibraryVersion = Lucene29.Net.Util.Version.LUCENE_29;

        #endregion Fields
    }
}