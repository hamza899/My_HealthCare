using System.Net.Http.Json;
using MyHealthcare.Shared.DTOs;

namespace MyHealthcare.Web.Services;

public class CategoryService
{
    private readonly HttpClient _http;

    public CategoryService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        try
        {
            var result = await _http.GetFromJsonAsync<List<CategoryDto>>("api/categories");
            return result ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task<CategoryDto?> GetBySlugAsync(string slug)
    {
        try
        {
            return await _http.GetFromJsonAsync<CategoryDto>($"api/categories/slug/{slug}");
        }
        catch
        {
            return null;
        }
    }
}
