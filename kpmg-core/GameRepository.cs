using kpmg_core.Interfaces.Db;
using kpmg_core.Interfaces.Repository;
using MongoDB.Driver;
using System.Collections.Generic;

namespace kpmg_core
{
    public class GameRepository : IGameRepository<Game>
    {
        private readonly IMongoCollection<Game> _gameCollection;

        public GameRepository(IDbClient<Game> dbClient)
        {
            _gameCollection = dbClient.GetCollection();
        }

        public void AddGame(Game game)
        {
            _gameCollection.InsertOne(game);
        }

        public Game FindGame(string key)
        {
            return _gameCollection.Find(x => x.Key.Equals(key)).FirstOrDefault();
        }

        public List<Game> GetRanking()
        {
            return _gameCollection.Find(game => true).SortByDescending(x => x.Win).Limit(100).ToList();
        }

        public long UpdateGame(string key, Game game)
        {
            var update = Builders<Game>.Update
                                  .Set(p => p.Timestamp, game.Timestamp)
                                  .Inc(p => p.Win, game.Win);

            var result = _gameCollection.UpdateOne(x => x.Key.Equals(key), update);

            return result.ModifiedCount;
        }
    }
}
