namespace MyHealthcare.Shared.DTOs;

public class MedicineDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public string Brand { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public bool PrescriptionRequired { get; set; }
    public List<string> Ingredients { get; set; } = new();
    public string? Usage { get; set; }
    public string? SideEffects { get; set; }
    public string? Manufacturer { get; set; }

    public decimal EffectivePrice => DiscountPrice ?? Price;
    public bool InStock => StockQuantity > 0;
}
