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
namespace IndexLibrary.Phonetics
{
    using System;

    /// <summary>
    /// An 'edit distance' defines the distance between two strings in similarity
    /// </summary>
    [System.CLSCompliant(true)]
    public static class EditDistance
    {
        #region Methods

        /// <summary>
        /// Calculates the levenshtein distance using any two strings
        /// </summary>
        /// <param name="sourceValue">The source string.</param>
        /// <param name="targetValue">The target string.</param>
        /// <returns></returns>
        /// <remarks>
        /// Algorithm provided w/o a license from http://www.merriampark.com/ldcsharp.htm
        /// </remarks>
        public static int CalculateLevenshteinDistance(string sourceValue, string targetValue)
        {
            if (string.IsNullOrEmpty(sourceValue) || string.IsNullOrEmpty(targetValue))
                return 0;

            int[,] matrix;
            int m;
            int n;
            int i;
            int j;
            int cost;

            // step 1
            n = sourceValue.Length;
            m = targetValue.Length;
            if (n == 0 || m == 0)
                return 0;
            matrix = new int[n + 1, m + 1];

            // step 2
            for (i = 0; i <= n; i++)
                matrix[i, 0] = i;
            for (j = 0; j <= m; j++)
                matrix[0, j] = j;

            // step 3
            for (i = 1; i <= n; i++) {
                for (j = 1; j <= m; j++) {
                    cost = targetValue.Substring(j - 1, 1) == sourceValue.Substring(i - 1, 1) ? 0 : 1;

                    matrix[i, j] = Minimum(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1, matrix[i - 1, j - 1] + cost);
                }
            }

            return matrix[n, m];
        }

        /// <summary>
        /// Calculates the MTR distance using two MTR codes
        /// </summary>
        /// <param name="sourceValue">The source string.</param>
        /// <param name="targetValue">The target string.</param>
        /// <returns></returns>
        public static int CalculateMTRDistance(string sourceValue, string targetValue)
        {
            return CalculateMTRDistance(sourceValue, targetValue, false);
        }

        /// <summary>
        /// Calculates the MTR distance using two MTR codes
        /// </summary>
        /// <param name="sourceValue">The source string.</param>
        /// <param name="targetValue">The target string.</param>
        /// <param name="createMTR">if set to <c>true</c> [create MTR].</param>
        /// <returns></returns>
        public static int CalculateMTRDistance(string sourceValue, string targetValue, bool createMTR)
        {
            if (string.IsNullOrEmpty(sourceValue) || string.IsNullOrEmpty(targetValue))
                return -1;

            if (createMTR) {
                sourceValue = PhoneticAlgorithms.CreateMra(sourceValue);
                targetValue = PhoneticAlgorithms.CreateMra(targetValue);
            }

            int sourceLength = sourceValue.Length;
            int targetLength = targetValue.Length;
            int sumLength = sourceLength + targetLength;
            int minimumRating = 0;

            if (Math.Abs(sourceLength - targetLength) > 4)
                return -1;

            if (sumLength <= 4)
                minimumRating = 5;
            else if (sumLength <= 7)
                minimumRating = 4;
            else if (sumLength <= 11)
                minimumRating = 3;
            else if (sumLength == 12)
                minimumRating = 2;
            else
                minimumRating = 0;

            string largeString;
            string smallString;
            if (sourceLength > targetLength) {
                largeString = sourceValue;
                smallString = targetValue;
            }
            else {
                largeString = targetValue;
                smallString = sourceValue;
            }
            targetValue = string.Empty;
            sourceValue = string.Empty;

            for (int i = 0; i < smallString.Length; ) {
                bool found = false;
                for (int j = 0; j < largeString.Length; j++) {
                    if (smallString[i] == largeString[j]) {
                        smallString = smallString.Remove(i, 1);
                        largeString = largeString.Remove(j, 1);
                        found = true;
                    }
                }

                if (!found)
                    i++;
            }

            int rating = 6 - largeString.Length;
            if (rating >= minimumRating)
                return rating;

            return 0;
        }

        /// <summary>
        /// Finds the minimum value between the three provided values.
        /// </summary>
        /// <param name="a">The first integer value</param>
        /// <param name="b">The second integer value</param>
        /// <param name="c">The third integer value</param>
        /// <returns>An integer representing the smallest value from all the provided parameters</returns>
        private static int Minimum(int a, int b, int c)
        {
            int mi = a;
            if (b < mi)
                mi = b;
            if (c < mi)
                mi = c;
            return mi;
        }

        #endregion Methods
    }
}