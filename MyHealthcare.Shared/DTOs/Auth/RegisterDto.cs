using System.ComponentModel.DataAnnotations;

namespace MyHealthcare.Shared.DTOs.Auth;

public class RegisterDto
{
    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, Phone]
    public string Phone { get; set; } = string.Empty;

    [Required, MinLength(6), MaxLength(100)]
    public string Password { get; set; } = string.Empty;
}
