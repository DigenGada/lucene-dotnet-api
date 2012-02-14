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

    using IndexLibrary.Interfaces;

    public class Index : IIndex
    {
        #region Fields

        private DirectoryInfo directory;
        private IndexType structure;

        #endregion Fields

        #region Constructors

        public Index(DirectoryInfo indexDirectory)
        {
            if (indexDirectory == null)
                throw new ArgumentNullException("indexDirectory", "indexDirectory cannot be null");
            if (!indexDirectory.Exists)
                indexDirectory.Create();
            this.directory = indexDirectory;
            this.structure = IndexLibrary.IndexType.SingleIndex;
        }

        #endregion Constructors

        #region Properties

        public DirectoryInfo IndexDirectory
        {
            get { return this.directory; }
            protected set { this.directory = value; }
        }

        public IndexType IndexStructure
        {
            get { return this.structure; }
            internal set { this.structure = value; }
        }

        #endregion Properties

        #region Methods

        public static IIndex Create(DirectoryInfo indexDirectory)
        {
            if (indexDirectory == null)
                throw new ArgumentNullException("indexDirectory", "indexDirectory cannot be null");
            if (!indexDirectory.Exists)
                indexDirectory.Create();
            return new Index(indexDirectory);
        }

        public virtual bool DeleteIndexFiles()
        {
            return DeleteIndexFiles(SearchOption.TopDirectoryOnly);
        }

        public Lucene29.Net.Store.Directory GetLuceneDirectory()
        {
            return Lucene29.Net.Store.FSDirectory.Open(this.directory);
        }

        public virtual IndexReader GetReader()
        {
            return GetReader(true);
        }

        public virtual IndexReader GetReader(bool openReadOnly)
        {
            return new IndexReader(this, openReadOnly);
        }

        public virtual IndexSearcher GetSearcher()
        {
            return GetSearcher(true);
        }

        public virtual IndexSearcher GetSearcher(bool openReadOnly)
        {
            //return new IndexSearcher(this.directory, openReadOnly);
            return new IndexSearcher(this);
        }

        public virtual IndexWriter GetWriter()
        {
            return GetWriter(AnalyzerType.Default);
        }

        public virtual IndexWriter GetWriter(AnalyzerType analyzerType)
        {
            return GetWriter(analyzerType, true);
        }

        public virtual IndexWriter GetWriter(AnalyzerType analyzerType, bool create)
        {
            return GetWriter(analyzerType, create, false);
        }

        public virtual IndexWriter GetWriter(AnalyzerType analyzerType, bool create, bool unlimitedFieldLength)
        {
            //return IndexWriter.Open(analyzerType, this.directory, unlimitedFieldLength, create);
            return new IndexWriter(this, analyzerType, create, unlimitedFieldLength);
        }

        public virtual bool HasIndexFiles()
        {
            return HasIndexFiles(this.directory);
        }

        public virtual void Refresh()
        {
            this.directory.Refresh();
        }

        protected virtual bool DeleteIndexFiles(SearchOption searchOption)
        {
            FileInfo[] files = this.directory.GetFiles("*.*", searchOption);
            int totalFiles = files.Length;
            bool success = true;
            for (int i = 0; i < totalFiles; i++) {
                try {
                    files[i].Delete();
                }
                catch {
                    success = false;
                }
            }
            return success;
        }

        protected bool HasIndexFiles(DirectoryInfo info)
        {
            if (info == null)
                throw new ArgumentNullException("info", "info cannot be null");
            if (!info.Exists)
                return false;
            if (info.GetFiles("*.gen", SearchOption.TopDirectoryOnly).Length == 0)
                return false;

            if (info.GetFiles("*.cfs", SearchOption.TopDirectoryOnly).Length == 0 && info.GetFiles("*.cfx", SearchOption.TopDirectoryOnly).Length == 0)
                return false;
            return true;
        }

        #endregion Methods
    }
}