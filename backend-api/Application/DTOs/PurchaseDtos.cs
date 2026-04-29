namespace WeatherAPI.Application.DTOs;
public class CreatePurchaseInvoiceDto {
    public Guid VendorId { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<PurchaseItemDto> Items { get; set; } = new();
}
public class PurchaseItemDto {
    public Guid PartId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
}