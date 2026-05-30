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
            // Support multiple config keys (user may use different secret names)
            string? GetCfg(params string[] keys) {
                foreach (var k in keys) {
                    var v = _configuration[k];
                    if (!string.IsNullOrWhiteSpace(v)) return v;
                }
                return null;
            }

            var emailFrom = GetCfg("Email:From", "Email:Sender", "EmailSettings:SenderEmail") ?? "no-reply@vparts.com";
            var smtpHost = GetCfg("Email:SmtpHost", "EmailSettings:Host") ?? "localhost";
            var smtpPort = int.TryParse(GetCfg("Email:SmtpPort", "EmailSettings:Port"), out var port) ? port : 25;
            var smtpUser = GetCfg("Email:SmtpUser", "Email:Sender", "EmailSettings:SenderEmail");
            var smtpPass = GetCfg("Email:SmtpPass", "Email:AppPassword", "EmailSettings:SenderPassword");
            var enableSsl = bool.TryParse(GetCfg("Email:EnableSsl", "EmailSettings:EnableSsl"), out var ssl) && ssl;

            // Basic validation to give clearer errors earlier
            if (string.IsNullOrWhiteSpace(to) || string.IsNullOrWhiteSpace(emailFrom)) {
                Console.WriteLine("Email send failed: missing to or from address.");
                return false;
            }

            using var message = new MailMessage(emailFrom, to, subject, body) { IsBodyHtml = false };
            using var client = new SmtpClient(smtpHost, smtpPort) {
                EnableSsl = enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            if (!string.IsNullOrWhiteSpace(smtpUser) && !string.IsNullOrWhiteSpace(smtpPass)) {
                client.Credentials = new NetworkCredential(smtpUser, smtpPass);
            } else {
                // If no credentials provided, ensure host is localhost (relay) otherwise warn
                if (!string.Equals(smtpHost, "localhost", StringComparison.OrdinalIgnoreCase))
                    Console.WriteLine("Email service: SMTP credentials not provided for host " + smtpHost + ". Check configuration.");
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

        string? GetCfg(params string[] keys) {
            foreach (var k in keys) {
                var v = _configuration[k];
                if (!string.IsNullOrWhiteSpace(v)) return v;
            }
            return null;
        }

        var emailFrom = GetCfg("Email:From", "Email:Sender", "EmailSettings:SenderEmail") ?? "no-reply@vparts.com";
        var smtpHost = GetCfg("Email:SmtpHost", "EmailSettings:Host") ?? "localhost";
        var smtpPort = int.TryParse(GetCfg("Email:SmtpPort", "EmailSettings:Port"), out var port) ? port : 25;
        var smtpUser = GetCfg("Email:SmtpUser", "Email:Sender", "EmailSettings:SenderEmail");
        var smtpPass = GetCfg("Email:SmtpPass", "Email:AppPassword", "EmailSettings:SenderPassword");
        var enableSsl = bool.TryParse(GetCfg("Email:EnableSsl", "EmailSettings:EnableSsl"), out var ssl) && ssl;

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
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            if (!string.IsNullOrWhiteSpace(smtpUser) && !string.IsNullOrWhiteSpace(smtpPass)) {
                client.Credentials = new NetworkCredential(smtpUser, smtpPass);
            } else if (!string.Equals(smtpHost, "localhost", StringComparison.OrdinalIgnoreCase)) {
                Console.WriteLine("Email service: SMTP credentials not provided for host " + smtpHost + ". Check configuration.");
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
