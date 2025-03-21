using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.DTOModels.BasketDTOs;
using BLL.DTOModels.OrderDTOs;
using BLL.ServiceInterfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace BLL_DB.Services
{
    public class BasketServiceDB : IBasketService
    {
        private readonly string _connectionString;

        public BasketServiceDB(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task AddProductToBasket(BasketRequestDTO dto)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync("AddProductToBasket", new
            {
                UserId = dto.UserID,
                ProductId = dto.ProductID,
                Amount = dto.Amount
            }, commandType: CommandType.StoredProcedure);
        }

        public async Task UpdateBasketItem(int userId, int productId, int amount)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync("UpdateBasketItem", new { UserId = userId, ProductId = productId, Amount = amount }, commandType: CommandType.StoredProcedure);
        }

        public async Task RemoveFromBasket(int userId, int productId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync("RemoveFromBasket", new { UserId = userId, ProductId = productId }, commandType: CommandType.StoredProcedure);
        }

        public async Task<OrderResponseDTO> CreateOrder(int userId)
        {
            using var conn = new SqlConnection(_connectionString);
            return await conn.QuerySingleAsync<OrderResponseDTO>("CreateOrder", new { UserId = userId }, commandType: CommandType.StoredProcedure);
        }
    }
}
