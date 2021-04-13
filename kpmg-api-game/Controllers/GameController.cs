using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using ServiceStack.Redis;
using System;
using System.Linq;

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





namespace kpmg_api_game.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        // GET: api/<GameController>
        [HttpGet]
        public IActionResult Get()
        {
            var client = new MongoClient("mongodb+srv://admin:QpK2uGBYjYD9gMEp@cluster0.qx3ic.mongodb.net/GameDb?retryWrites=true&w=majority");
            var database = client.GetDatabase("GameDb");
            var collection = database.GetCollection<Game>("Games");

            var gamesList = collection.Find(game => true).SortByDescending(x => x.Win).Limit(100).ToList();


            var result = from x in gamesList
                         select new
                         {
                             x.PlayerId,
                             Balance = x.Win,
                             lastUpdateDate = x.Timestamp
                         };

            return new JsonResult(result);


        }




        // POST api/<GameController>
        [HttpPost]
        public IActionResult Post([FromBody] Game game)
        {
            RedisEndpoint redisEndpoint = new RedisEndpoint("redis-17278.c263.us-east-1-2.ec2.cloud.redislabs.com", 17278, "9GrKGvUVbs7Z6eCX1n3ftDbbH6WNM7TF");

            using var redisClient = new RedisClient(redisEndpoint);

            game.SetKey();

            var previousResult = redisClient.Get<Game>(game.Key);
            var hasPreviousResult = previousResult != null;

            if (hasPreviousResult)
            {
                previousResult.Win += game.Win;
                previousResult.Timestamp = game.Timestamp;
            }
            else
                previousResult = game;

            var result = redisClient.Set(game.Key, previousResult);

            var client = new MongoClient("mongodb+srv://admin:QpK2uGBYjYD9gMEp@cluster0.qx3ic.mongodb.net/GameDb?retryWrites=true&w=majority");
            var database = client.GetDatabase("GameDb");
            var collection = database.GetCollection<Game>("Games");
            var gameX = collection.Find(x => x.Key.Equals(game.Key.ToString())).FirstOrDefault();

            if (gameX != null)
                previousResult.Win += gameX.Win;


            return new JsonResult(new { TotalBalance = previousResult.Win });
        }
    }
}
