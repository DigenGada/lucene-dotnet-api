namespace IndexLibrary.FileParsers
{
    using System;
    using System.IO;

    [CLSCompliant(true)]
    public static class PDFReader
    {
        #region Fields

        /// <summary>
        /// The total number of characters to hold in memory at any one time
        /// </summary>
        /// <remarks>
        /// Modifying this number by making it any greater or smaller will have no
        /// performance effect, it is strictly a constant
        /// </remarks>
        private static int _numberOfCharsToKeep = 15;

        #endregion Fields

        #region Methods

        /// <summary>
        /// Reads a PDF file and extracts all text-searchable content from it.
        /// </summary>
        /// <param name="file">The file to extract text from.</param>
        /// <returns>The text from the PDF file</returns>
        public static string ReadFile(string file)
        {
            if (string.IsNullOrEmpty(file)) throw new ArgumentNullException("file", "file cannot be null or empty");
            FileInfo info = new FileInfo(file);
            if (!info.Exists) throw new FileNotFoundException("file must exist");
            if (!info.Extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("File must have a .pdf extension");

            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            iTextSharp.text.pdf.PdfReader reader = null;
            try {
                reader = new iTextSharp.text.pdf.PdfReader(info.FullName);
                for (int i = 1; i <= reader.NumberOfPages; i++) {
                    builder.Append(ExtractTextFromPDFBytes(reader.GetPageContent(i)));
                    builder.Append(' ');
                }
            }
            catch (Exception) {

            }
            finally {
                if (reader != null) reader.Close();
            }

            return builder.ToString();
        }

        /// <summary>
        /// Check if a certain 2 character token just came along (e.g. BT)
        /// </summary>
        /// <param name="search">the searched token</param>
        /// <param name="recent">the recent character array</param>
        /// <returns></returns>
        private static bool CheckToken(string[] tokens, char[] recent)
        {
            foreach (string token in tokens) {
                if ((recent[_numberOfCharsToKeep - 3] == token[0]) &&
                    (recent[_numberOfCharsToKeep - 2] == token[1]) &&
                    ((recent[_numberOfCharsToKeep - 1] == ' ') ||
                    (recent[_numberOfCharsToKeep - 1] == 0x0d) ||
                    (recent[_numberOfCharsToKeep - 1] == 0x0a)) &&
                    ((recent[_numberOfCharsToKeep - 4] == ' ') ||
                    (recent[_numberOfCharsToKeep - 4] == 0x0d) ||
                    (recent[_numberOfCharsToKeep - 4] == 0x0a))
                    ) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// This method processes an uncompressed Adobe (text) object 
        /// and extracts text.
        /// </summary>
        /// <param name="input">uncompressed</param>
        /// <returns></returns>
        private static string ExtractTextFromPDFBytes(byte[] input)
        {
            if (input == null || input.Length == 0) return "";

            try {
                string resultString = "";

                // Flag showing if we are we currently inside a text object
                bool inTextObject = false;

                // Flag showing if the next character is literal
                // e.g. '\\' to get a '\' character or '\(' to get '('
                bool nextLiteral = false;

                // () Bracket nesting level. Text appears inside ()
                int bracketDepth = 0;

                // Keep previous chars to get extract numbers etc.:
                char[] previousCharacters = new char[_numberOfCharsToKeep];
                for (int j = 0; j < _numberOfCharsToKeep; j++) previousCharacters[j] = ' ';

                for (int i = 0; i < input.Length; i++) {
                    char c = (char)input[i];

                    if (inTextObject) {
                        // Position the text
                        if (bracketDepth == 0) {
                            if (CheckToken(new string[] { "TD", "Td" }, previousCharacters)) {
                                resultString += "\n\r";
                            }
                            else {
                                if (CheckToken(new string[] { "'", "T*", "\"" }, previousCharacters)) {
                                    resultString += "\n";
                                }
                                else {
                                    if (CheckToken(new string[] { "Tj" }, previousCharacters)) {
                                        resultString += " ";
                                    }
                                }
                            }
                        }

                        // End of a text object, also go to a new line.
                        if (bracketDepth == 0 &&
                            CheckToken(new string[] { "ET" }, previousCharacters)) {

                            inTextObject = false;
                            resultString += " ";
                        }
                        else {
                            // Start outputting text
                            if ((c == '(') && (bracketDepth == 0) && (!nextLiteral)) {
                                bracketDepth = 1;
                            }
                            else {
                                // Stop outputting text
                                if ((c == ')') && (bracketDepth == 1) && (!nextLiteral)) {
                                    bracketDepth = 0;
                                }
                                else {
                                    // Just a normal text character:
                                    if (bracketDepth == 1) {
                                        // Only print out next character no matter what.
                                        // Do not interpret.
                                        if (c == '\\' && !nextLiteral) {
                                            nextLiteral = true;
                                        }
                                        else {
                                            if (((c >= ' ') && (c <= '~')) ||
                                                ((c >= 128) && (c < 255))) {
                                                resultString += c.ToString();
                                            }

                                            nextLiteral = false;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Store the recent characters for
                    // when we have to go back for a checking
                    for (int j = 0; j < _numberOfCharsToKeep - 1; j++) {
                        previousCharacters[j] = previousCharacters[j + 1];
                    }
                    previousCharacters[_numberOfCharsToKeep - 1] = c;

                    // Start of a text object
                    if (!inTextObject && CheckToken(new string[] { "BT" }, previousCharacters)) {
                        inTextObject = true;
                    }
                }
                return resultString;
            }
            catch {
                return "";
            }
        }

        #endregion Methods
    }
}