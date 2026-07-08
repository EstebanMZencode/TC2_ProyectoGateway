using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace MicroServicioCatalogo.Repository
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly IMongoDatabase _database;

        public DbConnectionFactory(IOptions<MongoDbSettings> settings)
        {
            var mongoClient = new MongoClient(settings.Value.ConnectionString);
            _database = mongoClient.GetDatabase(settings.Value.DatabaseName);
        }

        public IMongoCollection<TEntity> GetCollection<TEntity>(string collectionName)
        {
            return _database.GetCollection<TEntity>(collectionName);
        }
    }
}
