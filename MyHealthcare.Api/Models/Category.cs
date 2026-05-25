using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MyHealthcare.Api.Models;

public class Category
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("slug")]
    public string Slug { get; set; } = string.Empty;

    [BsonElement("iconUrl")]
    public string? IconUrl { get; set; }

    [BsonElement("displayOrder")]
    public int DisplayOrder { get; set; }

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;
}
