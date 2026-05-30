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
    public async Task<IActionResult> FinancialDaily([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate) {
        List<Domain.Entities.SalesInvoice> sales;
        List<Domain.Entities.PurchaseInvoice> purchases;
        if (fromDate.HasValue && toDate.HasValue) {
            var f = DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc);
            var t = DateTime.SpecifyKind(toDate.Value.Date, DateTimeKind.Utc);
            sales = await _db.SalesInvoices.AsNoTracking().Where(s => s.Date.Date >= f && s.Date.Date <= t).ToListAsync();
            purchases = await _db.PurchaseInvoices.AsNoTracking().Where(p => p.Date.Date >= f && p.Date.Date <= t).ToListAsync();
            return Ok(ApiResponse<WeatherAPI.Application.DTOs.FinancialReportDto>.Ok(BuildReport(sales, purchases, $"Daily: {f:yyyy-MM-dd} to {t:yyyy-MM-dd}")));
        }
        var d = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
        sales = await _db.SalesInvoices.AsNoTracking().Where(s => s.Date.Date == d).ToListAsync();
        purchases = await _db.PurchaseInvoices.AsNoTracking().Where(p => p.Date.Date == d).ToListAsync();
        return Ok(ApiResponse<WeatherAPI.Application.DTOs.FinancialReportDto>.Ok(BuildReport(sales, purchases, $"Daily: {d:yyyy-MM-dd}")));
    }

    [HttpGet("financial/monthly")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> FinancialMonthly([FromQuery] int? year, [FromQuery] int? month, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate) {
        if (fromDate.HasValue && toDate.HasValue) {
            var f = DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc);
            var t = DateTime.SpecifyKind(toDate.Value.Date, DateTimeKind.Utc);
            var fSales = await _db.SalesInvoices.AsNoTracking().Where(s => s.Date.Date >= f && s.Date.Date <= t).ToListAsync();
            var fPurchases = await _db.PurchaseInvoices.AsNoTracking().Where(p => p.Date.Date >= f && p.Date.Date <= t).ToListAsync();
            return Ok(ApiResponse<WeatherAPI.Application.DTOs.FinancialReportDto>.Ok(BuildReport(fSales, fPurchases, $"Monthly: {f:yyyy-MM-dd} to {t:yyyy-MM-dd}")));
        }
        var y = year ?? DateTime.UtcNow.Year; var m = month ?? DateTime.UtcNow.Month;
        var sales = await _db.SalesInvoices.AsNoTracking().Where(s => s.Date.Year == y && s.Date.Month == m).ToListAsync();
        var purchases = await _db.PurchaseInvoices.AsNoTracking().Where(p => p.Date.Year == y && p.Date.Month == m).ToListAsync();
        return Ok(ApiResponse<WeatherAPI.Application.DTOs.FinancialReportDto>.Ok(BuildReport(sales, purchases, $"Monthly: {y}-{m:D2}")));
    }

    [HttpGet("financial/yearly")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> FinancialYearly([FromQuery] int? year, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate) {
        if (fromDate.HasValue && toDate.HasValue) {
            var f = DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc);
            var t = DateTime.SpecifyKind(toDate.Value.Date, DateTimeKind.Utc);
            var fSales = await _db.SalesInvoices.AsNoTracking().Where(s => s.Date.Date >= f && s.Date.Date <= t).ToListAsync();
            var fPurchases = await _db.PurchaseInvoices.AsNoTracking().Where(p => p.Date.Date >= f && p.Date.Date <= t).ToListAsync();
            return Ok(ApiResponse<WeatherAPI.Application.DTOs.FinancialReportDto>.Ok(BuildReport(fSales, fPurchases, $"Yearly: {f:yyyy-MM-dd} to {t:yyyy-MM-dd}")));
        }
        var y = year ?? DateTime.UtcNow.Year;
        var sales = await _db.SalesInvoices.AsNoTracking().Where(s => s.Date.Year == y).ToListAsync();
        var purchases = await _db.PurchaseInvoices.AsNoTracking().Where(p => p.Date.Year == y).ToListAsync();
        return Ok(ApiResponse<WeatherAPI.Application.DTOs.FinancialReportDto>.Ok(BuildReport(sales, purchases, $"Yearly: {y}")));
    }

    [HttpGet("customers/pending-credits")]
    public async Task<IActionResult> GetPendingCredits([FromQuery] int limit = 100) {
        var now = DateTime.UtcNow;
        var overdue = await _db.SalesInvoices.AsNoTracking()
            .Include(s => s.Customer).ThenInclude(c => c!.User)
            .Where(s => (s.PaymentStatus == "Credit" || s.PaymentStatus == "Partial") && s.CreditDueDate < now)
            .OrderBy(s => s.CreditDueDate)
            .Take(limit)
            .Select(s => new {
                s.Id,
                s.InvoiceNumber,
                s.TotalAmount,
                s.CreditDueDate,
                s.Date,
                CustomerId    = s.CustomerId,
                CustomerName  = s.Customer!.User.FullName,
                CustomerPhone = s.Customer!.User.PhoneNumber,
                CustomerEmail = s.Customer!.User.Email,
                DaysOverdue   = (int)(now - s.CreditDueDate!.Value).TotalDays,
            })
            .ToListAsync();
        return Ok(ApiResponse<object>.Ok(overdue));
    }

    [HttpGet("customers/high-spenders")]
    public async Task<IActionResult> GetHighSpenders([FromQuery] int limit = 20) {
        var raw = await _db.SalesInvoices.AsNoTracking()
            .Include(s => s.Customer).ThenInclude(c => c!.User)
            .GroupBy(s => new { s.CustomerId, s.Customer!.User.FullName, s.Customer!.User.PhoneNumber })
            .Select(g => new {
                CustomerId   = g.Key.CustomerId,
                FullName     = g.Key.FullName,
                Phone        = g.Key.PhoneNumber,
                TotalSpent   = g.Sum(x => x.TotalAmount),
                InvoiceCount = g.Count(),
            })
            .OrderByDescending(x => x.TotalSpent)
            .Take(limit)
            .ToListAsync();
        var result = raw.Select((r, i) => new {
            Rank         = i + 1,
            r.CustomerId,
            r.FullName,
            r.Phone,
            r.TotalSpent,
            r.InvoiceCount,
        });
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("customers/regulars")]
    public async Task<IActionResult> GetRegulars([FromQuery] int limit = 100) {
        var raw = await _db.SalesInvoices.AsNoTracking()
            .Include(s => s.Customer).ThenInclude(c => c!.User)
            .GroupBy(s => new { s.CustomerId, s.Customer!.User.FullName, s.Customer!.User.PhoneNumber })
            .Select(g => new {
                CustomerId   = g.Key.CustomerId,
                FullName     = g.Key.FullName,
                Phone        = g.Key.PhoneNumber,
                InvoiceCount = g.Count(),
                TotalSpent   = g.Sum(x => x.TotalAmount),
                LastPurchase = g.Max(x => x.Date),
            })
            .Where(x => x.InvoiceCount >= 1)
            .OrderByDescending(x => x.InvoiceCount)
            .Take(limit)
            .ToListAsync();
        var result = raw.Select((r, i) => new {
            Rank         = i + 1,
            r.CustomerId,
            r.FullName,
            r.Phone,
            r.InvoiceCount,
            r.TotalSpent,
            r.LastPurchase,
        });
        return Ok(ApiResponse<object>.Ok(result));
    }

    private static WeatherAPI.Application.DTOs.FinancialReportDto BuildReport(List<Domain.Entities.SalesInvoice> sales, List<Domain.Entities.PurchaseInvoice> purchases, string period) {
        var totalSubtotals  = sales.Sum(s => s.Subtotal);
        var discountGiven   = sales.Sum(s => s.Discount);
        var totalRevenue    = sales.Sum(s => s.TotalAmount); // post-discount actual revenue
        var totalPurchases  = purchases.Sum(p => p.TotalAmount);
        var netProfit       = totalRevenue - totalPurchases;
        var creditSales     = sales.Where(s => s.PaymentStatus == "Credit").Sum(s => s.TotalAmount);
        return new WeatherAPI.Application.DTOs.FinancialReportDto {
            Period           = period,
            TotalRevenue     = totalRevenue,
            TotalSales       = totalSubtotals,
            TotalPurchaseCost= totalPurchases,
            TotalPurchases   = totalPurchases,
            DiscountGiven    = discountGiven,
            NetProfit        = netProfit,
            NetRevenue       = netProfit,
            CreditSales      = creditSales,
            InvoiceCount     = sales.Count
        };
    }
}