using Bogus;
using MongoDB.Driver;
using MongoDB.Bson;
using BLL_MongoDb.MongoModels;
using BLL_MongoDb.Services;

Console.WriteLine("Generowanie danych dla MongoDB...");

var client = new MongoClient("mongodb://localhost:27017");
var database = client.GetDatabase("webStoreDb");
var sequenceService = new SequenceService(database);

database.DropCollection("products");
database.DropCollection("groups");
database.DropCollection("users");
database.DropCollection("usergroups");
database.DropCollection("orders");
database.DropCollection("basket");
database.DropCollection("orderpositions");

var groupCollection = database.GetCollection<MongoProductGroup>("groups");
var productCollection = database.GetCollection<MongoProduct>("products");
var userGroupCollection = database.GetCollection<MongoUserGroup>("usergroups");
var userCollection = database.GetCollection<MongoUser>("users");
var orderCollection = database.GetCollection<MongoOrder>("orders");
var basketCollection = database.GetCollection<MongoBasketItem>("basket");

var userGroups = new Faker<MongoUserGroup>()
    .RuleFor(ug => ug.Id, _ => sequenceService.GetNextSequence("usergroups"))
    .RuleFor(ug => ug.Name, f => f.Commerce.Department())
    .Generate(3);
await userGroupCollection.InsertManyAsync(userGroups);

var users = new Faker<MongoUser>()
    .RuleFor(u => u.Id, _ => sequenceService.GetNextSequence("users"))
    .RuleFor(u => u.Login, f => f.Internet.UserName())
    .RuleFor(u => u.Password, f => f.Internet.Password())
    .RuleFor(u => u.Type, f => f.PickRandom<UserType>())
    .RuleFor(u => u.IsActive, f => f.Random.Bool())
    .RuleFor(u => u.GroupId, f => f.PickRandom(userGroups).Id)
    .Generate(10);
await userCollection.InsertManyAsync(users);

var parentGroups = new Faker<MongoProductGroup>()
    .RuleFor(g => g.Id, _ => sequenceService.GetNextSequence("groups"))
    .RuleFor(g => g.Name, f => f.Commerce.Categories(1)[0])
    .Generate(3);
await groupCollection.InsertManyAsync(parentGroups);

var childGroups = new List<MongoProductGroup>();
foreach (var parent in parentGroups)
{
    var children = new Faker<MongoProductGroup>()
        .RuleFor(g => g.Id, _ => sequenceService.GetNextSequence("groups"))
        .RuleFor(g => g.Name, f => f.Commerce.Categories(1)[0])
        .RuleFor(g => g.ParentId, _ => parent.Id)
        .Generate(3);
    childGroups.AddRange(children);
}
await groupCollection.InsertManyAsync(childGroups);

var allGroups = parentGroups.Concat(childGroups).ToList();

var products = new Faker<MongoProduct>()
    .RuleFor(p => p.Id, _ => sequenceService.GetNextSequence("products"))
    .RuleFor(p => p.Name, f => f.Commerce.ProductName())
    .RuleFor(p => p.Price, f => Math.Round(f.Random.Double(10, 1000), 2))
    .RuleFor(p => p.Image, f => f.Image.PicsumUrl())
    .RuleFor(p => p.IsActive, f => f.Random.Bool(0.8f))
    .RuleFor(p => p.GroupId, f => f.PickRandom(allGroups).Id)
    .Generate(30);
await productCollection.InsertManyAsync(products);

var orders = new Faker<MongoOrder>()
    .RuleFor(o => o.Id, _ => sequenceService.GetNextSequence("orders"))
    .RuleFor(o => o.UserId, f => f.PickRandom(users).Id)
    .RuleFor(o => o.Date, f => f.Date.Past(1))
    .RuleFor(o => o.IsPaid, f => f.Random.Bool())
    .RuleFor(o => o.OrderPositions, f =>
    {
        return products.OrderBy(p => Guid.NewGuid()).Take(3).Select(product => new MongoOrderItem
        {
            ProductId = product.Id,
            ProductName = product.Name,
            Price = product.Price,
            Amount = f.Random.Int(1, 5)
        }).ToList();
    })
    .Generate(10);
await orderCollection.InsertManyAsync(orders);

var basketItems = new Faker<MongoBasketItem>()
    .RuleFor(b => b.Id, _ => sequenceService.GetNextSequence("basket"))
    .RuleFor(b => b.UserId, f => f.PickRandom(users).Id)
    .RuleFor(b => b.ProductId, f => f.PickRandom(products).Id)
    .RuleFor(b => b.Amount, f => f.Random.Int(1, 4))
    .Generate(15);
await basketCollection.InsertManyAsync(basketItems);

Console.WriteLine("Dane MongoDB wygenerowane!");