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
        return Ok(ApiResponse<object>.Ok(new { a.Id, a.Status }, "Status updated."));
    }
}

public class AppointmentCreateDto {
    public Guid CustomerId { get; set; }
    public Guid VehicleId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
