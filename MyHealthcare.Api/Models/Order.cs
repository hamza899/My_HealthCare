using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MyHealthcare.Shared.Enums;

namespace MyHealthcare.Api.Models;

public class Order
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("orderNumber")]
    public string OrderNumber { get; set; } = string.Empty;

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("items")]
    public List<OrderItem> Items { get; set; } = new();

    [BsonElement("totalAmount")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal TotalAmount { get; set; }

    [BsonElement("deliveryAddress")]
    public EmbeddedAddress DeliveryAddress { get; set; } = new();

    [BsonElement("paymentMethod")]
    [BsonRepresentation(BsonType.String)]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;

    [BsonElement("paymentStatus")]
    [BsonRepresentation(BsonType.String)]
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    [BsonElement("orderStatus")]
    [BsonRepresentation(BsonType.String)]
    public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;

    [BsonElement("prescriptionId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? PrescriptionId { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("deliveredAt")]
    public DateTime? DeliveredAt { get; set; }
}

public class OrderItem
{
    [BsonElement("medicineId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string MedicineId { get; set; } = string.Empty;

    [BsonElement("medicineName")]
    public string MedicineName { get; set; } = string.Empty;

    [BsonElement("quantity")]
    public int Quantity { get; set; }

    [BsonElement("unitPrice")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal UnitPrice { get; set; }

    [BsonElement("subtotal")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Subtotal { get; set; }
}
