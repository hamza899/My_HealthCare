using MongoDB.Driver;
using MyHealthcare.Api.Data;
using MyHealthcare.Api.Models;

namespace MyHealthcare.Api.Services;

public class SeedDataService
{
    private readonly MongoDbContext _db;
    private readonly ILogger<SeedDataService> _logger;

    public SeedDataService(MongoDbContext db, ILogger<SeedDataService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        await SeedAdminAsync();
        await SeedCategoriesAsync();
        await SeedMedicinesAsync();
    }

    private async Task SeedAdminAsync()
    {
        const string adminEmail = "admin@myhealthcare.com";
        var existing = await _db.Users.Find(u => u.Email == adminEmail).FirstOrDefaultAsync();
        if (existing is not null)
        {
            _logger.LogInformation("Admin user already exists. Skipping.");
            return;
        }

        var admin = new Models.User
        {
            FullName = "Admin",
            Email = adminEmail,
            Phone = "+923000000000",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = Shared.Enums.UserRole.Admin,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _db.Users.InsertOneAsync(admin);
        _logger.LogInformation("Seeded admin user: {Email} / Admin@123", adminEmail);
    }

    private async Task SeedCategoriesAsync()
    {
        var existing = await _db.Categories.CountDocumentsAsync(FilterDefinition<Category>.Empty);
        if (existing > 0)
        {
            _logger.LogInformation("Categories already seeded ({Count} existing). Skipping.", existing);
            return;
        }

        var categories = new List<Category>
        {
            new() { Name = "Pain Relief",  Slug = "pain-relief",  DisplayOrder = 1 },
            new() { Name = "Vitamins",     Slug = "vitamins",     DisplayOrder = 2 },
            new() { Name = "Diabetes",     Slug = "diabetes",     DisplayOrder = 3 },
            new() { Name = "Heart Care",   Slug = "heart-care",   DisplayOrder = 4 },
            new() { Name = "Skin Care",    Slug = "skin-care",    DisplayOrder = 5 },
            new() { Name = "Cold & Flu",   Slug = "cold-flu",     DisplayOrder = 6 },
            new() { Name = "Baby Care",    Slug = "baby-care",    DisplayOrder = 7 },
            new() { Name = "First Aid",    Slug = "first-aid",    DisplayOrder = 8 },
        };

        await _db.Categories.InsertManyAsync(categories);
        _logger.LogInformation("Seeded {Count} categories.", categories.Count);
    }

    private async Task SeedMedicinesAsync()
    {
        var existing = await _db.Medicines.CountDocumentsAsync(FilterDefinition<Medicine>.Empty);
        if (existing > 0)
        {
            _logger.LogInformation("Medicines already seeded ({Count} existing). Skipping.", existing);
            return;
        }

        var categories = await _db.Categories.Find(_ => true).ToListAsync();
        var byName = categories.ToDictionary(c => c.Name, c => c.Id);

        string Cat(string name) => byName[name];

        var medicines = new List<Medicine>
        {
            // Pain Relief
            new() {
                Name = "Panadol Extra", Brand = "GSK", CategoryId = Cat("Pain Relief"),
                Description = "Fast relief from headache, fever, and body pain.",
                Price = 250m, DiscountPrice = 220m, StockQuantity = 150,
                Ingredients = new() { "Paracetamol 500mg", "Caffeine 65mg" },
                Usage = "1-2 tablets every 4-6 hours. Max 8 tablets/day.",
                Manufacturer = "GSK Pakistan",
                PrescriptionRequired = false,
            },
            new() {
                Name = "Brufen 400mg", Brand = "Abbott", CategoryId = Cat("Pain Relief"),
                Description = "Anti-inflammatory pain reliever for muscle and joint pain.",
                Price = 180m, StockQuantity = 100,
                Ingredients = new() { "Ibuprofen 400mg" },
                Usage = "1 tablet 3 times a day after meals.",
                Manufacturer = "Abbott Pakistan",
                PrescriptionRequired = false,
            },
            new() {
                Name = "Disprin", Brand = "Reckitt", CategoryId = Cat("Pain Relief"),
                Description = "Effervescent aspirin tablets for quick pain relief.",
                Price = 80m, StockQuantity = 200,
                Ingredients = new() { "Aspirin 325mg" },
                Manufacturer = "Reckitt Benckiser",
                PrescriptionRequired = false,
            },
            new() {
                Name = "Ponstan Forte", Brand = "Pfizer", CategoryId = Cat("Pain Relief"),
                Description = "Strong pain relief for menstrual cramps and dental pain.",
                Price = 320m, StockQuantity = 80,
                Ingredients = new() { "Mefenamic Acid 500mg" },
                Manufacturer = "Pfizer Pakistan",
                PrescriptionRequired = false,
            },

            // Vitamins
            new() {
                Name = "Centrum Multivitamin", Brand = "Pfizer", CategoryId = Cat("Vitamins"),
                Description = "Complete multivitamin and mineral supplement.",
                Price = 1200m, DiscountPrice = 999m, StockQuantity = 60,
                Ingredients = new() { "Vitamins A, B, C, D, E", "Iron", "Zinc", "Calcium" },
                Usage = "1 tablet daily after breakfast.",
                Manufacturer = "Pfizer",
                PrescriptionRequired = false,
            },
            new() {
                Name = "Vitamin D3 60000 IU", Brand = "Getz Pharma", CategoryId = Cat("Vitamins"),
                Description = "Weekly Vitamin D3 supplement for bone health.",
                Price = 350m, StockQuantity = 90,
                Ingredients = new() { "Cholecalciferol 60000 IU" },
                Usage = "1 capsule per week.",
                Manufacturer = "Getz Pharma",
                PrescriptionRequired = false,
            },
            new() {
                Name = "Vitamin C 1000mg", Brand = "Nestle", CategoryId = Cat("Vitamins"),
                Description = "High-strength Vitamin C for immunity boost.",
                Price = 450m, StockQuantity = 120,
                Ingredients = new() { "Ascorbic Acid 1000mg" },
                Manufacturer = "Nestle Health Science",
                PrescriptionRequired = false,
            },

            // Diabetes
            new() {
                Name = "Glucophage 500mg", Brand = "Merck", CategoryId = Cat("Diabetes"),
                Description = "First-line treatment for type 2 diabetes.",
                Price = 280m, StockQuantity = 70,
                Ingredients = new() { "Metformin HCl 500mg" },
                Usage = "1 tablet twice daily with meals.",
                Manufacturer = "Merck Pakistan",
                PrescriptionRequired = true,
            },
            new() {
                Name = "Diamicron MR 60mg", Brand = "Servier", CategoryId = Cat("Diabetes"),
                Description = "Modified-release tablets for blood sugar control.",
                Price = 620m, StockQuantity = 45,
                Ingredients = new() { "Gliclazide 60mg" },
                Manufacturer = "Servier",
                PrescriptionRequired = true,
            },

            // Heart Care
            new() {
                Name = "Cardiprin 75mg", Brand = "Reckitt", CategoryId = Cat("Heart Care"),
                Description = "Low-dose aspirin for heart protection.",
                Price = 150m, StockQuantity = 100,
                Ingredients = new() { "Aspirin 75mg" },
                Usage = "1 tablet daily.",
                Manufacturer = "Reckitt Benckiser",
                PrescriptionRequired = true,
            },
            new() {
                Name = "Concor 5mg", Brand = "Merck", CategoryId = Cat("Heart Care"),
                Description = "Beta-blocker for high blood pressure.",
                Price = 480m, StockQuantity = 55,
                Ingredients = new() { "Bisoprolol Fumarate 5mg" },
                Manufacturer = "Merck",
                PrescriptionRequired = true,
            },

            // Skin Care
            new() {
                Name = "Cetaphil Gentle Cleanser 250ml", Brand = "Galderma", CategoryId = Cat("Skin Care"),
                Description = "Gentle, soap-free cleanser for sensitive skin.",
                Price = 1450m, DiscountPrice = 1299m, StockQuantity = 40,
                Manufacturer = "Galderma",
                PrescriptionRequired = false,
            },
            new() {
                Name = "Bio-Oil 60ml", Brand = "Bio-Oil", CategoryId = Cat("Skin Care"),
                Description = "Specialist skincare oil for scars and stretch marks.",
                Price = 1850m, StockQuantity = 35,
                Manufacturer = "Bio-Oil Naturals",
                PrescriptionRequired = false,
            },
            new() {
                Name = "Sunblock SPF 60", Brand = "Beaphar", CategoryId = Cat("Skin Care"),
                Description = "Broad-spectrum sunscreen with SPF 60.",
                Price = 950m, StockQuantity = 80,
                Manufacturer = "Beaphar Pharma",
                PrescriptionRequired = false,
            },

            // Cold & Flu
            new() {
                Name = "Panadol Cold & Flu", Brand = "GSK", CategoryId = Cat("Cold & Flu"),
                Description = "Multi-symptom relief for cold and flu.",
                Price = 220m, StockQuantity = 130,
                Ingredients = new() { "Paracetamol 500mg", "Phenylephrine 5mg", "Chlorpheniramine 2mg" },
                Manufacturer = "GSK Pakistan",
                PrescriptionRequired = false,
            },
            new() {
                Name = "Strepsils Honey & Lemon", Brand = "Reckitt", CategoryId = Cat("Cold & Flu"),
                Description = "Soothing lozenges for sore throat.",
                Price = 180m, StockQuantity = 150,
                Manufacturer = "Reckitt Benckiser",
                PrescriptionRequired = false,
            },
            new() {
                Name = "Vicks VapoRub 50g", Brand = "P&G", CategoryId = Cat("Cold & Flu"),
                Description = "Topical cough suppressant and chest rub.",
                Price = 320m, StockQuantity = 75,
                Manufacturer = "Procter & Gamble",
                PrescriptionRequired = false,
            },
            new() {
                Name = "Actifed Syrup 60ml", Brand = "GSK", CategoryId = Cat("Cold & Flu"),
                Description = "Cough and cold syrup for adults.",
                Price = 260m, StockQuantity = 90,
                Ingredients = new() { "Triprolidine", "Pseudoephedrine" },
                Manufacturer = "GSK Pakistan",
                PrescriptionRequired = false,
            },

            // Baby Care
            new() {
                Name = "Pampers Baby Dry (Medium, 60 pcs)", Brand = "P&G", CategoryId = Cat("Baby Care"),
                Description = "Up to 12 hours of dryness for babies.",
                Price = 1650m, DiscountPrice = 1499m, StockQuantity = 50,
                Manufacturer = "Procter & Gamble",
                PrescriptionRequired = false,
            },
            new() {
                Name = "Johnson's Baby Shampoo 200ml", Brand = "Johnson's", CategoryId = Cat("Baby Care"),
                Description = "No more tears formula, gentle on baby's eyes.",
                Price = 580m, StockQuantity = 80,
                Manufacturer = "Johnson & Johnson",
                PrescriptionRequired = false,
            },
            new() {
                Name = "Nan Optipro 1 (800g)", Brand = "Nestle", CategoryId = Cat("Baby Care"),
                Description = "Starter infant formula 0-6 months.",
                Price = 2850m, StockQuantity = 30,
                Manufacturer = "Nestle",
                PrescriptionRequired = false,
            },

            // First Aid
            new() {
                Name = "Pyodine Solution 60ml", Brand = "Brookes", CategoryId = Cat("First Aid"),
                Description = "Antiseptic solution for wounds and cuts.",
                Price = 95m, StockQuantity = 200,
                Ingredients = new() { "Povidone Iodine 10%" },
                Manufacturer = "Brookes Pharma",
                PrescriptionRequired = false,
            },
            new() {
                Name = "Band-Aid Flexible Fabric (40 pcs)", Brand = "Johnson's", CategoryId = Cat("First Aid"),
                Description = "Adhesive bandages for minor cuts.",
                Price = 250m, StockQuantity = 150,
                Manufacturer = "Johnson & Johnson",
                PrescriptionRequired = false,
            },
            new() {
                Name = "Burnol Cream 20g", Brand = "Beecham", CategoryId = Cat("First Aid"),
                Description = "Antiseptic cream for burns and skin infections.",
                Price = 140m, StockQuantity = 110,
                Manufacturer = "Beecham Pakistan",
                PrescriptionRequired = false,
            },
            new() {
                Name = "ORS Sachets (Pack of 10)", Brand = "Hilton", CategoryId = Cat("First Aid"),
                Description = "Oral rehydration salts for dehydration.",
                Price = 120m, StockQuantity = 180,
                Manufacturer = "Hilton Pharma",
                PrescriptionRequired = false,
            },
        };

        await _db.Medicines.InsertManyAsync(medicines);
        _logger.LogInformation("Seeded {Count} medicines.", medicines.Count);
    }
}
