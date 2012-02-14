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
    /// Specifies whether and how a field should have term vectors
    /// </summary>
    /// <remarks>
    /// A term vector is a list of the document's terms and their number of occurrences in that document
    /// </remarks>
    [System.CLSCompliant(true)]
    public enum FieldVectorRule
    {
        /// <summary>
        /// Do not store term vectors
        /// </summary>
        No,

        /// <summary>
        /// Store the term vector and token offset information
        /// </summary>
        WithOffsets,

        /// <summary>
        /// Store the term vector and token position information
        /// </summary>
        WithPositions,

        /// <summary>
        /// Store the term vector, token position, and offset information
        /// </summary>
        WithPositionsOffsets,

        /// <summary>
        /// Store the term vectors of each document
        /// </summary>
        Yes
    }

    #endregion Enumerations
}