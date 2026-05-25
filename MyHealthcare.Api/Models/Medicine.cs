using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MyHealthcare.Api.Models;

public class Medicine
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("categoryId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string CategoryId { get; set; } = string.Empty;

    [BsonElement("brand")]
    public string Brand { get; set; } = string.Empty;

    [BsonElement("price")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Price { get; set; }

    [BsonElement("discountPrice")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal? DiscountPrice { get; set; }

    [BsonElement("stockQuantity")]
    public int StockQuantity { get; set; }

    [BsonElement("imageUrl")]
    public string? ImageUrl { get; set; }

    [BsonElement("prescriptionRequired")]
    public bool PrescriptionRequired { get; set; }

    [BsonElement("ingredients")]
    public List<string> Ingredients { get; set; } = new();

    [BsonElement("usage")]
    public string? Usage { get; set; }

    [BsonElement("sideEffects")]
    public string? SideEffects { get; set; }

    [BsonElement("manufacturer")]
    public string? Manufacturer { get; set; }

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
