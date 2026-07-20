using FileManager.Core.Services;
using FileManager.Infrastructure.FileSystem;
using FileManager.Infrastructure.Repositories;
using FileManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FileManager.Cli.Commands;

public class WatchCommand
{
    public async Task ExecuteAsync(string path)
    {
        var queue = new FileChangeQueue();

        var options = new DbContextOptionsBuilder<FileManagerDbContext>()
            .UseSqlite("Data Source=filemanager.db")
            .Options;

        using var context = new FileManagerDbContext(options);

        context.Database.Migrate();

        var repository = new FileRepository(context);

        var indexingService = new IndexingService(repository);

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