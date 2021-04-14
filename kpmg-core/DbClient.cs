using kpmg_core.Interfaces.Db;
using MongoDB.Driver;

namespace kpmg_core
{
    public class DbClient : IDbClient<Game>
    {
        private readonly IMongoCollection<Game> _games;

        public DbClient(string endpoint, string databaseName, string collectionName)
        {
            var client = new MongoClient(endpoint);
            var database = client.GetDatabase(databaseName);
            _games = database.GetCollection<Game>(collectionName);
        }

        public IMongoCollection<Game> GetCollection() => _games;
    }
}
