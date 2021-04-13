using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using ServiceStack.Redis;
using System;
using System.Linq;

namespace kpmg_worker
{
    public class Game
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }
        public long PlayerId { get; set; }
        public long GameId { get; set; }
        public long Win { get; set; }
        public DateTime Timestamp { get; set; }
        public string Key { get; private set; }

        public Game()
        {
            Timestamp = DateTime.UtcNow;
        }

        public void SetKey()
        {
            Key = PlayerId.ToString() + ":" + GameId.ToString();
        }
    }

    public interface IDbClient
    {
        IMongoCollection<Game> GetGameCollection();
    }

    public class GameDbConfig
    {
        public string Database_Name { get; set; }
        public string Books_Collection_Name { get; set; }
        public string Connection_String { get; set; }
    }

    public class DbClient : IDbClient
    {
        private readonly IMongoCollection<Game> _games;

        public DbClient(IOptions<GameDbConfig> gameDbConfig)
        {
            var client = new MongoClient(gameDbConfig.Value.Connection_String);
            var database = client.GetDatabase(gameDbConfig.Value.Database_Name);
            _games = database.GetCollection<Game>(gameDbConfig.Value.Books_Collection_Name);
        }

        public IMongoCollection<Game> GetGameCollection() => _games;
    }


    class Program
    {
        private readonly IMongoCollection<Game> _games;

        static void Main(string[] args)
        {

            RedisEndpoint redisEndpoint = new RedisEndpoint("redis-17278.c263.us-east-1-2.ec2.cloud.redislabs.com", 17278, "9GrKGvUVbs7Z6eCX1n3ftDbbH6WNM7TF");

            using var redisClient = new RedisClient(redisEndpoint);


            while (true)
            {
                Console.WriteLine("*** Buscando novos registros no cache... ***\n");
                Console.WriteLine("-Início " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));

                var keys = redisClient.GetAllKeys();
                var executou = false;

                if (keys.Any())
                {
                    var client = new MongoClient("mongodb+srv://admin:QpK2uGBYjYD9gMEp@cluster0.qx3ic.mongodb.net/GameDb?retryWrites=true&w=majority");
                    var database = client.GetDatabase("GameDb");
                    var collection = database.GetCollection<Game>("Games");

                    var gamesTemporary = redisClient.GetAll<Game>(keys);

                    var count = 1;

                    foreach (var gameTemp in gamesTemporary)
                    {

                        Console.WriteLine($"\n-Processando {count}/{gamesTemporary.Count}.\n");

                        var previousResult = redisClient.Get<Game>(gameTemp.Key);

                        Console.WriteLine("-Inserindo no MongoDB \n");

                        var game = collection.Find(x => x.Key.Equals(gameTemp.Key)).FirstOrDefault();

                        if (game == null)
                            collection.InsertOne(previousResult);
                        else
                        {
                            var update = Builders<Game>.Update
                                  .Set(p => p.Timestamp, previousResult.Timestamp)
                                  .Inc(p => p.Win, previousResult.Win);

                            collection.UpdateOne(x => x.Key.Equals(gameTemp.Key), update);
                        }

                        Console.WriteLine("-Removendo do Cache \n");
                        redisClient.Remove(gameTemp.Key);
                        count++;
                    }

                    executou = true;
                }

                if (!executou) Console.WriteLine("\n-Não existem games no cache para processar\n");

                var proximaExecucao = DateTime.Now;
                proximaExecucao = proximaExecucao.AddMilliseconds(1000 * 60 * 1);

                Console.WriteLine("-Fim " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));

                Console.WriteLine($"\n*** Próxima execução {proximaExecucao.ToString("MM/dd/yyyy HH:mm:ss")} ***\n\n");

                System.Threading.Thread.Sleep(1000 * 60 * 1); // 1 minuto

                Console.WriteLine("*********************************************\n");
            }
        }
    }
}
