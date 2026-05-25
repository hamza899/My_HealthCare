using System.Net.Http.Json;
using MyHealthcare.Shared.DTOs;
using MyHealthcare.Shared.Enums;

namespace MyHealthcare.Web.Services;

public class AdminService
{
    private readonly HttpClient _http;

    public string? LastError { get; private set; }

    public AdminService(HttpClient http)
    {
        _http = http;
    }

    public async Task<AdminStatsDto?> GetStatsAsync()
    {
        LastError = null;
        try
        {
            var response = await _http.GetAsync("api/admin/stats");
            if (!response.IsSuccessStatusCode)
            {
                LastError = $"HTTP {(int)response.StatusCode} {response.StatusCode}: " +
                            await response.Content.ReadAsStringAsync();
                return null;
            }
            return await response.Content.ReadFromJsonAsync<AdminStatsDto>();
        }
        catch (Exception ex)
        {
            LastError = $"Exception: {ex.GetType().Name}: {ex.Message}";
            return null;
        }
    }

    public async Task<List<MedicineDto>> GetAllMedicinesAsync(string? search = null, bool includeInactive = true)
    {
        var query = new List<string> { $"includeInactive={includeInactive}", "pageSize=100" };
        if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search)}");
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

    public async Task<AdminResult<MedicineDto>> CreateMedicineAsync(CreateMedicineDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/medicines", dto);
            if (response.IsSuccessStatusCode)
            {
                var med = await response.Content.ReadFromJsonAsync<MedicineDto>();
                return AdminResult<MedicineDto>.Success(med!);
            }
            var error = await TryReadErrorMessage(response);
            return AdminResult<MedicineDto>.Fail(error ?? $"Failed ({(int)response.StatusCode})");
        }
        catch (Exception ex)
        {
            return AdminResult<MedicineDto>.Fail(ex.Message);
        }
    }

    public async Task<AdminResult<MedicineDto>> UpdateMedicineAsync(string id, CreateMedicineDto dto)
    {
        try
        {
            var response = await _http.PutAsJsonAsync($"api/medicines/{id}", dto);
            if (response.IsSuccessStatusCode)
            {
                var med = await response.Content.ReadFromJsonAsync<MedicineDto>();
                return AdminResult<MedicineDto>.Success(med!);
            }
            var error = await TryReadErrorMessage(response);
            return AdminResult<MedicineDto>.Fail(error ?? $"Failed ({(int)response.StatusCode})");
        }
        catch (Exception ex)
        {
            return AdminResult<MedicineDto>.Fail(ex.Message);
        }
    }

    public async Task<bool> DeleteMedicineAsync(string id)
    {
        try
        {
            var response = await _http.DeleteAsync($"api/medicines/{id}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RestoreMedicineAsync(string id)
    {
        try
        {
            var response = await _http.PostAsync($"api/medicines/{id}/restore", null);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<OrderDto>> GetAllOrdersAsync(OrderStatus? status = null)
    {
        var url = status.HasValue ? $"api/orders?status={status.Value}" : "api/orders";
        try
        {
            var result = await _http.GetFromJsonAsync<List<OrderDto>>(url);
            return result ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task<bool> UpdateOrderStatusAsync(string orderId, OrderStatus status)
    {
        try
        {
            var response = await _http.PutAsJsonAsync($"api/orders/{orderId}/status", new { status });
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
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

public record AdminResult<T>(bool IsSuccess, T? Data, string? ErrorMessage)
{
    public static AdminResult<T> Success(T data) => new(true, data, null);
    public static AdminResult<T> Fail(string msg) => new(false, default, msg);
}
