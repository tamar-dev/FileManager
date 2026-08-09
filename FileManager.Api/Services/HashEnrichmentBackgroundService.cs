using FileManager.Core.Services;

namespace FileManager.Api.Services;

public sealed class HashEnrichmentBackgroundService : BackgroundService
{
    private static readonly TimeSpan IdleDelay = TimeSpan.FromSeconds(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<HashEnrichmentBackgroundService> _logger;

    public HashEnrichmentBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<HashEnrichmentBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        string? cursor = null;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var enrichmentService = scope.ServiceProvider
                    .GetRequiredService<HashEnrichmentService>();

                var result = await enrichmentService
                    .EnrichNextBatchWithCursorAsync(
                        cursor,
                        cancellationToken: stoppingToken);

                if (result.HasMore && result.LastExaminedPath is not null)
                {
                    cursor = result.LastExaminedPath;
                    continue;
                }

                cursor = null;
                await Task.Delay(IdleDelay, stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Hash enrichment background processing failed.");

                cursor = null;
                await Task.Delay(IdleDelay, stoppingToken);
            }
        }
    }
}
