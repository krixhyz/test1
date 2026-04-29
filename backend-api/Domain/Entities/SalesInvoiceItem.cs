namespace WeatherAPI.Domain.Entities;
public class SalesInvoiceItem {
    public Guid Id { get; set; }
    public Guid SalesInvoiceId { get; set; }
    public SalesInvoice SalesInvoice { get; set; } = null!;
    public Guid PartId { get; set; }
    public Part Part { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal => Quantity * UnitPrice;
}