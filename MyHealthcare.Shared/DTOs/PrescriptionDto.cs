using MyHealthcare.Shared.Enums;

namespace MyHealthcare.Shared.DTOs;

public class PrescriptionDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public PrescriptionStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime UploadedAt { get; set; }
}

public class CreatePrescriptionDto
{
    public string ImageUrl { get; set; } = string.Empty;
}
