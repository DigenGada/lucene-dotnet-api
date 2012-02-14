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
    /// Specifies the part of the method body where a SearcherEventArgsAbstract event was called from
    /// </summary>
    [System.CLSCompliant(true)]
    public enum SearchMethodLocation
    {
        /// <summary>
        /// Specifies the event was fired at the beginning/initialization portion of the method body
        /// </summary>
        Beginning,

        /// <summary>
        /// Specifies the event was fired when a SearchResult was found
        /// </summary>
        ResultFound,

        /// <summary>
        /// Specifies the event was fired at the ending/disposing portion of the method body
        /// </summary>
        Ending
    }

    #endregion Enumerations
}