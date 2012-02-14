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

    using Lucene29.Net.Analysis;
    using Lucene29.Net.Documents;
    using Lucene29.Net.Search;

    /// <summary>
    /// Internal class used to create friendly enums with lucene enums
    /// </summary>
    /// <remarks>
    /// CLS compliance marking is not required for classes that are not 
    /// visible from outside this assembly
    /// </remarks>
    internal static class TypeConverter
    {
        #region Methods

        /// <summary>
        /// Converts a <see cref="IndexLibrary.ClauseOccurrence"/> to the Lucene equivalent object, <c>BooleanClause.Occur</c>
        /// </summary>
        /// <param name="occurrence">The <c>ClauseOccurrence</c> to convert into a Lucene object</param>
        /// <returns>A Lucene <c>BooleanClause.Occur</c> that is equivalent to the specified <c>ClauseOccurrence</c></returns>
        public static BooleanClause.Occur ConvertToLuceneClauseOccurrence(ClauseOccurrence occurrence)
        {
            switch (occurrence) {
                case ClauseOccurrence.MustNotOccur:
                    return BooleanClause.Occur.MUST_NOT;
                case ClauseOccurrence.MustOccur:
                    return BooleanClause.Occur.MUST;
                case ClauseOccurrence.Default:
                case ClauseOccurrence.ShouldOccur:
                default:
                    return BooleanClause.Occur.SHOULD;
            }
        }

        /// <summary>
        /// Converts a <see cref="IndexLibrary.FieldSearchableRule"/> to the Lucene equivalent object, <c>Field.Index</c>
        /// </summary>
        /// <param name="index">The <c>FieldSearchableRule</c> to convert into a Lucene object</param>
        /// <returns>A Lucene <c>Field.Index</c> that is equivalent to the specified <c>FieldSearchableRule</c></returns>
        public static Field.Index ConvertToLuceneFieldIndex(FieldSearchableRule index)
        {
            switch (index) {
                case FieldSearchableRule.Analyzed:
                    return Field.Index.ANALYZED;
                case FieldSearchableRule.AnalyzedNoNorms:
                    return Field.Index.ANALYZED_NO_NORMS;
                case FieldSearchableRule.NotAnalyzed:
                    return Field.Index.NOT_ANALYZED;
                case FieldSearchableRule.NotAnalyzedNoNorms:
                    return Field.Index.NOT_ANALYZED_NO_NORMS;
                case FieldSearchableRule.No:
                default:
                    return Field.Index.NO;
            }
        }

        /// <summary>
        /// Converts a <see cref="IndexLibrary.FieldOption"/> to the Lucene equivalent object, <c>IndexReader.FieldOption</c>
        /// </summary>
        /// <param name="option">The <c>FieldOption</c> to convert into a Lucene object</param>
        /// <returns>A Lucene <c>IndexReader.FieldOption</c> that is equivalent to the specified <c>FieldOption</c></returns>
        public static Lucene29.Net.Index.IndexReader.FieldOption ConvertToLuceneFieldOption(FieldOption option)
        {
            switch (option) {
                case FieldOption.All:
                    return Lucene29.Net.Index.IndexReader.FieldOption.ALL;
                case FieldOption.Indexed:
                    return Lucene29.Net.Index.IndexReader.FieldOption.INDEXED;
                case FieldOption.IndexedNoTermvectors:
                    return Lucene29.Net.Index.IndexReader.FieldOption.INDEXED_NO_TERMVECTOR;
                case FieldOption.IndexedWithTermvectors:
                    return Lucene29.Net.Index.IndexReader.FieldOption.INDEXED_WITH_TERMVECTOR;
                case FieldOption.OmitTermFrequencyAndPositions:
                    return Lucene29.Net.Index.IndexReader.FieldOption.OMIT_TERM_FREQ_AND_POSITIONS;
                case FieldOption.StoresPayloads:
                    return Lucene29.Net.Index.IndexReader.FieldOption.STORES_PAYLOADS;
                case FieldOption.Termvector:
                    return Lucene29.Net.Index.IndexReader.FieldOption.TERMVECTOR;
                case FieldOption.TermvectorWithOffset:
                    return Lucene29.Net.Index.IndexReader.FieldOption.TERMVECTOR_WITH_OFFSET;
                case FieldOption.TermVectorWithPosition:
                    return Lucene29.Net.Index.IndexReader.FieldOption.TERMVECTOR_WITH_POSITION;
                case FieldOption.TermVectorWithPositionOffset:
                    return Lucene29.Net.Index.IndexReader.FieldOption.TERMVECTOR_WITH_POSITION_OFFSET;
                case FieldOption.Unindexed:
                    return Lucene29.Net.Index.IndexReader.FieldOption.UNINDEXED;
                default:
                    return Lucene29.Net.Index.IndexReader.FieldOption.ALL;
            }
        }

        /// <summary>
        /// Converts a <see cref="IndexLibrary.FieldVectorRule"/> to the Lucene equivalent object, <c>Field.TermVector</c>
        /// </summary>
        /// <param name="vector">The <c>FieldVectorRule</c> to convert into a Lucene object</param>
        /// <returns>A Lucene <c>Field.TermVector</c> that is equivalent to the specified <c>FieldVectorRule</c></returns>
        public static Field.TermVector ConvertToLuceneFieldTermVector(FieldVectorRule vector)
        {
            switch (vector) {
                case FieldVectorRule.Yes:
                    return Field.TermVector.YES;
                case FieldVectorRule.WithOffsets:
                    return Field.TermVector.WITH_OFFSETS;
                case FieldVectorRule.WithPositions:
                    return Field.TermVector.WITH_POSITIONS;
                case FieldVectorRule.WithPositionsOffsets:
                    return Field.TermVector.WITH_POSITIONS_OFFSETS;
                case FieldVectorRule.No:
                default:
                    return Field.TermVector.NO;
            }
        }

        /// <summary>
        /// Gets a Lucene <c>Analyzer</c> from the API enumerator <see cref="IndexLibrary.AnalyzerType"/>
        /// </summary>
        /// <param name="type">The type of <c>Analyzer</c> to create</param>
        /// <returns>A real Lucene <c>Analyzer</c> that is equivalent to the specified <c>AnalyzerType</c></returns>
        public static Analyzer GetAnalyzer(AnalyzerType type)
        {
            switch (type) {
                case AnalyzerType.Standard:
                case AnalyzerType.Default:
                    return new Lucene29.Net.Analysis.Standard.StandardAnalyzer(StaticValues.LibraryVersion);
                case AnalyzerType.Simple:
                    return new SimpleAnalyzer();
                case AnalyzerType.Stop:
                    return new StopAnalyzer(StaticValues.LibraryVersion);
                case AnalyzerType.Keyword:
                    return new KeywordAnalyzer();
                case AnalyzerType.Whitespace:
                    return new WhitespaceAnalyzer();
                case AnalyzerType.None:
                    return null;
                default:
                    return new Lucene29.Net.Analysis.Standard.StandardAnalyzer(StaticValues.LibraryVersion);
            }
        }

        /// <summary>
        /// Gets the <see cref="IndexLibrary.AnalyzerType"/> from a Lucene <c>Analyzer</c>
        /// </summary>
        /// <param name="analyzer">The <c>Analyzer</c> to get a type from</param>
        /// <returns>An API <c>AnalyzerType</c> that represents the type of the specified Lucene <c>Analyzer</c></returns>
        public static AnalyzerType GetAnalyzerType(Analyzer analyzer)
        {
            if (analyzer == null)
                return AnalyzerType.None;
            Type luceneType = analyzer.GetType();
            if (luceneType == typeof(Lucene29.Net.Analysis.Standard.StandardAnalyzer))
                return AnalyzerType.Standard;
            else if (luceneType == typeof(SimpleAnalyzer))
                return AnalyzerType.Simple;
            else if (luceneType == typeof(StopAnalyzer))
                return AnalyzerType.Stop;
            else if (luceneType == typeof(WhitespaceAnalyzer))
                return AnalyzerType.Whitespace;
            else if (luceneType == typeof(KeywordAnalyzer))
                return AnalyzerType.Keyword;
            else
                return AnalyzerType.Unknown;
        }

        #endregion Methods
    }
}