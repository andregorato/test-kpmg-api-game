using kpmg_core;
using kpmg_core.Interfaces.Cache;
using kpmg_core.Interfaces.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace kpmg_api_game.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly IGameRepository<Game> _gameRepository;
        private readonly IRedis<Game> _redis;

        public GameController(IGameRepository<Game> gameRepository, IRedis<Game> redis)
        {
            _gameRepository = gameRepository;
            _redis = redis;
        }


        [HttpGet]
        public IActionResult Get()
        {
            var gamesList = _gameRepository.GetRanking();

            var result = from x in gamesList
                         select new
                         {
                             x.PlayerId,
                             Balance = x.Win,
                             lastUpdateDate = x.Timestamp
                         };

            return new JsonResult(result);
        }

        [HttpPost]
        public IActionResult Post([FromBody] Game game)
        {
            game.SetKey();

            var previousResult = _redis.Get<Game>(game.Key);
            var hasPreviousResult = previousResult != null;

            if (hasPreviousResult)
            {
                previousResult.Win += game.Win;
                previousResult.Timestamp = game.Timestamp;
            }
            else
                previousResult = game;

            _redis.Set(game.Key, previousResult);

            var gameX = _gameRepository.FindGame(game.Key);

            if (gameX != null)
                previousResult.Win += gameX.Win;

            return new JsonResult(new { TotalBalance = previousResult.Win });
        }
    }
}
