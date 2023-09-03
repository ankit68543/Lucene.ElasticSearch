using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;

namespace Lucene.ElasticSearch
{
    public class PersonSearchEngine
    {
        private const LuceneVersion version = LuceneVersion.LUCENE_48;
        private readonly StandardAnalyzer _analyzer;
        private readonly SimpleFSDirectory _directory;
        private readonly IndexWriter _writer;

        public PersonSearchEngine()
        {

            _analyzer = new StandardAnalyzer(version);
            string indexName = "example_index";
            string indexPath = Path.Combine(Environment.CurrentDirectory, indexName);
            _directory = new SimpleFSDirectory(indexPath);
            //_directory = new RAMDirectory(); Note: We can use the ram as well for storing our Indexes.
            var config = new IndexWriterConfig(version, _analyzer);
            _writer = new IndexWriter(_directory, config);
        }

        public void AddPersonsToIndex(IEnumerable<Person> persons)
        {
            foreach (var person in persons)
            {
                _writer.AddDocument(new Document
                {
                    new StringField(nameof(Person.Id), person.Id.ToString(), Field.Store.YES),
                    new TextField(nameof(Person.FirstName), person.FirstName, Field.Store.YES),
                    new TextField(nameof(Person.LastName), person.LastName, Field.Store.YES),
                    new TextField(nameof(Person.Company), person.Company, Field.Store.YES),
                    new TextField(nameof(Person.Description), person.Description, Field.Store.YES)
                });
            }

            _writer.Commit();
        }

        public IEnumerable<Person> Search(string searchTerm)
        {
            var directoryReader = DirectoryReader.Open(_directory);
            var indexSearcher = new IndexSearcher(directoryReader);
            string[] fields = { nameof(Person.Company), nameof(Person.LastName), nameof(Person.FirstName), nameof(Person.Description) };
            var queryParser = new MultiFieldQueryParser(version, fields, _analyzer);
            var query = queryParser.Parse(searchTerm);
            var hits = indexSearcher.Search(query, 1000).ScoreDocs;
            var persons = new List<Person>();
            foreach (var hit in hits)
            {
                var document = indexSearcher.Doc(hit.Doc);
                persons.Add(new Person
                {
                    Id = new Guid(document.Get(nameof(Person.Id))),
                    FirstName = document.Get(nameof(Person.FirstName)),
                    LastName = document.Get(nameof(Person.LastName)),
                    Company = document.Get(nameof(Person.Company)),
                    Description = document.Get(nameof(Person.Description)),
                });
            }
            return persons;
        }


    }
}
