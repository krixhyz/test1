using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
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
        Ok(await _db.Reviews.AsNoTracking().Include(r => r.Customer).ThenInclude(c => c.User).OrderByDescending(r => r.Date).ToListAsync());

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetByCustomer(Guid customerId) =>
        Ok(await _db.Reviews.AsNoTracking().Where(r => r.CustomerId == customerId).OrderByDescending(r => r.Date).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReviewCreateDto dto) {
        if (dto.Rating < 1 || dto.Rating > 5)
            return BadRequest(ApiResponse<object>.Fail("Rating must be between 1 and 5."));
        
        // If AppointmentId is provided, verify the appointment is completed
        if (dto.AppointmentId.HasValue) {
            var appointment = await _db.Appointments.FindAsync(dto.AppointmentId.Value);
            if (appointment == null)
                return NotFound(ApiResponse<object>.Fail("Appointment not found."));
            if (appointment.Status != "Completed")
                return BadRequest(ApiResponse<object>.Fail("Review can only be submitted for completed appointments."));
        }
        
        var review = new Review { 
            CustomerId = dto.CustomerId, 
            AppointmentId = dto.AppointmentId, 
            Rating = dto.Rating, 
            Comment = dto.Comment,
            Date = DateTime.UtcNow
        };
        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), ApiResponse<Review>.Ok(review, "Review submitted."));
    }
}

public class ReviewCreateDto {
    public Guid CustomerId { get; set; }
    public Guid? AppointmentId { get; set; }
    [Range(1, 5)] public int Rating { get; set; }
    [Required] public string Comment { get; set; } = string.Empty;
}
