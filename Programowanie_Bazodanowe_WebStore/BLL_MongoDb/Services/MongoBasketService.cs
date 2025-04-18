using BLL.DTOModels.BasketDTOs;
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

    public class MongoBasketService : IBasketService
    {
        private readonly IMongoCollection<MongoBasketItem> _basket;
        private readonly IMongoCollection<MongoProduct> _products;
        private readonly IMongoCollection<MongoOrder> _orders;
        private readonly SequenceService _sequenceService;

        public MongoBasketService(IMongoDatabase database)
        {
            _basket = database.GetCollection<MongoBasketItem>("basket");
            _products = database.GetCollection<MongoProduct>("products");
            _orders = database.GetCollection<MongoOrder>("orders");
            _sequenceService = new SequenceService(database);
        }

        public async Task AddProductToBasket(BasketRequestDTO dto)
        {
            var existing = await _basket
                .Find(x => x.UserId == dto.UserID && x.ProductId == dto.ProductID)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                existing.Amount += dto.Amount;
                await _basket.ReplaceOneAsync(x => x.Id == existing.Id, existing);
            }
            else
            {
                var newItem = new MongoBasketItem
                {
                    Id = _sequenceService.GetNextSequence("basket"),
                    ProductId = dto.ProductID,
                    UserId = dto.UserID,
                    Amount = dto.Amount
                };
                await _basket.InsertOneAsync(newItem);
            }
        }

        public async Task UpdateBasketItem(int userId, int productId, int amount)
        {
            var item = await _basket.Find(x => x.UserId == userId && x.ProductId == productId)
                .FirstOrDefaultAsync();
            if (item != null)
            {
                item.Amount = amount;
                await _basket.ReplaceOneAsync(x => x.Id == item.Id, item);
            }
        }

        public async Task RemoveFromBasket(int userId, int productId)
        {
            await _basket.DeleteOneAsync(x => x.UserId == userId && x.ProductId == productId);
        }

        public async Task<OrderResponseDTO> CreateOrder(int userId)
        {
            var items = await _basket.Find(x => x.UserId == userId).ToListAsync();
            if (items.Count == 0)
                return new OrderResponseDTO();

            var productMap = (await _products.Find(_ => true).ToListAsync()).ToDictionary(p => p.Id);

            var orderItems = items.Select(item =>
            {
                var product = productMap[item.ProductId];
                return new MongoOrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Amount = item.Amount
                };
            }).ToList();

            var order = new MongoOrder
            {
                Id = _sequenceService.GetNextSequence("orders"),
                UserId = userId,
                OrderPositions = orderItems,
                IsPaid = false,
                Date = DateTime.UtcNow
            };

            await _orders.InsertOneAsync(order);
            await _basket.DeleteManyAsync(x => x.UserId == userId);

            var total = orderItems.Sum(i => i.Price * i.Amount);

            return new OrderResponseDTO
            {
                OrderID = order.Id,
                OrderDate = order.Date,
                TotalPrice = total,
                IsPaid = false
            };
        }
    }

}
