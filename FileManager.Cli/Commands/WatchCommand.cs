using FileManager.Core.Services;
using FileManager.Core.Interfaces;
using FileManager.Infrastructure.FileSystem;

namespace FileManager.Cli.Commands;

public class WatchCommand
{
    private readonly IFileRepository _repository;

    public WatchCommand(IFileRepository repository)
    {
        _repository = repository;
    }

    public async Task ExecuteAsync(string path)
    {
        var queue = new FileChangeQueue();

        var indexingService = new IndexingService(_repository);

        var worker = new FileChangeWorker(
            queue,
            indexingService);

        using var cts = new CancellationTokenSource();

        _ = worker.RunAsync(cts.Token);

        using var watcher = new LocalFileWatcher(path, queue);

        watcher.Start();

        Console.WriteLine($"Watching: {path}");
        Console.WriteLine("Press Ctrl+C to stop");

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        try
        {
            await Task.Delay(Timeout.Infinite, cts.Token);
        }
        catch (OperationCanceledException)
        {
            // יציאה תקינה
        }
    }
}