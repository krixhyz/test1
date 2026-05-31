using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WeatherAPI.Infrastructure.Data;
using WeatherAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using WeatherAPI.Application.Common;

namespace WeatherAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class EmailLogsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public EmailLogsController(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var logs = await _db.EmailLogs.AsNoTracking()
            .OrderByDescending(e => e.SentAt)
            .Take(500)
            .Select(e => new {
                e.Id,
                e.ToEmail,
                e.Subject,
                e.Status,
                e.SentAt,
                e.InvoiceId,
                e.CustomerId
            })
            .ToListAsync();
        return Ok(WeatherAPI.Application.Common.ApiResponse<object>.Ok(logs));
    }

    [HttpPost("seed")]
    public async Task<IActionResult> Seed()
    {
        // Only allow seeding in Development to avoid accidental production data changes
        if (!_env.IsDevelopment()) return Forbid();

        if (await _db.EmailLogs.AnyAsync()) return BadRequest(WeatherAPI.Application.Common.ApiResponse<object>.Fail("Email logs already exist"));

        var now = DateTime.UtcNow;
        var sample = new List<EmailLog> {
            new EmailLog { ToEmail = "customer3@vparts.com", Subject = "Overdue Credit Reminder - INV-20260320001", Status = "Sent", SentAt = now.AddDays(-1) },
            new EmailLog { ToEmail = "customer1@vparts.com", Subject = "Your Invoice INV-20260530001 - VParts", Status = "Sent", SentAt = now },
            new EmailLog { ToEmail = "invalid@@broken.c", Subject = "Invoice Delivery Test", Status = "Failed", SentAt = now.AddDays(-3) },
        };
        _db.EmailLogs.AddRange(sample);
        await _db.SaveChangesAsync();
        return Ok(WeatherAPI.Application.Common.ApiResponse<object>.Ok(sample, "Seeded sample logs"));
    }

    // Dev endpoints - only available in Development environment
    [HttpGet]
    [Route("/api/dev/emaillogs")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDev()
    {
        if (!_env.IsDevelopment()) return NotFound();
        var logs = await _db.EmailLogs.AsNoTracking().OrderByDescending(e => e.SentAt).ToListAsync();
        return Ok(WeatherAPI.Application.Common.ApiResponse<object>.Ok(logs));
    }

    [HttpPost]
    [Route("/api/dev/emaillogs/seed")]
    [AllowAnonymous]
    public async Task<IActionResult> SeedDev()
    {
        if (!_env.IsDevelopment()) return NotFound();
        // reuse DataSeeder-like samples
        if (await _db.EmailLogs.AnyAsync()) return BadRequest(WeatherAPI.Application.Common.ApiResponse<object>.Fail("Email logs already exist"));
        var now = DateTime.UtcNow;
        _db.EmailLogs.AddRange(new List<EmailLog> {
            new EmailLog { ToEmail = "dev1@vparts.local", Subject = "Dev Seed 1", Status = "Sent", SentAt = now },
            new EmailLog { ToEmail = "dev2@vparts.local", Subject = "Dev Seed 2", Status = "Failed", SentAt = now.AddDays(-2) },
        });
        await _db.SaveChangesAsync();
        return Ok(WeatherAPI.Application.Common.ApiResponse<object>.Ok(null, "Seeded dev logs"));
    }
}
