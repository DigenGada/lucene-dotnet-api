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
namespace IndexLibrary.Compression
{
    using System;

    #region Enumerations

    /// <summary>
    /// A list of available/supported compression types
    /// </summary>
    [CLSCompliant(true)]
    public enum CompressionType
    {
        /// <summary>
        /// Zip compression
        /// </summary>
        Zip = 0,
        /// <summary>
        /// BZip compression
        /// </summary>
        BZip = 1,
        /// <summary>
        /// Tar compression
        /// </summary>
        /// <remarks>
        /// Not implemented
        /// </remarks>
        [Obsolete("Not yet implemented", true)]
        Tar = 2,
        /// <summary>
        /// GZip compression
        /// </summary>
        GZip = 3
    }

    #endregion Enumerations
}