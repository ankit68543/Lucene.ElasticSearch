using Lucene.ElasticSearch;
using System.Text.Json;

var personsFileText = File.ReadAllText("persons.json");
var persons = JsonSerializer.Deserialize<List<Person>>(personsFileText, new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
});

var engine = new PersonSearchEngine();
if (persons == null)
    return;

engine.AddPersonsToIndex(persons);

while (true)
{
    Console.Clear();
    Console.WriteLine("Enter search query :");
    var query = Console.ReadLine();
    if (string.IsNullOrEmpty(query))
    {
        continue;
    }
    var results= engine.Search(query);
    if (!results.Any()) 
    {
        Console.WriteLine("No results found");
    }

    foreach (var person in results)
    {
        Console.WriteLine($"({person.Company}): {person.FirstName} {person.LastName}");
    }

    Console.WriteLine("Press any key to continue");
    Console.ReadKey(); 
}

