using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MyHealthcare.Api.Data;
using MyHealthcare.Api.Models;
using MyHealthcare.Api.Services;
using MyHealthcare.Shared.DTOs.Auth;
using MyHealthcare.Shared.Enums;

namespace MyHealthcare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly MongoDbContext _db;
    private readonly TokenService _tokens;

    public AuthController(MongoDbContext db, TokenService tokens)
    {
        _db = db;
        _tokens = tokens;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        var existing = await _db.Users
            .Find(u => u.Email == email)
            .FirstOrDefaultAsync();

        if (existing is not null)
            return Conflict(new { message = "Email already registered." });

        var user = new User
        {
            FullName = dto.FullName.Trim(),
            Email = email,
            Phone = dto.Phone.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = UserRole.Customer,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _db.Users.InsertOneAsync(user);

        var (token, expiresAt) = _tokens.GenerateToken(user);

        return Ok(new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            ExpiresAt = expiresAt
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        var user = await _db.Users
            .Find(u => u.Email == email)
            .FirstOrDefaultAsync();

        if (user is null || !user.IsActive)
            return Unauthorized(new { message = "Invalid email or password." });

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid email or password." });

        var (token, expiresAt) = _tokens.GenerateToken(user);

        return Ok(new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            ExpiresAt = expiresAt
        });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<AuthResponseDto>> GetMe()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var user = await _db.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        if (user is null) return NotFound();

        return Ok(new AuthResponseDto
        {
            Token = string.Empty,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            ExpiresAt = DateTime.UtcNow
        });
    }
}
