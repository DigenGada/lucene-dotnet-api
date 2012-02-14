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

    [System.CLSCompliant(true)]
    internal abstract class SqlQuery
    {
        #region Fields

        private static System.Collections.ObjectModel.ReadOnlyCollection<string> Keywords = new System.Collections.ObjectModel.ReadOnlyCollection<string>(new string[] { "select", "insert", "update", "delete", "where", "order", "by", "between", "in", "asc", "desc", "values", "into", "set", "from" });

        private List<string> tablesList = new List<string>();
        private QueryType type = QueryType.Unknown;

        #endregion Fields

        #region Properties

        public QueryType QueryType
        {
            get { return this.type; }
            internal set { this.type = value; }
        }

        public IEnumerable<string> SelectedTables
        {
            get { return this.tablesList; }
            protected set { this.tablesList = new List<string>(value); }
        }

        #endregion Properties

        #region Methods

        public static SqlQuery Parse(string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException("text", "text cannot be null or empty");
            text = text.Trim();

            // check beginning
            if (text.StartsWith("select ", StringComparison.OrdinalIgnoreCase)) {
                SelectQuery selectQuery = new SelectQuery();
                text = text.Substring(text.IndexOf(' ') + 1).TrimStart();

                #region Top and Distinct
                // we can have just distinct
                if (MatchNextWord(ref text, "distinct ")) {
                    // process
                    selectQuery.IsDistinctClause = true;
                    text = text.Remove(0, 8).TrimStart();

                    // check for top
                    if (MatchNextWord(ref text, "top "))
                        throw new ArgumentException("The supplied query is invalid because the Distinct keyword cannot occur before the Top clause.", "text");
                }

                // or top and then distinct
                if (MatchNextWord(ref text, "top ")) {
                    // process
                    int topN = 0;
                    selectQuery.Top = GetTop(ref text, out topN);
                    if (topN < 0)
                        throw new ArgumentOutOfRangeException("text", "The supplied query is invalid because the Top clause requests less than zero rows.");
                    if (selectQuery.Top == Top.Percent && topN > 100)
                        throw new ArgumentOutOfRangeException("text", "The supplied query is invalid because the Top clause requests greater than 100 percent of rows. Percent values must be between 0 and 100");
                    selectQuery.TopNumber = topN;

                    // do distinct again
                    if (MatchNextWord(ref text, "distinct ")) {
                        selectQuery.IsDistinctClause = true;
                        text = text.Remove(0, 8).TrimStart();
                    }
                }
                #endregion

                #region Get Column Names
                // field names
                int fromIndex = text.IndexOf(" from ", StringComparison.OrdinalIgnoreCase);
                if (fromIndex < 0)
                    throw new ArgumentException("The supplied query is invalid because there is no From clause in the Select statement", "text");
                selectQuery.SelectedColumnsList.AddRange(GetGroupedNames(text.Substring(0, fromIndex).TrimEnd(), true).Select(x => new QualifiedFieldName(x)));
                if (selectQuery.SelectedColumnsList.Count == 0)
                    throw new ArgumentException("The supplied query is invalid because there are no selected column names in the Select statement", "text");
                text = text.Substring(fromIndex).TrimStart();
                #endregion

                #region From, Where, and Order By
                // from (which we determined in the last step exists)
                if (!MatchNextWord(ref text, "from "))
                    throw new ArgumentException("A from clause must exist in this query", "text");
                text = text.Substring(text.IndexOf(' ')).TrimStart();
                int whereIndex = text.IndexOf(" where ", StringComparison.OrdinalIgnoreCase);
                int orderByIndex = text.IndexOf(" order by ", StringComparison.OrdinalIgnoreCase);

                // ensure they're in the correct order
                if (whereIndex > orderByIndex && orderByIndex > -1)
                    throw new ArgumentException("The supplied query is invalid. The Where clause must preceed the Order By clause.", "text");

                // no additional clauses
                if (whereIndex < 0 && orderByIndex < 0) {
                    selectQuery.tablesList.AddRange(GetGroupedNames(text, false));
                    if (selectQuery.tablesList.Count == 0)
                        throw new ArgumentException("The supplied query is invalid because there were no tables listed in the From clause.", "text");
                }

                if (whereIndex > -1) {
                    selectQuery.tablesList.AddRange(GetGroupedNames(text.Substring(0, whereIndex).TrimStart(), false));
                    if (selectQuery.tablesList.Count == 0)
                        throw new ArgumentException("The supplied query is invalid because there were no tables listed in the From clause.", "text");
                    text = text.Substring(whereIndex + 7).TrimStart();

                    // process where clause
                    if (orderByIndex > -1) {
                        selectQuery.WhereExpression = new WhereExpression(text.Substring(0, orderByIndex).Trim(), selectQuery.tablesList);
                        text = text.Substring(orderByIndex).TrimStart(); // bring us up to the order by clause but not past it
                    }
                    else {
                        selectQuery.WhereExpression = new WhereExpression(text, selectQuery.tablesList);
                    }
                }

                if (orderByIndex > -1) {
                    // it hasn't been processed yet, give it a shot
                    if (selectQuery.tablesList.Count == 0) {
                        selectQuery.tablesList.AddRange(GetGroupedNames(text.Substring(0, orderByIndex).TrimStart(), false));
                        if (selectQuery.tablesList.Count == 0)
                            throw new ArgumentException("The supplied query is invalid because there were no tables listed in the From clause.", "text");
                        text = text.Substring(orderByIndex).TrimStart();
                    }

                    // now process order by clause
                    if (!MatchNextWord(ref text, "order "))
                        throw new ArgumentException("The supplied query is invalid because of a malformed Order By clause.", "text");
                    text = text.Substring(text.IndexOf(' ')).TrimStart();
                    if (!MatchNextWord(ref text, "by "))
                        throw new ArgumentException("The supplied query is invalid because of a malformed Order By clause.", "text");
                    text = text.Substring(text.IndexOf(' ')).TrimStart();

                    IEnumerable<QualifiedFieldName> orderByFields = GetGroupedNames(text, false).Select(x => new QualifiedFieldName(x));
                    foreach (var field in orderByFields) {
                        // CAUTION: not sure if this is necessary but its a safety precaution
                        //if (!selectQuery.SelectedColumnsList.Contains(field) && !selectQuery.SelectedColumnsList.Contains(new QualifiedFieldName(field.TableQualifier + ".*"))) throw new ArgumentException("The supplied query is invalid because the Order By clause references a field that is not included in the Select clause.", "text");
                        // Lets just check to make sure the table is referenced if its a multipart identified
                        // its the table identifier is all tables then don't bother
                        if (field.TableQualifier.Equals("*"))
                            continue;
                        if (selectQuery.SelectedTables.FirstOrDefault(x => x.Equals(field.TableQualifier, StringComparison.OrdinalIgnoreCase)) == null)
                            throw new ArgumentException("The supplied query is invalid because the Order By clause references a table that is not included in the From clause.", "text");
                    }

                    selectQuery.OrderByFieldsList.AddRange(orderByFields);
                }
                #endregion

                #region Validate Select Columns
                // make sure that any multipart identifiers in the selected field names are referenced in the from clause
                foreach (var field in selectQuery.SelectedColumns) {
                    if (field.TableQualifier.Equals("*"))
                        continue;
                    if (selectQuery.SelectedTables.FirstOrDefault(x => x.Equals(field.TableQualifier, StringComparison.OrdinalIgnoreCase)) == null)
                        throw new ArgumentException("The supplied query is invalid because the Select clause references a table that is not included in the From clause.", "text");
                }
                #endregion

                return selectQuery;
            }
            else if (text.StartsWith("insert ", StringComparison.OrdinalIgnoreCase)) {
                InsertQuery insertQuery = new InsertQuery();

                return insertQuery;
            }
            else if (text.StartsWith("delete ", StringComparison.OrdinalIgnoreCase)) {
                DeleteQuery deleteQuery = new DeleteQuery();

                return deleteQuery;
            }
            else if (text.StartsWith("update ", StringComparison.OrdinalIgnoreCase)) {
                // this is just a parser, allow it here but blowup if they try to execute it
                UpdateQuery updateQuery = new UpdateQuery();

                return updateQuery;
            }

            throw new ArgumentException("The supplied query is invalid because it does not begin with a valid SQL keyword.", "text");
        }

        private static List<string> GetGroupedNames(string text, bool allowAllCharacter)
        {
            // text should only be field names comma seperated
            List<string> fieldNames = new List<string>();
            if (string.IsNullOrEmpty(text))
                return fieldNames;
            string[] fields = text.Split(',');
            int totalFields = fields.Length;
            for (int i = 0; i < totalFields; i++) {
                if (string.IsNullOrEmpty(fields[i]))
                    throw new ArgumentException("The supplied query is invalid because there is an empty field name", "text");
                fields[i] = fields[i].Trim();
                if (fields[i].Contains(' '))
                    throw new ArgumentException("The supplied query is invalid because there is an invalid space inside a field name.", "text");
                if (fields[i].StartsWith("[") && fields[i].EndsWith("]")) {
                    if (fields[i].Length == 2)
                        throw new ArgumentException("The supplied query is invalid because a there is a blank field name enclosed in brackets", "text");
                    fields[i] = fields[i].Substring(1, fields[i].Length - 2);
                }
                else {
                    if (Keywords.FirstOrDefault(x => x.Equals(fields[i], StringComparison.OrdinalIgnoreCase)) != null)
                        throw new ArgumentException("You must wrap any ambiguous column names with square brackets, or ensure that they are fully qualified.", "text");
                }
                fields[i] = fields[i].Trim();
                if (!allowAllCharacter && fields[i].Equals("*"))
                    throw new ArgumentException("The '*' character is not allowed in this part of the query", "text");
                fieldNames.Add(fields[i]);
            }
            if (fieldNames.Contains("*") && fieldNames.Count > 1)
                throw new ArgumentException("The supplied query is invalid because all columns are marked to be selected as well as specific columns. Either select all columns or particular ones.", "text");
            return fieldNames;
        }

        private static Top GetTop(ref string text, out int number)
        {
            number = -1;
            if (!MatchNextWord(ref text, "top "))
                return Top.None;
            text = text.Substring(text.IndexOf(' ')).TrimStart();
            string numberPortion = text.Substring(0, text.IndexOf(' ')).Trim();
            foreach (char c in numberPortion.ToCharArray()) if (!char.IsDigit(c))
                    throw new ArgumentException("The supplied Top clause is invalid because the supplied number is not a number", "text");
            if (!int.TryParse(numberPortion, out number))
                throw new ArgumentOutOfRangeException("text", "There was an error parsing out the number of requested rows from the Top clause. It's likely that the selected number is too large.");

            text = text.Substring(text.IndexOf(' ')).TrimStart();
            if (MatchNextWord(ref text, "Percent ")) {
                text = text.Substring(text.IndexOf(' ')).TrimStart();
                return Top.Percent;
            }
            return Top.Number;
        }

        private static bool MatchNextWord(ref string text, string expectedWord)
        {
            int totalText = text.Length;
            int expectedWordIndex = 0;
            int expectedWordLength = expectedWord.Length;
            bool started = false;

            for (int i = 0; i < totalText; i++) {
                // skip spaces
                if (!started && text[i] == ' ')
                    continue;
                started = true;

                if (expectedWordIndex == expectedWordLength)
                    return true;
                if (!text[i].ToString().Equals(expectedWord[expectedWordIndex++].ToString(), StringComparison.OrdinalIgnoreCase))
                    return false;
            }
            return false;
        }

        #endregion Methods
    }
}