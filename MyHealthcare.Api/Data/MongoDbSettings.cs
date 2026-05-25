namespace MyHealthcare.Api.Data;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;

    public string UsersCollectionName { get; set; } = "users";
    public string CategoriesCollectionName { get; set; } = "categories";
    public string MedicinesCollectionName { get; set; } = "medicines";
    public string OrdersCollectionName { get; set; } = "orders";
    public string PrescriptionsCollectionName { get; set; } = "prescriptions";
}
