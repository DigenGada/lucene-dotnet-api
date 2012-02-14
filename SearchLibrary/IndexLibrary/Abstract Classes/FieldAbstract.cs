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
    using Lucene29.Net.Documents;

    /// <summary>
    /// Abstract class for fields that are used when creating an index
    /// </summary>
    [System.CLSCompliant(true)]
    public abstract class FieldAbstract : SearchTerm
    {
        #region Fields

        /// <summary>
        /// Flag that determines whether this instance is marked as Stored or NotStored when
        /// written to an index
        /// </summary>
        /// <remarks>
        /// Marking a field as Stored when writing it to the index indicates that the original value of the
        /// field will be stored, unaltered, into the index. Because it is stored unaltered the value can
        /// then be returned when a search result is found or a document is requested from the index.
        /// </remarks>
        private bool isStored;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldAbstract"/> class.
        /// </summary>
        /// <param name="fieldName">Name of the field to index.</param>
        /// <param name="fieldValue">The value of the field being indexed</param>
        protected FieldAbstract(string fieldName, string fieldValue)
            : this(fieldName, fieldValue, 1.0f, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldAbstract"/> class.
        /// </summary>
        /// <param name="fieldName">Name of the field to index.</param>
        /// <param name="fieldValue">The value of the field being indexed</param>
        /// <param name="boost">The amount of relevance to apply to this field</param>
        protected FieldAbstract(string fieldName, string fieldValue, float boost)
            : this(fieldName, fieldValue, boost, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldAbstract"/> class.
        /// </summary>
        /// <param name="fieldName">Name of the field to index.</param>
        /// <param name="fieldValue">The value of the field being indexed</param>
        /// <param name="boost">The amount of relevance to apply to this field</param>
        /// <param name="isStored">Indicates whether or not this original value will be stored in the index, untokenized, or not</param>
        protected FieldAbstract(string fieldName, string fieldValue, float boost, bool isStored)
            : base(fieldName, fieldValue, boost)
        {
            this.isStored = isStored;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this field's original value is stored in the index or nto.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this value is stored; otherwise, <c>false</c>.
        /// </value>
        protected bool IsFieldStored
        {
            get { return this.isStored; }
            set { this.isStored = value; }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Performs an explicit conversion from <see cref="IndexLibrary.FieldAbstract"/> to <see cref="Lucene29.Net.Documents.Field"/>.
        /// </summary>
        /// <param name="internalField">The FieldAbstract instance to convert into a Lucene Field object.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator Field(FieldAbstract internalField)
        {
            if (internalField == null)
                throw new System.ArgumentNullException("internalField", "internalField cannot be null");
            return internalField.GetLuceneField();
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Lucene29.Net.Documents.Field"/> to <see cref="IndexLibrary.FieldAbstract"/>.
        /// </summary>
        /// <param name="luceneField">The Lucene Field object to convert into a FieldAbstract instance.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator FieldAbstract(Field luceneField)
        {
            if (luceneField == null)
                throw new System.ArgumentNullException("luceneField", "luceneField cannot be null");
            FieldAbstract returnField = null;
            FieldNormal normal = new FieldNormal(luceneField.Name(), luceneField.StringValue(), luceneField.IsStored());
            normal.Boost = luceneField.GetBoost();
            if (luceneField.IsTermVectorStored()) {
                if (luceneField.IsStoreOffsetWithTermVector()) {
                    if (luceneField.IsStorePositionWithTermVector()) {
                        normal.TermVector = FieldVectorRule.WithPositionsOffsets;
                    }
                    else {
                        normal.TermVector = FieldVectorRule.WithOffsets;
                    }
                }
                else if (luceneField.IsStorePositionWithTermVector()) {
                    normal.TermVector = FieldVectorRule.WithPositions;
                }
                else {
                    normal.TermVector = FieldVectorRule.Yes;
                }
            }
            else {
                normal.TermVector = FieldVectorRule.No;
            }

            if (luceneField.IsIndexed()) {
                if (luceneField.IsTokenized()) {
                    normal.IndexMethod = FieldSearchableRule.Analyzed;
                }
                else {
                    normal.IndexMethod = FieldSearchableRule.NotAnalyzed;
                }
            }
            else if (luceneField.IsTokenized()) {
                normal.IndexMethod = FieldSearchableRule.AnalyzedNoNorms;
            }
            else {
                normal.IndexMethod = FieldSearchableRule.No;
            }
            returnField = normal;

            return returnField;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(FieldAbstract))
                return false;
            FieldAbstract temp = (FieldAbstract)obj;
            return temp.FieldName == this.FieldName && temp.FieldValue == this.FieldValue && temp.Boost == this.Boost && temp.isStored == this.isStored;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
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
            return string.Format("<{0} - {1}:{2}>^{3}", this.isStored ? "stored" : "notstored", this.FieldName, this.FieldValue, this.Boost);
        }

        /// <summary>
        /// Gets the Lucene Field equivalent of this instance.
        /// </summary>
        /// <returns>Returns a Lucene Field object that represents this instance.</returns>
        internal virtual Field GetLuceneField()
        {
            return new Field(this.FieldName, this.FieldValue, this.isStored ? Field.Store.YES : Field.Store.NO, Field.Index.NO);
        }

        /// <summary>
        /// Gets the Lucene Term equivalent of this intstance.
        /// </summary>
        /// <returns>Returns a Lucene Term object that represents this instance</returns>
        internal override Lucene29.Net.Index.Term GetLuceneTerm()
        {
            return new Lucene29.Net.Index.Term(this.FieldName, this.FieldValue);
        }

        #endregion Methods
    }
}