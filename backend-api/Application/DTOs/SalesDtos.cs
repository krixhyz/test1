namespace WeatherAPI.Application.DTOs;
public class CreateSalesInvoiceDto {
    public Guid CustomerId { get; set; }
    public string PaymentStatus { get; set; } = "Paid";
    public DateTime? CreditDueDate { get; set; }
    public List<SalesItemDto> Items { get; set; } = new();
}
public class SalesItemDto {
    public Guid PartId { get; set; }
    public int Quantity { get; set; }
}