using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL_MongoDb.MongoModels
{
    public class MongoBasketItem
    {
        [BsonId]
        public int Id { get; set; }

        [BsonElement("productId")]
        public int ProductId { get; set; }

        [BsonElement("userId")]
        public int UserId { get; set; }

        [BsonElement("amount")]
        public int Amount { get; set; }
    }
}
