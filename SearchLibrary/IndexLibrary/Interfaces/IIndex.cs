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
namespace IndexLibrary.Interfaces
{
    using System.IO;

    [System.CLSCompliant(true)]
    public interface IIndex
    {
        #region Properties

        DirectoryInfo IndexDirectory
        {
            get;
        }

        IndexType IndexStructure
        {
            get;
        }

        #endregion Properties

        #region Methods

        bool DeleteIndexFiles();

        // Can't be apart of the interface because then the interface method would be
        // more visible than the actual method
        Lucene29.Net.Store.Directory GetLuceneDirectory();

        IndexReader GetReader();

        IndexReader GetReader(bool openReadOnly);

        IndexSearcher GetSearcher();

        IndexSearcher GetSearcher(bool openReadOnly);

        IndexWriter GetWriter();

        IndexWriter GetWriter(AnalyzerType analyzerType);

        IndexWriter GetWriter(AnalyzerType analyzerType, bool create);

        IndexWriter GetWriter(AnalyzerType analyzerType, bool create, bool unlimitedFieldLength);

        bool HasIndexFiles();

        void Refresh();

        #endregion Methods
    }
}