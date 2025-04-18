using BLL.DTOModels.UserDTOs;
using BLL.ServiceInterfaces;
using BLL_MongoDb.MongoModels;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL_MongoDb.Services
{
    public class MongoUserService : IUserService
    {
        private readonly IMongoCollection<MongoUser> _users;
        private static string? _sessionLogin = null;

        public MongoUserService(IMongoDatabase database)
        {
            _users = database.GetCollection<MongoUser>("users");
        }

        public async Task<bool> Login(UserLoginRequestDTO dto)
        {
            var filter = Builders<MongoUser>.Filter.Eq(u => u.Login, dto.Login) &
                         Builders<MongoUser>.Filter.Eq(u => u.Password, dto.Password) &
                         Builders<MongoUser>.Filter.Eq(u => u.IsActive, true);

            var user = await _users.Find(filter).FirstOrDefaultAsync();

            if (user != null)
            {
                _sessionLogin = user.Login;
                return true;
            }

            return false;
        }

        public Task Logout()
        {
            _sessionLogin = null;
            return Task.CompletedTask;
        }
    }
}
