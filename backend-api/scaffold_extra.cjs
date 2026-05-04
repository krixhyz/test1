const fs = require('fs');
const path = require('path');
const root = __dirname;
const write = (filepath, content) => {
  const full = path.join(root, filepath);
  fs.mkdirSync(path.dirname(full), { recursive: true });
  fs.writeFileSync(full, content.trim() + '\n');
};

// PurchaseInvoicesController
write('Controllers/PurchaseInvoicesController.cs', `
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
}`);

// AppointmentsController
write('Controllers/AppointmentsController.cs', `
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WeatherAPI.Domain.Entities;
using WeatherAPI.Infrastructure.Data;
using WeatherAPI.Application.Common;

namespace WeatherAPI.Controllers;

[ApiController][Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase {
    private readonly ApplicationDbContext _db;
    public AppointmentsController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Appointments.Include(a => a.Customer).ThenInclude(c => c.User).Include(a => a.Vehicle).ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id) {
        var a = await _db.Appointments.Include(a => a.Customer).ThenInclude(c => c.User).Include(a => a.Vehicle).FirstOrDefaultAsync(a => a.Id == id);
        return a == null ? NotFound() : Ok(a);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AppointmentCreateDto dto) {
        if (dto.AppointmentDate < DateTime.UtcNow)
            return BadRequest(ApiResponse<object>.Fail("Appointment date cannot be in the past."));
        var appointment = new Appointment {
            CustomerId = dto.CustomerId, VehicleId = dto.VehicleId,
            AppointmentDate = dto.AppointmentDate, ServiceType = dto.ServiceType, Description = dto.Description
        };
        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<Appointment>.Ok(appointment, "Appointment booked."));
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] string status) {
        var a = await _db.Appointments.FindAsync(id);
        if (a == null) return NotFound();
        a.Status = status;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(null, "Status updated."));
    }
}

public class AppointmentCreateDto {
    public Guid CustomerId { get; set; }
    public Guid VehicleId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}`);

// ReviewsController
write('Controllers/ReviewsController.cs', `
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WeatherAPI.Domain.Entities;
using WeatherAPI.Infrastructure.Data;
using WeatherAPI.Application.Common;

namespace WeatherAPI.Controllers;

[ApiController][Route("api/[controller]")]
[Authorize]
public class ReviewsController : ControllerBase {
    private readonly ApplicationDbContext _db;
    public ReviewsController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Reviews.Include(r => r.Customer).ThenInclude(c => c.User).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReviewCreateDto dto) {
        if (dto.Rating < 1 || dto.Rating > 5) return BadRequest(ApiResponse<object>.Fail("Rating must be between 1 and 5."));
        var review = new Review { CustomerId = dto.CustomerId, AppointmentId = dto.AppointmentId, Rating = dto.Rating, Comment = dto.Comment };
        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<Review>.Ok(review, "Review submitted."));
    }
}

public class ReviewCreateDto {
    public Guid CustomerId { get; set; }
    public Guid? AppointmentId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}`);

// UnavailablePartRequestsController
write('Controllers/UnavailablePartRequestsController.cs', `
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WeatherAPI.Domain.Entities;
using WeatherAPI.Infrastructure.Data;
using WeatherAPI.Application.Common;

namespace WeatherAPI.Controllers;

[ApiController][Route("api/[controller]")]
[Authorize]
public class UnavailablePartRequestsController : ControllerBase {
    private readonly ApplicationDbContext _db;
    public UnavailablePartRequestsController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.UnavailablePartRequests.Include(r => r.Customer).ThenInclude(c => c.User).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PartRequestCreateDto dto) {
        var req = new UnavailablePartRequest { CustomerId = dto.CustomerId, PartName = dto.PartName, Description = dto.Description };
        _db.UnavailablePartRequests.Add(req);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<UnavailablePartRequest>.Ok(req, "Part request submitted."));
    }
}

public class PartRequestCreateDto {
    public Guid CustomerId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}`);

// NotificationsController
write('Controllers/NotificationsController.cs', `
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WeatherAPI.Infrastructure.Data;

namespace WeatherAPI.Controllers;

[ApiController][Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase {
    private readonly ApplicationDbContext _db;
    public NotificationsController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _db.Notifications.OrderByDescending(n => n.CreatedAt).Take(50).ToListAsync());

    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkRead(Guid id) {
        var n = await _db.Notifications.FindAsync(id);
        if (n == null) return NotFound();
        n.IsRead = true;
        await _db.SaveChangesAsync();
        return Ok();
    }
}`);

// AiController
write('Controllers/AiController.cs', `
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WeatherAPI.Application.Services;

namespace WeatherAPI.Controllers;

[ApiController][Route("api/[controller]")]
[Authorize]
public class AiController : ControllerBase {
    private readonly AiPredictionService _ai;
    public AiController(AiPredictionService ai) => _ai = ai;

    [HttpGet("predictions/customer/{customerId}")]
    public async Task<IActionResult> GetPredictions(Guid customerId) {
        var result = await _ai.PredictFailureAsync(customerId);
        return Ok(result);
    }
}`);

// Extended SalesInvoicesController - add customer endpoint
write('Controllers/SalesInvoicesController.cs', `
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WeatherAPI.Application.Services;
using WeatherAPI.Application.DTOs;
using WeatherAPI.Infrastructure.Data;
using WeatherAPI.Application.Common;
using System.Security.Claims;

namespace WeatherAPI.Controllers;

[ApiController][Route("api/[controller]")]
[Authorize(Roles = "Staff,Admin")]
public class SalesInvoicesController : ControllerBase {
    private readonly SalesService _sales;
    private readonly ApplicationDbContext _db;
    public SalesInvoicesController(SalesService sales, ApplicationDbContext db) { _sales = sales; _db = db; }

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.SalesInvoices.Include(s => s.Customer).ThenInclude(c => c.User)
            .Include(s => s.Items).ThenInclude(i => i.Part).ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id) {
        var inv = await _db.SalesInvoices.Include(s => s.Customer).ThenInclude(c => c.User)
            .Include(s => s.Items).ThenInclude(i => i.Part).FirstOrDefaultAsync(s => s.Id == id);
        return inv == null ? NotFound() : Ok(inv);
    }

    [HttpGet("customer/{customerId}")]
    [Authorize]
    public async Task<IActionResult> GetByCustomer(Guid customerId) =>
        Ok(await _db.SalesInvoices.Include(s => s.Items).ThenInclude(i => i.Part)
            .Where(s => s.CustomerId == customerId).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create(CreateSalesInvoiceDto dto) {
        var staffIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(staffIdStr, out var staffId)) return Unauthorized();
        var res = await _sales.CreateSalesInvoiceAsync(dto, staffId);
        return Ok(res);
    }
}`);

console.log('Done writing all extra controllers.');
