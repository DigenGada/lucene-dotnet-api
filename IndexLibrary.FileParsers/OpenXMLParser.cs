namespace IndexLibrary.FileParsers
{
    using System;
    using System.IO;
    using System.IO.Packaging;

    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Spreadsheet;
    using DocumentFormat.OpenXml.Wordprocessing;

    public sealed class OpenXMLParser
    {
        #region Methods

        public static void test(string fileName)
        {
            using (WordprocessingDocument document = WordprocessingDocument.Open(fileName, false)) {

            }
        }

        #endregion Methods
    }
}