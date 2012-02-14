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
    /// Specifies whether and how a field should be indexed
    /// </summary>
    /// <remarks>
    /// Note that you can also separately enable/disable norms by calling AbstractField.setOmitNorms(boolean). 
    /// No norms means that index-time field and document boosting and field length normalization are disabled. 
    /// The benefit is less memory usage as norms take up one byte of RAM per indexed field for every document in the index, 
    /// during searching. Note that once you index a given field with norms enabled, disabling norms will have no effect. 
    /// In other words, for this to have the above described effect on a field, 
    /// all instances of that field must be indexed with NOT_ANALYZED_NO_NORMS from the beginning.
    /// </remarks>
    [System.CLSCompliant(true)]
    public enum FieldSearchableRule
    {
        /// <summary>
        /// Index the tokens produced by running the field's value through an analyzer (can be searched)
        /// </summary>
        Analyzed,

        /// <summary>
        /// Index the tokens produced by running the field's value through an analyzer, and disable the storing of norms (can be searched)
        /// </summary>
        AnalyzedNoNorms,

        /// <summary>
        /// Do not index the field (cannot be searched)
        /// </summary>
        No,

        /// <summary>
        /// Index the field's value without using the analyzer (can be searched)
        /// </summary>
        NotAnalyzed,

        /// <summary>
        /// Index the field's value without using the analyzer, and disable the storing of norms (can be searched)
        /// </summary>
        NotAnalyzedNoNorms
    }

    #endregion Enumerations
}