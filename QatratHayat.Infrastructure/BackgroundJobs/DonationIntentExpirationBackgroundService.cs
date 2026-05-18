using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QatratHayat.Domain.Enums;
using QatratHayat.Infrastructure.Persistence;

namespace QatratHayat.Infrastructure.BackgroundJobs
{
    public class DonationIntentExpirationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DonationIntentExpirationBackgroundService> _logger;

        private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(15);

        public DonationIntentExpirationBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<DonationIntentExpirationBackgroundService> logger
        )
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Donation intent expiration background service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ExpireDonationIntentsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while expiring donation intents.");
                }

                await Task.Delay(CheckInterval, stoppingToken);
            }
        }

        private async Task ExpireDonationIntentsAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var now = DateTime.UtcNow;

            var expiredIntents = await context.DonationIntents
                .Where(x =>
                    x.DonationIntentStatus == DonationIntentStatus.Active
                    && x.ExpiresAt <= now
                )
                .ToListAsync(cancellationToken);

            if (!expiredIntents.Any())
            {
                return;
            }

            foreach (var intent in expiredIntents)
            {
                intent.DonationIntentStatus = DonationIntentStatus.Expired;
            }

            await context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Expired {Count} donation intents.",
                expiredIntents.Count
            );
        }
    }
}