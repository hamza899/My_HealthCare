using MyHealthcare.Shared.Enums;

namespace MyHealthcare.Shared.DTOs;

public class OrderDto
{
    public string Id { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public List<OrderItemDto> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public AddressDto DeliveryAddress { get; set; } = new();
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public string? PrescriptionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
}

public class OrderItemDto
{
    public string MedicineId { get; set; } = string.Empty;
    public string MedicineName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal => UnitPrice * Quantity;
}
