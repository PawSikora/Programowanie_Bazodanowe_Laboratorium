using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL_MongoDb.MongoModels
{
    public class MongoOrder
    {
        [BsonId]
        public int Id { get; set; }

        [BsonElement("userId")]
        public int UserId { get; set; }

        [BsonElement("date")]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [BsonElement("isPaid")]
        public bool IsPaid { get; set; } = false;

        [BsonElement("orderPositions")]
        public List<MongoOrderItem> OrderPositions { get; set; } = new();
    }
}
