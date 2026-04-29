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
        Ok(await _db.UnavailablePartRequests.AsNoTracking().Include(r => r.Customer).ThenInclude(c => c.User).OrderByDescending(r => r.RequestDate).ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id) {
        var r = await _db.UnavailablePartRequests.AsNoTracking().Include(r => r.Customer).ThenInclude(c => c.User).FirstOrDefaultAsync(r => r.Id == id);
        return r == null ? NotFound(ApiResponse<object>.Fail("Request not found.")) : Ok(ApiResponse<object>.Ok(r));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PartRequestCreateDto dto) {
        var req = new UnavailablePartRequest { CustomerId = dto.CustomerId, PartName = dto.PartName, Description = dto.Description, VehicleId = dto.VehicleId, Urgency = dto.Urgency ?? "Medium" };
        _db.UnavailablePartRequests.Add(req);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = req.Id }, ApiResponse<UnavailablePartRequest>.Ok(req, "Part request submitted."));
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] string status) {
        var req = await _db.UnavailablePartRequests.FindAsync(id);
        if (req == null) return NotFound(ApiResponse<object>.Fail("Request not found."));
        req.Status = status;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { req.Id, req.Status }));
    }
}

public class PartRequestCreateDto {
    public Guid CustomerId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? VehicleId { get; set; }
    public string? Urgency { get; set; }
}
