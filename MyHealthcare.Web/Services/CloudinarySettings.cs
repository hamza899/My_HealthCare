namespace MyHealthcare.Web.Services;

public class CloudinarySettings
{
    public string CloudName { get; set; } = string.Empty;
    public string UploadPreset { get; set; } = string.Empty;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(CloudName) && !string.IsNullOrWhiteSpace(UploadPreset);

    public string UploadUrl => $"https://api.cloudinary.com/v1_1/{CloudName}/image/upload";
}
