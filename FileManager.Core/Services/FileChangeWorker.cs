using FileManager.Core.Events;

namespace FileManager.Core.Services;

public class FileChangeWorker
{
    private readonly FileChangeQueue _queue;
    private readonly IndexingService _indexingService;


    public FileChangeWorker(
        FileChangeQueue queue,
        IndexingService indexingService)
    {
        _queue = queue;
        _indexingService = indexingService;
    }


    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (_queue.TryDequeue(out var change))
            {
                await ProcessAsync(change!);
            }
            else
            {
                await Task.Delay(100, cancellationToken);
            }
        }
    }


    private async Task ProcessAsync(FileChangeEvent change)
    {
        switch (change.ChangeType)
        {
            case FileChangeType.Created:
            case FileChangeType.Changed:
                await _indexingService.IndexAsync(change.FullPath);
                break;


            case FileChangeType.Deleted:
                await _indexingService.RemoveAsync(change.FullPath);
                break;


            case FileChangeType.Renamed:

                await _indexingService.RemoveAsync(
                    change.OldFullPath!);

                await _indexingService.IndexAsync(
                    change.FullPath);

                break;
        }
    }
}