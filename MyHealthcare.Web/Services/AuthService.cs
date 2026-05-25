using System.Net.Http.Json;
using MyHealthcare.Shared.DTOs.Auth;

namespace MyHealthcare.Web.Services;

public class AuthService
{
    private const string StorageKey = "myhealthcare_auth";
    private readonly HttpClient _http;
    private readonly LocalStorageService _storage;

    private AuthResponseDto? _currentUser;
    private Task? _initializationTask;

    public event Action? OnChange;

    public AuthService(HttpClient http, LocalStorageService storage)
    {
        _http = http;
        _storage = storage;
    }

    public AuthResponseDto? CurrentUser => _currentUser;
    public bool IsAuthenticated => _currentUser is not null
        && !string.IsNullOrEmpty(_currentUser.Token)
        && _currentUser.ExpiresAt > DateTime.UtcNow;

    public Task InitializeAsync()
    {
        _initializationTask ??= DoInitializeAsync();
        return _initializationTask;
    }

    private async Task DoInitializeAsync()
    {
        var stored = await _storage.GetAsync<AuthResponseDto>(StorageKey);
        if (stored is not null && stored.ExpiresAt > DateTime.UtcNow)
        {
            _currentUser = stored;
        }
        else if (stored is not null)
        {
            await _storage.RemoveAsync(StorageKey);
        }
        OnChange?.Invoke();
    }

    public string? GetToken()
    {
        if (_currentUser is null) return null;
        if (_currentUser.ExpiresAt <= DateTime.UtcNow) return null;
        return _currentUser.Token;
    }

    public async Task<AuthResult> RegisterAsync(RegisterDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/register", dto);
            if (response.IsSuccessStatusCode)
            {
                var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                if (auth is not null) await PersistAsync(auth);
                return AuthResult.Success();
            }

            var error = await TryReadErrorMessage(response);
            return AuthResult.Fail(error ?? $"Registration failed ({(int)response.StatusCode}).");
        }
        catch (Exception ex)
        {
            return AuthResult.Fail($"Network error: {ex.Message}");
        }
    }

    public async Task<AuthResult> LoginAsync(LoginDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", dto);
            if (response.IsSuccessStatusCode)
            {
                var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                if (auth is not null) await PersistAsync(auth);
                return AuthResult.Success();
            }

            var error = await TryReadErrorMessage(response);
            return AuthResult.Fail(error ?? "Invalid email or password.");
        }
        catch (Exception ex)
        {
            return AuthResult.Fail($"Network error: {ex.Message}");
        }
    }

    public async Task LogoutAsync()
    {
        _currentUser = null;
        _initializationTask = Task.CompletedTask;
        await _storage.RemoveAsync(StorageKey);
        OnChange?.Invoke();
    }

    private async Task PersistAsync(AuthResponseDto auth)
    {
        _currentUser = auth;
        _initializationTask = Task.CompletedTask;
        await _storage.SetAsync(StorageKey, auth);
        OnChange?.Invoke();
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

public record AuthResult(bool IsSuccess, string? ErrorMessage)
{
    public static AuthResult Success() => new(true, null);
    public static AuthResult Fail(string msg) => new(false, msg);
}
