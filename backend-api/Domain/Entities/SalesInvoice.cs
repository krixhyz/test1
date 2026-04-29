namespace WeatherAPI.Domain.Entities;
public class SalesInvoice {
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public Guid StaffId { get; set; }
    public ApplicationUser Staff { get; set; } = null!;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime? CreditDueDate { get; set; }
    public bool EmailSent { get; set; }
    public ICollection<SalesInvoiceItem> Items { get; set; } = new List<SalesInvoiceItem>();
}