using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MyHealthcare.Shared.Enums;

namespace MyHealthcare.Api.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("fullName")]
    public string FullName { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    [BsonElement("phone")]
    public string Phone { get; set; } = string.Empty;

    [BsonElement("role")]
    [BsonRepresentation(BsonType.String)]
    public UserRole Role { get; set; } = UserRole.Customer;

    [BsonElement("address")]
    public EmbeddedAddress? Address { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;
}

public class EmbeddedAddress
{
    [BsonElement("fullName")] public string FullName { get; set; } = string.Empty;
    [BsonElement("phone")] public string Phone { get; set; } = string.Empty;
    [BsonElement("street")] public string Street { get; set; } = string.Empty;
    [BsonElement("city")] public string City { get; set; } = string.Empty;
    [BsonElement("postalCode")] public string? PostalCode { get; set; }
}
