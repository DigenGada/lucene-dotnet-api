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
    /// Represents the type of index structure to use
    /// </summary>
    /// <remarks>
    /// <para>
    /// Different types of IndexStructures have different kinds of advantages and disadvantages. This section will review all selections of this enumerator.
    /// </para>
    /// <para>
    /// None:
    ///     Specifying <c>None</c> means that no index structure was found or no index will be created. Obviously, not a good choice.
    /// </para>
    /// <para>
    /// SingleIndex:
    ///     The <c>SingleIndex</c> structure is the fastest way to move data from any source into an index; it is also the simplest of the index structures.
    ///     Advantages:
    ///         - Simplest/fastest structure when creating an index. Straightforward and lacking complexity.
    ///         - Great for testing or instance where the index does not need to be updated frequently AND the index is not searched frequently.
    ///         - Data is output to a single specified directory and no additional files and folders are needed to manager the structure.
    ///     Disadvatages:
    ///         - When you attempt to write the index other users could be attempting to read from it which can cause IO contention. While Lucene has
    ///           it's own built in methods to attempt to handle this, you can never completely handle all contention issues in this fashion, so it's
    ///           safer to avoid this structure when dealing with indexes that are searched or written to frequently.
    /// </para>
    /// <para>
    /// DoubleIndex:
    ///     The <c>DoubleIndex</c> structure is the midway point between a <c>SingleIndex</c> and a <c>SummaryAndDetailIndex</c> structure. 
    ///     Advantages:
    ///         - Bait-and-switch methodology is used to guarantee that there is no IO contention when reading and writing to an index
    ///     Disadvantes:
    ///         - The index is stored to the disc twice, thus, it requires twice the space on disc
    ///         - Slightly more complex file structure is used, two additional folders and one additional file is required to make this work.
    /// </para>
    /// </remarks>
    [System.CLSCompliant(true)]
    public enum IndexType
    {
        /// <summary>
        /// There is no identifible structure
        /// </summary>
        None,

        /// <summary>
        /// Creates a single index in a specified direction
        /// </summary>
        /// <remarks>
        /// Represents the following structure:
        /// ..\Indexes
        ///     ..\Users
        ///         ..\*.gen files
        ///         ..\*.seg files
        /// </remarks>
        SingleIndex,

        /// <summary>
        /// Creates an index in one directory, switches, and creates an index in the other directory. Classic bait-and-switch.
        /// </summary>
        /// <remarks>
        /// Represents the following structure:
        /// ..\Indexes
        ///     ..\Users
        ///         ..\A
        ///             ..\*.gen files
        ///             ..\*.seg files
        ///         ..\B
        ///         ..\ToggleFile.txt
        /// </remarks>
        DoubleIndex,
        CyclicalIndex
    }

    #endregion Enumerations
}