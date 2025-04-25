using MongoDB.Bson;
using MongoDB.Driver;

class Program
{
    static async Task Main(string[] args)
    {
        var connectionString = "mongodb://admin:admin123@localhost:27017";
        var client = new MongoClient(connectionString);

        var database = client.GetDatabase("testdb");
        var collection = database.GetCollection<BsonDocument>("testcollection");

        var doc = new BsonDocument
        {
            { "name", "Mateusz" },
            { "age", 25 }
        };

        await collection.InsertOneAsync(doc);
        Console.WriteLine("Dodano dokument!");
    }
}
