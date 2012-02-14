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
    using System;
    using System.IO;

    /// <summary>
    /// Represents the directory that actually contains the *.gen files for the index
    /// </summary>
    [System.CLSCompliant(true)]
    public sealed class IndexDirectory
    {
        #region Fields

        /// <summary>
        /// The full path to where the files exist
        /// </summary>
        private string directoryPath;

        /// <summary>
        /// The type of directory structure used to store an index
        /// </summary>
        private DirectoryType directoryType;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexDirectory"/> class.
        /// </summary>
        /// <remarks>
        /// Assigns an empty string to the DirectoryPath
        /// </remarks>
        /// <param name="type">The DirectoryType of this <c>IndexDirectory</c>.</param>
        public IndexDirectory(DirectoryType type)
            : this(type, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexDirectory"/> class.
        /// </summary>
        /// <param name="type">The DirectoryType of this <c>IndexDirectory</c>.</param>
        /// <param name="directoryPath">The directory path.</param>
        public IndexDirectory(DirectoryType type, string directoryPath)
        {
            if (type == DirectoryType.FileSystemDirectory && (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath)))
                throw new ArgumentException("DirectoryPath cannot be null and must exist for file system operations");
            this.directoryType = type;
            this.directoryPath = directoryPath;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexDirectory"/> class.
        /// </summary>
        /// <param name="type">The DirectoryType of this <c>IndexDirectory</c>.</param>
        /// <param name="directoryInfo">The directory info.</param>
        public IndexDirectory(DirectoryType type, DirectoryInfo directoryInfo)
        {
            if (directoryInfo == null)
                throw new ArgumentNullException("directoryInfo", "directoryInfo cannot be null");
            if (type == IndexLibrary.DirectoryType.FileSystemDirectory && !directoryInfo.Exists)
                throw new DirectoryNotFoundException("directoryInfo does not exist, and needs to for FileSystemDirectory types");
            this.directoryPath = directoryInfo.FullName;
            this.directoryType = type;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the directory info for the location of the *.gen files.
        /// </summary>
        public DirectoryInfo DirectoryInfo
        {
            get {
                if (string.IsNullOrEmpty(this.directoryPath))
                    return null;
                return new DirectoryInfo(this.directoryPath);
            }
        }

        /// <summary>
        /// Gets the directory path for the location of the *.gen files.
        /// </summary>
        public string DirectoryPath
        {
            get {
                if (string.IsNullOrEmpty(this.directoryPath))
                    return null;
                return this.directoryPath;
            }
        }

        /// <summary>
        /// Gets the type of the directory used for this <c>IndexDirectory</c>
        /// </summary>
        /// <value>
        /// The type of the directory.
        /// </value>
        public DirectoryType DirectoryType
        {
            get { return this.directoryType; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid for searching (files exist).
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is valid for search; otherwise, <c>false</c>.
        /// </value>
        public bool IsValidForSearch
        {
            get {
                DirectoryInfo di = this.DirectoryInfo;
                if (di == null || !di.Exists)
                    return false;
                return this.DirectoryInfo.GetFiles("*.gen", SearchOption.TopDirectoryOnly).Length > 0;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        /// </exception>
        public override sealed bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(IndexDirectory))
                return false;
            IndexDirectory temp = (IndexDirectory)obj;
            return temp.directoryType == this.directoryType && temp.directoryPath.Equals(this.directoryPath, StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override sealed int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (this.directoryPath == null)
                return this.directoryType.ToString();
            return string.Format("{0} - {1}", this.directoryType.ToString(), this.directoryPath);
        }

        /// <summary>
        /// Gets the Lucene equivalent of this object.
        /// </summary>
        /// <returns>Returns a Lucene object that represents this API object</returns>
        internal Lucene29.Net.Store.Directory GetLuceneDirectory()
        {
            switch (this.directoryType) {
                case DirectoryType.RamDirectory:
                    if (string.IsNullOrEmpty(this.directoryPath))
                        return new Lucene29.Net.Store.RAMDirectory();
                    else
                        return new Lucene29.Net.Store.RAMDirectory(Lucene29.Net.Store.FSDirectory.Open(new System.IO.DirectoryInfo(this.directoryPath)));
                case DirectoryType.FileSystemDirectory:
                default:
                    return Lucene29.Net.Store.FSDirectory.Open(new DirectoryInfo(this.directoryPath));
            }
        }

        #endregion Methods
    }
}