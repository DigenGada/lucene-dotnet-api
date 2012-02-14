namespace IndexLibrary.FileParsers
{
    using System;
    using System.IO;

    using GetDocText.Doc;

    /// <summary>
    /// Class for reading all data out of .doc files
    /// </summary>
    [CLSCompliant(true)]
    public static class DocReader
    {
        #region Methods

        /// <summary>
        /// Reads all text out of a .doc file.
        /// </summary>
        /// <param name="file">The file to read all text out of.</param>
        /// <returns>A string that contains all the text from the specified file</returns>
        public static string ReadFile(string file)
        {
            if (string.IsNullOrEmpty(file)) throw new ArgumentNullException("file", "file cannot be null or empty");
            FileInfo info = new FileInfo(file);
            if (!info.Exists) throw new FileNotFoundException("file must exist");
            if (!info.Extension.Equals(".doc", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("File must have a .doc extension");

            string text;
            TextLoader loader = new TextLoader(info.FullName);
            loader.LoadText(out text);
            return text;
        }

        #endregion Methods
    }

    /// <summary>
    /// Class for reading all data ouf of .docx files
    /// </summary>
    [CLSCompliant(true)]
    public static class DocXReader
    {
        #region Methods

        /// <summary>
        /// Reads all text out of a .docx file.
        /// </summary>
        /// <param name="file">The file to read all text out of.</param>
        /// <returns>A string that contains all the text from the specified file</returns>
        public static string ReadFile(string file)
        {
            if (string.IsNullOrEmpty(file)) throw new ArgumentNullException("file", "file cannot be null or empty");
            FileInfo info = new FileInfo(file);
            if (!info.Exists) throw new FileNotFoundException("file must exist");
            if (!info.Extension.Equals(".docx", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("File must have a .doc extension");

            DocxToText extractor = new DocxToText(file);
            return extractor.ExtractText();
        }

        #endregion Methods
    }

    /// <summary>
    /// Class for reading all data out of text files
    /// </summary>
    [CLSCompliant(true)]
    public static class TextFileReader
    {
        #region Methods

        /// <summary>
        /// Reads all text out of a text file.
        /// </summary>
        /// <param name="file">The file to read all text out of.</param>
        /// <returns>A string that contains all the text from the specified file</returns>
        public static string ReadFile(string file)
        {
            if (string.IsNullOrEmpty(file)) throw new ArgumentNullException("file", "file cannot be null or empty");
            FileInfo info = new FileInfo(file);
            if (!info.Exists) throw new FileNotFoundException("file must exist");

            return File.ReadAllText(file);
        }

        #endregion Methods
    }
}