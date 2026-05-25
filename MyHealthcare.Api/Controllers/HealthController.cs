using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MyHealthcare.Api.Data;

namespace MyHealthcare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly MongoDbContext _db;
    private readonly ILogger<HealthController> _logger;

    public HealthController(MongoDbContext db, ILogger<HealthController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        status = "healthy",
        timestamp = DateTime.UtcNow,
        service = "MyHealthcare API"
    });

    [HttpGet("db")]
    public async Task<IActionResult> CheckDatabase()
    {
        try
        {
            var ping = await _db.Users.Database.RunCommandAsync<BsonDocument>(
                new BsonDocument("ping", 1));

            return Ok(new
            {
                status = "connected",
                database = _db.Users.Database.DatabaseNamespace.DatabaseName,
                pingResult = ping.ToString()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connection failed");
            return StatusCode(500, new
            {
                status = "failed",
                error = ex.Message
            });
        }
    }
}
