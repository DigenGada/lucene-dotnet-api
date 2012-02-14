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
    /// Represents a collection of writing rules
    /// </summary>
    /// <remarks>
    /// Adds several benefits over just creating a list of <see cref="IndexLibrary.IndexWriterRule"/> 
    /// </remarks>
    public sealed class IndexWriterRuleCollection
    {
        #region Fields

        /// <summary>
        /// List of <see cref="IndexLibrary.IndexWriterRule"/> that this class encapsulates
        /// </summary>
        private List<IndexWriterRule> columns = new List<IndexWriterRule>();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexWriterRuleCollection"/> class.
        /// </summary>
        public IndexWriterRuleCollection()
        {
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Adds a rule to this collection
        /// </summary>
        /// <param name="rule">The rule to add.</param>
        public void AddRule(IndexWriterRule rule)
        {
            if (!columns.Contains(rule))
                columns.Add(rule);
        }

        /// <summary>
        /// Clears all rules from this instance
        /// </summary>
        public void ClearRules()
        {
            columns.Clear();
        }

        /// <summary>
        /// Determines whether the specified rule is contained in this instance
        /// </summary>
        /// <param name="rule">The rule to search for.</param>
        /// <returns>
        ///   <c>true</c> if the specified rule contains rule; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsRule(IndexWriterRule rule)
        {
            return columns.Contains(rule);
        }

        /// <summary>
        /// Determines whether the specified rule is contained in this instance.
        /// </summary>
        /// <param name="ruleType">The rule to search for.</param>
        /// <returns>
        ///   <c>true</c> if the specified rule type contains rule; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsRule(Type ruleType)
        {
            return columns.Contains(new IndexWriterRule(ruleType));
        }

        /// <summary>
        /// Gets a <see cref="IndexLibrary.IndexWriterRule"/> from the specified type
        /// </summary>
        /// <param name="ruleType">Type of the rule to grab.</param>
        /// <returns>If a rule with the specified type exists then that rule; otherwise a new rule for that type</returns>
        public IndexWriterRule GetRuleFromType(Type ruleType)
        {
            IndexWriterRule rule = columns.FirstOrDefault(x => x.AppliedType == ruleType);
            if (rule == null) {
                rule = new IndexWriterRule(ruleType);
                rule.DefaultValueIfNull = "false";
                rule.MaxAllowedFieldLength = 1000;
                rule.OnlyIndexIfRuleContainsFieldName = false;
                rule.SkipColumnIfNull = true;
                this.columns.Add(rule);
            }
            return rule;
        }

        /// <summary>
        /// Removes a rule from this collection
        /// </summary>
        /// <param name="rule">The rule to remove.</param>
        public void RemoveRule(IndexWriterRule rule)
        {
            if (columns.Contains(rule))
                columns.Remove(rule);
        }

        /// <summary>
        /// Determines if the specified fieldname should be skipped depending on the rule contained in this instance
        /// </summary>
        /// <param name="fieldType">Type of the field.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// The <paramref name="fieldType"/> parameter is null
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// The <paramref name="fieldName"/> parameter is null or empty
        /// </exception>
        public bool SkipField(Type fieldType, string fieldName)
        {
            if (fieldType == null)
                throw new ArgumentNullException("fieldType", "fieldType cannot be null");
            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentNullException("fieldName", "fieldName cannot be null or empty");
            IndexWriterRule rule = GetRuleFromType(fieldType);
            // no rule? include it!
            if (rule == null)
                return false;
            return rule.SkipField(fieldType, fieldName);
        }

        #endregion Methods
    }
}