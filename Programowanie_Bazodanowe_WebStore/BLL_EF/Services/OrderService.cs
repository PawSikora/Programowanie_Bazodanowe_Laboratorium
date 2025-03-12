using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.DTOModels.OrderDTOs;
using BLL.ServiceInterfaces;
using DAL;
using Microsoft.EntityFrameworkCore;

namespace BLL_EF.Services
{
    public class OrderService : IOrderService
    {
        private readonly WebstoreContext _context;

        public OrderService(WebstoreContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderResponseDTO>> GetOrder(int? idFilter, bool? paidFilter, string? sortBy, bool sortOrder)
        {
            var query = _context.Orders.AsQueryable();

            if (idFilter.HasValue)
                query = query.Where(o => o.ID == idFilter.Value);

            if (paidFilter.HasValue)
                query = query.Where(o => o.IsPaid == paidFilter.Value);

            query = sortBy switch
            {
                "value" => sortOrder ? query.OrderBy(o => o.OrderPositions.Sum(op => op.Price * op.Amount)) : query.OrderByDescending(o => o.OrderPositions.Sum(op => op.Price * op.Amount)),
                "date" => sortOrder ? query.OrderBy(o => o.Date) : query.OrderByDescending(o => o.Date),
                "id" => sortOrder ? query.OrderBy(o => o.ID) : query.OrderByDescending(o => o.ID),
                _ => query
            };

            return await query.Select(o => new OrderResponseDTO
            {
                OrderID = o.ID,
                OrderDate = o.Date,
                TotalPrice = o.OrderPositions.Sum(op => op.Price * op.Amount),
                IsPaid = o.IsPaid
            }).ToListAsync();
        }

        public async Task<OrderDetailsResponseDTO> GetOrderDetails(int orderID)
        {
            var order = await _context.Orders
                .Include(o => o.OrderPositions)
                .ThenInclude(op => op.Product)
                .FirstOrDefaultAsync(o => o.ID == orderID);

            return new OrderDetailsResponseDTO
            {
                OrderPositions = order.OrderPositions.Select(op => new OrderItemDTO
                {
                    ProductName = op.Product.Name,
                    Price = op.Price,
                    Amount = op.Amount,
                    TotalValue = op.Price * op.Amount
                }).ToList()
            };
        }


        public async Task PayOrder(int orderId, double amount)
        {
            var order = await _context.Orders
                .Include(o => o.OrderPositions)
                .FirstOrDefaultAsync(o => o.ID == orderId);

            if (order == null)
                throw new InvalidOperationException("Order not found.");

            double totalValue = order.OrderPositions.Sum(op => op.Price * op.Amount);
            if (amount >= totalValue)
            {
                order.IsPaid = true;
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new InvalidOperationException("Insufficient amount.");
            }
        }

    }
}
