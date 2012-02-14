namespace TestConsole {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    using IndexLibrary;
    using IndexLibrary.Analysis;
    using IndexLibrary.Compression;
    using IndexLibrary.Extensions;
    using IndexLibrary.FileParsers;
    using IndexLibrary.Interfaces;
    using IndexLibrary.Web;

    class Program {
        #region Fields

        static string connectionString = @"Persist Security Info=False;database=Marty;server=sql2k8;user id=sa;password=iHKybd!;Current Language=English;Connection Timeout=10;";
        static string indexDirectory = @"C:\Net4\Indexes";
        static string sqlQuery = @"dbo.INDEX_GlobalItems";

        #endregion Fields

        #region Methods

        static string AppendFileName(string currentFileName, string newFileName) {
            if (string.IsNullOrEmpty(currentFileName))
                return newFileName;
            if (currentFileName.Contains(' ')) {
                string docNumber = currentFileName.Substring(0, currentFileName.IndexOf(' ')).Trim();
                if (docNumber.Contains('-')) {
                    newFileName = docNumber + " " + newFileName;
                }
                int index = currentFileName.IndexOf("REV", StringComparison.OrdinalIgnoreCase);
                if (index != -1)
                    newFileName += " " + currentFileName.Substring(index);
            }
            return newFileName;
        }

        static void Main(string[] args) {
            IIndex cardsIndex = Index.Create(new DirectoryInfo(@"C:\Net4\Crime Stats\Indexes\Cards"));
            IIndex formsIndex = Index.Create(new DirectoryInfo(@"C:\Net4\Crime Stats\Indexes\Forms"));
            IIndex monthlyFormsIndex = Index.Create(new DirectoryInfo(@"C:\Net4\Crime Stats\Indexes\MonthlyForms"));

            var cardsSearcher = cardsIndex.GetSearcher();
            var formsSearcher = formsIndex.GetSearcher();
            var monthlySearcher = monthlyFormsIndex.GetSearcher();

            QueryBuilder formBuilder = new QueryBuilder();
            formBuilder.AddStringQuery("+(Year:2005 Year:2006 Year:2007 Year:2008 Year:2009) +(NumericStateCode:28)", ClauseOccurrence.MustOccur, AnalyzerType.Standard, false);

            foreach (var formResult in formsSearcher.Search(formBuilder)) {
                
            }

            DateTime startTime = DateTime.Now;

            //Random random = new Random();
            //double requestNumber = ((random.NextDouble() + 0.0000000001) * 1000000000) % 9999999999;
            //double cookieNumber = ((random.NextDouble() + 0.0000001) * 10000000) % 99999999;
            //double randomNumber = ((random.NextDouble() + 0.0000000001) * 1000000000) % 2147483647;

            //GoogleAnalyticsPageTrackSubmitter submitter = new GoogleAnalyticsPageTrackSubmitter(GoogleAnalyticsRequestType.Page);
            //submitter.AccountString.Value = "UA-27532234-1";
            //submitter.ClientBrowserLanguageEncoding.Value = "UTF-8";
            //submitter.PageTitle.Value = "ILB";
            //submitter.HostName.Value = "clients.theatomgroup.com";
            //submitter.ClientBrowserJavaEnabled.Value = true;
            //submitter.RelativeUrl.Value = @"/fakePage/index.html";
            //submitter.CookieValues.Value = @"__utma=247248150.2078399374.1323295963.1323369219.1323373536.3;+__utmz=247248150.1323295963.1.1.utmcsr=(direct)|utmccn=(direct)|utmcmd=(none);";
            //submitter.ClientBrowserCultureCode.Value = "en-us";
            //submitter.TrackingCodeVersion.Value = "5.1.6";
            //submitter.RequestID.Value = (ulong)requestNumber;
            //submitter.AdsenseID.Value = (ulong)cookieNumber;
            //submitter.Post();

            return;

            Console.WriteLine();
            Console.WriteLine("Completed in {0} seconds", DateTime.Now.Subtract(startTime).TotalSeconds);
            Console.Write("Press any key to exit...");
            Console.ReadKey(true);

            AnalysisWriter.Instance.Dispose();
        }

        private static void Print(IEnumerable<SearchResult> results) {
            foreach (var result in results) {
                Console.WriteLine(result.GetValue("nodealiaspath"));
            }
        }

        #endregion Methods
    }
}