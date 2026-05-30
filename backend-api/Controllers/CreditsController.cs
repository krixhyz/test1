using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WeatherAPI.Application.Common;
using WeatherAPI.Application.Interfaces;

namespace WeatherAPI.Controllers;

[ApiController]
[Route("api/credits")]
[Authorize(Roles = "Admin,Staff")]
public class CreditsController : ControllerBase
{
    private readonly ICreditReminderService _reminder;
    public CreditsController(ICreditReminderService reminder) => _reminder = reminder;

    /// <summary>Send overdue credit reminders to all eligible customers.</summary>
    [HttpPost("send-reminders")]
    public async Task<IActionResult> SendAll()
    {
        var sent = await _reminder.SendCreditRemindersAsync();
        return Ok(ApiResponse<object>.Ok(new { remindersSent = sent },
            $"{sent} reminder(s) sent."));
    }

    /// <summary>Send a credit reminder for a specific invoice — useful for demo.</summary>
    [HttpPost("send-reminder/{invoiceId:guid}")]
    public async Task<IActionResult> SendOne(Guid invoiceId)
    {
        var ok = await _reminder.SendCreditReminderAsync(invoiceId);
        return ok
            ? Ok(ApiResponse<object>.Ok(new { invoiceId, sent = true }, "Reminder sent."))
            : BadRequest(ApiResponse<object>.Fail("Failed to send reminder. Check SMTP config or invoice ID."));
    }
}
