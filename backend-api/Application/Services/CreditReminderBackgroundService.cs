using WeatherAPI.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace WeatherAPI.Application.Services;

public class CreditReminderBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CreditReminderBackgroundService> _logger;

    // Run once at startup then every 24 hours
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);

    public CreditReminderBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<CreditReminderBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Credit reminder background service started.");

        // Wait until midnight UTC for first run, then every 24 h
        var now = DateTime.UtcNow;
        var nextRun = now.Date.AddDays(1); // midnight tonight
        var delay = nextRun - now;
        if (delay < TimeSpan.FromMinutes(1)) delay = TimeSpan.FromMinutes(1);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(delay, stoppingToken);
                await RunAsync(stoppingToken);
                delay = Interval;
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Credit reminder background service encountered an error. Retrying in 1 hour.");
                delay = TimeSpan.FromHours(1);
            }
        }

        _logger.LogInformation("Credit reminder background service stopped.");
    }

    private async Task RunAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ICreditReminderService>();
        var sent = await service.SendCreditRemindersAsync();
        _logger.LogInformation("Daily credit reminder job completed. Emails sent: {Count}", sent);
    }
}
