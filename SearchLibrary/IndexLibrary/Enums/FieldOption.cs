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
    /// When reading from an index specifies the types of fields to read or skip
    /// </summary>
    [Experimental("Untested")]
    [System.CLSCompliant(true)]
    public enum FieldOption
    {
        /// <summary>
        /// All fields
        /// </summary>
        All = 0,

        /// <summary>
        /// All indexed fields
        /// </summary>
        Indexed = 1,

        /// <summary>
        /// All fields which are indexed but don't have termvectors enabled
        /// </summary>
        IndexedNoTermvectors,

        /// <summary>
        /// All fields which are indexed with termvectors enabled
        /// </summary>
        IndexedWithTermvectors,

        /// <summary>
        /// All fields taht omit term frequency and positions
        /// </summary>
        OmitTermFrequencyAndPositions,

        /// <summary>
        /// All fields that store payloads
        /// </summary>
        StoresPayloads,

        /// <summary>
        /// All fields with termvectors enabled
        /// </summary>
        Termvector,

        /// <summary>
        /// All fields with termvectors with offset values enabled
        /// </summary>
        TermvectorWithOffset,

        /// <summary>
        /// All fields with termvectors with position values enabled
        /// </summary>
        TermVectorWithPosition,

        /// <summary>
        /// All fields with termvectors with offset values and position values enabled
        /// </summary>
        TermVectorWithPositionOffset,

        /// <summary>
        /// All fields which are not indexed
        /// </summary>
        Unindexed
    }

    #endregion Enumerations
}