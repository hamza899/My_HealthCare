using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MyHealthcare.Shared.Enums;

namespace MyHealthcare.Api.Models;

public class Prescription
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("imageUrl")]
    public string ImageUrl { get; set; } = string.Empty;

    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public PrescriptionStatus Status { get; set; } = PrescriptionStatus.Pending;

    [BsonElement("notes")]
    public string? Notes { get; set; }

    [BsonElement("uploadedAt")]
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("reviewedBy")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ReviewedBy { get; set; }

    [BsonElement("reviewedAt")]
    public DateTime? ReviewedAt { get; set; }
}
