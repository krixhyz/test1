using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WeatherAPI.Application.Services;
using WeatherAPI.Application.DTOs;
using WeatherAPI.Infrastructure.Data;
using WeatherAPI.Application.Common;
using WeatherAPI.Domain.Entities;
using System.Security.Claims;

namespace WeatherAPI.Controllers;

[ApiController][Route("api/[controller]")]
[Authorize(Roles = "Staff,Admin")]
public class SalesInvoicesController : ControllerBase {
    private readonly ApplicationDbContext _db;
    public SalesInvoicesController(ApplicationDbContext db) { _db = db; }

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.SalesInvoices.AsNoTracking()
            .Include(s => s.Customer).ThenInclude(c => c!.User)
            .Include(s => s.Items).ThenInclude(i => i.Part)
            .OrderByDescending(s => s.Date).ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id) {
        var inv = await _db.SalesInvoices.AsNoTracking()
            .Include(s => s.Customer).ThenInclude(c => c!.User)
            .Include(s => s.Items).ThenInclude(i => i.Part)
            .FirstOrDefaultAsync(s => s.Id == id);
        return inv == null ? NotFound(ApiResponse<object>.Fail("Invoice not found.")) : Ok(ApiResponse<object>.Ok(inv));
    }

    [HttpGet("customer/{customerId}")]
    [Authorize]
    public async Task<IActionResult> GetByCustomer(Guid customerId) =>
        Ok(await _db.SalesInvoices.AsNoTracking()
            .Include(s => s.Items).ThenInclude(i => i.Part)
            .Where(s => s.CustomerId == customerId)
            .OrderByDescending(s => s.Date).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create(CreateSalesInvoiceDto dto) {
        var staffIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(staffIdStr, out var staffId)) return Unauthorized();
        if (dto.Items == null || dto.Items.Count == 0)
            return BadRequest(ApiResponse<object>.Fail("At least one item is required."));
        if ((dto.PaymentStatus == "Credit" || dto.PaymentStatus == "Partial") && dto.CreditDueDate == null)
            return BadRequest(ApiResponse<object>.Fail("Credit due date is required for Credit or Partial payment."));

        await using var tx = await _db.Database.BeginTransactionAsync();
        try {
            decimal subtotal = 0;
            var invoiceItems = new List<SalesInvoiceItem>();
            foreach (var item in dto.Items) {
                var part = await _db.Parts.FindAsync(item.PartId);
                if (part == null) return BadRequest(ApiResponse<object>.Fail($"Part {item.PartId} not found."));
                if (part.StockQuantity < item.Quantity)
                    return BadRequest(ApiResponse<object>.Fail($"Insufficient stock for {part.PartName}. Available: {part.StockQuantity}"));
                part.StockQuantity -= item.Quantity;
                subtotal += part.UnitPrice * item.Quantity;
                invoiceItems.Add(new SalesInvoiceItem { PartId = part.Id, Quantity = item.Quantity, UnitPrice = part.UnitPrice });
                if (part.StockQuantity <= part.ReorderLevel)
                    _db.Notifications.Add(new Notification { Title = "Low Stock Alert", Message = $"{part.PartName} stock is low ({part.StockQuantity} remaining).", Type = "LowStock" });
            }
            var discount = subtotal > 5000 ? subtotal * 0.10m : 0;
            var invoice = new SalesInvoice {
                CustomerId = dto.CustomerId, StaffId = staffId,
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMddHHmmss}",
                Subtotal = subtotal, Discount = discount, TotalAmount = subtotal - discount,
                PaymentStatus = dto.PaymentStatus, CreditDueDate = dto.CreditDueDate,
                Items = invoiceItems
            };
            _db.SalesInvoices.Add(invoice);
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            return CreatedAtAction(nameof(GetById), new { id = invoice.Id }, ApiResponse<SalesInvoice>.Ok(invoice, "Invoice created."));
        } catch { await tx.RollbackAsync(); throw; }
    }
}
