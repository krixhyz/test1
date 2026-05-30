using WeatherAPI.Application.Common;

namespace WeatherAPI.Application.Interfaces;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string body);
    Task<ApiResponse<object>> SendSalesInvoiceEmailAsync(Guid invoiceId);
}

