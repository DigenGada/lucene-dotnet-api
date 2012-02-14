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

    public class DoubleIndex : Index, IDoubleIndex
    {
        #region Fields

        private DirectoryInfo dirA;
        private DirectoryInfo dirB;
        private FileInfo toggleInfo;

        #endregion Fields

        #region Constructors

        public DoubleIndex(DirectoryInfo indexDirectory)
            : base(indexDirectory)
        {
            if (indexDirectory == null)
                throw new ArgumentNullException("indexDirectory", "indexDirectory cannot be null");
            if (!indexDirectory.Exists)
                indexDirectory.Create();
            this.IndexStructure = IndexLibrary.IndexType.DoubleIndex;
            this.dirA = new DirectoryInfo(Path.Combine(indexDirectory.FullName, StaticValues.DirectoryA));
            this.dirB = new DirectoryInfo(Path.Combine(indexDirectory.FullName, StaticValues.DirectoryB));
            this.toggleInfo = new FileInfo(Path.Combine(indexDirectory.FullName, StaticValues.DirectoryToggleFile));
            if (!this.dirA.Exists)
                this.dirA.Create();
            if (!this.dirB.Exists)
                this.dirB.Create();
            if (!this.toggleInfo.Exists)
                File.WriteAllText(this.toggleInfo.FullName, StaticValues.DirectoryA);
        }

        #endregion Constructors

        #region Properties

        public DirectoryInfo DirectoryA
        {
            get { return this.dirA; }
            protected set { this.dirA = value; }
        }

        public DirectoryInfo DirectoryB
        {
            get { return this.dirB; }
            protected set { this.dirB = value; }
        }

        public FileInfo ToggleFile
        {
            get { return this.toggleInfo; }
            protected set { this.toggleInfo = value; }
        }

        #endregion Properties

        #region Methods

        public static new IDoubleIndex Create(DirectoryInfo indexDirectory)
        {
            if (indexDirectory == null)
                throw new ArgumentNullException("indexDirectory", "indexDirectory cannot be null");
            if (!indexDirectory.Exists)
                indexDirectory.Create();
            string dirA = Path.Combine(indexDirectory.FullName, StaticValues.DirectoryA);
            string dirB = Path.Combine(indexDirectory.FullName, StaticValues.DirectoryB);
            string toggle = Path.Combine(indexDirectory.FullName, StaticValues.DirectoryToggleFile);
            if (!Directory.Exists(dirA))
                Directory.CreateDirectory(dirA);
            if (!Directory.Exists(dirB))
                Directory.CreateDirectory(dirB);
            if (!File.Exists(toggle))
                File.WriteAllText(toggle, StaticValues.DirectoryA);
            return new DoubleIndex(indexDirectory);
        }

        public override bool DeleteIndexFiles()
        {
            return this.DeleteIndexFiles(SearchOption.AllDirectories);
        }

        public virtual void FlipToggleSwitch()
        {
            string toggleValue = this.GetToggleSwitch();
            if (toggleValue.Equals(StaticValues.DirectoryA)) {
                File.WriteAllText(this.toggleInfo.FullName, StaticValues.DirectoryB);
            }
            else {
                File.WriteAllText(this.toggleInfo.FullName, StaticValues.DirectoryA);
            }
        }

        public new Lucene29.Net.Store.Directory GetLuceneDirectory()
        {
            return Lucene29.Net.Store.FSDirectory.Open(this.GetReadDirectory());
        }

        public Lucene29.Net.Store.Directory GetLuceneWriteDirectory()
        {
            return Lucene29.Net.Store.FSDirectory.Open(this.GetWriteDirectory());
        }

        public DirectoryInfo GetReadDirectory()
        {
            string toggleValue = this.GetToggleSwitch();
            if (toggleValue.Equals(StaticValues.DirectoryA))
                return this.dirA;
            return this.dirB;
        }

        public override IndexReader GetReader()
        {
            return this.GetReader(true);
        }

        public override IndexReader GetReader(bool openReadOnly)
        {
            return new IndexReader(this, openReadOnly);
        }

        public override IndexSearcher GetSearcher()
        {
            return this.GetSearcher(true);
        }

        public override IndexSearcher GetSearcher(bool openReadOnly)
        {
            // var readDir = GetReadDirectory();
            // return new IndexSearcher(readDir, openReadOnly);
            return new IndexSearcher(this);
        }

        public virtual string GetToggleSwitch()
        {
            this.toggleInfo.Refresh();
            if (!this.toggleInfo.Exists)
                return StaticValues.DirectoryB;
            string results = File.ReadAllText(this.toggleInfo.FullName);
            if (string.IsNullOrEmpty(results)) {
                File.WriteAllText(this.toggleInfo.FullName, StaticValues.DirectoryA);
                return StaticValues.DirectoryA;
            }
            if (results.Equals(StaticValues.DirectoryB, StringComparison.OrdinalIgnoreCase))
                return StaticValues.DirectoryB;
            return StaticValues.DirectoryA;
        }

        public virtual DirectoryInfo GetWriteDirectory()
        {
            string toggleValue = this.GetToggleSwitch();
            if (toggleValue.Equals(StaticValues.DirectoryA))
                return this.dirB;
            return this.dirA;
        }

        public override IndexWriter GetWriter()
        {
            return this.GetWriter(AnalyzerType.Default);
        }

        public override IndexWriter GetWriter(AnalyzerType analyzerType)
        {
            return this.GetWriter(analyzerType, true);
        }

        public override IndexWriter GetWriter(AnalyzerType analyzerType, bool create)
        {
            return this.GetWriter(analyzerType, create, true);
        }

        public override IndexWriter GetWriter(AnalyzerType analyzerType, bool create, bool unlimitedFieldLength)
        {
            return new IndexWriter(this, analyzerType, create, unlimitedFieldLength);
        }

        public override bool HasIndexFiles()
        {
            return this.HasIndexFiles(this.GetReadDirectory());
        }

        public bool HasWriteIndexFiles()
        {
            return this.HasIndexFiles(this.GetWriteDirectory());
        }

        public override void Refresh()
        {
            base.Refresh();
            this.dirA.Refresh();
            this.dirB.Refresh();
        }

        protected override bool DeleteIndexFiles(SearchOption searchOption)
        {
            // the only file we need to keep is the toggle file, so recreate it when we're done
            this.FlipToggleSwitch();
            return base.DeleteIndexFiles(searchOption);
        }

        #endregion Methods
    }
}