using MongoDB.Driver;

namespace kpmg_core.Interfaces.Db
{
    public interface IDbClient<T>
    {
        IMongoCollection<T> GetCollection();
    }
}
