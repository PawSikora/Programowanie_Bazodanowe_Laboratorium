using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL_MongoDb.MongoModels
{
    public class MongoUser
    {
        [BsonId]
        public int Id { get; set; }

        [BsonElement("login")]
        public string Login { get; set; }

        [BsonElement("password")]
        public string Password { get; set; }

        [BsonElement("type")]
        public UserType Type { get; set; } = UserType.Casual;

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("groupId")]
        public int? GroupId { get; set; }
    }

    public enum UserType
    {
        Admin,
        Casual
    }
}
