using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MyHealthcare.Api.Models;

namespace MyHealthcare.Api.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly MongoDbSettings _settings;

    public MongoDbContext(IOptions<MongoDbSettings> options)
    {
        _settings = options.Value;
        var client = new MongoClient(_settings.ConnectionString);
        _database = client.GetDatabase(_settings.DatabaseName);
    }

    public IMongoCollection<User> Users =>
        _database.GetCollection<User>(_settings.UsersCollectionName);

    public IMongoCollection<Category> Categories =>
        _database.GetCollection<Category>(_settings.CategoriesCollectionName);

    public IMongoCollection<Medicine> Medicines =>
        _database.GetCollection<Medicine>(_settings.MedicinesCollectionName);

    public IMongoCollection<Order> Orders =>
        _database.GetCollection<Order>(_settings.OrdersCollectionName);

    public IMongoCollection<Prescription> Prescriptions =>
        _database.GetCollection<Prescription>(_settings.PrescriptionsCollectionName);
}
