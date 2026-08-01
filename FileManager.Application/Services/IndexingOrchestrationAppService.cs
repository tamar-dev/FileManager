using FileManager.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FileManager.Application.Services;

public class IndexingOrchestrationAppService : IIndexingOrchestrationAppService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IIndexStatusService _statusService;

    public IndexingOrchestrationAppService(
        IServiceScopeFactory scopeFactory,
        IIndexStatusService statusService)
    {
        _scopeFactory = scopeFactory;
        _statusService = statusService;
    }

    public bool TryStartIndexing(string path)
    {
        if (!_statusService.TryStart(path))
        {
            return false;
        }

        _ = Task.Run(() => RunIndexingAsync(path));

        return true;
    }

    private async Task RunIndexingAsync(string path)
    {
        var filesProcessed = 0;

        try
        {
            using var scope = _scopeFactory.CreateScope();

            var indexingAppService =
                scope.ServiceProvider
                    .GetRequiredService<IIndexingAppService>();

            var progress =
                new SynchronousProgress<string>(_ =>
                {
                    var current =
                        Interlocked.Increment(
                            ref filesProcessed);

                    _statusService.ReportProgress(
                        current);
                });

            var result =
                await indexingAppService
                    .IndexDirectoryAsync(
                        path,
                        progress);

            if (!result.Success)
            {
                _statusService.Fail(
                    result.ErrorMessage
                    ?? "Indexing failed.");

                return;
            }

            // The searchable metadata index is now ready.
            // Hash calculation continues afterward and does not delay
            // completion of the initial indexing operation.
            _statusService.Complete();

            await RunHashEnrichmentAsync(
                scope.ServiceProvider);
        }
        catch (Exception ex)
        {
            _statusService.Fail(ex.Message);
        }
    }

    private static async Task RunHashEnrichmentAsync(
        IServiceProvider serviceProvider)
    {
        try
        {
            var hashEnrichmentService =
                serviceProvider
                    .GetRequiredService<HashEnrichmentService>();

            await hashEnrichmentService
                .EnrichMissingHashesAsync();
        }
        catch (Exception)
        {
            // Hash enrichment is secondary background work.
            // Failure here must not change a successfully completed
            // initial indexing operation into a failed operation.
        }
    }

    private sealed class SynchronousProgress<T> : IProgress<T>
    {
        private readonly Action<T> _handler;

        public SynchronousProgress(
            Action<T> handler)
        {
            _handler = handler;
        }

        public void Report(T value)
        {
            _handler(value);
        }
    }
}