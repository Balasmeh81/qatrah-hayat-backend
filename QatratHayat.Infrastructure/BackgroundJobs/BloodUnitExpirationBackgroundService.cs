using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QatratHayat.Domain.Enums;
using QatratHayat.Infrastructure.Persistence;

namespace QatratHayat.Infrastructure.BackgroundJobs
{
    public class BloodUnitExpirationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BloodUnitExpirationBackgroundService> _logger;

        private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(1);

        public BloodUnitExpirationBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<BloodUnitExpirationBackgroundService> logger
        )
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Blood unit expiration background service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ExpireBloodUnitsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while expiring blood units.");
                }

                await Task.Delay(CheckInterval, stoppingToken);
            }
        }

        private async Task ExpireBloodUnitsAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var now = DateTime.UtcNow;

            var expiredUnits = await context.BloodUnits
                .Where(x =>
                    x.UnitStatus == UnitStatus.Available
                    && x.ExpiresAt <= now
                )
                .ToListAsync(cancellationToken);

            if (!expiredUnits.Any())
            {
                return;
            }

            foreach (var unit in expiredUnits)
            {
                unit.UnitStatus = UnitStatus.Expired;
                unit.UpdatedAt = now;
            }

            await context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Expired {Count} blood units.",
                expiredUnits.Count
            );
        }
    }
}