using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.DTOModels.UserDTOs;
using BLL.ServiceInterfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace BLL_DB.Services
{
    public class UserServiceDB : IUserService
    {
        private readonly string _connectionString;
        private static string? _userSession = null;

        public UserServiceDB(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<bool> Login(UserLoginRequestDTO dto)
        {
            using var conn = new SqlConnection(_connectionString);
            var success = await conn.ExecuteScalarAsync<int>("LoginUser", new { dto.Login, dto.Password }, commandType: CommandType.StoredProcedure);
            if (success > 0)
            {
                _userSession = dto.Login;
                return true;
            }
            return false;
        }

        public Task Logout()
        {
            _userSession = null;
            return Task.CompletedTask;
        }
    }
}
