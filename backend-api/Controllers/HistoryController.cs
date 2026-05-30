using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WeatherAPI.Infrastructure.Data;
using WeatherAPI.Application.Common;
using WeatherAPI.Application.DTOs;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Controllers;

[ApiController][Route("api/[controller]")]
[Authorize]
public class HistoryController : ControllerBase {
    private readonly ApplicationDbContext _db;
    
    public HistoryController(ApplicationDbContext db) => _db = db;

    private async Task<bool> CanAccessCustomerHistory(Guid customerId) {
        var userId = GetCurrentUserId();
        if (userId == null) return false;

        // Get user manager from DI
        var userManager = HttpContext.RequestServices.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>>();
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        var roles = await userManager.GetRolesAsync(user);

        // Staff and Admin can access any customer's history
        if (roles.Contains("Staff") || roles.Contains("Admin")) return true;

        // Customers can only access their own history
        var customer = await _db.Customers.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == customerId && c.UserId == userId);
        return customer != null;
    }

    private Guid? GetCurrentUserId() {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userIdClaim, out var userId)) return userId;
        return null;
    }

    /// <summary>
    /// Get purchase history for a specific customer
    /// </summary>
    [HttpGet("purchases/{customerId}")]
    public async Task<IActionResult> GetPurchaseHistory(Guid customerId) {
        if (!await CanAccessCustomerHistory(customerId)) 
            return Forbid();

        var purchases = await _db.SalesInvoices.AsNoTracking()
            .Include(s => s.Items).ThenInclude(i => i.Part)
            .Include(s => s.Customer).ThenInclude(c => c!.User)
            .Where(s => s.CustomerId == customerId)
            .OrderByDescending(s => s.Date)
            .ToListAsync();

        var response = purchases.Select(p => new {
            p.Id,
            p.InvoiceNumber,
            p.Date,
            p.TotalAmount,
            p.PaymentStatus,
            Items = p.Items.Select(i => new {
                i.Part.PartName,
                i.Quantity,
                i.UnitPrice,
                LineTotal = i.LineTotal
            })
        });

        return Ok(ApiResponse<object>.Ok(response, $"Found {purchases.Count} purchases."));
    }

    /// <summary>
    /// Get service history (completed appointments) for a specific customer
    /// </summary>
    [HttpGet("services/{customerId}")]
    public async Task<IActionResult> GetServiceHistory(Guid customerId) {
        if (!await CanAccessCustomerHistory(customerId)) 
            return Forbid();

        // Get completed appointments which represent services
        var services = await _db.Appointments.AsNoTracking()
            .Include(a => a.Customer).ThenInclude(c => c.User)
            .Include(a => a.Vehicle)
            .Where(a => a.CustomerId == customerId && a.Status == "Completed")
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();

        // Also get service history records if they exist
        var serviceHistoryRecords = await _db.ServiceHistories.AsNoTracking()
            .Include(sh => sh.Customer).ThenInclude(c => c.User)
            .Include(sh => sh.Vehicle)
            .Where(sh => sh.CustomerId == customerId)
            .OrderByDescending(sh => sh.ServiceDate)
            .ToListAsync();

        var response = new {
            Appointments = services.Select(a => new {
                a.Id,
                a.ServiceType,
                Date = a.AppointmentDate,
                a.Description,
                Vehicle = new { a.Vehicle.VehicleNumber, a.Vehicle.Brand, a.Vehicle.Model },
                Status = "Completed"
            }),
            ServiceHistory = serviceHistoryRecords.Select(sh => new {
                sh.Id,
                sh.ServiceType,
                Date = sh.ServiceDate,
                sh.Description,
                sh.Technician,
                sh.Cost,
                Vehicle = new { sh.Vehicle.VehicleNumber, sh.Vehicle.Brand, sh.Vehicle.Model }
            })
        };

        return Ok(ApiResponse<object>.Ok(response, "Service history retrieved."));
    }

    /// <summary>
    /// Get all history (purchases and services) for a customer
    /// </summary>
    [HttpGet("{customerId}")]
    public async Task<IActionResult> GetAllHistory(Guid customerId) {
        if (!await CanAccessCustomerHistory(customerId)) 
            return Forbid();

        var purchases = await _db.SalesInvoices.AsNoTracking()
            .Include(s => s.Items).ThenInclude(i => i.Part)
            .Where(s => s.CustomerId == customerId)
            .OrderByDescending(s => s.Date)
            .ToListAsync();

        var services = await _db.Appointments.AsNoTracking()
            .Include(a => a.Vehicle)
            .Where(a => a.CustomerId == customerId && a.Status == "Completed")
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();

        var response = new {
            PurchaseHistory = purchases.Select(p => new {
                p.Id,
                p.InvoiceNumber,
                p.Date,
                p.TotalAmount,
                p.PaymentStatus,
                ItemCount = p.Items.Count
            }),
            ServiceHistory = services.Select(s => new {
                s.Id,
                s.ServiceType,
                Date = s.AppointmentDate,
                s.Description
            })
        };

        return Ok(ApiResponse<object>.Ok(response));
    }
}
