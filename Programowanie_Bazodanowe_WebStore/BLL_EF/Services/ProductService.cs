using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BLL.DTOModels.GroupDTOs;
using BLL.DTOModels.ProductDTOs;
using BLL.ServiceInterfaces;
using DAL;
using Microsoft.EntityFrameworkCore;
using Model;

namespace BLL_EF.Services
{
    public class ProductService : IProductService
    {
        private readonly WebstoreContext _context;

        public ProductService(WebstoreContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductResponseDTO>> GetProducts(
    string? nameFilter, string? groupNameFilter, int? groupIdFilter,
    string? sortBy, bool sortOrder, bool includeInactive)
        {
            var query = _context.Products
                .Include(p => p.Group)
                .ThenInclude(g => g.Parent)
                .Where(p => includeInactive || p.IsActive)
                .AsQueryable();

            if (!string.IsNullOrEmpty(nameFilter))
                query = query.Where(p => p.Name.Contains(nameFilter));

            if (!string.IsNullOrEmpty(groupNameFilter))
                query = query.Where(p => p.Group != null && p.Group.Name.Contains(groupNameFilter));

            if (groupIdFilter.HasValue)
                query = query.Where(p => p.GroupID == groupIdFilter.Value);

            query = sortBy switch
            {
                "name" => sortOrder ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
                "price" => sortOrder ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price),
                _ => query
            };

            var products = await query.ToListAsync();

            var productResponseDTOs = new List<ProductResponseDTO>();

            foreach (var product in products)
            {
                var groupName = product.Group != null ? await GetGroupHierarchy(product.Group) : string.Empty;
                productResponseDTOs.Add(new ProductResponseDTO
                {
                    ProductID = product.ID,
                    Name = product.Name,
                    Price = product.Price,
                    GroupName = groupName
                });
            }

            return productResponseDTOs;
        }

        private async Task<string> GetGroupHierarchy(ProductGroup group)
        {
            List<string> groupHierarchy = new List<string>();
            var currentGroup = await _context.ProductGroups
                .Include(g => g.Parent)
                .FirstOrDefaultAsync(g => g.ID == group.ID);

            while (currentGroup != null)
            {
                groupHierarchy.Insert(0, currentGroup.Name);
                currentGroup = await _context.ProductGroups
                    .Include(g => g.Parent)
                    .FirstOrDefaultAsync(g => g.ID == currentGroup.ParentID);
            }

            return string.Join("/", groupHierarchy);
        }

        public async Task AddProduct(ProductRequestDTO productDto)
        {
            var product = new Product
            {
                Name = productDto.Name,
                Price = productDto.Price,
                GroupID = productDto.GroupID,
                IsActive = true
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task ChangeProductStatus(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                product.IsActive = !product.IsActive;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteProduct(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<GroupResponseDTO>> GetGroups(int? parentId, string? sortBy, bool sortOrder)
        {
            var query = _context.ProductGroups.AsQueryable();

            if (parentId.HasValue)
                query = query.Where(g => g.ParentID == parentId.Value);
            else
                query = query.Where(g => g.ParentID == null);

            query = sortBy switch
            {
                "name" => sortOrder ? query.OrderBy(g => g.Name) : query.OrderByDescending(g => g.Name),
                _ => query
            };

            return await query.Select(g => new GroupResponseDTO
            {
                Id = g.ID,
                Name = g.Name,
                HasChildren = g.SubGroups.Any()
            }).ToListAsync();
        }

        public async Task AddGroup(GroupRequestDTO groupRequestDTO)
        {
            var group = new ProductGroup
            {
                Name = groupRequestDTO.Name,
                ParentID = groupRequestDTO.ParentId
            };

            _context.ProductGroups.Add(group);
            await _context.SaveChangesAsync();
        }
    }
}
