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
            var indexingAppService = scope.ServiceProvider.GetRequiredService<IIndexingAppService>();

            var progress = new Progress<string>(_ =>
            {
                filesProcessed++;
                _statusService.ReportProgress(filesProcessed);
            });

            var result = await indexingAppService.IndexDirectoryAsync(path, progress);

            if (result.Success)
            {
                _statusService.Complete();
            }
            else
            {
                _statusService.Fail(result.ErrorMessage ?? "Indexing failed.");
            }
        }
        catch (Exception ex)
        {
            _statusService.Fail(ex.Message);
        }
    }
}
