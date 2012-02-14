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
namespace IndexLibrary.SqlParser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    [System.CLSCompliant(true)]
    internal sealed class SelectQuery : SqlQuery
    {
        #region Fields

        private bool distinct;
        private List<QualifiedFieldName> orderByFields;
        private List<QualifiedFieldName> selectedFieldNames;
        private Top selectTop = Top.None;
        private int topNumber = 0;
        private WhereExpression whereExpression;

        #endregion Fields

        #region Constructors

        internal SelectQuery()
            : base()
        {
            this.QueryType = QueryType.Select;
            this.distinct = false;
            this.selectedFieldNames = new List<QualifiedFieldName>();
            this.orderByFields = new List<QualifiedFieldName>();
        }

        #endregion Constructors

        #region Properties

        public bool HasOrderByClause
        {
            get { return (this.orderByFields != null && this.orderByFields.Count > 0); }
        }

        public bool HasWhereClause
        {
            get { return this.whereExpression != null; }
        }

        public bool IsDistinctClause
        {
            get { return this.distinct; }
            internal set { this.distinct = value; }
        }

        public IEnumerable<QualifiedFieldName> OrderByFields
        {
            get { return this.orderByFields; }
        }

        public IEnumerable<QualifiedFieldName> SelectedColumns
        {
            get { return this.selectedFieldNames; }
        }

        public bool SelectsAllColumns
        {
            get {
                if (this.selectedFieldNames != null && this.selectedFieldNames.Count > 0)
                    return this.selectedFieldNames[0].Equals("*");
                return false;
            }
        }

        public Top Top
        {
            get { return this.selectTop; }
            internal set { this.selectTop = value; }
        }

        public int TopNumber
        {
            get { return this.topNumber; }
            internal set { this.topNumber = value; }
        }

        public WhereExpression WhereExpression
        {
            get { return this.whereExpression; }
            internal set { this.whereExpression = value; }
        }

        internal List<QualifiedFieldName> OrderByFieldsList
        {
            get { return this.orderByFields; }
        }

        internal List<QualifiedFieldName> SelectedColumnsList
        {
            get { return this.selectedFieldNames; }
        }

        #endregion Properties
    }
}