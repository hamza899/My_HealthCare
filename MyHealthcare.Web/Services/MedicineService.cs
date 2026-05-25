using System.Net.Http.Json;
using MyHealthcare.Shared.DTOs;

namespace MyHealthcare.Web.Services;

public class MedicineService
{
    private readonly HttpClient _http;

    public MedicineService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<MedicineDto>> GetAllAsync(
        string? search = null,
        string? categoryId = null,
        bool? prescriptionRequired = null,
        int page = 1,
        int pageSize = 20)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search)}");
        if (!string.IsNullOrWhiteSpace(categoryId)) query.Add($"categoryId={categoryId}");
        if (prescriptionRequired.HasValue) query.Add($"prescriptionRequired={prescriptionRequired.Value}");
        query.Add($"page={page}");
        query.Add($"pageSize={pageSize}");

        var url = $"api/medicines?{string.Join("&", query)}";

        try
        {
            var result = await _http.GetFromJsonAsync<List<MedicineDto>>(url);
            return result ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task<MedicineDto?> GetByIdAsync(string id)
    {
        try
        {
            return await _http.GetFromJsonAsync<MedicineDto>($"api/medicines/{id}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<long> GetCountAsync(string? search = null, string? categoryId = null)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search)}");
        if (!string.IsNullOrWhiteSpace(categoryId)) query.Add($"categoryId={categoryId}");

        var url = $"api/medicines/count{(query.Any() ? "?" + string.Join("&", query) : "")}";

        try
        {
            return await _http.GetFromJsonAsync<long>(url);
        }
        catch
        {
            return 0;
        }
    }
}
