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

    using Lucene29.Net.Documents;

    /// <summary>
    /// Represents a standard field that is used to search through an index
    /// </summary>
    [System.CLSCompliant(true)]
    public sealed class FieldNormal : FieldAbstract
    {
        #region Fields

        /// <summary>
        /// The rule used to apply analysis/tokenization to this field
        /// </summary>
        private FieldSearchableRule indexMethod;

        /// <summary>
        /// The rule used to apply a field vector to this field
        /// </summary>
        private FieldVectorRule termVector;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldNormal"/> class.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="fieldValue">The field value.</param>
        public FieldNormal(string fieldName, string fieldValue)
            : this(fieldName, fieldValue, true, FieldSearchableRule.Analyzed, FieldVectorRule.No)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldNormal"/> class.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <param name="storeValueInIndex">if set to <c>true</c> [store value in index].</param>
        public FieldNormal(string fieldName, string fieldValue, bool storeValueInIndex)
            : this(fieldName, fieldValue, storeValueInIndex, FieldSearchableRule.Analyzed, FieldVectorRule.No)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldNormal"/> class.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <param name="storeValueInIndex">if set to <c>true</c> [store value in index].</param>
        /// <param name="indexMethod">The index method.</param>
        public FieldNormal(string fieldName, string fieldValue, bool storeValueInIndex, FieldSearchableRule indexMethod)
            : this(fieldName, fieldValue, storeValueInIndex, indexMethod, FieldVectorRule.No)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldNormal"/> class.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <param name="storeValueInIndex">if set to <c>true</c> [store value in index].</param>
        /// <param name="indexMethod">The index method.</param>
        /// <param name="termVector">The term vector.</param>
        public FieldNormal(string fieldName, string fieldValue, bool storeValueInIndex, FieldSearchableRule indexMethod, FieldVectorRule termVector)
            : base(fieldName, fieldValue, StaticValues.DefaultBoost, storeValueInIndex)
        {
            if (this.IsFieldStored == false && indexMethod == FieldSearchableRule.No)
                throw new InvalidOperationException("You cannot set a field to be not stored and not indexed");
            this.indexMethod = indexMethod;
            this.termVector = termVector;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the index method applied to this field.
        /// </summary>
        /// <value>
        /// The index method.
        /// </value>
        public FieldSearchableRule IndexMethod
        {
            get {
                return this.indexMethod;
            }
            set {
                if (value == FieldSearchableRule.No && this.IsFieldStored == false)
                    throw new InvalidOperationException("You cannot set a field to be not stored and not indexed");
                this.indexMethod = value;
            }
        }

        /// <summary>
        /// Gets or sets the term vector applied to this field.
        /// </summary>
        /// <value>
        /// The term vector.
        /// </value>
        public FieldVectorRule TermVector
        {
            get { return this.termVector; }
            set { this.termVector = value; }
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
            if (obj == null || obj.GetType() != typeof(FieldNormal))
                return false;
            FieldNormal temp = (FieldNormal)obj;
            return temp.Boost == this.Boost && temp.indexMethod == this.indexMethod && temp.IsFieldStored == this.IsFieldStored && temp.termVector == this.termVector && temp.FieldName == this.FieldName && temp.FieldValue == this.FieldValue;
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
        /// Gets the Lucene equivalent of this instance
        /// </summary>
        /// <returns>
        /// Returns a Lucene representation of this object
        /// </returns>
        internal override sealed Field GetLuceneField()
        {
            Field field = new Field(this.FieldName, this.FieldValue, this.IsFieldStored ? Field.Store.YES : Field.Store.NO, TypeConverter.ConvertToLuceneFieldIndex(this.indexMethod), TypeConverter.ConvertToLuceneFieldTermVector(this.termVector));
            field.SetBoost(this.Boost);
            return field;
        }

        #endregion Methods
    }
}