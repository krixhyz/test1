namespace WeatherAPI.Domain.Entities;
public class Part {
    public Guid Id { get; set; }
    public string PartName { get; set; } = string.Empty;
    public string PartCode { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
    public int ReorderLevel { get; set; }
    public Guid VendorId { get; set; }
    public Vendor Vendor { get; set; } = null!;
}