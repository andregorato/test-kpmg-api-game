using kpmg_core;
using kpmg_core.Interfaces.Cache;
using kpmg_core.Interfaces.Db;
using kpmg_core.Interfaces.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace kpmg_worker
{
    class Program
    {  
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.json");

            IConfiguration _configuration = builder.Build();

            var _mongoConfig = new MongoConfig();
            var _redisConfig = new RedisConfig();
            var _workerConfig = new WorkerConfig();
            new ConfigureFromConfigurationOptions<MongoConfig>(_configuration.GetSection("MongoConfig")).Configure(_mongoConfig);
            new ConfigureFromConfigurationOptions<RedisConfig>(_configuration.GetSection("RedisConfig")).Configure(_redisConfig);
            new ConfigureFromConfigurationOptions<WorkerConfig>(_configuration.GetSection("WorkerConfig")).Configure(_workerConfig);

            var services = new ServiceCollection();
            services.AddTransient(typeof(IDbClient<Game>), (s => new DbClient(_mongoConfig.Endpoint, _mongoConfig.DatabaseName, _mongoConfig.CollectionName)));
            services.AddTransient(typeof(IGameRepository<Game>), typeof(GameRepository));
            services.AddTransient(typeof(IRedis<Game>), (s => new Redis(_redisConfig.Endpoint, _redisConfig.Port, _redisConfig.Password)));

            var provider = services.BuildServiceProvider();
            var _gameRepository = provider.GetService<IGameRepository<Game>>();
            var _redisClient = provider.GetService<IRedis<Game>>();         
                
            while (true)
            {
                Console.WriteLine("*** Buscando novos registros no cache... ***\n");
                Console.WriteLine("-Início " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));

                var keys = _redisClient.GetAllKeys();
                var wasExecute = false;

                if (keys.Any())
                {
                    var gamesCache = _redisClient.GetAll<Game>(keys);

                    var count = 1;

                    foreach (var gameItem in gamesCache)
                    {
                        Console.WriteLine($"\n-Processando {count}/{gamesCache.Count}.\n");

                        var previousResult = _redisClient.Get<Game>(gameItem.Key);

                        Console.WriteLine("-Inserindo no MongoDB \n");

                        var game = _gameRepository.FindGame(gameItem.Key);

                        if (game == null)
                            _gameRepository.AddGame(previousResult);
                        else
                            _gameRepository.UpdateGame(gameItem.Key, previousResult);
                     
                        Console.WriteLine("-Removendo do Cache \n");
                        _redisClient.Remove(gameItem.Key);
                        count++;
                    }

                    wasExecute = true;
                }

                if (!wasExecute) Console.WriteLine("\n-Não existem games no cache para processar\n");

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
