using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MyHealthcare.Api.Data;
using MyHealthcare.Api.Models;
using MyHealthcare.Shared.DTOs;
using MyHealthcare.Shared.Enums;

namespace MyHealthcare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PrescriptionsController : ControllerBase
{
    private readonly MongoDbContext _db;
    private readonly string _cloudName;

    public PrescriptionsController(MongoDbContext db, IConfiguration config)
    {
        _db = db;
        _cloudName = config["Cloudinary:CloudName"] ?? string.Empty;
    }

    private string UserId =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? throw new InvalidOperationException("Missing user id claim.");

    [HttpPost]
    public async Task<ActionResult<PrescriptionDto>> Create(CreatePrescriptionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ImageUrl))
            return BadRequest(new { message = "Image URL is required." });

        if (!IsTrustedUrl(dto.ImageUrl))
            return BadRequest(new { message = "Image URL must be a Cloudinary upload." });

        var prescription = new Prescription
        {
            UserId = UserId,
            ImageUrl = dto.ImageUrl,
            Status = PrescriptionStatus.Pending,
            UploadedAt = DateTime.UtcNow
        };

        await _db.Prescriptions.InsertOneAsync(prescription);

        return Ok(ToDto(prescription));
    }

    [HttpGet("me")]
    public async Task<ActionResult<IEnumerable<PrescriptionDto>>> GetMine()
    {
        var prescriptions = await _db.Prescriptions
            .Find(p => p.UserId == UserId)
            .SortByDescending(p => p.UploadedAt)
            .ToListAsync();

        return Ok(prescriptions.Select(ToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PrescriptionDto>> GetById(string id)
    {
        var prescription = await _db.Prescriptions.Find(p => p.Id == id).FirstOrDefaultAsync();
        if (prescription is null) return NotFound();

        var isAdmin = User.IsInRole("Admin");
        if (!isAdmin && prescription.UserId != UserId) return Forbid();

        return Ok(ToDto(prescription));
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PrescriptionDto>> UpdateStatus(string id, UpdatePrescriptionStatusRequest body)
    {
        var update = Builders<Prescription>.Update
            .Set(p => p.Status, body.Status)
            .Set(p => p.Notes, body.Notes)
            .Set(p => p.ReviewedBy, UserId)
            .Set(p => p.ReviewedAt, DateTime.UtcNow);

        var result = await _db.Prescriptions.UpdateOneAsync(p => p.Id == id, update);
        if (result.MatchedCount == 0) return NotFound();

        var updated = await _db.Prescriptions.Find(p => p.Id == id).FirstOrDefaultAsync();
        return Ok(ToDto(updated));
    }

    private bool IsTrustedUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;
        if (uri.Scheme != Uri.UriSchemeHttps)
            return false;
        if (!uri.Host.EndsWith("cloudinary.com", StringComparison.OrdinalIgnoreCase))
            return false;
        if (!string.IsNullOrEmpty(_cloudName) && !uri.AbsolutePath.Contains(_cloudName))
            return false;
        return true;
    }

    private static PrescriptionDto ToDto(Prescription p) => new()
    {
        Id = p.Id,
        UserId = p.UserId,
        ImageUrl = p.ImageUrl,
        Status = p.Status,
        Notes = p.Notes,
        UploadedAt = p.UploadedAt
    };

    public class UpdatePrescriptionStatusRequest
    {
        public PrescriptionStatus Status { get; set; }
        public string? Notes { get; set; }
    }
}
