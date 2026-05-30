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
public class SalesInvoiceResponseDto {
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Guid StaffId { get; set; }
    public DateTime Date { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public int DiscountPercent { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime? CreditDueDate { get; set; }
    public bool EmailSent { get; set; }
}