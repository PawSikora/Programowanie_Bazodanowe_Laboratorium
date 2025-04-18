using BLL.DTOModels.OrderDTOs;
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
    public class MongoOrderService : IOrderService
    {
        private readonly IMongoCollection<MongoOrder> _orders;

        public MongoOrderService(IMongoDatabase database)
        {
            _orders = database.GetCollection<MongoOrder>("orders");
        }

        public async Task<IEnumerable<OrderResponseDTO>> GetOrder(int? idFilter, bool? paidFilter, string? sortBy, bool sortOrder)
        {
            var filter = Builders<MongoOrder>.Filter.Empty;

            if (idFilter.HasValue)
                filter &= Builders<MongoOrder>.Filter.Eq(x => x.Id, idFilter.Value);

            if (paidFilter.HasValue)
                filter &= Builders<MongoOrder>.Filter.Eq(x => x.IsPaid, paidFilter.Value);

            var orders = await _orders.Find(filter).ToListAsync();

            var mapped = orders.Select(o => new OrderResponseDTO
            {
                OrderID = o.Id,
                OrderDate = o.Date,
                IsPaid = o.IsPaid,
                TotalPrice = o.OrderPositions.Sum(p => p.Price * p.Amount)
            });

            return sortBy?.ToLower() switch
            {
                "orderdate" => sortOrder ? mapped.OrderBy(x => x.OrderDate) : mapped.OrderByDescending(x => x.OrderDate),
                "totalprice" => sortOrder ? mapped.OrderBy(x => x.TotalPrice) : mapped.OrderByDescending(x => x.TotalPrice),
                "orderid" => sortOrder ? mapped.OrderBy(x => x.OrderID) : mapped.OrderByDescending(x => x.OrderID),
                _ => mapped
            };
        }

        public async Task<OrderDetailsResponseDTO> GetOrderDetails(int orderId)
        {
            var order = await _orders.Find(x => x.Id == orderId).FirstOrDefaultAsync();

            if (order == null)
                return new OrderDetailsResponseDTO { OrderPositions = new List<OrderItemDTO>() };

            return new OrderDetailsResponseDTO
            {
                OrderPositions = order.OrderPositions.Select(p => new OrderItemDTO
                {
                    ProductName = p.ProductName,
                    Price = p.Price,
                    Amount = p.Amount,
                    TotalValue = p.Price * p.Amount
                }).ToList()
            };
        }

        public async Task PayOrder(int orderId, double amount)
        {
            var order = await _orders.Find(x => x.Id == orderId).FirstOrDefaultAsync();

            if (order == null) return;

            var total = order.OrderPositions.Sum(p => p.Price * p.Amount);

            if (Math.Abs(total - amount) < 0.01)
            {
                order.IsPaid = true;
                await _orders.ReplaceOneAsync(x => x.Id == order.Id, order);
            }
            else
            {
                throw new Exception("Amount does not match order total");
            }
        }
    }

}
