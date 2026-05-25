using MyHealthcare.Api.Models;
using MyHealthcare.Shared.DTOs;

namespace MyHealthcare.Api.Mapping;

public static class MappingExtensions
{
    public static CategoryDto ToDto(this Category c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Slug = c.Slug,
        IconUrl = c.IconUrl,
        DisplayOrder = c.DisplayOrder
    };

    public static MedicineDto ToDto(this Medicine m, string? categoryName = null) => new()
    {
        Id = m.Id,
        Name = m.Name,
        Description = m.Description,
        CategoryId = m.CategoryId,
        CategoryName = categoryName,
        Brand = m.Brand,
        Price = m.Price,
        DiscountPrice = m.DiscountPrice,
        StockQuantity = m.StockQuantity,
        ImageUrl = m.ImageUrl,
        PrescriptionRequired = m.PrescriptionRequired,
        Ingredients = m.Ingredients,
        Usage = m.Usage,
        SideEffects = m.SideEffects,
        Manufacturer = m.Manufacturer
    };

    public static Medicine ToEntity(this CreateMedicineDto dto) => new()
    {
        Name = dto.Name,
        Description = dto.Description,
        CategoryId = dto.CategoryId,
        Brand = dto.Brand,
        Price = dto.Price,
        DiscountPrice = dto.DiscountPrice,
        StockQuantity = dto.StockQuantity,
        ImageUrl = dto.ImageUrl,
        PrescriptionRequired = dto.PrescriptionRequired,
        Ingredients = dto.Ingredients,
        Usage = dto.Usage,
        SideEffects = dto.SideEffects,
        Manufacturer = dto.Manufacturer
    };
}
