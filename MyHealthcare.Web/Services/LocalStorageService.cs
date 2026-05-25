using System.Text.Json;
using Microsoft.JSInterop;

namespace MyHealthcare.Web.Services;

public class LocalStorageService
{
    private readonly IJSRuntime _js;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public LocalStorageService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var json = await _js.InvokeAsync<string?>("localStorage.getItem", key);
        if (string.IsNullOrEmpty(json)) return default;
        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOpts);
        }
        catch
        {
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value, JsonOpts);
        await _js.InvokeVoidAsync("localStorage.setItem", key, json);
    }

    public async Task RemoveAsync(string key)
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", key);
    }
}
