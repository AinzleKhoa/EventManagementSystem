using EventAPI.Services;

namespace EventAPI.Background
{
    public class ValidatorCleanupBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ValidatorCleanupBackgroundService> _logger;
        private readonly TimeSpan _period = TimeSpan.FromHours(1);

        public ValidatorCleanupBackgroundService(IServiceProvider serviceProvider, ILogger<ValidatorCleanupBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ValidatorCleanupBackgroundService is starting.");

            using var timer = new PeriodicTimer(_period);
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    _logger.LogInformation("ValidatorCleanupBackgroundService executing deactivation check...");
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
                        await eventService.DeactivateExpiredValidatorsAsync();
                    }
                    _logger.LogInformation("ValidatorCleanupBackgroundService deactivation check completed.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during ValidatorCleanupBackgroundService execution.");
                }
            }
        }
    }
}
