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
    using System.Text;

    /// <summary>
    /// Helper for stripping Lucene special characters from a string
    /// </summary>
    /// <remarks>
    /// http://lucene.apache.org/java/2_1_0/queryparsersyntax.html
    /// </remarks>
    public static class StringFormatter
    {
        #region Methods

        /// <summary>
        /// Escapes any special characters from the provided string.
        /// </summary>
        /// <param name="searchText">The input string.</param>
        /// <returns>Returns the input string with any special characters escaped.</returns>
        public static string EscapeSpecialCharacters(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
                return string.Empty;
            StringBuilder builder = new StringBuilder(searchText);
            string[] specials = GetSpecialCharacters();
            int specialsLength = specials.Length;

            for (int i = 0; i < specialsLength; i++)
                builder.Replace(specials[i], "\\" + specials[i]);

            return builder.ToString();
        }

        /// <summary>
        /// Gets the Lucene recognized special characters.
        /// </summary>
        /// <remarks>
        /// These are in a specific order, do not rearrange.
        /// </remarks>
        public static string[] GetSpecialCharacters()
        {
            return new string[] { "\\", "\"", "+", "-", "&&", "||", "!", "(", ")", "{", "}", "[", "]", "^", "~", "*", "?", ":" };
        }

        /// <summary>
        /// Strips any special characters from the provided string.
        /// </summary>
        /// <param name="searchText">The input string.</param>
        /// <returns>Returns the input string with any special characters removed.</returns>
        public static string StripSpecialCharacters(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
                return string.Empty;
            StringBuilder builder = new StringBuilder(searchText);
            string[] specials = GetSpecialCharacters();
            int specialsLength = specials.Length;

            for (int i = 0; i < specialsLength; i++)
                builder.Replace(specials[i], "");

            return builder.ToString();
        }

        #endregion Methods
    }
}