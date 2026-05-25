using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MyHealthcare.Api.Data;
using MyHealthcare.Api.Mapping;
using MyHealthcare.Shared.DTOs;

namespace MyHealthcare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly MongoDbContext _db;

    public CategoriesController(MongoDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll()
    {
        var categories = await _db.Categories
            .Find(c => c.IsActive)
            .SortBy(c => c.DisplayOrder)
            .ToListAsync();

        return Ok(categories.Select(c => c.ToDto()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetById(string id)
    {
        var category = await _db.Categories.Find(c => c.Id == id).FirstOrDefaultAsync();
        if (category is null) return NotFound();
        return Ok(category.ToDto());
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<CategoryDto>> GetBySlug(string slug)
    {
        var category = await _db.Categories.Find(c => c.Slug == slug).FirstOrDefaultAsync();
        if (category is null) return NotFound();
        return Ok(category.ToDto());
    }
}
