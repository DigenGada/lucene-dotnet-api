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
    /// <summary>
    /// Represents a storage rule for a field
    /// </summary>
    /// <remarks>
    /// Whenever you add a field to a document in an index it must have a rule for how it is
    /// going to be added to the index. The storage rule specifies if the field is searchable,
    /// retrievable, and if so how.
    /// </remarks>
    [System.CLSCompliant(true)]
    [System.Serializable]
    public sealed class FieldStorage
    {
        #region Fields

        /// <summary>
        /// Specifies how the field should be searchable
        /// </summary>
        private FieldSearchableRule searchRule;

        /// <summary>
        /// Specifies if the field should be stored
        /// </summary>
        private bool store;

        /// <summary>
        /// Specifies how vectors are applied to the field
        /// </summary>
        private FieldVectorRule vectorRule;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldStorage"/> class.
        /// </summary>
        /// <param name="isStored">if set to <c>true</c> the field [is stored].</param>
        public FieldStorage(bool isStored)
            : this(isStored, FieldSearchableRule.AnalyzedNoNorms, FieldVectorRule.No)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldStorage"/> class.
        /// </summary>
        /// <param name="isStored">if set to <c>true</c> the field [is stored].</param>
        /// <param name="fieldSearchRule">The field search rule.</param>
        public FieldStorage(bool isStored, FieldSearchableRule fieldSearchRule)
            : this(isStored, fieldSearchRule, FieldVectorRule.No)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldStorage"/> class.
        /// </summary>
        /// <param name="isStored">if set to <c>true</c> the field [is stored].</param>
        /// <param name="fieldSearchRule">The field search rule.</param>
        /// <param name="fieldVectorRule">The field vector rule.</param>
        public FieldStorage(bool isStored, FieldSearchableRule fieldSearchRule, FieldVectorRule fieldVectorRule)
        {
            this.store = isStored;
            this.searchRule = fieldSearchRule;
            this.vectorRule = fieldVectorRule;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Creates an instance of this class which is useful for compressed fields
        /// </summary>
        public static FieldStorage Compressed
        {
            get { return new FieldStorage(true, FieldSearchableRule.No, FieldVectorRule.No); }
        }

        /// <summary>
        /// Creates a default instance of this class which is useful for most fields
        /// </summary>
        public static FieldStorage Default
        {
            get { return new FieldStorage(true, FieldSearchableRule.Analyzed, FieldVectorRule.No); }
        }

        /// <summary>
        /// Creates an instance of this class which is useful for phonetic fields
        /// </summary>
        public static FieldStorage Phonetic
        {
            get { return new FieldStorage(false, FieldSearchableRule.Analyzed, FieldVectorRule.No); }
        }

        /// <summary>
        /// Gets or sets a value indicating how a field should be searchable
        /// </summary>
        /// <value>
        /// The search rule.
        /// </value>
        public FieldSearchableRule SearchRule
        {
            get { return this.searchRule; }
            set { this.searchRule = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field is stored or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if store; otherwise, <c>false</c>.
        /// </value>
        public bool Store
        {
            get { return this.store; }
            set { this.store = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating how vectors apply to the field
        /// </summary>
        /// <value>
        /// The vector rule.
        /// </value>
        public FieldVectorRule VectorRule
        {
            get { return this.vectorRule; }
            set { this.vectorRule = value; }
        }

        #endregion Properties
    }
}