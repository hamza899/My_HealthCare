namespace MyHealthcare.Shared.DTOs;

public class CartItemDto
{
    public string MedicineId { get; set; } = string.Empty;
    public string MedicineName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public bool PrescriptionRequired { get; set; }

    public decimal Subtotal => UnitPrice * Quantity;
}
