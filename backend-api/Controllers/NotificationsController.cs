using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WeatherAPI.Infrastructure.Data;
using WeatherAPI.Application.Common;

namespace WeatherAPI.Controllers;

[ApiController][Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class NotificationsController : ControllerBase {
    private readonly ApplicationDbContext _db;
    public NotificationsController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() {
        var notifications = await _db.Notifications
            .OrderByDescending(n => n.CreatedAt)
            .Take(100)
            .ToListAsync();
        return Ok(ApiResponse<object>.Ok(notifications));
    }

    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock() {
        var alerts = await _db.Notifications
            .Where(n => n.Type == "Alert" || n.Type == "LowStock")
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .ToListAsync();
        return Ok(ApiResponse<object>.Ok(alerts));
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount() {
        var count = await _db.Notifications.CountAsync(n => !n.IsRead);
        return Ok(ApiResponse<object>.Ok(new { count }));
    }

    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkRead(Guid id) {
        var n = await _db.Notifications.FindAsync(id);
        if (n == null) return NotFound();
        n.IsRead = true;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { id, isRead = true }));
    }

    [HttpPatch("mark-all-read")]
    public async Task<IActionResult> MarkAllRead() {
        var unread = await _db.Notifications.Where(n => !n.IsRead).ToListAsync();
        foreach (var n in unread) n.IsRead = true;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { marked = unread.Count }));
    }
}
