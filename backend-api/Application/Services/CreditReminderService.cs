using Microsoft.EntityFrameworkCore;
using WeatherAPI.Application.Interfaces;
using WeatherAPI.Infrastructure.Data;

namespace WeatherAPI.Application.Services;

public class CreditReminderService : ICreditReminderService
{
    private readonly ApplicationDbContext _db;
    private readonly IEmailService _email;
    private readonly ILogger<CreditReminderService> _logger;

    public CreditReminderService(ApplicationDbContext db, IEmailService email, ILogger<CreditReminderService> logger)
    {
        _db = db;
        _email = email;
        _logger = logger;
    }

    public async Task<int> SendCreditRemindersAsync()
    {
        var now = DateTime.UtcNow;
        var overdueInvoices = await _db.SalesInvoices
            .Include(s => s.Customer).ThenInclude(c => c!.User)
            .Where(s => (s.PaymentStatus == "Credit" || s.PaymentStatus == "Partial")
                        && s.CreditDueDate < now)
            .ToListAsync();

        int sent = 0;
        foreach (var invoice in overdueInvoices)
        {
            var email = invoice.Customer?.User?.Email;
            if (string.IsNullOrWhiteSpace(email)) continue;

            // Idempotency: skip if a reminder was already logged today for this invoice
            var today = now.Date;
            var alreadySent = await _db.EmailLogs.AnyAsync(e =>
                e.ToEmail == email &&
                e.Subject!.Contains(invoice.InvoiceNumber) &&
                e.SentAt.Date == today &&
                e.Status == "Sent");
            if (alreadySent) continue;

            var daysOverdue = (int)(now - invoice.CreditDueDate!.Value).TotalDays;
            var subject = $"Payment Reminder — Invoice {invoice.InvoiceNumber} is {daysOverdue} days overdue";
            var body = BuildReminderBody(invoice.Customer!.User!.FullName, invoice.InvoiceNumber,
                invoice.TotalAmount, invoice.Date, daysOverdue);

            var log = new WeatherAPI.Domain.Entities.EmailLog {
                ToEmail = email,
                Subject = subject,
                Status  = "Pending",
                SentAt  = now,
            };
            _db.EmailLogs.Add(log);

            try
            {
                var ok = await _email.SendEmailAsync(email, subject, body);
                log.Status = ok ? "Sent" : "Failed";
                if (ok) sent++;
            }
            catch (Exception ex)
            {
                log.Status = $"Failed: {ex.Message}";
                _logger.LogError(ex, "Credit reminder email failed for invoice {InvoiceNumber}", invoice.InvoiceNumber);
            }
        }

        await _db.SaveChangesAsync();
        return sent;
    }

    public async Task<bool> SendCreditReminderAsync(Guid invoiceId)
    {
        var invoice = await _db.SalesInvoices
            .Include(s => s.Customer).ThenInclude(c => c!.User)
            .FirstOrDefaultAsync(s => s.Id == invoiceId);

        if (invoice == null) return false;

        var email = invoice.Customer?.User?.Email;
        if (string.IsNullOrWhiteSpace(email)) return false;

        var now = DateTime.UtcNow;
        var daysOverdue = invoice.CreditDueDate.HasValue
            ? (int)(now - invoice.CreditDueDate.Value).TotalDays
            : 0;

        var subject = $"Payment Reminder — Invoice {invoice.InvoiceNumber} overdue by {daysOverdue} day(s)";
        var body = BuildReminderBody(invoice.Customer!.User!.FullName, invoice.InvoiceNumber,
            invoice.TotalAmount, invoice.Date, daysOverdue);

        var log = new WeatherAPI.Domain.Entities.EmailLog {
            ToEmail = email,
            Subject = subject,
            Status  = "Pending",
            SentAt  = now,
        };
        _db.EmailLogs.Add(log);

        try
        {
            var ok = await _email.SendEmailAsync(email, subject, body);
            log.Status = ok ? "Sent" : "Failed";
            await _db.SaveChangesAsync();
            return ok;
        }
        catch (Exception ex)
        {
            log.Status = $"Failed: {ex.Message}";
            await _db.SaveChangesAsync();
            _logger.LogError(ex, "Credit reminder failed for invoice {InvoiceId}", invoiceId);
            return false;
        }
    }

    private static string BuildReminderBody(string customerName, string invoiceNumber,
        decimal amount, DateTime invoiceDate, int daysOverdue)
    {
        return $"""
            Dear {customerName},

            This is a reminder that the following invoice is overdue:

              Invoice Number : {invoiceNumber}
              Invoice Date   : {invoiceDate:yyyy-MM-dd}
              Amount Due     : Rs. {amount:N2}
              Days Overdue   : {daysOverdue} day(s)

            Please contact us at your earliest convenience to settle this balance.

            Thank you,
            VehicleParts Team
            """;
    }
}
