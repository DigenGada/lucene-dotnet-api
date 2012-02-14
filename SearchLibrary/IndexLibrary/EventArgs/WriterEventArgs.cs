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

    /// <summary>
    /// Represents event arguments that are passed into <see cref="IndexLibrary.IndexWriter"/> events
    /// </summary>
    [CLSCompliant(true)]
    public sealed class WriterEventArgs : EventArgs
    {
        #region Fields

        /// <summary>
        /// The type of analyzer being used with this writer
        /// </summary>
        private AnalyzerType analyzer;

        /// <summary>
        /// Indicates if the write operation was canceled or not
        /// </summary>
        private bool cancel;

        /// <summary>
        /// The <see cref="IndexLibrary.IndexDocument"/> that is being written
        /// </summary>
        private IndexDocument document;

        /// <summary>
        /// The index being written to
        /// </summary>
        private string index;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WriterEventArgs"/> class.
        /// </summary>
        /// <param name="document">The document to write.</param>
        /// <param name="analyzer">The analyzer being used to write.</param>
        /// <param name="indexName">Name of the index being written to.</param>
        public WriterEventArgs(IndexDocument document, AnalyzerType analyzer, string indexName)
        {
            if (document == null)
                throw new ArgumentNullException("document", "document cannot be null");
            if (string.IsNullOrEmpty(indexName))
                throw new ArgumentNullException("indexName", "indexName cannot be null or empty");

            this.document = document;
            this.analyzer = analyzer;
            this.index = indexName;
            this.cancel = false;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// The type of analyzer being used with this write operation
        /// </summary>
        public AnalyzerType Analyzer
        {
            get { return this.analyzer; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this write operation is being canceled or not
        /// </summary>
        /// <value>
        ///   <c>true</c> if cancel; otherwise, <c>false</c>.
        /// </value>
        public bool Cancel
        {
            get { return this.cancel; }
            set { this.cancel = value; }
        }

        /// <summary>
        /// Gets the document being written
        /// </summary>
        public IndexDocument Document
        {
            get { return this.document; }
        }

        /// <summary>
        /// Gets the index being written to
        /// </summary>
        public string Index
        {
            get { return this.index; }
        }

        #endregion Properties
    }
}