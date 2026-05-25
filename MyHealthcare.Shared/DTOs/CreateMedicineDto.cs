using System.ComponentModel.DataAnnotations;

namespace MyHealthcare.Shared.DTOs;

public class CreateMedicineDto
{
    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string CategoryId { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Brand { get; set; } = string.Empty;

    [Range(0, 1_000_000)]
    public decimal Price { get; set; }

    [Range(0, 1_000_000)]
    public decimal? DiscountPrice { get; set; }

    [Range(0, 100_000)]
    public int StockQuantity { get; set; }

    public string? ImageUrl { get; set; }
    public bool PrescriptionRequired { get; set; }
    public List<string> Ingredients { get; set; } = new();
    public string? Usage { get; set; }
    public string? SideEffects { get; set; }
    public string? Manufacturer { get; set; }
}
