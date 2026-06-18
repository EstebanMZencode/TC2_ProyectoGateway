using MongoDB.Driver;

namespace MicroServicioCatalogo.Repository
{
    public interface IDbConnectionFactory
    {
        IMongoCollection<TEntity> GetCollection<TEntity>(string collectionName);
    }
}
