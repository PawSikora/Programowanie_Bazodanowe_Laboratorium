using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL_MongoDb.MongoModels
{
    public class MongoOrderItem
    {
        [BsonElement("productId")]
        public int ProductId { get; set; }

        [BsonElement("productName")]
        public string ProductName { get; set; }

        [BsonElement("price")]
        public double Price { get; set; }

        [BsonElement("amount")]
        public int Amount { get; set; }
    }
}
