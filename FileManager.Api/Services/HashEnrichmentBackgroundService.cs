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
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var enrichmentService = scope.ServiceProvider
                    .GetRequiredService<HashEnrichmentService>();

                var enrichedCount = await enrichmentService
                    .EnrichNextBatchAsync(
                        cancellationToken: stoppingToken);

                if (enrichedCount == 0)
                {
                    await Task.Delay(IdleDelay, stoppingToken);
                }
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

                await Task.Delay(IdleDelay, stoppingToken);
            }
        }
    }
}
