namespace MyHealthcare.Shared.DTOs;

public class CategoryDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public int DisplayOrder { get; set; }
}
