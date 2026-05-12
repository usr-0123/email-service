using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EmailPlatform.BuildingBlocks.Persistence.Mongo;

public class MongoDbContext
{
    public IMongoClient Client { get; }
    public IMongoDatabase Database { get; }

    public MongoDbContext(IMongoClient client, IOptions<MongoOptions> options)
    {
        Client = client;
        var dbName = options.Value.DatabaseName;
        if (string.IsNullOrWhiteSpace(dbName))
        {
            throw new InvalidOperationException(
                "MongoOptions.DatabaseName is required. Configure the 'Mongo:DatabaseName' setting.");
        }
        Database = client.GetDatabase(dbName);
    }

    public IMongoCollection<T> GetCollection<T>(string name) => Database.GetCollection<T>(name);
}
