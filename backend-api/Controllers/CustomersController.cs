using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using WeatherAPI.Domain.Entities;
using WeatherAPI.Infrastructure.Data;
using WeatherAPI.Application.Common;
using WeatherAPI.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace WeatherAPI.Controllers;
[ApiController][Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase {
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    public CustomersController(ApplicationDbContext db, UserManager<ApplicationUser> userManager) {
        _db = db; _userManager = userManager;
    }

    [Authorize(Roles = "Staff,Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Customers.AsNoTracking().Include(c => c.User).Include(c => c.Vehicles).ToListAsync());

    [Authorize(Roles = "Customer")]
    [HttpGet("me")]
    public async Task<IActionResult> Me() {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();
        var customer = await _db.Customers
            .Include(c => c.User)
            .Include(c => c.Vehicles)
            .FirstOrDefaultAsync(c => c.UserId == userId.Value);

        // Self-heal legacy accounts that have Customer role but no Customer profile row yet.
        if (customer == null) {
            customer = new Customer { UserId = userId.Value, Address = string.Empty };
            _db.Customers.Add(customer);
            await _db.SaveChangesAsync();

            customer = await _db.Customers.AsNoTracking()
                .Include(c => c.User)
                .Include(c => c.Vehicles)
                .FirstOrDefaultAsync(c => c.UserId == userId.Value);
        }

        return customer == null
            ? NotFound(ApiResponse<object>.Fail("Customer profile not found."))
            : Ok(ApiResponse<object>.Ok(customer));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id) {
        if (!await CanAccessCustomer(id)) return Forbid();
        var c = await _db.Customers.AsNoTracking().Include(c => c.User).Include(c => c.Vehicles).FirstOrDefaultAsync(c => c.Id == id);
        return c == null ? NotFound(ApiResponse<object>.Fail("Customer not found.")) : Ok(ApiResponse<object>.Ok(c));
    }

    [Authorize(Roles = "Staff,Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateCustomerDto dto) {
        var user = new ApplicationUser {
            UserName = dto.Phone,
            Email = dto.Email ?? $"{dto.Phone}@vparts.local",
            FullName = dto.FullName,
            PhoneNumber = dto.Phone
        };
        var pass = "Customer@" + DateTime.UtcNow.Year;
        var res = await _userManager.CreateAsync(user, pass);
        if (!res.Succeeded) return BadRequest(ApiResponse<object>.Fail(string.Join(", ", res.Errors.Select(e => e.Description))));
        await _userManager.AddToRoleAsync(user, "Customer");
        var customer = new Customer { UserId = user.Id, Address = dto.Address ?? string.Empty };
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = customer.Id },
            ApiResponse<object>.Ok(new { customer.Id, user.FullName, user.PhoneNumber, defaultPassword = pass }));
    }

    [Authorize(Roles = "Staff,Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateCustomerDto dto) {
        var customer = await _db.Customers.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == id);
        if (customer == null) return NotFound(ApiResponse<object>.Fail("Customer not found."));
        customer.User.FullName = dto.FullName;
        customer.User.PhoneNumber = dto.Phone;
        if (dto.Email != null) customer.User.Email = dto.Email;
        customer.Address = dto.Address ?? string.Empty;
        await _userManager.UpdateAsync(customer.User);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { customer.Id, customer.User.FullName }));
    }

    [Authorize(Roles = "Staff,Admin")]
    [HttpGet("search")]
    public async Task<IActionResult> Search(string keyword) {
        var res = await _db.Customers.AsNoTracking().Include(c => c.User).Include(c => c.Vehicles)
            .Where(c => c.User.FullName.Contains(keyword) ||
                        (c.User.PhoneNumber != null && c.User.PhoneNumber.Contains(keyword)) ||
                        c.Vehicles.Any(v => v.VehicleNumber.Contains(keyword)))
            .ToListAsync();
        return Ok(res);
    }

    [HttpGet("{id}/vehicles")]
    public async Task<IActionResult> GetVehicles(Guid id) {
        if (!await CanAccessCustomer(id)) return Forbid();
        return Ok(await _db.CustomerVehicles.AsNoTracking().Where(v => v.CustomerId == id).ToListAsync());
    }

    [HttpPost("{id}/vehicles")]
    public async Task<IActionResult> AddVehicle(Guid id, CreateVehicleDto dto) {
        if (!await CanAccessCustomer(id)) return Forbid();
        var exists = await _db.Customers.AnyAsync(c => c.Id == id);
        if (!exists) return NotFound(ApiResponse<object>.Fail("Customer not found."));
        var dup = await _db.CustomerVehicles.AnyAsync(v => v.VehicleNumber == dto.VehicleNumber);
        if (dup) return BadRequest(ApiResponse<object>.Fail("Vehicle number already registered."));
        var vehicle = new CustomerVehicle {
            CustomerId = id,
            VehicleNumber = dto.VehicleNumber,
            VehicleType = dto.VehicleType,
            Brand = dto.Brand,
            Model = dto.Model ?? string.Empty
        };
        _db.CustomerVehicles.Add(vehicle);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetVehicles), new { id }, ApiResponse<CustomerVehicle>.Ok(vehicle));
    }

    [HttpPut("{id}/vehicles/{vehicleId}")]
    public async Task<IActionResult> UpdateVehicle(Guid id, Guid vehicleId, UpdateVehicleDto dto) {
        if (!await CanAccessCustomer(id)) return Forbid();
        var vehicle = await _db.CustomerVehicles.FirstOrDefaultAsync(v => v.Id == vehicleId && v.CustomerId == id);
        if (vehicle == null) return NotFound(ApiResponse<object>.Fail("Vehicle not found."));
        var dup = await _db.CustomerVehicles.AnyAsync(v => v.VehicleNumber == dto.VehicleNumber && v.Id != vehicleId);
        if (dup) return BadRequest(ApiResponse<object>.Fail("Vehicle number already registered."));

        vehicle.VehicleNumber = dto.VehicleNumber;
        vehicle.VehicleType = dto.VehicleType;
        vehicle.Brand = dto.Brand;
        vehicle.Model = dto.Model ?? string.Empty;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<CustomerVehicle>.Ok(vehicle, "Vehicle updated."));
    }

    [HttpDelete("{id}/vehicles/{vehicleId}")]
    public async Task<IActionResult> DeleteVehicle(Guid id, Guid vehicleId) {
        if (!await CanAccessCustomer(id)) return Forbid();
        var vehicle = await _db.CustomerVehicles.FirstOrDefaultAsync(v => v.Id == vehicleId && v.CustomerId == id);
        if (vehicle == null) return NotFound(ApiResponse<object>.Fail("Vehicle not found."));
        _db.CustomerVehicles.Remove(vehicle);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetHistory(Guid id) {
        if (!await CanAccessCustomer(id)) return Forbid();
        var purchases = await _db.SalesInvoices.AsNoTracking()
            .Include(s => s.Items).ThenInclude(i => i.Part)
            .Where(s => s.CustomerId == id).OrderByDescending(s => s.Date).ToListAsync();
        var appointments = await _db.Appointments.AsNoTracking()
            .Where(a => a.CustomerId == id).OrderByDescending(a => a.AppointmentDate).ToListAsync();
        return Ok(ApiResponse<object>.Ok(new { purchases, appointments }));
    }

    [HttpGet("{id}/credit-summary")]
    public async Task<IActionResult> GetCreditSummary(Guid id) {
        if (!await CanAccessCustomer(id)) return Forbid();
        var invoices = await _db.SalesInvoices.AsNoTracking()
            .Where(s => s.CustomerId == id && (s.PaymentStatus == "Credit" || s.PaymentStatus == "Partial"))
            .ToListAsync();
        return Ok(ApiResponse<object>.Ok(new {
            totalCredit = invoices.Sum(i => i.TotalAmount),
            overdueCredit = invoices.Where(i => i.CreditDueDate < DateTime.UtcNow).Sum(i => i.TotalAmount),
            invoices
        }));
    }

    private Guid? GetCurrentUserId() {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userId, out var parsed) ? parsed : null;
    }

    private async Task<bool> CanAccessCustomer(Guid customerId) {
        if (User.IsInRole("Admin") || User.IsInRole("Staff")) return true;
        if (!User.IsInRole("Customer")) return false;

        var userId = GetCurrentUserId();
        if (userId == null) return false;

        return await _db.Customers.AsNoTracking()
            .AnyAsync(c => c.Id == customerId && c.UserId == userId.Value);
    }
}
