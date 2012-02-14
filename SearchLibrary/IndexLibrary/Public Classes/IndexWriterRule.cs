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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Represents a type rule where, by object type / field name you and
    /// specify a unique index rule
    /// </summary>
    public sealed class IndexWriterRule
    {
        #region Fields

        /// <summary>
        /// A list of field names and <see cref="IndexLibrary.FieldStorage"/>'s for each name
        /// </summary>
        private Dictionary<string, FieldStorage> appliedColumnNames;

        /// <summary>
        /// The System.Type this rule applies to
        /// </summary>
        private Type appliedType;

        /// <summary>
        /// The default <see cref="IndexLibrary.FieldStorage"/> for this instance
        /// </summary>
        private FieldStorage defaultStorageRule;

        /// <summary>
        /// Indicates the maximum allowed length for a field
        /// </summary>
        private int maxAllowedLength;

        /// <summary>
        /// Indicates whether or not fields that are not contained in the AppliedColumnName list are indexed or not
        /// </summary>
        private bool onlyIndexIfContainsFieldName;

        /// <summary>
        /// Indicates whether a null field should be skipped
        /// </summary>
        private bool skipIfNull;

        /// <summary>
        /// Indicates the default value if a field is null
        /// </summary>
        private string valueWhenNull;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexWriterRule"/> class.
        /// </summary>
        /// <param name="appliedType">The System.Type that this rule applies to.</param>
        public IndexWriterRule(Type appliedType)
        {
            if (appliedType == null)
                throw new ArgumentNullException("appliedType", "appliedType cannot be null");
            this.appliedType = appliedType;
            this.skipIfNull = true;
            this.maxAllowedLength = int.MaxValue;
            this.valueWhenNull = "empty";
            this.appliedColumnNames = new Dictionary<string, FieldStorage>();
            this.onlyIndexIfContainsFieldName = false;
            this.defaultStorageRule = new FieldStorage(true, FieldSearchableRule.Analyzed, FieldVectorRule.No);
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the list of applied columns and the <see cref="IndexLibrary.FieldStorage"/> that applies to it.
        /// </summary>
        public IDictionary<string, FieldStorage> AppliedColumns
        {
            get { return this.appliedColumnNames; }
        }

        /// <summary>
        /// Gets the type this rule is applied to
        /// </summary>
        /// <value>
        /// The System.Type this rule applies to
        /// </value>
        public Type AppliedType
        {
            get { return this.appliedType; }
        }

        /// <summary>
        /// Gets or sets the default storage rule to use if one is not supplied for the specified field name
        /// </summary>
        /// <value>
        /// The default storage rule.
        /// </value>
        public FieldStorage DefaultStorageRule
        {
            get { return this.defaultStorageRule; }
            set {
                if (value == null)
                    throw new ArgumentNullException("value", "value cannot be null");
                this.defaultStorageRule = value;
            }
        }

        /// <summary>
        /// Gets or sets the default value to insert into the index if the supplied one is null.
        /// </summary>
        /// <value>
        /// The default value if null.
        /// </value>
        public string DefaultValueIfNull
        {
            get { return this.valueWhenNull; }
            set { this.valueWhenNull = value; }
        }

        /// <summary>
        /// Gets or sets the max allowed field length.
        /// </summary>
        /// <value>
        /// The max allowed field length
        /// </value>
        public int MaxAllowedFieldLength
        {
            get { return this.maxAllowedLength; }
            set {
                if (this.maxAllowedLength < 1)
                    throw new ArgumentOutOfRangeException("value", "value cannot be less than one");
                this.maxAllowedLength = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to [only index a field if this rule contains it's field name].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [only index if rule contains field name]; otherwise, <c>false</c>.
        /// </value>
        public bool OnlyIndexIfRuleContainsFieldName
        {
            get { return this.onlyIndexIfContainsFieldName; }
            set { this.onlyIndexIfContainsFieldName = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to [skip column if null].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [skip column if null]; otherwise, <c>false</c>.
        /// </value>
        public bool SkipColumnIfNull
        {
            get { return this.skipIfNull; }
            set { this.skipIfNull = value; }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Adds a new field name that this rule applies to
        /// </summary>
        /// <param name="name">The field name.</param>
        /// <param name="storageRule">The storage rule that applies to the supplied field name.</param>
        /// <exception cref="System.ArgumentNullException">
        /// The <paramref name="name"/> parameter is null or empty
        /// </exception>
        public void AddAppliedColumnName(string name, FieldStorage storageRule)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name", "name cannot be null or empty");
            if (!this.appliedColumnNames.ContainsKey(name))
                this.appliedColumnNames.Add(name, storageRule);
        }

        /// <summary>
        /// Clears all applied column names.
        /// </summary>
        public void ClearAllAppliedColumnNames()
        {
            this.appliedColumnNames.Clear();
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        ///   </exception>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(IndexWriterRule))
                return false;
            IndexWriterRule tempRule = (IndexWriterRule)obj;
            return tempRule.appliedType == this.appliedType;
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
        /// Gets a <see cref="IndexLibrary.FieldStorage"/> rule for the specified field name
        /// </summary>
        /// <param name="name">The field name.</param>
        /// <returns>A <see cref="IndexLibrary.FieldStorage"/> that belongs to the specified field if it exists; otherwise, the default rule</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The <paramref name="name"/> parameter is null or empty
        ///   </exception>
        public FieldStorage GetStorageRule(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name", "name cannot be null or empty");
            if (this.appliedColumnNames.ContainsKey(name))
                return this.appliedColumnNames[name];
            return defaultStorageRule;
        }

        /// <summary>
        /// Removes the name of the applied column.
        /// </summary>
        /// <param name="name">The name to remove.</param>
        /// <exception cref="System.ArgumentNullException">
        /// The <paramref name="name"/> parameter is null or empty
        /// </exception>
        public void RemoveAppliedColumnName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name", "name cannot be null or empty");
            if (this.appliedColumnNames.ContainsKey(name))
                this.appliedColumnNames.Remove(name);
        }

        /// <summary>
        /// Sets a <see cref="IndexLibrary.FieldStorage"/> rule for the specified field name
        /// </summary>
        /// <param name="name">The field name.</param>
        /// <param name="storageRule">The storage rule that applies to the supplied field name.</param>
        /// <exception cref="System.ArgumentNullException">
        /// The <paramref name="name"/> parameter is null or empty
        ///   </exception>
        public void SetStorageRule(string name, FieldStorage storageRule)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name", "name cannot be null or empty");
            if (this.appliedColumnNames.ContainsKey(name))
                this.appliedColumnNames[name] = storageRule;
        }

        /// <summary>
        /// Determeines whether or not the specified field should be skipped or included in the index
        /// </summary>
        public bool SkipField(Type fieldType, string name)
        {
            if (fieldType == null)
                throw new ArgumentNullException("fieldType", "fieldType cannot be null");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name", "name cannot be null or empty");
            if (fieldType != this.appliedType)
                return true;

            bool containsColumn = this.appliedColumnNames.ContainsKey(name);

            // if we omit the specified terms and we contain that term, then omit!
            //if (this.ruleType == IndexWritingRuleType.Exclusive && containsColumn) return true;
            // if we only index the specified terms and we don't contain it, then omit!
            //if (this.ruleType == IndexWritingRuleType.Inclusive && !containsColumn) return true;

            if (onlyIndexIfContainsFieldName && !containsColumn)
                return true;

            return false;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0} -> {1}", this.appliedType.Name, string.Join(", ", appliedColumnNames.Keys.ToArray()));
        }

        #endregion Methods

        #region Other

        // allows you to say
        // for columns of this type, only index them if their name is ''
        // for columns of this type, only index them if their name is NOT ''

        #endregion Other
    }
}