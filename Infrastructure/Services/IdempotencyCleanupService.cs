using Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

internal sealed class IdempotencyCleanupService(
    IServiceScopeFactory scopeFactory,
    ILogger<IdempotencyCleanupService> logger) : BackgroundService
{
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(CleanupInterval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await CleanupExpiredRecordsAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    private async Task CleanupExpiredRecordsAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();

            var repository = scope.ServiceProvider
                .GetRequiredService<IIdempotencyRepository>();

            var now = DateTime.UtcNow;

            var deletedRecordsCount = await repository
                .DeleteExpiredAsync(now, cancellationToken);

            if (deletedRecordsCount > 0)
            {
                logger.LogInformation(
                    "Deleted {DeletedCount} expired idempotency records at {Time}.",
                    deletedRecordsCount,
                    now);
            }
            else
            {
                logger.LogDebug(
                    "No expired idempotency records found at {Time}.",
                    now);
            }
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Failed to clean up expired idempotency records.");
        }
    }
}