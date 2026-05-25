using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components.Forms;
using MyHealthcare.Shared.DTOs;

namespace MyHealthcare.Web.Services;

public class PrescriptionService
{
    private const long MaxFileSize = 5 * 1024 * 1024;

    private readonly HttpClient _api;
    private readonly CloudinarySettings _cloudinary;

    public PrescriptionService(HttpClient api, CloudinarySettings cloudinary)
    {
        _api = api;
        _cloudinary = cloudinary;
    }

    public bool IsConfigured => _cloudinary.IsConfigured;

    public async Task<PrescriptionResult> UploadAsync(IBrowserFile file)
    {
        if (!_cloudinary.IsConfigured)
            return PrescriptionResult.Fail("Image upload is not configured.");

        if (file.Size > MaxFileSize)
            return PrescriptionResult.Fail($"File too large (max 5MB).");

        if (!file.ContentType.StartsWith("image/"))
            return PrescriptionResult.Fail("Only image files are allowed.");

        try
        {
            using var content = new MultipartFormDataContent();
            using var fileStream = file.OpenReadStream(MaxFileSize);
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

            content.Add(fileContent, "file", file.Name);
            content.Add(new StringContent(_cloudinary.UploadPreset), "upload_preset");

            using var http = new HttpClient();
            var response = await http.PostAsync(_cloudinary.UploadUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                return PrescriptionResult.Fail($"Upload failed: {err}");
            }

            var uploadResult = await response.Content.ReadFromJsonAsync<CloudinaryUploadResponse>();
            if (uploadResult is null || string.IsNullOrEmpty(uploadResult.SecureUrl))
                return PrescriptionResult.Fail("Upload succeeded but no URL returned.");

            var apiResponse = await _api.PostAsJsonAsync("api/prescriptions",
                new CreatePrescriptionDto { ImageUrl = uploadResult.SecureUrl });

            if (!apiResponse.IsSuccessStatusCode)
            {
                var err = await apiResponse.Content.ReadAsStringAsync();
                return PrescriptionResult.Fail($"Failed to save prescription: {err}");
            }

            var prescription = await apiResponse.Content.ReadFromJsonAsync<PrescriptionDto>();
            return PrescriptionResult.Success(prescription!);
        }
        catch (Exception ex)
        {
            return PrescriptionResult.Fail($"Error: {ex.Message}");
        }
    }

    private class CloudinaryUploadResponse
    {
        [JsonPropertyName("secure_url")]
        public string SecureUrl { get; set; } = string.Empty;
    }
}

public record PrescriptionResult(bool IsSuccess, PrescriptionDto? Prescription, string? ErrorMessage)
{
    public static PrescriptionResult Success(PrescriptionDto p) => new(true, p, null);
    public static PrescriptionResult Fail(string msg) => new(false, null, msg);
}
