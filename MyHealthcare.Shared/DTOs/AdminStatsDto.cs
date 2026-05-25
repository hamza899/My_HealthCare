namespace MyHealthcare.Shared.DTOs;

public class AdminStatsDto
{
    public long TotalMedicines { get; set; }
    public long TotalCustomers { get; set; }
    public long TotalOrders { get; set; }
    public long PendingOrders { get; set; }
    public long DeliveredOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<LowStockItem> LowStockMedicines { get; set; } = new();
}

public class LowStockItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
}
