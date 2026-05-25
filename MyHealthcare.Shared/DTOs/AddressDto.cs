using System.ComponentModel.DataAnnotations;

namespace MyHealthcare.Shared.DTOs;

public class AddressDto
{
    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, Phone]
    public string Phone { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Street { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string City { get; set; } = string.Empty;

    [StringLength(20)]
    public string? PostalCode { get; set; }
}
