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
    /// Wraps a search term in Lucene
    /// </summary>
    [System.CLSCompliant(true)]
    public class SearchTerm
    {
        #region Fields

        /// <summary>
        /// Specifies the boost applied to this term
        /// </summary>
        private float fieldBoost;

        /// <summary>
        /// The term's field name
        /// </summary>
        private string fieldName;

        /// <summary>
        /// The term's field value
        /// </summary>
        private string fieldValue;

        /// <summary>
        /// Specifies if this term's value is a phrase or a term
        /// </summary>
        private bool isPhrase;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchTerm"/> class.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <remarks>
        /// Default boost is applied.
        /// Default escapeSpecialCharacters value is false
        /// </remarks>
        public SearchTerm(string fieldName, string fieldValue)
            : this(fieldName, fieldValue, StaticValues.DefaultBoost)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchTerm"/> class.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <param name="boost">The boost.</param>
        /// <remarks>
        /// Default escapeSpecialCharacters value is false
        /// </remarks>
        public SearchTerm(string fieldName, string fieldValue, float boost)
            : this(fieldName, fieldValue, boost, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchTerm"/> class.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="fieldValue">The field value.</param>
        /// <param name="boost">The boost.</param>
        /// <param name="escapeSpecialCharacters"><c>true</c> to escape special characters so you can search them as literals; <c>false</c> to leave them.</param>
        public SearchTerm(string fieldName, string fieldValue, float boost, bool escapeSpecialCharacters)
        {
            this.Boost = boost;
            this.FieldName = fieldName;
            this.FieldValue = escapeSpecialCharacters ? StringFormatter.EscapeSpecialCharacters(fieldValue) : fieldValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchTerm"/> class.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="lowerValue">The lower value of the range.</param>
        /// <param name="upperValue">The upper value of the range.</param>
        public SearchTerm(string fieldName, int lowerValue, int upperValue)
            : this(fieldName, lowerValue, upperValue, true, true, StaticValues.DefaultBoost)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchTerm"/> class.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="lowerValue">The lower value of the range.</param>
        /// <param name="upperValue">The upper value of the range.</param>
        /// <param name="inclusiveLowerValue">if set to <c>true</c> the lower value is inclusive.</param>
        /// <param name="inclusiveUpperValue">if set to <c>true</c> the upper value is inclusive.</param>
        public SearchTerm(string fieldName, int lowerValue, int upperValue, bool inclusiveLowerValue, bool inclusiveUpperValue)
            : this(fieldName, lowerValue, upperValue, inclusiveLowerValue, inclusiveUpperValue, StaticValues.DefaultBoost)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchTerm"/> class.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="lowerValue">The lower value of the range.</param>
        /// <param name="upperValue">The upper value of the range.</param>
        /// <param name="inclusiveLowerValue">if set to <c>true</c> the lower value is inclusive.</param>
        /// <param name="inclusiveUpperValue">if set to <c>true</c> the upper value is inclusive.</param>
        /// <param name="boost">The boost applied to this term.</param>
        public SearchTerm(string fieldName, int lowerValue, int upperValue, bool inclusiveLowerValue, bool inclusiveUpperValue, float boost)
        {
            this.FieldName = fieldName;
            this.FieldValue = string.Format("{0}:{1}{2} TO {3}{4}", fieldName, inclusiveLowerValue ? "[" : "{", lowerValue, upperValue, inclusiveUpperValue ? "]" : "}");
            this.Boost = boost;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchTerm"/> class.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="lowerValue">The lower value of the range.</param>
        /// <param name="upperValue">The upper value of the range.</param>
        public SearchTerm(string fieldName, float lowerValue, float upperValue)
            : this(fieldName, lowerValue, upperValue, true, true, StaticValues.DefaultBoost)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchTerm"/> class.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="lowerValue">The lower value of the range.</param>
        /// <param name="upperValue">The upper value of the range.</param>
        /// <param name="inclusiveLowerValue">if set to <c>true</c> the lower value is inclusive.</param>
        /// <param name="inclusiveUpperValue">if set to <c>true</c> the upper value is inclusive.</param>
        public SearchTerm(string fieldName, float lowerValue, float upperValue, bool inclusiveLowerValue, bool inclusiveUpperValue)
            : this(fieldName, lowerValue, upperValue, inclusiveLowerValue, inclusiveUpperValue, StaticValues.DefaultBoost)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchTerm"/> class.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="lowerValue">The lower value of the range.</param>
        /// <param name="upperValue">The upper value of the range.</param>
        /// <param name="inclusiveLowerValue">if set to <c>true</c> the lower value is inclusive.</param>
        /// <param name="inclusiveUpperValue">if set to <c>true</c> the upper value is inclusive.</param>
        /// <param name="boost">The boost applied to this term.</param>
        public SearchTerm(string fieldName, float lowerValue, float upperValue, bool inclusiveLowerValue, bool inclusiveUpperValue, float boost)
        {
            this.FieldName = fieldName;
            this.FieldValue = string.Format("{0}:{1}{2} TO {3}{4}", fieldName, inclusiveLowerValue ? "[" : "{", lowerValue, upperValue, inclusiveUpperValue ? "]" : "}");
            this.Boost = boost;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchTerm"/> class.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="lowerValue">The lower value of the range.</param>
        /// <param name="upperValue">The upper value of the range.</param>
        public SearchTerm(string fieldName, double lowerValue, double upperValue)
            : this(fieldName, lowerValue, upperValue, true, true, StaticValues.DefaultBoost)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchTerm"/> class.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="lowerValue">The lower value of the range.</param>
        /// <param name="upperValue">The upper value of the range.</param>
        /// <param name="inclusiveLowerValue">if set to <c>true</c> the lower value is inclusive.</param>
        /// <param name="inclusiveUpperValue">if set to <c>true</c> the upper value is inclusive.</param>
        public SearchTerm(string fieldName, double lowerValue, double upperValue, bool inclusiveLowerValue, bool inclusiveUpperValue)
            : this(fieldName, lowerValue, upperValue, inclusiveLowerValue, inclusiveUpperValue, StaticValues.DefaultBoost)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchTerm"/> class.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="lowerValue">The lower value of the range.</param>
        /// <param name="upperValue">The upper value of the range.</param>
        /// <param name="inclusiveLowerValue">if set to <c>true</c> the lower value is inclusive.</param>
        /// <param name="inclusiveUpperValue">if set to <c>true</c> the upper value is inclusive.</param>
        /// <param name="boost">The boost applied to this term.</param>
        public SearchTerm(string fieldName, double lowerValue, double upperValue, bool inclusiveLowerValue, bool inclusiveUpperValue, float boost)
        {
            this.FieldName = fieldName;
            this.FieldValue = string.Format("{0}:{1}{2} TO {3}{4}", fieldName, inclusiveLowerValue ? "[" : "{", lowerValue, upperValue, inclusiveUpperValue ? "]" : "}");
            this.Boost = boost;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchTerm"/> class.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="lowerValue">The lower value of the range.</param>
        /// <param name="upperValue">The upper value of the range.</param>
        public SearchTerm(string fieldName, long lowerValue, long upperValue)
            : this(fieldName, lowerValue, upperValue, true, true, StaticValues.DefaultBoost)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchTerm"/> class.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="lowerValue">The lower value of the range.</param>
        /// <param name="upperValue">The upper value of the range.</param>
        /// <param name="inclusiveLowerValue">if set to <c>true</c> the lower value is inclusive.</param>
        /// <param name="inclusiveUpperValue">if set to <c>true</c> the upper value is inclusive.</param>
        public SearchTerm(string fieldName, long lowerValue, long upperValue, bool inclusiveLowerValue, bool inclusiveUpperValue)
            : this(fieldName, lowerValue, upperValue, inclusiveLowerValue, inclusiveUpperValue, StaticValues.DefaultBoost)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchTerm"/> class.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="lowerValue">The lower value of the range.</param>
        /// <param name="upperValue">The upper value of the range.</param>
        /// <param name="inclusiveLowerValue">if set to <c>true</c> the lower value is inclusive.</param>
        /// <param name="inclusiveUpperValue">if set to <c>true</c> the upper value is inclusive.</param>
        /// <param name="boost">The boost applied to this term.</param>
        public SearchTerm(string fieldName, long lowerValue, long upperValue, bool inclusiveLowerValue, bool inclusiveUpperValue, float boost)
        {
            this.FieldName = fieldName;
            this.FieldValue = string.Format("{0}:{1}{2} TO {3}{4}", fieldName, inclusiveLowerValue ? "[" : "{", lowerValue, upperValue, inclusiveUpperValue ? "]" : "}");
            this.Boost = boost;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchTerm"/> class.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="lowerValue">The lower value of the range.</param>
        /// <param name="upperValue">The upper value of the range.</param>
        public SearchTerm(string fieldName, string lowerValue, string upperValue)
            : this(fieldName, lowerValue, upperValue, true, true, StaticValues.DefaultBoost)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchTerm"/> class.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="lowerValue">The lower value of the range.</param>
        /// <param name="upperValue">The upper value of the range.</param>
        /// <param name="inclusiveLowerValue">if set to <c>true</c> the lower value is inclusive.</param>
        /// <param name="inclusiveUpperValue">if set to <c>true</c> the upper value is inclusive.</param>
        public SearchTerm(string fieldName, string lowerValue, string upperValue, bool inclusiveLowerValue, bool inclusiveUpperValue)
            : this(fieldName, lowerValue, upperValue, inclusiveLowerValue, inclusiveUpperValue, StaticValues.DefaultBoost)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchTerm"/> class.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="lowerValue">The lower value of the range.</param>
        /// <param name="upperValue">The upper value of the range.</param>
        /// <param name="inclusiveLowerValue">if set to <c>true</c> the lower value is inclusive.</param>
        /// <param name="inclusiveUpperValue">if set to <c>true</c> the upper value is inclusive.</param>
        /// <param name="boost">The boost applied to this term.</param>
        public SearchTerm(string fieldName, string lowerValue, string upperValue, bool inclusiveLowerValue, bool inclusiveUpperValue, float boost)
        {
            if (string.IsNullOrEmpty(lowerValue))
                throw new ArgumentNullException("lowerValue", "lowerValue cannot be null or empty");
            if (string.IsNullOrEmpty(upperValue))
                throw new ArgumentNullException("upperValue", "upperValue cannot be null or empty");
            this.FieldName = fieldName;
            this.FieldValue = string.Format("{0}:{1}{2} TO {3}{4}", DeterminePhrase(lowerValue) ? "\"" + fieldName + "\"" : fieldName, inclusiveLowerValue ? "[" : "{", lowerValue, DeterminePhrase(upperValue) ? "\"" + upperValue + "\"" : upperValue, inclusiveUpperValue ? "]" : "}");
            this.Boost = boost;
            this.isPhrase = true;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the boost value for the field
        /// </summary>
        /// <value>
        /// The boost value (must be greater than zero)
        /// </value>
        public float Boost
        {
            get { return this.fieldBoost; }
            set {
                if (value <= 0f)
                    throw new ArgumentOutOfRangeException("value", "Boost must be greater than zero");
                this.fieldBoost = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the field.
        /// </summary>
        /// <value>
        /// The name of the field.
        /// </value>
        public string FieldName
        {
            get { return this.fieldName; }
            set {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value", "FieldName cannot be null or empty");
                this.fieldName = value;
            }
        }

        /// <summary>
        /// Gets or sets the field value.
        /// </summary>
        /// <value>
        /// The field value.
        /// </value>
        public string FieldValue
        {
            get { return this.fieldValue; }
            set {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value", "FieldValue cannot be null or empty");
                this.fieldValue = value;
                DeterminePhrase();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the field value is phrase.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is phrase; otherwise, <c>false</c>.
        /// </value>
        public bool IsPhrase
        {
            get { return this.isPhrase; }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Determines whether the specified string value is a phrase or not.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static bool DeterminePhrase(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;
            if (value.Length > 100)
                return true;
            string[] parts = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 1;
        }

        /// <summary>
        /// Overrides the IsPhrase property which determines if this field should be treated as a termquery
        /// or a phrasequery when searching.
        /// </summary>
        /// <param name="isTermValueAPhrase">if set to <c>true</c> this term value will be treated as a phrase.</param>
        public void OverridePhrase(bool isTermValueAPhrase)
        {
            this.isPhrase = isTermValueAPhrase;
        }

        /// <summary>
        /// Gets a Lucene object that represents this API object.
        /// </summary>
        /// <returns>A Lucene Term object that is equivalent to this instance</returns>
        internal virtual Lucene29.Net.Index.Term GetLuceneTerm()
        {
            //return new Lucene29.Net.Index.Term(this.fieldName, this.isPhrase ? "\"" + this.fieldValue + "\"" : this.fieldValue);
            return new Lucene29.Net.Index.Term(this.fieldName, this.fieldValue);
        }

        /// <summary>
        /// Determines whether or not the field value is a phrase
        /// </summary>
        protected void DeterminePhrase()
        {
            string[] parts = this.fieldValue.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            this.isPhrase = parts.Length > 1;
            parts = null;
        }

        #endregion Methods
    }
}