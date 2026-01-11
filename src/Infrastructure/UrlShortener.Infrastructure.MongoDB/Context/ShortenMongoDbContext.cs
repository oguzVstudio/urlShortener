using MongoDB.Driver;
using UrlShortener.Domain;

namespace UrlShortener.Infrastructure.MongoDB.Context;

public class ShortenMongoDbContext : IShortenBaseDbContext
{
    public IMongoDatabase Database { get; }
    public IMongoClient MongoClient { get; }
    
    public ShortenMongoDbContext(MongoDbOptions options)
    {
        MongoClient = new MongoClient(options.ConnectionString);
        var databaseName = options.DatabaseName;
        Database = MongoClient.GetDatabase(databaseName);
    }
    
    public IMongoCollection<T> GetCollection<T>(string? name = null)
    {
        return Database.GetCollection<T>(name ?? typeof(T).Name.ToLower());
    }
}