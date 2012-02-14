#region Header

//
// DocxToText.cs
// Copyright (C) 2007  Eugene Pankov
//

#endregion Header

namespace IndexLibrary.FileParsers
{
    using System;
    using System.IO;
    using System.Text;
    using System.Xml;

    using ICSharpCode.SharpZipLib.Zip;

    /// <summary>
    /// Class made to extract text from docx files
    /// </summary>
    /// <remarks>
    /// Created by Eugene Pankov Copyright (c) 2007 for public use
    /// </remarks>
    internal class DocxToText
    {
        #region Fields

        /// <summary>
        /// The document body xpath
        /// </summary>
        private const string BodyXPath = "/w:document/w:body";

        /// <summary>
        /// The content type namespace
        /// </summary>
        private const string ContentTypeNamespace = @"http://schemas.openxmlformats.org/package/2006/content-types";

        /// <summary>
        /// The document xml xpath
        /// </summary>
        private const string DocumentXmlXPath = "/t:Types/t:Override[@ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml\"]";

        /// <summary>
        /// The word processing ml namespace
        /// </summary>
        private const string WordprocessingMlNamespace = @"http://schemas.openxmlformats.org/wordprocessingml/2006/main";

        /// <summary>
        /// The docx file provided by the user
        /// </summary>
        private string docxFile = "";

        /// <summary>
        /// The full location to the docx file provided by the user
        /// </summary>
        private string docxFileLocation = "";

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DocxToText"/> class.
        /// </summary>
        /// <param name="fileName">Full path to the file to read from.</param>
        public DocxToText(string fileName)
        {
            docxFile = fileName;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Extracts text from the Docx file.
        /// </summary>
        /// <returns>Extracted text.</returns>
        public string ExtractText()
        {
            if (string.IsNullOrEmpty(docxFile))
                throw new Exception("Input file not specified.");

            // Usually it is "/word/document.xml"

            docxFileLocation = FindDocumentXmlLocation();

            if (string.IsNullOrEmpty(docxFileLocation))
                throw new Exception("It is not a valid Docx file.");

            return ReadDocumentXml();
        }

        /// <summary>
        /// Gets location of the "document.xml" zip entry.
        /// </summary>
        /// <returns>Location of the "document.xml".</returns>
        private string FindDocumentXmlLocation()
        {
            ZipFile zip = new ZipFile(docxFile);
            foreach (ZipEntry entry in zip) {
                // Find "[Content_Types].xml" zip entry

                if (string.Compare(entry.Name, "[Content_Types].xml", true) == 0) {
                    Stream contentTypes = zip.GetInputStream(entry);

                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.PreserveWhitespace = true;
                    xmlDoc.Load(contentTypes);
                    contentTypes.Close();

                    //Create an XmlNamespaceManager for resolving namespaces

                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
                    nsmgr.AddNamespace("t", ContentTypeNamespace);

                    // Find location of "document.xml"

                    XmlNode node = xmlDoc.DocumentElement.SelectSingleNode(DocumentXmlXPath, nsmgr);

                    if (node != null) {
                        string location = ((XmlElement)node).GetAttribute("PartName");
                        return location.TrimStart(new char[] { '/' });
                    }
                    break;
                }
            }
            zip.Close();
            return null;
        }

        /// <summary>
        /// Reads "document.xml" zip entry.
        /// </summary>
        /// <returns>Text containing in the document.</returns>
        private string ReadDocumentXml()
        {
            StringBuilder sb = new StringBuilder();

            ZipFile zip = new ZipFile(docxFile);
            foreach (ZipEntry entry in zip) {
                if (string.Compare(entry.Name, docxFileLocation, true) == 0) {
                    Stream documentXml = zip.GetInputStream(entry);

                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.PreserveWhitespace = true;
                    xmlDoc.Load(documentXml);
                    documentXml.Close();

                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
                    nsmgr.AddNamespace("w", WordprocessingMlNamespace);

                    XmlNode node = xmlDoc.DocumentElement.SelectSingleNode(BodyXPath, nsmgr);

                    if (node == null)
                        return string.Empty;

                    sb.Append(ReadNode(node));

                    break;
                }
            }
            zip.Close();
            return sb.ToString();
        }

        /// <summary>
        /// Reads content of the node and its nested childs.
        /// </summary>
        /// <param name="node">XmlNode.</param>
        /// <returns>Text containing in the node.</returns>
        private string ReadNode(XmlNode node)
        {
            if (node == null || node.NodeType != XmlNodeType.Element)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            foreach (XmlNode child in node.ChildNodes) {
                if (child.NodeType != XmlNodeType.Element) continue;

                switch (child.LocalName) {
                    case "t":                           // Text
                        sb.Append(child.InnerText.TrimEnd());

                        string space = ((XmlElement)child).GetAttribute("xml:space");
                        if (!string.IsNullOrEmpty(space) && space == "preserve")
                            sb.Append(' ');

                        break;

                    case "cr":                          // Carriage return
                    case "br":                          // Page break
                        sb.Append(Environment.NewLine);
                        break;

                    case "tab":                         // Tab
                        sb.Append("\t");
                        break;

                    case "p":                           // Paragraph
                        sb.Append(ReadNode(child));
                        sb.Append(Environment.NewLine);
                        sb.Append(Environment.NewLine);
                        break;

                    default:
                        sb.Append(ReadNode(child));
                        break;
                }
            }
            return sb.ToString();
        }

        #endregion Methods
    }
}