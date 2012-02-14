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
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Helper class used to easily create phonetic tokens.
    /// </summary>
    [CLSCompliant(true)]
    public static class PhoneticAlgorithms
    {
        #region Fields

        /// <summary>
        /// Represents the null character
        /// </summary>
        public const char NullChar = (char)0;

        /// <summary>
        /// Represents all vowels in the english alphabet
        /// </summary>
        public const string Vowels = "AEIOU";

        /// <summary>
        /// A simple regex helper used for Caverphone and MRA algorithms
        /// </summary>
        private static Regex regex = new System.Text.RegularExpressions.Regex("(.)(?<=\\1\\1\\1)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        #endregion Fields

        #region Methods

        /// <summary>
        /// Caverphone algorithm
        /// </summary>
        /// <param name="text">Text to create caverphone from</param>
        /// <returns>A string token that represents the input text</returns>
        /// <remarks>
        /// Taken from the non-licensed document http://caversham.otago.ac.nz/files/working/ctp150804.pdf
        /// </remarks>
        public static string CreateCaverphone(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            if (text.Length < 2)
                return string.Empty;

            // convert to lower
            // remove any non-letter characters
            text = RemoveNonLetters(text.ToLower());

            StringBuilder builder;

            // remove final e
            if (text.EndsWith("e"))
                builder = new StringBuilder(text.Substring(0, text.Length - 1));
            else
                builder = new StringBuilder(text);

            char temp;
            char temp2;
            temp = builder[0];
            temp2 = builder[1];

            // starts with
            if (temp == 'c' || temp == 'r' || temp == 't' || temp == 'e' && builder.Length >= 5) {
                if (temp2 == 'o') {
                    if (builder[2] == 'u' && builder[3] == 'g' && builder[4] == 'h') {
                        builder[3] = '2';
                        builder[4] = 'f';
                    }
                }
                else if (builder.Length >= 6 && ((temp == 'e' && temp2 == 'n' && builder[2] == 'o' && builder[3] == 'u' && builder[4] == 'g' && builder[5] == 'h') || (temp == 't' && temp2 == 'r' && builder[2] == 'o' && builder[3] == 'u' && builder[4] == 'g' && builder[5] == 'h'))) {
                    builder[4] = '2';
                    builder[5] = 'f';
                }
            }
            else if (temp == 'g' && temp2 == 'n') {
                builder[0] = '2';
            }
            else if (temp == 'm' && temp2 == 'b') {
                builder[1] = '2';
            }

            // replace
            builder.Replace("cq", "2q");
            builder.Replace("ci", "si");
            builder.Replace("ce", "se");
            builder.Replace("cy", "sy");
            builder.Replace("tch", "2ch");
            builder.Replace('c', 'k');
            builder.Replace('q', 'k');
            builder.Replace('x', 'k');
            builder.Replace('v', 'f');
            builder.Replace("dg", "2g");
            builder.Replace("tio", "sio");
            builder.Replace('d', 't');
            builder.Replace("ph", "fh");
            builder.Replace('b', 'p');
            builder.Replace("sh", "s2");
            builder.Replace('z', 's');
            temp = builder[0];
            if (temp == 'a' || temp == 'e' || temp == 'i' || temp == 'o' || temp == 'u')
                builder[0] = 'A';
            builder.Replace('a', '3', 1, builder.Length - 1);
            builder.Replace('e', '3', 1, builder.Length - 1);
            builder.Replace('i', '3', 1, builder.Length - 1);
            builder.Replace('o', '3', 1, builder.Length - 1);
            builder.Replace('u', '3', 1, builder.Length - 1);
            builder.Replace('j', 'y');
            temp = builder[0];
            temp2 = builder[1];
            if (temp == 'y') {
                if (temp2 == '3')
                    builder[0] = 'Y';
                else
                    builder[0] = 'A';
            }
            builder.Replace('y', '3');
            builder.Replace("3gh3", "3kh3");
            builder.Replace("gh", "22");
            builder.Replace('g', 'k');

            builder = new StringBuilder(regex.Replace(builder.ToString(), string.Empty));
            builder.Replace("ss", "S");
            builder.Replace("tt", "T");
            builder.Replace("pp", "P");
            builder.Replace("kk", "K");
            builder.Replace("ff", "F");
            builder.Replace("mm", "M");
            builder.Replace("nn", "N");
            builder.Replace('s', 'S');
            builder.Replace('t', 'T');
            builder.Replace('p', 'P');
            builder.Replace('k', 'K');
            builder.Replace('f', 'F');
            builder.Replace('m', 'M');
            builder.Replace('n', 'N');
            builder.Replace("w3", "W3");
            builder.Replace("wh3", "Wh3");
            if (builder[builder.Length - 1] == 'w')
                builder[builder.Length - 1] = '3';
            builder.Replace('w', '2');
            if (builder[0] == 'h')
                builder[0] = 'A';
            builder.Replace('h', '2');
            builder.Replace("r3", "R3");
            if (builder[builder.Length - 1] == 'r')
                builder[builder.Length - 1] = '3';
            builder.Replace('r', '2');
            builder.Replace("l3", "L3");
            if (builder[builder.Length - 1] == 'l')
                builder[builder.Length - 1] = '3';
            builder.Replace('l', '2');

            int builderLength = builder.Length;
            for (int i = builderLength - 1; i >= 0; i--) if (builder[i] == '2')
                    builder.Remove(i, 1);

            if (builder[builder.Length - 1] == '3')
                builder[builder.Length - 1] = 'A';
            builderLength = builder.Length;
            for (int i = builderLength - 1; i >= 0; i--) if (builder[i] == '3')
                    builder.Remove(i, 1);

            builder.Append("1111111111");
            return builder.ToString().Substring(0, 10);
        }

        /// <summary>
        /// Metaphone algorithm
        /// </summary>
        /// <param name="text">Text to create a metaphone from</param>
        /// <returns>A string token that represents the input text</returns>
        public static string CreateMetaphone(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            // Process normalized text
            string _text;
            int _pos;

            StringBuilder builder = new StringBuilder();
            foreach (char c in text) if (Char.IsLetter(c))
                    builder.Append(Char.ToUpper(c));
            _text = builder.ToString();
            _pos = 0;

            // clear builder
            builder.Remove(0, builder.Length);

            // Special handling of some string prefixes:
            //     PN, KN, GN, AE, WR, WH and X
            switch (Peek(0, _pos, ref _text)) {
                case 'P':
                case 'K':
                case 'G':
                    if (Peek(1, _pos, ref _text) == 'N')
                        _pos = Math.Min(_pos + 1, _text.Length);
                    break;
                case 'A':
                    if (Peek(1, _pos, ref _text) == 'E')
                        _pos = Math.Min(_pos + 1, _text.Length);
                    break;

                case 'W':
                    if (Peek(1, _pos, ref _text) == 'R') {
                        _pos = Math.Min(_pos + 1, _text.Length);
                    }
                    else if (Peek(1, _pos, ref _text) == 'H') {
                        builder.Append('W');
                        _pos = Math.Min(_pos + 2, _text.Length);
                    }
                    break;

                case 'X':
                    builder.Append('S');
                    _pos = Math.Min(_pos + 1, _text.Length);
                    break;
            }

            //while (!ENDOfText // EndOfText: get { return _pos>= _text.Length });
            while (_pos < _text.Length && builder.Length < 6) {
                // Cache this character
                char c = Peek(0, _pos, ref _text);

                // Ignore duplicates except CC
                if (c == Peek(-1, _pos, ref _text) && c != 'C') {
                    _pos = Math.Min(_pos + 1, _text.Length);
                    continue;
                }

                // Don't change F, J, L, M, N, R or first-letter vowel
                if (IsOneOf(c, "FJLMNR") ||
                    (builder.Length == 0 && IsOneOf(c, Vowels))) {
                    builder.Append(c);
                    _pos = Math.Min(_pos + 1, _text.Length);
                }
                else {
                    int charsConsumed = 1;

                    switch (c) {
                        case 'B':
                            // B = 'B' if not -MB
                            if (Peek(-1, _pos, ref _text) != 'M' || Peek(1, _pos, ref _text) != NullChar)
                                builder.Append('B');
                            break;

                        case 'C':
                            // C = 'X' if -CIA- or -CH-
                            // Else 'S' if -CE-, -CI- or -CY-
                            // Else 'K' if not -SCE-, -SCI- or -SCY-
                            if (Peek(-1, _pos, ref _text) != 'S' || !IsOneOf(Peek(1, _pos, ref _text), "EIY")) {
                                if (Peek(1, _pos, ref _text) == 'I' && Peek(2, _pos, ref _text) == 'A') {
                                    builder.Append('X');
                                }
                                else if (IsOneOf(Peek(1, _pos, ref _text), "EIY")) {
                                    builder.Append('S');
                                }
                                else if (Peek(1, _pos, ref _text) == 'H') {
                                    if ((_pos == 0 && !IsOneOf(Peek(2, _pos, ref _text), Vowels)) ||
                                        Peek(-1, _pos, ref _text) == 'S')
                                        builder.Append('K');
                                    else
                                        builder.Append('X');
                                    charsConsumed++;    // Eat 'CH'
                                }
                                else {
                                    builder.Append('K');
                                }
                            }
                            break;

                        case 'D':
                            // D = 'J' if DGE, DGI or DGY
                            // Else 'T'
                            if (Peek(1, _pos, ref _text) == 'G' && IsOneOf(Peek(2, _pos, ref _text), "EIY"))
                                builder.Append('J');
                            else
                                builder.Append('T');
                            break;

                        case 'G':
                            // G = 'F' if -GH and not B--GH, D--GH, -H--GH, -H---GH
                            // Else dropped if -GNED, -GN, -DGE-, -DGI-, -DGY-
                            // Else 'J' if -GE-, -GI-, -GY- and not GG
                            // Else K
                            if ((Peek(1, _pos, ref _text) != 'H' || IsOneOf(Peek(2, _pos, ref _text), Vowels)) && (Peek(1, _pos, ref _text) != 'N' || (Peek(1, _pos, ref _text) != NullChar && (Peek(2, _pos, ref _text) != 'E' || Peek(3, _pos, ref _text) != 'D'))) && (Peek(-1, _pos, ref _text) != 'D' || !IsOneOf(Peek(1, _pos, ref _text), "EIY"))) {
                                if (IsOneOf(Peek(1, _pos, ref _text), "EIY") && Peek(2, _pos, ref _text) != 'G')
                                    builder.Append('J');
                                else
                                    builder.Append('K');
                            }
                            // Eat GH
                            if (Peek(1, _pos, ref _text) == 'H')
                                charsConsumed++;
                            break;
                        case 'H':
                            // H = 'H' if before or not after vowel
                            if (!IsOneOf(Peek(-1, _pos, ref _text), Vowels) || IsOneOf(Peek(1, _pos, ref _text), Vowels))
                                builder.Append('H');
                            break;
                        case 'K':
                            // K = 'C' if not CK
                            if (Peek(-1, _pos, ref _text) != 'C')
                                builder.Append('K');
                            break;
                        case 'P':
                            // P = 'F' if PH
                            // Else 'P'
                            if (Peek(1, _pos, ref _text) == 'H') {
                                builder.Append('F');
                                charsConsumed++;    // Eat 'PH'
                            }
                            else {
                                builder.Append('P');
                            }
                            break;
                        case 'Q':
                            // Q = 'K'
                            builder.Append('K');
                            break;
                        case 'S':
                            // S = 'X' if SH, SIO or SIA
                            // Else 'S'
                            if (Peek(1, _pos, ref _text) == 'H') {
                                builder.Append('X');
                                charsConsumed++;    // Eat 'SH'
                            }
                            else if (Peek(1, _pos, ref _text) == 'I' && IsOneOf(Peek(2, _pos, ref _text), "AO")) {
                                builder.Append('X');
                            }
                            else {
                                builder.Append('S');
                            }
                            break;
                        case 'T':
                            // T = 'X' if TIO or TIA
                            // Else '0' if TH
                            // Else 'T' if not TCH
                            if (Peek(1, _pos, ref _text) == 'I' && IsOneOf(Peek(2, _pos, ref _text), "AO")) {
                                builder.Append('X');
                            }
                            else if (Peek(1, _pos, ref _text) == 'H') {
                                builder.Append('0');
                                charsConsumed++;    // Eat 'TH'
                            }
                            else if (Peek(1, _pos, ref _text) != 'C' || Peek(2, _pos, ref _text) != 'H') {
                                builder.Append('T');
                            }
                            break;
                        case 'V':
                            // V = 'F'
                            builder.Append('F');
                            break;
                        case 'W':
                        case 'Y':
                            // W,Y = Keep if not followed by vowel
                            if (IsOneOf(Peek(1, _pos, ref _text), Vowels))
                                builder.Append(c);
                            break;
                        case 'X':
                            // X = 'S' if first character (already done)
                            // Else 'KS'
                            builder.Append("KS");
                            break;
                        case 'Z':
                            // Z = 'S'
                            builder.Append('S');
                            break;
                    }
                    // Advance over consumed characters
                    _pos = Math.Min(_pos + charsConsumed, _text.Length);
                }
            }

            // Return result
            return builder.ToString();
        }

        /// <summary>
        /// Match Rating Approach algorithm
        /// </summary>
        /// <param name="text">Text to create caverphone from</param>
        /// <returns>A string token that represents the input text</returns>
        /// <remarks>
        /// Taken from wikipedia http://en.wikipedia.org/wiki/Match_Rating_Approach
        /// </remarks>
        public static string CreateMra(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            StringBuilder builder = new StringBuilder(RemoveNonLetters(text.ToUpper()));
            builder.Replace("A", "", 1, builder.Length - 1);
            builder.Replace("E", "", 1, builder.Length - 1);
            builder.Replace("I", "", 1, builder.Length - 1);
            builder.Replace("O", "", 1, builder.Length - 1);
            builder.Replace("U", "", 1, builder.Length - 1);
            builder = new StringBuilder(regex.Replace(builder.ToString(), string.Empty));
            char littleChar;
            char bigChar;
            for (int i = 97; i <= 122; i++) {
                littleChar = (char)i;
                bigChar = (char)(i - 32);
                builder.Replace(new string(new char[] { littleChar, littleChar }), bigChar.ToString());
            }

            if (builder.Length < 6)
                return builder.ToString();
            else
                return new string(new char[] { builder[0], builder[1], builder[2], builder[builder.Length - 3], builder[builder.Length - 2], builder[builder.Length - 1] });
        }

        /// <summary>
        /// Soundex algorithm
        /// </summary>
        /// <param name="text">Text to create a soundex from</param>
        /// <returns>A string token that represents the input text</returns>
        public static string CreateSoundEx(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            while (text.Length > 0 && !char.IsLetter(text[0]))
                text = text.Substring(1);
            int textLength = text.Length;

            StringBuilder builder = new StringBuilder();
            if (textLength > 0)
                builder.Append(text[0].ToString().ToUpper());
            else
                builder.Append('N');

            for (int i = 1; i < textLength; i++) {
                if (!char.IsLetter(text[i]))
                    continue;
                switch (text[i]) {
                    case 'b':
                    case 'f':
                    case 'p':
                    case 'v':
                        if (text[i - 1] != '1')
                            builder.Append('1');
                        break;
                    case 'c':
                    case 'g':
                    case 'j':
                    case 'k':
                    case 'q':
                    case 's':
                    case 'x':
                    case 'z':
                        if (text[i - 1] != '2')
                            builder.Append('2');
                        break;
                    case 'd':
                    case 't':
                        if (text[i - 1] != '3')
                            builder.Append('3');
                        break;
                    case 'l':
                        if (text[i - 1] != '4')
                            builder.Append('4');
                        break;
                    case 'm':
                    case 'n':
                        if (text[i - 1] != '5')
                            builder.Append('5');
                        break;
                    case 'r':
                        if (text[i - 1] != '6')
                            builder.Append('6');
                        break;
                }
            }

            if (builder.Length < 4)
                builder.Append(new string('0', 4 - builder.Length));
            if (builder.Length > 4)
                builder.Remove(4, builder.Length - 4);

            return builder.ToString();
        }

        /// <summary>
        /// Determines whether c is one of the specified chars.
        /// </summary>
        /// <param name="c">The char to search for.</param>
        /// <param name="chars">The collection of chars to search against.</param>
        /// <returns>
        ///   <c>true</c> if c [is one of] [the specified chars]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsOneOf(char c, string chars)
        {
            return (chars.IndexOf(c) != -1);
        }

        /// <summary>
        /// Peeks the specified amount ahead in a string.
        /// </summary>
        /// <param name="ahead">The ahead amount to peek ahead.</param>
        /// <param name="_pos">The current position in the text.</param>
        /// <param name="_text">The text being investigated.</param>
        /// <returns>The character at the peeked location</returns>
        private static char Peek(int ahead, int _pos, ref string _text)
        {
            int pos = (_pos + ahead);
            if (pos < 0 || pos >= _text.Length)
                return NullChar;
            return _text[pos];
        }

        /// <summary>
        /// Removes non-letters from a string.
        /// </summary>
        /// <param name="text">The string to remove non-letters from.</param>
        /// <returns>A string that does not contain non-letter characters</returns>
        private static string RemoveNonLetters(string text)
        {
            int textLength = text.Length;
            char c;
            if (textLength > 10) {
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < textLength; i++) {
                    c = text[i];
                    if (char.IsLetter(c))
                        builder.Append(c);
                }

                return builder.ToString();
            }

            string newText = string.Empty;
            for (int i = 0; i < textLength; i++) {
                c = text[i];
                if (char.IsLetter(c))
                    newText += c;
            }
            return newText;
        }

        #endregion Methods
    }
}