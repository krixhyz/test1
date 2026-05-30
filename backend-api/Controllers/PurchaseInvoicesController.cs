using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WeatherAPI.Domain.Entities;
using WeatherAPI.Infrastructure.Data;
using WeatherAPI.Application.DTOs;
using WeatherAPI.Application.Common;

namespace WeatherAPI.Controllers;

[ApiController][Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class PurchaseInvoicesController : ControllerBase {
    private readonly ApplicationDbContext _db;
    public PurchaseInvoicesController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.PurchaseInvoices.Include(p => p.Vendor).Include(p => p.Items).ThenInclude(i => i.Part).ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id) {
        var inv = await _db.PurchaseInvoices.Include(p => p.Vendor).Include(p => p.Items).ThenInclude(i => i.Part).FirstOrDefaultAsync(p => p.Id == id);
        if (inv == null) return NotFound();
        return Ok(inv);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePurchaseInvoiceDto dto) {
        if (!dto.Items.Any()) return BadRequest(ApiResponse<object>.Fail("At least one item is required."));

        var invoice = new PurchaseInvoice {
            InvoiceNumber = "PUR-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
            VendorId = dto.VendorId,
            Notes = dto.Notes
        };

        decimal total = 0;
        foreach (var item in dto.Items) {
            if (item.Quantity <= 0) return BadRequest(ApiResponse<object>.Fail($"Invalid quantity for part {item.PartId}. Quantity must be greater than zero."));
            if (item.UnitCost < 0) return BadRequest(ApiResponse<object>.Fail($"Invalid unit cost for part {item.PartId}. Unit cost cannot be negative."));

            var part = await _db.Parts.FindAsync(item.PartId);
            if (part == null) return NotFound(ApiResponse<object>.Fail($"Part {item.PartId} not found"));

            part.StockQuantity += item.Quantity;
            var lineTotal = item.Quantity * item.UnitCost;
            total += lineTotal;
            invoice.Items.Add(new PurchaseInvoiceItem { PartId = item.PartId, Quantity = item.Quantity, UnitCost = item.UnitCost });
        }
        invoice.TotalAmount = total;
        _db.PurchaseInvoices.Add(invoice);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<PurchaseInvoice>.Ok(invoice, "Purchase invoice created."));
    }
}
