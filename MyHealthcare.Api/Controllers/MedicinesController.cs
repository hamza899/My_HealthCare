using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MyHealthcare.Api.Data;
using MyHealthcare.Api.Mapping;
using MyHealthcare.Api.Models;
using MyHealthcare.Shared.DTOs;

namespace MyHealthcare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MedicinesController : ControllerBase
{
    private readonly MongoDbContext _db;

    public MedicinesController(MongoDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MedicineDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? categoryId,
        [FromQuery] bool? prescriptionRequired,
        [FromQuery] bool includeInactive = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var filterBuilder = Builders<Medicine>.Filter;
        var filter = includeInactive
            ? FilterDefinition<Medicine>.Empty
            : filterBuilder.Eq(m => m.IsActive, true);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var regex = new MongoDB.Bson.BsonRegularExpression(search, "i");
            filter &= filterBuilder.Or(
                filterBuilder.Regex(m => m.Name, regex),
                filterBuilder.Regex(m => m.Brand, regex),
                filterBuilder.Regex(m => m.Description, regex)
            );
        }

        if (!string.IsNullOrWhiteSpace(categoryId))
            filter &= filterBuilder.Eq(m => m.CategoryId, categoryId);

        if (prescriptionRequired.HasValue)
            filter &= filterBuilder.Eq(m => m.PrescriptionRequired, prescriptionRequired.Value);

        var medicines = await _db.Medicines
            .Find(filter)
            .SortByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        var categoryIds = medicines.Select(m => m.CategoryId).Distinct().ToList();
        var categories = await _db.Categories
            .Find(c => categoryIds.Contains(c.Id))
            .ToListAsync();
        var categoryMap = categories.ToDictionary(c => c.Id, c => c.Name);

        var result = medicines.Select(m =>
            m.ToDto(categoryMap.GetValueOrDefault(m.CategoryId)));

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MedicineDto>> GetById(string id)
    {
        var medicine = await _db.Medicines.Find(m => m.Id == id).FirstOrDefaultAsync();
        if (medicine is null) return NotFound();

        var category = await _db.Categories
            .Find(c => c.Id == medicine.CategoryId)
            .FirstOrDefaultAsync();

        return Ok(medicine.ToDto(category?.Name));
    }

    [HttpGet("count")]
    public async Task<ActionResult<long>> Count(
        [FromQuery] string? search,
        [FromQuery] string? categoryId)
    {
        var filterBuilder = Builders<Medicine>.Filter;
        var filter = filterBuilder.Eq(m => m.IsActive, true);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var regex = new MongoDB.Bson.BsonRegularExpression(search, "i");
            filter &= filterBuilder.Or(
                filterBuilder.Regex(m => m.Name, regex),
                filterBuilder.Regex(m => m.Brand, regex)
            );
        }

        if (!string.IsNullOrWhiteSpace(categoryId))
            filter &= filterBuilder.Eq(m => m.CategoryId, categoryId);

        var count = await _db.Medicines.CountDocumentsAsync(filter);
        return Ok(count);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<MedicineDto>> Create(CreateMedicineDto dto)
    {
        var category = await _db.Categories
            .Find(c => c.Id == dto.CategoryId)
            .FirstOrDefaultAsync();
        if (category is null) return BadRequest(new { message = "Invalid category." });

        var medicine = dto.ToEntity();
        await _db.Medicines.InsertOneAsync(medicine);

        return CreatedAtAction(nameof(GetById), new { id = medicine.Id }, medicine.ToDto(category.Name));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<MedicineDto>> Update(string id, CreateMedicineDto dto)
    {
        var category = await _db.Categories
            .Find(c => c.Id == dto.CategoryId)
            .FirstOrDefaultAsync();
        if (category is null) return BadRequest(new { message = "Invalid category." });

        var update = Builders<Medicine>.Update
            .Set(m => m.Name, dto.Name)
            .Set(m => m.Description, dto.Description)
            .Set(m => m.CategoryId, dto.CategoryId)
            .Set(m => m.Brand, dto.Brand)
            .Set(m => m.Price, dto.Price)
            .Set(m => m.DiscountPrice, dto.DiscountPrice)
            .Set(m => m.StockQuantity, dto.StockQuantity)
            .Set(m => m.ImageUrl, dto.ImageUrl)
            .Set(m => m.PrescriptionRequired, dto.PrescriptionRequired)
            .Set(m => m.Ingredients, dto.Ingredients)
            .Set(m => m.Usage, dto.Usage)
            .Set(m => m.SideEffects, dto.SideEffects)
            .Set(m => m.Manufacturer, dto.Manufacturer);

        var result = await _db.Medicines.UpdateOneAsync(m => m.Id == id, update);
        if (result.MatchedCount == 0) return NotFound();

        var updated = await _db.Medicines.Find(m => m.Id == id).FirstOrDefaultAsync();
        return Ok(updated.ToDto(category.Name));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id)
    {
        var update = Builders<Medicine>.Update.Set(m => m.IsActive, false);
        var result = await _db.Medicines.UpdateOneAsync(m => m.Id == id, update);
        if (result.MatchedCount == 0) return NotFound();
        return NoContent();
    }

    [HttpPost("{id}/restore")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Restore(string id)
    {
        var update = Builders<Medicine>.Update.Set(m => m.IsActive, true);
        var result = await _db.Medicines.UpdateOneAsync(m => m.Id == id, update);
        if (result.MatchedCount == 0) return NotFound();
        return NoContent();
    }
}
