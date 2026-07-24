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
                // A single failed filesystem operation must never take down the worker
                // loop. Any unexpected exception from processing one change is logged
                // and the loop continues with the next queued change.
                try
                {
                    await ProcessAsync(change!);
                }
                catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine(
                        $"[FileChangeWorker] Failed to process {change!.ChangeType} change for '{change.FullPath}': {ex.Message}");
                }
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

                // Remove and index are independent operations. If removing the old
                // path fails, the new path should still be indexed (and vice versa)
                // instead of one failure silently skipping the other half of the
                // rename. Each failure is logged individually.
                await TryRunAsync(
                    () => _indexingService.RemoveAsync(change.OldFullPath!),
                    $"remove old path '{change.OldFullPath}' during rename");

                await TryRunAsync(
                    () => _indexingService.IndexAsync(change.FullPath),
                    $"index new path '{change.FullPath}' during rename");

                break;
        }
    }


    private static async Task TryRunAsync(Func<Task> operation, string description)
    {
        try
        {
            await operation();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FileChangeWorker] Failed to {description}: {ex.Message}");
        }
    }
}