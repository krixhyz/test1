using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WeatherAPI.Infrastructure.Data;
using WeatherAPI.Application.Common;
using WeatherAPI.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace WeatherAPI.Controllers;
[ApiController][Route("api/[controller]")]
[Authorize(Roles = "Admin,Staff")]
public class ReportsController : ControllerBase {
    private readonly ApplicationDbContext _db;
    public ReportsController(ApplicationDbContext db) => _db = db;

    [HttpGet("financial/daily")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> FinancialDaily([FromQuery] DateTime? date) {
        var d = date?.Date ?? DateTime.UtcNow.Date;
        var sales = await _db.SalesInvoices.AsNoTracking().Where(s => s.Date.Date == d).ToListAsync();
        var purchases = await _db.PurchaseInvoices.AsNoTracking().Where(p => p.Date.Date == d).ToListAsync();
        return Ok(ApiResponse<object>.Ok(BuildReport(sales, purchases, $"Daily: {d:yyyy-MM-dd}")));
    }

    [HttpGet("financial/monthly")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> FinancialMonthly([FromQuery] int? year, [FromQuery] int? month) {
        var y = year ?? DateTime.UtcNow.Year; var m = month ?? DateTime.UtcNow.Month;
        var sales = await _db.SalesInvoices.AsNoTracking().Where(s => s.Date.Year == y && s.Date.Month == m).ToListAsync();
        var purchases = await _db.PurchaseInvoices.AsNoTracking().Where(p => p.Date.Year == y && p.Date.Month == m).ToListAsync();
        return Ok(ApiResponse<object>.Ok(BuildReport(sales, purchases, $"Monthly: {y}-{m:D2}")));
    }

    [HttpGet("financial/yearly")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> FinancialYearly([FromQuery] int? year) {
        var y = year ?? DateTime.UtcNow.Year;
        var sales = await _db.SalesInvoices.AsNoTracking().Where(s => s.Date.Year == y).ToListAsync();
        var purchases = await _db.PurchaseInvoices.AsNoTracking().Where(p => p.Date.Year == y).ToListAsync();
        return Ok(ApiResponse<object>.Ok(BuildReport(sales, purchases, $"Yearly: {y}")));
    }

    [HttpGet("customers/pending-credits")]
    public async Task<IActionResult> GetPendingCredits() {
        var overdue = await _db.SalesInvoices.AsNoTracking()
            .Include(s => s.Customer).ThenInclude(c => c!.User)
            .Where(s => s.PaymentStatus == "Credit" && s.CreditDueDate < DateTime.UtcNow)
            .Select(s => new { s.Id, s.InvoiceNumber, s.TotalAmount, s.CreditDueDate, CustomerName = s.Customer!.User.FullName, s.Date })
            .ToListAsync();
        return Ok(ApiResponse<object>.Ok(overdue));
    }

    [HttpGet("customers/high-spenders")]
    public async Task<IActionResult> GetHighSpenders() {
        var result = await _db.SalesInvoices.AsNoTracking()
            .Include(s => s.Customer).ThenInclude(c => c!.User)
            .GroupBy(s => new { s.CustomerId, s.Customer!.User.FullName })
            .Select(g => new { g.Key.CustomerId, g.Key.FullName, TotalSpent = g.Sum(x => x.TotalAmount), InvoiceCount = g.Count() })
            .OrderByDescending(x => x.TotalSpent).Take(20).ToListAsync();
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("customers/regulars")]
    public async Task<IActionResult> GetRegulars() {
        var result = await _db.SalesInvoices.AsNoTracking()
            .Include(s => s.Customer).ThenInclude(c => c!.User)
            .GroupBy(s => new { s.CustomerId, s.Customer!.User.FullName })
            .Select(g => new { g.Key.CustomerId, g.Key.FullName, InvoiceCount = g.Count(), TotalSpent = g.Sum(x => x.TotalAmount), LastPurchase = g.Max(x => x.Date) })
            .Where(x => x.InvoiceCount >= 2)
            .OrderByDescending(x => x.InvoiceCount).ToListAsync();
        return Ok(ApiResponse<object>.Ok(result));
    }

    private static object BuildReport(List<Domain.Entities.SalesInvoice> sales, List<Domain.Entities.PurchaseInvoice> purchases, string period) =>
        new { Period = period, TotalSales = sales.Sum(s => s.TotalAmount + s.Discount), NetRevenue = sales.Sum(s => s.TotalAmount),
              TotalPurchases = purchases.Sum(p => p.TotalAmount), DiscountGiven = sales.Sum(s => s.Discount),
              CreditSales = sales.Where(s => s.PaymentStatus == "Credit").Sum(s => s.TotalAmount),
              SalesCount = sales.Count, PurchaseCount = purchases.Count };
}