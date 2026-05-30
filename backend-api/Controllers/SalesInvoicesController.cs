using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WeatherAPI.Application.Services;
using WeatherAPI.Application.Interfaces;
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
    private readonly SalesService _sales;
    private readonly IEmailService _emailService;

    public SalesInvoicesController(ApplicationDbContext db, SalesService sales, IEmailService emailService) { 
        _db = db; 
        _sales = sales; 
        _emailService = emailService; 
    }

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
        if (dto.PaymentStatus == "Credit" || dto.PaymentStatus == "Partial") {
            if (dto.CreditDueDate == null)
                return BadRequest(ApiResponse<object>.Fail("Credit due date is required for Credit or Partial payment."));
            if (dto.CreditDueDate.Value.Date < DateTime.Today)
                return BadRequest(ApiResponse<object>.Fail("Credit due date cannot be in the past."));
        }

        var res = await _sales.CreateSalesInvoiceAsync(dto, staffId);
        if (!res.Success) return BadRequest(res);
        var invoiceObj = res.Data as SalesInvoice;
        if (invoiceObj == null) return Ok(res);
        return CreatedAtAction(nameof(GetById), new { id = invoiceObj.Id }, ApiResponse<SalesInvoice>.Ok(invoiceObj, "Invoice created."));
    }

    [HttpPost("{id}/send-email")]
    public async Task<IActionResult> SendEmail(Guid id) {
        var result = await _emailService.SendSalesInvoiceEmailAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
