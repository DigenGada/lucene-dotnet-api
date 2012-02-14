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

    public class CyclicalIndex : DoubleIndex, ICyclicalIndex
    {
        #region Fields

        private DirectoryInfo mirrorDir;

        #endregion Fields

        #region Constructors

        public CyclicalIndex(DirectoryInfo indexDirectory)
            : base(indexDirectory)
        {
            if (indexDirectory == null)
                throw new ArgumentNullException("indexDirectory", "indexDirectory cannot be null");
            if (!indexDirectory.Exists)
                indexDirectory.Create();
            this.IndexStructure = IndexLibrary.IndexType.CyclicalIndex;
            this.mirrorDir = new DirectoryInfo(Path.Combine(indexDirectory.FullName, StaticValues.DirectoryMirror));
            if (!this.mirrorDir.Exists)
                this.mirrorDir.Create();
        }

        #endregion Constructors

        #region Properties

        public DirectoryInfo MirrorDirectory
        {
            get { return this.mirrorDir; }
            protected set { this.mirrorDir = value; }
        }

        #endregion Properties

        #region Methods

        public static new ICyclicalIndex Create(DirectoryInfo indexDirectory)
        {
            if (indexDirectory == null)
                throw new ArgumentNullException("indexDirectory", "indexDirectory cannot be null");
            if (!indexDirectory.Exists)
                indexDirectory.Create();
            string dirA = Path.Combine(indexDirectory.FullName, StaticValues.DirectoryA);
            string dirB = Path.Combine(indexDirectory.FullName, StaticValues.DirectoryB);
            string toggle = Path.Combine(indexDirectory.FullName, StaticValues.DirectoryToggleFile);
            string mirrorDir = Path.Combine(indexDirectory.FullName, StaticValues.DirectoryMirror);
            if (!Directory.Exists(dirA))
                Directory.CreateDirectory(dirA);
            if (!Directory.Exists(dirB))
                Directory.CreateDirectory(dirB);
            if (!File.Exists(toggle))
                File.WriteAllText(toggle, StaticValues.DirectoryA);
            if (!Directory.Exists(mirrorDir))
                Directory.CreateDirectory(mirrorDir);
            return new CyclicalIndex(indexDirectory);
        }

        public virtual void CopyMirror()
        {
            string toggle = base.GetToggleSwitch();
            FileInfo[] files = this.mirrorDir.GetFiles("*.*", SearchOption.TopDirectoryOnly);
            int totalFiles = files.Length;
            if (toggle.Equals(StaticValues.DirectoryB)) {
                // pointed at b (read from b, write to a)
                // clean house in other directory
                FileInfo[] existingFiles = this.DirectoryA.GetFiles("*.*", SearchOption.TopDirectoryOnly);
                foreach (var existingfile in existingFiles)
                    existingfile.Delete();
                for (int i = 0; i < totalFiles; i++)
                    files[i].CopyTo(Path.Combine(this.DirectoryA.FullName, files[i].Name), true);
            }
            else {
                // pointed at a (read from a, write to b)
                FileInfo[] existingFiles = this.DirectoryB.GetFiles("*.*", SearchOption.TopDirectoryOnly);
                foreach (var existingfile in existingFiles)
                    existingfile.Delete();
                for (int i = 0; i < totalFiles; i++)
                    files[i].CopyTo(Path.Combine(this.DirectoryB.FullName, files[i].Name), true);
            }
        }

        public virtual bool DeleteMirrorFiles()
        {
            FileInfo[] files = this.mirrorDir.GetFiles("*.*", SearchOption.TopDirectoryOnly);
            bool success = true;
            int totalFiles = files.Length;
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

        public override DirectoryInfo GetWriteDirectory()
        {
            return this.mirrorDir;
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
            // return IndexWriter.Open(analyzerType, this.mirrorDirectory, unlimitedFieldLength, create);
            return new IndexWriter(this, analyzerType, create, unlimitedFieldLength);
        }

        public override bool HasIndexFiles()
        {
            return base.HasIndexFiles(this.mirrorDir);
        }

        public virtual bool HasMirror()
        {
            return this.HasIndexFiles(this.mirrorDir);
        }

        public override void Refresh()
        {
            base.Refresh();
            this.mirrorDir.Refresh();
        }

        public bool SyncIndexesToMirror()
        {
            var writeDir = this.GetWriteDirectory();
            FileInfo fileLock = new FileInfo(Path.Combine(writeDir.FullName, "write.lock"));
            while (fileLock.Exists)
                System.Threading.Thread.Sleep(100);
            File.WriteAllText(fileLock.FullName, string.Empty);

            FileInfo[] files = writeDir.GetFiles("*.*", SearchOption.TopDirectoryOnly);
            int totalFiles = files.Length;
            int i = 0;
            bool success = true;
            for (i = 0; i < totalFiles; i++) {
                if (files[i].Name.Contains("write"))
                    continue;
                try {
                    files[i].Delete();
                }
                catch {
                    success = false;
                }
            }
            if (!success)
                return success;
            if (fileLock.Exists)
                fileLock.Delete();

            files = this.mirrorDir.GetFiles("*.*", SearchOption.TopDirectoryOnly);
            totalFiles = files.Length;
            for (i = 0; i < totalFiles; i++) {
                try {
                    files[i].CopyTo(Path.Combine(writeDir.FullName, files[i].Name));
                }
                catch {
                    // oh shit, we're fractured if we hit here
                    success = false;
                }
            }
            return success;
        }

        internal Lucene29.Net.Store.Directory GetLuceneMirrorDirectory()
        {
            return Lucene29.Net.Store.FSDirectory.Open(this.mirrorDir);
        }

        internal Lucene29.Net.Store.Directory GetLuceneReadDirectory()
        {
            return Lucene29.Net.Store.FSDirectory.Open(this.GetReadDirectory());
        }

        internal new Lucene29.Net.Store.Directory GetLuceneWriteDirectory()
        {
            return Lucene29.Net.Store.FSDirectory.Open(this.GetWriteDirectory());
        }

        #endregion Methods
    }
}