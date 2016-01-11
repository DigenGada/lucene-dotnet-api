## Overview ##
True to its core mission, consuming The Hammer (codename for this project) is a breeze. The following page details several common scenarios and how to handle them.


### Scenario 1: Searching A Single Index ###
Practically the most common scenario. This particular case involves opening an index named Forms and searching for all forms submitted by a particular user within a given year.

Within the code:
  * First we create the index, this call is synonymous to _Index.Create(new DirectoryInfo("C:\\Code\\Indexes\\Forms"))_ which is create OR LOAD an index
  * Then we create a searcher for that index
  * Now we build the query to run against the index
  * And finally, we feed the query and the number of results we want back to the searcher to get results

```
     IIndex index = IndexHelper.LoadIndex(new DirectoryInfo("C:\\Code\\Indexes\\Forms"));

     IndexSearcher searcher = index.GetSearcher();

     QueryBuilder query = new QueryBuilder();
     query.AddBooleanClause(new SearchTerm("UserID", "15"), ClauseOccurrence.MustOccur);
     query.AddBooleanClause(new SearchTerm("Year", "2005"), ClauseOccurrence.MustOccur);

     IEnumerable<SearchResult> results = searcher.Search(query, totalResultsRequested: 15);
```


### Scenario 2: Writing To An Index ###
Another extremely common scenario. This example case involves creating a new index and writing several Forms objects to it


Within the code:
  * First we create the index, this call is synonymous to _Index.Create(new DirectoryInfo("C:\\Code\\Indexes\\Forms"))_ which is create OR LOAD an index
  * We get all of the forms we need to index from some arbitrary method
  * Then we create an index writer for that index. We optionally supply an analyzer type and a boolean indicating whether we should append to an existing index or overwrite it
  * We iterate each form, create a document, and write that document to the writer. **Note:** There are a couple of dozen different write methods that allow you to more easily write data; one, for instance, uses reflection to index all of the properties which would practically have the same result as below.
  * Finally, before the index writer is disposed we optimize the index

```
     IIndex index = IndexHelper.LoadIndex(new DirectoryInfo("C:\\Code\\Indexes\\Forms"));
           
     Form[] forms = GetForms();

     using (IndexWriter writer = index.GetWriter(AnalyzerType.Standard, true)) {

         foreach (Form form in forms) {
             IndexDocument document = new IndexDocument();
             document.Add(new FieldNormal("ID", form.ID.ToString(), storeValueInIndex: true, indexMethod: FieldSearchableRule.Analyzed, termVector: FieldVectorRule.No));
             document.Add(new FieldNormal("FirstName", form.FirstName, true, FieldSearchableRule.No));
             document.Add(new FieldNormal("LastName", form.LastName, true, FieldSearchableRule.No));
             document.Add(new FieldNormal("DateSubmitted", form.DateSubmitted.ToString("g")));
             document.Add(new FieldNormal("ExpectedSubmissionDate", form.ExpectedSubmissionDate.ToString("g")));
             writer.Write(document);
         }

         writer.Optimize();
     }
```

Just for reference, here's the dummy Form class
```
    public sealed class Form {

        public int ID { get; set; }
        public DateTime DateSubmitted { get; set; }
        public DateTime ExpectedSubmissionDate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

    }
```


### Scenario 3: Reading From An Index ###
The final common scenario, reading directly from the index. The reader allows you to read the top N documents, a single document by its document id, or all documents.

Within the code:
  * First we create the index, this call is synonymous to _Index.Create(new DirectoryInfo("C:\\Code\\Indexes\\Forms"))_ which is create OR LOAD an index
  * We the get the reader from the index in a using statement. The reader optionally takes a bool so you can open it in readonly mode.
  * Finally, we grab the first 15 rows that are contained within the index and dispose of the reader

```
    IIndex index = IndexHelper.LoadIndex(new DirectoryInfo("C:\\Code\\Indexes\\Forms"));

    using (IndexReader reader = index.GetReader()) {
        reader.ReadDocuments(topNResults: 15);
    }
```