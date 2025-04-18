using BLL_MongoDb.MongoModels;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.DTOModels.GroupDTOs;
using BLL.DTOModels.ProductDTOs;
using BLL.ServiceInterfaces;

namespace BLL_MongoDb.Services
{

    public class MongoProductService : IProductService
    {
        private readonly IMongoCollection<MongoProduct> _products;
        private readonly IMongoCollection<MongoProductGroup> _groups;
        private readonly SequenceService _sequenceService;

        public MongoProductService(IMongoDatabase database)
        {
            _products = database.GetCollection<MongoProduct>("products");
            _groups = database.GetCollection<MongoProductGroup>("groups");
            _sequenceService = new SequenceService(database);
        }

        public async Task<IEnumerable<ProductResponseDTO>> GetProducts(string? nameFilter, string? groupNameFilter, int? groupIdFilter, string? sortBy, bool sortOrder, bool includeInactive)
        {
            var filterBuilder = Builders<MongoProduct>.Filter;
            var filter = includeInactive ? FilterDefinition<MongoProduct>.Empty : filterBuilder.Eq(p => p.IsActive, true);

            if (!string.IsNullOrEmpty(nameFilter))
                filter &= filterBuilder.Regex(p => p.Name, new MongoDB.Bson.BsonRegularExpression(nameFilter, "i"));

            var allGroups = await _groups.Find(_ => true).ToListAsync();
            var groupMap = allGroups.ToDictionary(g => g.Id);

            if (!string.IsNullOrEmpty(groupNameFilter))
            {
                var matchingGroupIds = allGroups
                    .Where(g => BuildGroupPath(g, groupMap).Contains(groupNameFilter, StringComparison.OrdinalIgnoreCase))
                    .Select(g => g.Id)
                    .ToHashSet();

                filter &= filterBuilder.In(p => p.GroupId, matchingGroupIds);
            }

            if (groupIdFilter.HasValue)
            {
                var descendantIds = GetAllDescendantGroupIds(groupIdFilter.Value, allGroups);
                filter &= filterBuilder.In(p => p.GroupId, descendantIds);
            }

            var products = await _products.Find(filter).ToListAsync();

            var result = products.Select(p => new ProductResponseDTO
            {
                ProductID = p.Id,
                Name = p.Name,
                Price = p.Price,
                GroupName = groupMap.TryGetValue(p.GroupId, out var group) ? BuildGroupPath(group, groupMap) : string.Empty
            });

            return sortBy switch
            {
                "name" => sortOrder ? result.OrderBy(p => p.Name) : result.OrderByDescending(p => p.Name),
                "price" => sortOrder ? result.OrderBy(p => p.Price) : result.OrderByDescending(p => p.Price),
                "group" => sortOrder ? result.OrderBy(p => p.GroupName) : result.OrderByDescending(p => p.GroupName),
                _ => result
            };
        }

        private static string BuildGroupPath(MongoProductGroup group, Dictionary<int, MongoProductGroup> allGroups)
        {
            var hierarchy = new List<string> { group.Name };
            var current = group;

            while (current.ParentId.HasValue && allGroups.TryGetValue(current.ParentId.Value, out var parent))
            {
                hierarchy.Insert(0, parent.Name);
                current = parent;
            }

            return string.Join(" / ", hierarchy);
        }

        private static HashSet<int> GetAllDescendantGroupIds(int groupId, List<MongoProductGroup> allGroups)
        {
            var result = new HashSet<int> { groupId };
            var queue = new Queue<int>();
            queue.Enqueue(groupId);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var child in allGroups.Where(g => g.ParentId == current))
                {
                    if (result.Add(child.Id))
                        queue.Enqueue(child.Id);
                }
            }

            return result;
        }

        public async Task AddProduct(ProductRequestDTO dto)
        {
            var product = new MongoProduct
            {
                Id = _sequenceService.GetNextSequence("products"),
                Name = dto.Name,
                Price = dto.Price,
                GroupId = dto.GroupID,
                IsActive = true
            };

            await _products.InsertOneAsync(product);
        }

        public async Task ChangeProductStatus(int productId)
        {
            var product = await _products.Find(p => p.Id == productId).FirstOrDefaultAsync();
            if (product != null)
            {
                product.IsActive = !product.IsActive;
                await _products.ReplaceOneAsync(p => p.Id == productId, product);
            }
        }

        public async Task DeleteProduct(int productId)
        {
            await _products.DeleteOneAsync(p => p.Id == productId);
        }

        public async Task<IEnumerable<GroupResponseDTO>> GetGroups(int? parentId, string? sortBy, bool sortOrder)
        {
            var allGroups = await _groups.Find(_ => true).ToListAsync();
            var filtered = parentId.HasValue
                ? allGroups.Where(g => g.ParentId == parentId)
                : allGroups.Where(g => g.ParentId == null);

            var result = filtered.Select(g => new GroupResponseDTO
            {
                Id = g.Id,
                Name = g.Name,
                ParentId = g.ParentId,
                HasChildren = allGroups.Any(child => child.ParentId == g.Id)
            });

            return sortBy?.ToLower() == "name"
                ? (sortOrder ? result.OrderBy(g => g.Name) : result.OrderByDescending(g => g.Name))
                : result;
        }

        public async Task AddGroup(GroupRequestDTO dto)
        {
            var group = new MongoProductGroup
            {
                Id = _sequenceService.GetNextSequence("groups"),
                Name = dto.Name,
                ParentId = dto.ParentId
            };

            await _groups.InsertOneAsync(group);
        }
    }

}
