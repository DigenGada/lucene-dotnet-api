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
    using System.Text;

    [System.CLSCompliant(true)]
    internal sealed class WhereExpression
    {
        #region Fields

        // ToDo: Implement!
        private List<string> allowedTableNames;
        private string expressionString;

        #endregion Fields

        #region Constructors

        internal WhereExpression(string expression, List<string> allowedTables)
        {
            if (allowedTables == null || allowedTables.Count == 0)
                throw new ArgumentNullException("allowedTables", "A where expression cannot be created without a table constraint.");
            this.allowedTableNames = allowedTables;
            this.expressionString = expression;
        }

        #endregion Constructors

        #region Methods

        public QueryBuilder CreateQuery()
        {
            return CreateQuery(true);
        }

        public QueryBuilder CreateQuery(bool disableCoord)
        {
            if (string.IsNullOrEmpty(this.expressionString))
                return new QueryBuilder(disableCoord);

            ValidateQuery(this.expressionString);

            List<string> parts = new List<string>();
            // FirstName Should Equal 'Tim', MUST(LastName Must Equal 'James', Type Must Equal '0')

            string[] tempParts = this.expressionString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int totalParts = tempParts.Length;
            for (int i = 0; i < totalParts; i++) {
                tempParts[i] = tempParts[i].Trim();
                if (tempParts[i].Equals("Equals", StringComparison.OrdinalIgnoreCase))
                    parts.Add("=");
                else if (tempParts[i].Equals("Equal", StringComparison.OrdinalIgnoreCase))
                    parts.Add("=");
                else if (tempParts[i].Equals("GreaterThan", StringComparison.OrdinalIgnoreCase))
                    parts.Add(">");
                else if (tempParts[i].Equals("GreaterOrEqual", StringComparison.OrdinalIgnoreCase))
                    parts.Add(">=");
                else if (tempParts[i].Equals("LessThan", StringComparison.OrdinalIgnoreCase))
                    parts.Add("<");
                else if (tempParts[i].Equals("LessOrEqual", StringComparison.OrdinalIgnoreCase))
                    parts.Add("<=");
                else if (tempParts[i].Equals("NotEqual", StringComparison.OrdinalIgnoreCase))
                    parts.Add("!=");
                else if (tempParts[i].Equals("<>", StringComparison.OrdinalIgnoreCase))
                    parts.Add("!=");
                else if (tempParts[i].Equals("And", StringComparison.OrdinalIgnoreCase))
                    parts.Add("+"); // parts.Add("&");
                else if (tempParts[i].Equals("Or", StringComparison.OrdinalIgnoreCase))
                    parts.Add("#"); // "|");
                else if (tempParts[i].Equals("Not", StringComparison.OrdinalIgnoreCase))
                    parts.Add("-");
                else if (tempParts[i].Equals("Must", StringComparison.OrdinalIgnoreCase) || tempParts[i].Equals("MustBe", StringComparison.OrdinalIgnoreCase)) {
                    if (parts.Count > 0)
                        parts[parts.Count - 1] += "+";
                }
                else if (tempParts[i].StartsWith("Must(", StringComparison.OrdinalIgnoreCase)) {
                    parts.Add("+" + tempParts[i].Substring(4));
                }
                else if (tempParts[i].StartsWith("MustBe(", StringComparison.OrdinalIgnoreCase)) {
                    parts.Add("+" + tempParts[i].Substring(6));
                }
                else if (tempParts[i].Equals("Should", StringComparison.OrdinalIgnoreCase) || tempParts[i].Equals("ShouldBe", StringComparison.OrdinalIgnoreCase)) {
                    if (parts.Count > 0)
                        parts[parts.Count - 1] += "#";
                }
                else if (tempParts[i].StartsWith("Should(", StringComparison.OrdinalIgnoreCase)) {
                    parts.Add("#" + tempParts[i].Substring(6));
                }
                else if (tempParts[i].StartsWith("ShouldBe(", StringComparison.OrdinalIgnoreCase)) {
                    parts.Add("#" + tempParts[i].Substring(8));
                }
                else if (tempParts[i].Equals("MustNot", StringComparison.OrdinalIgnoreCase) || tempParts[i].Equals("MustNotBe", StringComparison.OrdinalIgnoreCase)) {
                    if (parts.Count > 0)
                        parts[parts.Count - 1] += "-";
                }
                else if (tempParts[i].StartsWith("MustNot(", StringComparison.OrdinalIgnoreCase)) {
                    parts.Add("-" + tempParts[i].Substring(7));
                }
                else if (tempParts[i].StartsWith("MustNotBe(", StringComparison.OrdinalIgnoreCase)) {
                    parts.Add("-" + tempParts[i].Substring(9));
                }
                else
                    parts.Add(tempParts[i]);
                tempParts[i] = string.Empty;
            }
            tempParts = null;

            parts = CreateQueryTier(string.Join(" ", parts.ToArray()));
            totalParts = parts.Count;
            Stack<QueryBuilder> builders = new Stack<QueryBuilder>();
            Stack<ClauseOccurrence> occurrences = new Stack<ClauseOccurrence>();
            // FirstName# = 'Tim', +(LastName+ = 'James', Type+ = '0')
            // FirstName# = 'Tim
            // +(LastName+ = 'James', Type+ = '0')

            for (int i = 0; i < totalParts; i++) {
                ClauseOccurrence occurrence;
                builders.Push(CreateBuilderFromExpression(parts[i].Trim(), out occurrence));
                occurrences.Push(occurrence);
            }

            QueryBuilder current = null;
            while (builders.Count > 0) {
                var pop = builders.Pop();
                var occurrence = occurrences.Pop();
                if (current == null) {
                    current = pop;
                    continue;
                }

                pop.Append(current, occurrence);
                current = pop;
            }

            return current;
        }

        private static QueryBuilder CreateBuilderFromExpression(string expression, out ClauseOccurrence occurrence)
        {
            // FirstName# = 'Tim
            // +(LastName+ = 'James', Type+ = '0')

            occurrence = ClauseOccurrence.Default;
            if (string.IsNullOrEmpty(expression))
                return new QueryBuilder();
            QueryBuilder builder = new QueryBuilder();

            if (expression.StartsWith("+")) {
                occurrence = ClauseOccurrence.MustOccur;
                expression = expression.Substring(1);
            }
            else if (expression.StartsWith("#")) {
                occurrence = ClauseOccurrence.ShouldOccur;
                expression = expression.Substring(1);
            }
            else if (expression.StartsWith("-")) {
                occurrence = ClauseOccurrence.MustNotOccur;
                expression = expression.Substring(1);
            }

            if (expression.StartsWith("("))
                expression = expression.Substring(1);
            if (expression.EndsWith(")"))
                expression = expression.Substring(0, expression.Length - 1);

            string[] parts = expression.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts) {
                string searchTerm;
                string boolOperator;
                string searchValue;
                ClauseOccurrence co = ClauseOccurrence.Default;

                string[] words = part.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length < 3)
                    throw new ArgumentNullException("expression", "Malformed expression");
                searchTerm = words[0];
                boolOperator = words[1];
                searchValue = string.Join(" ", words, 2, words.Length - 2).Replace("\'", "");
                if (searchTerm.EndsWith("+")) {
                    co = ClauseOccurrence.MustOccur;
                    searchTerm = searchTerm.Substring(0, searchTerm.Length - 1);
                }
                else if (searchTerm.EndsWith("#")) {
                    co = ClauseOccurrence.ShouldOccur;
                    searchTerm = searchTerm.Substring(0, searchTerm.Length - 1);
                }
                else if (searchTerm.EndsWith("-")) {
                    co = ClauseOccurrence.MustNotOccur;
                    searchTerm = searchTerm.Substring(0, searchTerm.Length - 1);
                }

                if (boolOperator.Equals("=")) {
                    builder.AddBooleanClause(new SearchTerm(searchTerm, searchValue, StaticValues.DefaultBoost, true), co);
                }
                else if (boolOperator.Equals(">")) {
                    throw new NotSupportedException("Boolean operator not yet supported");
                }
                else if (boolOperator.Equals(">=")) {
                    throw new NotSupportedException("Boolean operator not yet supported");
                }
                else if (boolOperator.Equals("<")) {
                    throw new NotSupportedException("Boolean operator not yet supported");
                }
                else if (boolOperator.Equals("<=")) {
                    throw new NotSupportedException("Boolean operator not yet supported");
                }
                else if (boolOperator.Equals("!=") || boolOperator.Equals("<>")) {
                    if (co == ClauseOccurrence.MustNotOccur)
                        co = ClauseOccurrence.MustOccur;
                    else if (co == ClauseOccurrence.MustOccur)
                        co = ClauseOccurrence.MustNotOccur;
                    builder.AddBooleanClause(new SearchTerm(searchTerm, searchValue, StaticValues.DefaultBoost, true), co);
                }
                else {
                    throw new ArgumentException("Malformed boolean operator in expression");
                }
            }

            return builder;
        }

        private static List<string> CreateQueryTier(string query)
        {
            List<string> results = new List<string>();
            if (string.IsNullOrEmpty(query))
                return results;

            Action<string, int> AddToResults = delegate(string pieceToAdd, int index) {
                if (results.Count <= index)
                    results.Add(pieceToAdd);
                else
                    results[index] += ", " + pieceToAdd;
            };

            // FirstName# = 'Tim', +(LastName+ = 'James', Type+ = '0')
            int resultIndex = 0;
            int position = -1;
            int lastMarker = -1;
            int queryLength = query.Length;
            int queryMaxIndex = queryLength - 1;
            for (int i = 0; i < queryLength; i++) {
                char c = query[i];
                position++;
                if (c == '(') {
                    resultIndex++;
                    continue;
                }
                else if (c == ')') {
                    AddToResults(query.Substring(lastMarker, position + 1 - lastMarker), resultIndex);
                    lastMarker = position + 1;
                    resultIndex--;
                    continue;
                }
                else if (c == ',') {
                    if (lastMarker == -1) {
                        lastMarker = position + 1;
                        AddToResults(query.Substring(0, position), resultIndex);
                    }
                    else {
                        AddToResults(query.Substring(lastMarker, position - lastMarker), resultIndex);
                        lastMarker = position + 1;
                    }
                    continue;
                }

                if (position == queryMaxIndex)
                    AddToResults(query.Substring(lastMarker, position - lastMarker), resultIndex);
            }

            return results;
        }

        private static string ValidateQuery(string query)
        {
            StringBuilder builder = new StringBuilder();
            if (string.IsNullOrEmpty(query))
                builder.ToString();

            int totalOpenParens = 0;
            int totalCloseParens = 0;
            int totalSingleQuotes = 0;
            int totalDoubleQuotes = 0;
            int queryLength = query.Length;
            char lastC = '\0';
            for (int i = 0; i < queryLength; i++) {
                char c = query[i];
                switch (c) {
                    case '(':
                        // escaped paren
                        if (lastC != '\\')
                            totalOpenParens++;
                        break;
                    case ')':
                        // esacped paren
                        if (lastC != '\\')
                            totalCloseParens++;
                        if (totalCloseParens > totalOpenParens)
                            throw new ArgumentException("Malformed Where statement. Parenthesis out of order.");
                        break;
                    case '\'':
                        totalSingleQuotes++;
                        break;
                    case '\"':
                        totalDoubleQuotes++;
                        break;
                }
                lastC = c;
            }

            if (totalOpenParens != totalCloseParens)
                throw new ArgumentException("Malformed Where statement. Unequal number of parenthesis.");
            if (totalSingleQuotes % 2 == 1)
                throw new ArgumentException("Malformed Where statement. Unequal number of single quotes.");

            return builder.ToString();
        }

        #endregion Methods
    }
}