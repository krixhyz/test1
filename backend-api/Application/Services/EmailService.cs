using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.EntityFrameworkCore;
using WeatherAPI.Application.Common;
using WeatherAPI.Application.Interfaces;
using WeatherAPI.Domain.Entities;
using WeatherAPI.Infrastructure.Data;

namespace WeatherAPI.Application.Services;

public class EmailService : IEmailService
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _configuration;

    public EmailService(ApplicationDbContext db, IConfiguration configuration) {
        _db = db;
        _configuration = configuration;
    }

    // TODO: Configure email service (SMTP settings, credentials, etc.)
    public async Task<bool> SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            var emailFrom = _configuration["Email:From"] ?? "no-reply@vparts.com";
            var smtpHost = _configuration["Email:SmtpHost"] ?? "localhost";
            var smtpPort = int.TryParse(_configuration["Email:SmtpPort"], out var port) ? port : 25;
            var smtpUser = _configuration["Email:SmtpUser"];
            var smtpPass = _configuration["Email:SmtpPass"];
            var enableSsl = bool.TryParse(_configuration["Email:EnableSsl"], out var ssl) && ssl;

            using var message = new MailMessage(emailFrom, to, subject, body) { IsBodyHtml = false };
            using var client = new SmtpClient(smtpHost, smtpPort) {
                EnableSsl = enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            if (!string.IsNullOrWhiteSpace(smtpUser) && !string.IsNullOrWhiteSpace(smtpPass)) {
                client.Credentials = new NetworkCredential(smtpUser, smtpPass);
            }

            await client.SendMailAsync(message);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Email send failed: {ex.Message}");
            return false;
        }
    }

    public async Task<ApiResponse<object>> SendSalesInvoiceEmailAsync(Guid invoiceId) {
        var invoice = await _db.SalesInvoices
            .Include(s => s.Customer).ThenInclude(c => c.User)
            .Include(s => s.Items).ThenInclude(i => i.Part)
            .Include(s => s.Staff)
            .FirstOrDefaultAsync(s => s.Id == invoiceId);

        if (invoice == null)
            return ApiResponse<object>.Fail("Invoice not found.");

        var customerEmail = invoice.Customer?.User?.Email;
        if (string.IsNullOrWhiteSpace(customerEmail))
            return ApiResponse<object>.Fail("Customer email is not available.");

        var emailFrom = _configuration["Email:From"] ?? "no-reply@vparts.com";
        var smtpHost = _configuration["Email:SmtpHost"] ?? "localhost";
        var smtpPort = int.TryParse(_configuration["Email:SmtpPort"], out var port) ? port : 25;
        var smtpUser = _configuration["Email:SmtpUser"];
        var smtpPass = _configuration["Email:SmtpPass"];
        var enableSsl = bool.TryParse(_configuration["Email:EnableSsl"], out var ssl) && ssl;

        var subject = $"Your Invoice {invoice.InvoiceNumber} from VParts";
        var body = BuildInvoiceBody(invoice);

        var emailLog = new EmailLog {
            ToEmail = customerEmail,
            Subject = subject,
            Status = "Pending",
            SentAt = DateTime.UtcNow
        };

        _db.EmailLogs.Add(emailLog);

        try {
            using var message = new MailMessage(emailFrom, customerEmail, subject, body) { IsBodyHtml = false };
            using var client = new SmtpClient(smtpHost, smtpPort) {
                EnableSsl = enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            if (!string.IsNullOrWhiteSpace(smtpUser) && !string.IsNullOrWhiteSpace(smtpPass)) {
                client.Credentials = new NetworkCredential(smtpUser, smtpPass);
            }

            await client.SendMailAsync(message);

            emailLog.Status = "Sent";
            invoice.EmailSent = true;
            await _db.SaveChangesAsync();
            return ApiResponse<object>.Ok(new { invoice.Id, invoice.EmailSent }, "Invoice email sent successfully.");
        } catch (Exception ex) {
            emailLog.Status = $"Failed: {ex.Message}";
            await _db.SaveChangesAsync();
            return ApiResponse<object>.Fail($"Failed to send invoice email: {ex.Message}");
        }
    }

    private static string BuildInvoiceBody(SalesInvoice invoice) {
        var builder = new StringBuilder();
        builder.AppendLine($"Invoice Number: {invoice.InvoiceNumber}");
        builder.AppendLine($"Date: {invoice.Date:yyyy-MM-dd HH:mm}");
        builder.AppendLine($"Customer: {invoice.Customer?.User?.FullName}");
        builder.AppendLine($"Email: {invoice.Customer?.User?.Email}");
        builder.AppendLine();
        builder.AppendLine("Items:");
        builder.AppendLine("Qty | Part | Unit Price | Total");
        builder.AppendLine(new string('-', 60));

        foreach (var item in invoice.Items) {
            var partName = item.Part?.PartName ?? "Unknown";
            var lineTotal = item.Quantity * item.UnitPrice;
            builder.AppendLine($"{item.Quantity,3} | {partName,-25} | {item.UnitPrice,10:C} | {lineTotal,10:C}");
        }

        builder.AppendLine();
        builder.AppendLine($"Subtotal: {invoice.Subtotal:C}");
        builder.AppendLine($"Discount: {invoice.Discount:C}");
        builder.AppendLine($"Total: {invoice.TotalAmount:C}");
        builder.AppendLine($"Payment status: {invoice.PaymentStatus}");
        if (invoice.CreditDueDate.HasValue)
            builder.AppendLine($"Credit due date: {invoice.CreditDueDate:yyyy-MM-dd}");

        builder.AppendLine();
        builder.AppendLine("Thank you for your purchase at VParts.");
        return builder.ToString();
    }
}
