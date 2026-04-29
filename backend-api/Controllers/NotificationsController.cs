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
}
