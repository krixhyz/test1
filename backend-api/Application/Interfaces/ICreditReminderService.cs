namespace WeatherAPI.Application.Interfaces;

public interface ICreditReminderService
{
    Task<int> SendCreditRemindersAsync();
    Task<bool> SendCreditReminderAsync(Guid invoiceId);
}
