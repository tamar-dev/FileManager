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
                new SynchronousProgress<string>(currentFilePath =>
                {
                    var current =
                        Interlocked.Increment(
                            ref filesProcessed);

                    _statusService.ReportProgress(
                        current,
                        currentFilePath);
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

            _statusService.Complete();
        }
        catch (Exception ex)
        {
            _statusService.Fail(ex.Message);
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
