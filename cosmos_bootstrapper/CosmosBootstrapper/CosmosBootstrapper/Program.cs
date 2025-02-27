using CosmosBootstrapper;
using MongoDB.Bson;
using MongoDB.Driver;

const string cosmosEmulatorConnectionString = "mongodb://cosmos:C2y6yDjf5%2FR%2Bob0N8A7Cgv30VRDJIWEHLM%2B4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw%2FJw%3D%3D@localhost:10255/admin?ssl=true&retrywrites=false&tlsInsecure=true";
const string mongoDbLocalConnectionString = "mongodb://localhost:27017/?retryWrites=false&serverSelectionTimeoutMS=5000&connectTimeoutMS=10000";
var mongoConfig = MongoClientSettings.FromConnectionString(mongoDbLocalConnectionString);
var mongoClient = new MongoClient(mongoConfig);

var collections = new Dictionary<string, IMongoCollection<BsonDocument>>();
var config = new BootstrapperConfiguration();
foreach (var map in config.DatabaseToCollectionMap)
{
    var database = mongoClient.GetDatabase(map.Key);
    foreach (var collectionName in map.Value)
    {
        if(!(await database.ListCollectionNamesAsync()).ToList().Contains(collectionName))
            await database.CreateCollectionAsync(collectionName);
        collections.Add(collectionName, database.GetCollection<BsonDocument>(collectionName));
    }
}

foreach (var map in config.CollectionToDocumentMap)
{
    var collection = collections[map.Key];
    foreach (var document in map.Value)
    {
        var bsonDocument = BsonDocument.Parse(document);
        await collection.InsertOneAsync(bsonDocument);
    }
}