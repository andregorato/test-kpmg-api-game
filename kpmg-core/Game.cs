using MongoDB.Bson.Serialization.Attributes;
using System;

namespace kpmg_core
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
}
