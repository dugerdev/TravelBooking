using TravelBooking.Application.Contracts;
using Microsoft.Extensions.Configuration;

namespace TravelBooking.Api.HostedServices;

public sealed class FlightSyncBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FlightSyncBackgroundService> _logger;

    public FlightSyncBackgroundService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<FlightSyncBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var enabled = _configuration.GetValue<bool>("FlightSync:Enabled");
        if (!enabled)
        {
            _logger.LogInformation("Flight sync background service is disabled");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var syncService = scope.ServiceProvider.GetRequiredService<IFlightDataSyncService>();
                
                _logger.LogInformation("Starting flight sync...");
                var result = await syncService.SyncFlightsAsync(stoppingToken);
                
                if (result.Success)
                    _logger.LogInformation("Flight sync completed successfully: {Message}", result.Message);
                else
                    _logger.LogWarning("Flight sync completed with errors: {Message}", result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Flight sync failed");
            }

            var intervalHours = _configuration.GetValue<int?>("FlightSync:IntervalHours") ?? 6;
            await Task.Delay(TimeSpan.FromHours(intervalHours), stoppingToken);
        }
    }
}
