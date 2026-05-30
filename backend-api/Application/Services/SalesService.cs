using Microsoft.EntityFrameworkCore;
using WeatherAPI.Infrastructure.Data;
using WeatherAPI.Domain.Entities;
using WeatherAPI.Application.DTOs;
using WeatherAPI.Application.Common;
namespace WeatherAPI.Application.Services;
public class SalesService {
    private readonly ApplicationDbContext _db;
    private readonly InventoryService _inv;

    public SalesService(ApplicationDbContext db, InventoryService inv) { _db = db; _inv = inv; }

    public async Task<ApiResponse<object>> CreateSalesInvoiceAsync(CreateSalesInvoiceDto dto, Guid staffId) {
        decimal subtotal = 0;
        var invoice = new SalesInvoice {
            InvoiceNumber = "INV-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
            CustomerId = dto.CustomerId,
            StaffId = staffId,
            PaymentStatus = dto.PaymentStatus,
            CreditDueDate = dto.CreditDueDate
        };

        foreach (var item in dto.Items) {
            var part = await _db.Parts.FindAsync(item.PartId);
            if (part == null || part.StockQuantity < item.Quantity)
                return ApiResponse<object>.Fail($"Not enough stock for part {item.PartId}");

            part.StockQuantity -= item.Quantity; // Decrement stock
            var lineTotal = part.UnitPrice * item.Quantity;
            subtotal += lineTotal;
            invoice.Items.Add(new SalesInvoiceItem { PartId = part.Id, Quantity = item.Quantity, UnitPrice = part.UnitPrice });
        }

        invoice.Subtotal = subtotal;
        
        // Feature 21: Loyalty discount
        if (subtotal > WeatherAPI.Application.Common.PricingConstants.LoyaltyThreshold) invoice.Discount = subtotal * WeatherAPI.Application.Common.PricingConstants.LoyaltyDiscountRate;
        else invoice.Discount = 0;

        invoice.TotalAmount = subtotal - invoice.Discount;
        _db.SalesInvoices.Add(invoice);

        if (dto.PaymentStatus == "Credit") {
            var customer = await _db.Customers.FindAsync(dto.CustomerId);
            if (customer != null) customer.CreditBalance += invoice.TotalAmount;
        }

        await _db.SaveChangesAsync();

        foreach (var item in dto.Items) await _inv.CheckLowStockAsync(item.PartId);

        return ApiResponse<object>.Ok(invoice);
    }
}