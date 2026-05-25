namespace MyHealthcare.Api.Data;

public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "MyHealthcareApi";
    public string Audience { get; set; } = "MyHealthcareClient";
    public int ExpiryMinutes { get; set; } = 1440;
}
