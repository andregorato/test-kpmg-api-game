using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using ServiceStack.Redis;
using System;
using System.IO;
using System.Linq;
using System.Threading;

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
        private static IConfiguration _configuration;






        private readonly IMongoCollection<Game> _games;

        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.json");

            _configuration = builder.Build();

            var _mongoConfig = new MongoConfig();
            var _redisConfig = new RedisConfig();
            var _workerConfig = new WorkerConfig();
            new ConfigureFromConfigurationOptions<MongoConfig>(_configuration.GetSection("MongoConfig")).Configure(_mongoConfig);
            new ConfigureFromConfigurationOptions<RedisConfig>(_configuration.GetSection("RedisConfig")).Configure(_redisConfig);
            new ConfigureFromConfigurationOptions<WorkerConfig>(_configuration.GetSection("WorkerConfig")).Configure(_workerConfig);

            RedisEndpoint redisEndpoint = new RedisEndpoint(_redisConfig.Endpoint, _redisConfig.Port, _redisConfig.Password);

            using var redisClient = new RedisClient(redisEndpoint);

            while (true)
            {
                Console.WriteLine("*** Buscando novos registros no cache... ***\n");
                Console.WriteLine("-Início " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));

                var keys = redisClient.GetAllKeys();
                var executou = false;

                if (keys.Any())
                {
                    var client = new MongoClient(_mongoConfig.Endpoint);
                    var database = client.GetDatabase(_mongoConfig.DatabaseName);
                    var collection = database.GetCollection<Game>(_mongoConfig.CollectionName);

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
                proximaExecucao = proximaExecucao.AddMilliseconds(_workerConfig.ScheduleInMilliseconds);

                Console.WriteLine("-Fim " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));

                Console.WriteLine($"\n*** Próxima execução {proximaExecucao.ToString("MM/dd/yyyy HH:mm:ss")} ***\n\n");

                Thread.Sleep((int)_workerConfig.ScheduleInMilliseconds);

                Console.WriteLine("*********************************************\n");
            }
        }
    }
}
