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
    /// Represents the type of directory to use when creating an index
    /// </summary>
    [System.CLSCompliant(true)]
    public enum DirectoryType
    {
        /// <summary>
        /// Standard directory locating on the filesystem
        /// </summary>
        FileSystemDirectory,

        /// <summary>
        /// [experimental] A memory resident directory implementation.
        /// </summary>
        [Experimental("Partially implemented")]
        RamDirectory,

        /// <summary>
        /// [experimental] Based on java.nio FileChannel positional read, which allows
        /// multiple threads to read from the same file without synchronizing.
        /// </summary>
        [Experimental("Partially implemented")]
        NioFileSystemDirectory,

        /// <summary>
        /// [experimental] A directory instance that switches files between two other directory instances.
        /// </summary>
        /// <remarks>
        /// Files with the specified extensions are placed in the primary directory; others are placed
        /// in the secondary one. The provided Set must not change once passed to this class, and
        /// must allow multithreading.
        /// </remarks>
        [Experimental("Partially implemented")]
        FileSwitchDirectory
    }

    #endregion Enumerations
}