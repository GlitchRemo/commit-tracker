using FakeDataLoader.CustomGenerators;
using FakeDataLoader.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FakeDataLoader;

public class MongoWriter
{
    private const short BatchSize = 1000;
    private readonly FakerWrapper _faker;
    private readonly IMongoDatabase _mongoDatabase;
    private IMongoCollection<BsonDocument>? _collection;

    public MongoWriter(string connectionString, string databaseName,List<CustomGeneratorConfig?> customGeneratorConfigs)
    {
        Console.WriteLine($"Connected to Mongo database '{databaseName}'");
        var mongoClient = new MongoClient(connectionString);
        _mongoDatabase = mongoClient.GetDatabase(databaseName);
        _faker = new FakerWrapper(customGeneratorConfigs);
    }

    public async Task EnsureCollectionCreated(string collectionName)
    {
        Console.WriteLine($"Ensuring collection '{collectionName}' exists");
        var collections = await (await _mongoDatabase.ListCollectionNamesAsync()).ToListAsync();
        if (collections.All(c => !c.Equals(collectionName)))
        {
            await _mongoDatabase.CreateCollectionAsync(collectionName);
        }

        _collection = _mongoDatabase.GetCollection<BsonDocument>(collectionName);
    }

    public async Task GenerateRecords(int numberOfRecords, IList<FieldDescriptor> fields)
    {
        if (_collection == null) throw new ApplicationException($"Call {nameof(EnsureCollectionCreated)} first");
        Console.WriteLine($"Creating {numberOfRecords} documents");
        var remaining = numberOfRecords;
        var totalCreated = 0;
        while (remaining > 0)
        {
            var inserts = new List<InsertOneModel<BsonDocument>>();
            for (int i = 0; i < BatchSize; i++)
            {
                inserts.Add(new InsertOneModel<BsonDocument>(CreateDocument(fields)));
            }

            await _collection.BulkWriteAsync(inserts);
            totalCreated += inserts.Count;
            remaining -= BatchSize;
            Console.WriteLine($"Progress: {totalCreated}/{numberOfRecords} documents created");
        }
    }

    private BsonDocument CreateDocument(IList<FieldDescriptor> fields)
    {
        var generatedValues = fields.Select(f =>
            new KeyValuePair<string, object?>(f.Name, _faker.GenerateFieldValue(f.Name,f.Type, f.SampleSize)));
        return new BsonDocument(generatedValues);
    }
}