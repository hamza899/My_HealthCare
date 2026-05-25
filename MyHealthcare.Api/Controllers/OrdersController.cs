using System.Security.Claims;
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
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly MongoDbContext _db;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(MongoDbContext db, ILogger<OrdersController> logger)
    {
        _db = db;
        _logger = logger;
    }

    private string UserId =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? throw new InvalidOperationException("Missing user id claim.");

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderDto dto)
    {
        if (dto.Items is null || !dto.Items.Any())
            return BadRequest(new { message = "Cart is empty." });

        var medicineIds = dto.Items.Select(i => i.MedicineId).Distinct().ToList();
        var medicines = await _db.Medicines
            .Find(m => medicineIds.Contains(m.Id) && m.IsActive)
            .ToListAsync();

        if (medicines.Count != medicineIds.Count)
            return BadRequest(new { message = "One or more medicines are unavailable." });

        var medicineMap = medicines.ToDictionary(m => m.Id);
        var orderItems = new List<OrderItem>();
        decimal total = 0m;

        foreach (var cartItem in dto.Items)
        {
            var med = medicineMap[cartItem.MedicineId];

            if (med.StockQuantity < cartItem.Quantity)
                return BadRequest(new
                {
                    message = $"Insufficient stock for {med.Name}. Available: {med.StockQuantity}"
                });

            var unitPrice = med.DiscountPrice ?? med.Price;
            var subtotal = unitPrice * cartItem.Quantity;
            total += subtotal;

            orderItems.Add(new OrderItem
            {
                MedicineId = med.Id,
                MedicineName = med.Name,
                Quantity = cartItem.Quantity,
                UnitPrice = unitPrice,
                Subtotal = subtotal
            });
        }

        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

        var order = new Order
        {
            OrderNumber = orderNumber,
            UserId = UserId,
            Items = orderItems,
            TotalAmount = total,
            DeliveryAddress = new EmbeddedAddress
            {
                FullName = dto.DeliveryAddress.FullName,
                Phone = dto.DeliveryAddress.Phone,
                Street = dto.DeliveryAddress.Street,
                City = dto.DeliveryAddress.City,
                PostalCode = dto.DeliveryAddress.PostalCode
            },
            PaymentMethod = dto.PaymentMethod,
            PaymentStatus = PaymentStatus.Pending,
            OrderStatus = OrderStatus.Pending,
            PrescriptionId = dto.PrescriptionId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _db.Orders.InsertOneAsync(order);

        foreach (var item in orderItems)
        {
            var update = Builders<Medicine>.Update.Inc(m => m.StockQuantity, -item.Quantity);
            await _db.Medicines.UpdateOneAsync(m => m.Id == item.MedicineId, update);
        }

        _logger.LogInformation("Order {OrderNumber} created for user {UserId}. Total: {Total}",
            orderNumber, UserId, total);

        return Ok(ToDto(order));
    }

    [HttpGet("me")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetMyOrders()
    {
        var orders = await _db.Orders
            .Find(o => o.UserId == UserId)
            .SortByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(orders.Select(ToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetById(string id)
    {
        var order = await _db.Orders.Find(o => o.Id == id).FirstOrDefaultAsync();
        if (order is null) return NotFound();

        var isAdmin = User.IsInRole("Admin");
        if (!isAdmin && order.UserId != UserId) return Forbid();

        return Ok(ToDto(order));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll(
        [FromQuery] OrderStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 200) pageSize = 50;

        var filter = status.HasValue
            ? Builders<Order>.Filter.Eq(o => o.OrderStatus, status.Value)
            : Builders<Order>.Filter.Empty;

        var orders = await _db.Orders
            .Find(filter)
            .SortByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        return Ok(orders.Select(ToDto));
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<OrderDto>> UpdateStatus(string id, [FromBody] UpdateOrderStatusRequest body)
    {
        var update = Builders<Order>.Update
            .Set(o => o.OrderStatus, body.Status)
            .Set(o => o.UpdatedAt, DateTime.UtcNow);

        if (body.Status == OrderStatus.Delivered)
        {
            update = update.Set(o => o.DeliveredAt, DateTime.UtcNow)
                           .Set(o => o.PaymentStatus, PaymentStatus.Paid);
        }

        var result = await _db.Orders.UpdateOneAsync(o => o.Id == id, update);
        if (result.MatchedCount == 0) return NotFound();

        var order = await _db.Orders.Find(o => o.Id == id).FirstOrDefaultAsync();
        return Ok(ToDto(order));
    }

    public class UpdateOrderStatusRequest
    {
        public OrderStatus Status { get; set; }
    }

    private static OrderDto ToDto(Order o) => new()
    {
        Id = o.Id,
        OrderNumber = o.OrderNumber,
        UserId = o.UserId,
        Items = o.Items.Select(i => new OrderItemDto
        {
            MedicineId = i.MedicineId,
            MedicineName = i.MedicineName,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice
        }).ToList(),
        TotalAmount = o.TotalAmount,
        DeliveryAddress = new AddressDto
        {
            FullName = o.DeliveryAddress.FullName,
            Phone = o.DeliveryAddress.Phone,
            Street = o.DeliveryAddress.Street,
            City = o.DeliveryAddress.City,
            PostalCode = o.DeliveryAddress.PostalCode
        },
        PaymentMethod = o.PaymentMethod,
        PaymentStatus = o.PaymentStatus,
        OrderStatus = o.OrderStatus,
        PrescriptionId = o.PrescriptionId,
        CreatedAt = o.CreatedAt,
        DeliveredAt = o.DeliveredAt
    };
}
