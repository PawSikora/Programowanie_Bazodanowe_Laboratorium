using BLL.ServiceInterfaces;
using BLL_EF.Services;
using DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using BLL_DB.Services;
using MongoDB.Driver;
using BLL_MongoDb.Services;
using MongoDB.Bson.Serialization.Conventions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


//builder.Services.AddDbContext<WebstoreContext>();

//builder.Services.AddScoped<IProductService, ProductService>();
//builder.Services.AddScoped<IBasketService, BasketService>();
//builder.Services.AddScoped<IOrderService, OrderService>();
//builder.Services.AddScoped<IUserService, UserService>();

//builder.Services.AddScoped<IProductService, ProductServiceDB>();
//builder.Services.AddScoped<IBasketService, BasketServiceDB>();
//builder.Services.AddScoped<IOrderService, OrderServiceDB>();
//builder.Services.AddScoped<IUserService, UserServiceDB>();

//var connectionString = builder.Configuration.GetConnectionString("WebstoreDb");

//builder.Services.AddScoped<IBasketService>(_ => new BasketServiceDB(connectionString));
//builder.Services.AddScoped<IOrderService>(_ => new OrderServiceDB(connectionString));
//builder.Services.AddScoped<IProductService>(_ => new ProductServiceDB(connectionString));
//builder.Services.AddScoped<IUserService>(_ => new UserServiceDB(connectionString));

builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient(builder.Configuration.GetConnectionString("WebStoreMongoDb")));

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase("webStoreDb"));

builder.Services.AddScoped<IProductService, MongoProductService>();
builder.Services.AddScoped<IBasketService, MongoBasketService>();
builder.Services.AddScoped<IOrderService, MongoOrderService>();
builder.Services.AddScoped<IUserService, MongoUserService>();

var conventionPack = new ConventionPack { new CamelCaseElementNameConvention() };
ConventionRegistry.Register("CamelCase", conventionPack, t => true);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
