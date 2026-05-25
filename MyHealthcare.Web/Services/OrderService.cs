using System.Net.Http.Json;
using MyHealthcare.Shared.DTOs;

namespace MyHealthcare.Web.Services;

public class OrderService
{
    private readonly HttpClient _http;

    public OrderService(HttpClient http)
    {
        _http = http;
    }

    public async Task<OrderResult> CreateOrderAsync(CreateOrderDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/orders", dto);
            if (response.IsSuccessStatusCode)
            {
                var order = await response.Content.ReadFromJsonAsync<OrderDto>();
                return OrderResult.Success(order!);
            }

            var error = await TryReadErrorMessage(response);
            return OrderResult.Fail(error ?? $"Failed to place order ({(int)response.StatusCode}).");
        }
        catch (Exception ex)
        {
            return OrderResult.Fail($"Network error: {ex.Message}");
        }
    }

    public async Task<List<OrderDto>> GetMyOrdersAsync()
    {
        try
        {
            var result = await _http.GetFromJsonAsync<List<OrderDto>>("api/orders/me");
            return result ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task<OrderDto?> GetByIdAsync(string id)
    {
        try
        {
            return await _http.GetFromJsonAsync<OrderDto>($"api/orders/{id}");
        }
        catch
        {
            return null;
        }
    }

    private static async Task<string?> TryReadErrorMessage(HttpResponseMessage response)
    {
        try
        {
            var doc = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            if (doc is not null && doc.TryGetValue("message", out var msg))
                return msg?.ToString();
        }
        catch { }
        return null;
    }
}

public record OrderResult(bool IsSuccess, OrderDto? Order, string? ErrorMessage)
{
    public static OrderResult Success(OrderDto order) => new(true, order, null);
    public static OrderResult Fail(string msg) => new(false, null, msg);
}
