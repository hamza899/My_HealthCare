using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MyHealthcare.Api.Data;
using MyHealthcare.Api.Models;
using MyHealthcare.Shared.DTOs;
using MyHealthcare.Shared.Enums;

namespace MyHealthcare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly MongoDbContext _db;

    public AdminController(MongoDbContext db)
    {
        _db = db;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<AdminStatsDto>> GetStats()
    {
        var totalMedicinesTask = _db.Medicines.CountDocumentsAsync(m => m.IsActive);
        var totalUsersTask = _db.Users.CountDocumentsAsync(u => u.Role == UserRole.Customer);
        var totalOrdersTask = _db.Orders.CountDocumentsAsync(FilterDefinition<Order>.Empty);
        var pendingOrdersTask = _db.Orders.CountDocumentsAsync(o => o.OrderStatus == OrderStatus.Pending);
        var deliveredOrdersTask = _db.Orders.CountDocumentsAsync(o => o.OrderStatus == OrderStatus.Delivered);

        await Task.WhenAll(totalMedicinesTask, totalUsersTask, totalOrdersTask,
                           pendingOrdersTask, deliveredOrdersTask);

        var deliveredOrders = await _db.Orders
            .Find(o => o.OrderStatus == OrderStatus.Delivered)
            .ToListAsync();
        var totalRevenue = deliveredOrders.Sum(o => o.TotalAmount);

        var lowStock = await _db.Medicines
            .Find(m => m.IsActive && m.StockQuantity < 20)
            .Limit(5)
            .ToListAsync();

        return Ok(new AdminStatsDto
        {
            TotalMedicines = await totalMedicinesTask,
            TotalCustomers = await totalUsersTask,
            TotalOrders = await totalOrdersTask,
            PendingOrders = await pendingOrdersTask,
            DeliveredOrders = await deliveredOrdersTask,
            TotalRevenue = totalRevenue,
            LowStockMedicines = lowStock.Select(m => new LowStockItem
            {
                Id = m.Id,
                Name = m.Name,
                StockQuantity = m.StockQuantity
            }).ToList()
        });
    }
}

