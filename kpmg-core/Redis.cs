using kpmg_core.Interfaces.Cache;
using ServiceStack.Redis;
using System.Collections.Generic;

namespace kpmg_core
{
    public class Redis : IRedis<Game>
    {
        private readonly RedisEndpoint _redisEndpoint;
        
        public Redis(string endpoint, int port, string password)
        {
            _redisEndpoint = new RedisEndpoint(endpoint, port, password);
        }

        public Game Get<Game>(string key)
        {
            using var redisClient = new RedisClient(_redisEndpoint);

           return  redisClient.Get<Game>(key);
        }

        public IDictionary<string, Game> GetAll<Game>(IEnumerable<string> keys)
        {
            using var redisClient = new RedisClient(_redisEndpoint);
            return redisClient.GetAll<Game>(keys);
        }

        public List<string> GetAllKeys()
        {
            using var redisClient = new RedisClient(_redisEndpoint);
            return redisClient.GetAllKeys();
        }

        public bool Remove(string key)
        {
            using var redisClient = new RedisClient(_redisEndpoint);
           return redisClient.Remove(key);
        }
    }
}
