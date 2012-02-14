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
    using System.Data;
    using System.Linq;

    /// <summary>
    /// Represents a document within an index.
    /// </summary>
    /// <remarks>
    /// Documents could be considered synonymous with DataRows in a SQL table.
    /// </remarks>
    [System.CLSCompliant(true)]
    public sealed class IndexDocument
    {
        #region Fields

        /// <summary>
        /// The Lucene object that this class wraps
        /// </summary>
        private Lucene29.Net.Documents.Document luceneDoc;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexDocument"/> class.
        /// </summary>
        public IndexDocument()
        {
            this.luceneDoc = new Lucene29.Net.Documents.Document();
            this.luceneDoc.SetBoost(StaticValues.DefaultBoost);
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the boost for this document.
        /// </summary>
        /// <value>
        /// The boost value.
        /// </value>
        public float Boost
        {
            get {
                return this.luceneDoc.GetBoost();
            }
            set {
                if (value < StaticValues.MinimumAllowedBoost) {
                    throw new ArgumentOutOfRangeException("Boost cannot be less than " + StaticValues.MinimumAllowedBoost.ToString());
                }
                else if (value > StaticValues.MaximumAllowedBoost) {
                    throw new ArgumentOutOfRangeException("Boost cannot be greater than " + StaticValues.MaximumAllowedBoost.ToString());
                }
                this.luceneDoc.SetBoost(value);
            }
        }

        /// <summary>
        /// Gets the total number of fields in this instance.
        /// </summary>
        public int TotalFields
        {
            get {
                return this.luceneDoc.GetFields().Count;
            }
        }

        /// <summary>
        /// Gets the get Lucene equivalent of this instance.
        /// </summary>
        internal Lucene29.Net.Documents.Document GetLuceneDocument
        {
            get {
                return this.luceneDoc;
            }
        }

        #endregion Properties

        #region Indexers

        /// <summary>
        /// Gets a <see cref="IndexLibrary.FieldAbstract"/> for the first field with the specified field name.
        /// </summary>
        /// <param name="fieldName">The field name to search for</param>
        public FieldAbstract this[string fieldName]
        {
            get {
                return (FieldAbstract)this.GetField(fieldName);
            }
        }

        #endregion Indexers

        #region Methods

        /// <summary>
        /// Performs an explicit conversion from <see cref="Lucene29.Net.Documents.Document"/> to <see cref="IndexLibrary.IndexDocument"/>.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator IndexDocument(Lucene29.Net.Documents.Document document)
        {
            if (document == null)
                throw new ArgumentNullException("document", "document cannot be null");
            System.Collections.IList fields = document.GetFields();
            IndexDocument indexDocument = new IndexDocument();
            foreach (object objField in fields) {
                Lucene29.Net.Documents.Field field = (Lucene29.Net.Documents.Field)objField;
                FieldAbstract normalField = (FieldAbstract)field;
                indexDocument.Add(normalField);
            }
            return indexDocument;
        }

        /// <summary>
        /// Documents the specified document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns></returns>
        public static explicit operator Lucene29.Net.Documents.Document(IndexDocument document)
        {
            if (document == null)
                throw new ArgumentNullException("document", "document cannot be null");
            return document.GetLuceneDocument;
        }

        /// <summary>
        /// Adds the specified field to this document. 
        /// </summary>
        /// <param name="field">The field to add.</param>
        public void Add(FieldAbstract field)
        {
            if (field == null)
                throw new ArgumentNullException("field", "field cannot be null");
            this.luceneDoc.Add(field.GetLuceneField());
        }

        /// <summary>
        /// Converts this <see cref="IndexLibrary.IndexDocument"/> into a <see cref="IndexLibrary.SearchResult"/>
        /// </summary>
        /// <returns>A <see cref="IndexLibrary.SearchResult"/> that represents this instance</returns>
        public SearchResult AsSearchResult(string indexName)
        {
            Dictionary<string, string> values = new Dictionary<string, string>();
            foreach (var field in this.GetFields()) {
                int totalTries = 1;
                string originalFieldName = field.FieldName;
                while (totalTries < 100 && values.ContainsKey(field.FieldName))
                    field.FieldName = originalFieldName + "(" + totalTries.ToString() + ")";
                if (totalTries >= 100)
                    continue;
                values.Add(field.FieldName, field.FieldValue);
            }
            return new SearchResult(values, indexName == null ? "No Index" : indexName, this.Boost);
        }

        /// <summary>
        /// Gets a <see cref="IndexLibrary.FieldAbstract"/> for the first field with the specified name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>Returns a FieldAbstract that represents the field with the specified name</returns>
        public FieldAbstract GetField(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentNullException("fieldName", "fieldName cannot be null or empty");
            return (FieldAbstract)this.luceneDoc.GetField(fieldName);
        }

        /// <summary>
        /// Gets all fields contained within this document.
        /// </summary>
        /// <returns>Returns a list of <see cref="IndexLibrary.FieldAbstract"/> representing all fields in this document.</returns>
        public IEnumerable<FieldAbstract> GetFields()
        {
            return this.luceneDoc.GetFields().Cast<FieldAbstract>();
        }

        /// <summary>
        /// Gets all fields contained within this document that have the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field to retrieve.</param>
        /// <returns>Returns a list of <see cref="IndexLibrary.FieldAbstract"/> representing all fields in this document</returns>
        public IEnumerable<FieldAbstract> GetFields(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentNullException("fieldName", "fieldName cannot be null or empty");
            return this.luceneDoc.GetFields(fieldName).Cast<FieldAbstract>();
        }

        /// <summary>
        /// Removes the specified field from this document.
        /// </summary>
        /// <param name="field">The field to remove.</param>
        public void RemoveField(SearchTerm field)
        {
            if (field == null)
                throw new ArgumentNullException("field", "field cannot be null");
            this.luceneDoc.RemoveField(field.FieldName);
        }

        /// <summary>
        /// Removes the first field in this document that has the specified field name. If none exists, 
        /// no fields are removed.
        /// </summary>
        /// <param name="fieldName">Name of the field to remove.</param>
        public void RemoveField(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentNullException("fieldName", "fieldName cannot be null or empty");
            this.luceneDoc.RemoveField(fieldName);
        }

        /// <summary>
        /// Removes all fields from this document that have the specified field name. If none exist,
        /// no fields are removed.
        /// </summary>
        /// <param name="fieldName">Name of the field to remove.</param>
        public void RemoveFields(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentNullException("fieldName", "fieldName cannot be null or empty");
            this.luceneDoc.RemoveFields(fieldName);
        }

        /// <summary>
        /// Converts this instance of an <see cref="IndexLibrary.IndexDocument"/> into a DataRow and adds it to the supplied DataTable.
        /// It also creates any fields that don't exist within the table but do exist within this instance.
        /// </summary>
        /// <param name="table">The table to add this instance to.</param>
        public void ToDataRow(DataTable table)
        {
            this.ToDataRow(table, true);
        }

        /// <summary>
        /// Converts this instance of an <see cref="IndexLibrary.IndexDocument"/> into a DataRow and adds it to the supplied DataTable.
        /// </summary>
        /// <param name="table">The table to add this instance to.</param>
        /// <param name="createFieldsThatDoNotExist">if set to <c>true</c> [create fields that dont exist] within the supplied DataTable; otherwise, only fields that exist in the table that match this instance will be populated.</param>
        public void ToDataRow(DataTable table, bool createFieldsThatDoNotExist)
        {
            if (table == null)
                throw new ArgumentNullException("table", "table cannot be null");
            var row = table.NewRow();
            foreach (var field in this.GetFields()) {
                if (createFieldsThatDoNotExist) if (!table.Columns.Contains(field.FieldName))
                        table.Columns.Add(field.FieldName);
                row[field.FieldName] = field.FieldValue;
            }
            table.Rows.Add(row);
        }

        #endregion Methods
    }
}