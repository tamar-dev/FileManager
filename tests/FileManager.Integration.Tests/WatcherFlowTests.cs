using FileManager.Core.Services;
using FileManager.Infrastructure.FileSystem;
using FileManager.Infrastructure.Persistence;
using FileManager.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FileManager.Integration.Tests;

public class WatcherFlowTests : IDisposable
{
    private readonly string _rootFolder;
    private readonly string _watchedFolder;
    private readonly string _dbPath;
    private readonly FileManagerDbContext _workerContext;
    private readonly FileRepository _workerRepository;
    private readonly FileChangeQueue _queue;
    private readonly IndexingService _indexingService;
    private readonly FileChangeWorker _worker;
    private readonly LocalFileWatcher _watcher;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _workerTask;

    private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);

    public WatcherFlowTests()
    {
        _rootFolder = Directory.CreateTempSubdirectory("WatcherFlowTests").FullName;
        _watchedFolder = Path.Combine(_rootFolder, "watched");
        Directory.CreateDirectory(_watchedFolder);

        _dbPath = Path.Combine(_rootFolder, "test.db");

        _workerContext = CreateContext();
        _workerContext.Database.EnsureCreated();

        _workerRepository = new FileRepository(_workerContext);
        _queue = new FileChangeQueue();
        _indexingService = new IndexingService(_workerRepository);
        _worker = new FileChangeWorker(_queue, _indexingService);

        _workerTask = _worker.RunAsync(_cts.Token);

        _watcher = new LocalFileWatcher(_watchedFolder, _queue);
        _watcher.Start();
    }

    private FileManagerDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<FileManagerDbContext>()
            .UseSqlite($"Data Source={_dbPath}")
            .Options;

        return new FileManagerDbContext(options);
    }

    private async Task<IReadOnlyList<Core.Entities.FileEntry>> ReadAllAsync()
    {
        using var context = CreateContext();
        var repository = new FileRepository(context);
        return await repository.GetAllAsync();
    }

    [Fact]
    public async Task CreatingFile_AddsEntryToIndex()
    {
        var filePath = Path.Combine(_watchedFolder, "created.txt");
        File.WriteAllText(filePath, "new file content");

        await WaitUntilAsync(async () =>
        {
            var files = await ReadAllAsync();
            return files.Any(f => f.FullPath == filePath);
        });

        var result = await ReadAllAsync();
        result.Should().Contain(f => f.FullPath == filePath);
    }

    [Fact]
    public async Task ModifyingFile_UpdatesHashAndSize()
    {
        var filePath = Path.Combine(_watchedFolder, "modify.txt");
        File.WriteAllText(filePath, "initial content");

        await WaitUntilAsync(async () =>
        {
            var files = await ReadAllAsync();
            return files.Any(f => f.FullPath == filePath);
        });

        var initial = (await ReadAllAsync()).Single(f => f.FullPath == filePath);

        File.WriteAllText(filePath, "modified content with more data");
        File.SetLastWriteTimeUtc(filePath, DateTime.UtcNow);

        await WaitUntilAsync(async () =>
        {
            var files = await ReadAllAsync();
            var current = files.SingleOrDefault(f => f.FullPath == filePath);
            return current != null && current.Hash != initial.Hash;
        });

        var updated = (await ReadAllAsync()).Single(f => f.FullPath == filePath);

        updated.Hash.Should().NotBe(initial.Hash);
        updated.Size.Should().NotBe(initial.Size);
    }
    [Fact]
    public async Task DeletingFile_RemovesEntryFromIndex()
    {
        var filePath = Path.Combine(_watchedFolder, "delete.txt");
        File.WriteAllText(filePath, "to be deleted");

        await WaitUntilAsync(async () =>
        {
            var files = await ReadAllAsync();
            return files.Any(f => f.FullPath == filePath);
        });

        File.Delete(filePath);

        await WaitUntilAsync(async () =>
        {
            var files = await ReadAllAsync();
            return files.All(f => f.FullPath != filePath);
        });

        var result = await ReadAllAsync();
        result.Should().NotContain(f => f.FullPath == filePath);
    }

    [Fact]
    public async Task RenamingFile_UpdatesIndexWithNewPathAndRemovesOldPath()
    {
        var oldPath = Path.Combine(_watchedFolder, "original.txt");
        var newPath = Path.Combine(_watchedFolder, "renamed.txt");

        File.WriteAllText(oldPath, "rename test content");

        // Wait for the created event to be processed before renaming.
        await WaitUntilAsync(async () =>
        {
            var files = await ReadAllAsync();
            return files.Any(f => f.FullPath == oldPath);
        });

        File.Move(oldPath, newPath);

        // New path must appear in the index.
        await WaitUntilAsync(async () =>
        {
            var files = await ReadAllAsync();
            return files.Any(f => f.FullPath == newPath);
        });

        // Old path must be removed.
        await WaitUntilAsync(async () =>
        {
            var files = await ReadAllAsync();
            return files.All(f => f.FullPath != oldPath);
        });

        var result = await ReadAllAsync();
        result.Should().Contain(f => f.FullPath == newPath);
        result.Should().NotContain(f => f.FullPath == oldPath);
    }

    private static async Task WaitUntilAsync(Func<Task<bool>> condition)
    {
        var start = DateTime.UtcNow;

        while (DateTime.UtcNow - start < WaitTimeout)
        {
            if (await condition())
            {
                return;
            }

            await Task.Delay(PollInterval);
        }

        throw new TimeoutException("Condition was not met within the expected timeout.");
    }

    public void Dispose()
    {
        _watcher.Stop();
        _watcher.Dispose();
        _cts.Cancel();

        try
        {
            _workerTask.Wait(TimeSpan.FromSeconds(2));
        }
        catch (AggregateException)
        {
            // expected on cancellation
        }

        _workerContext.Dispose();

        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

        if (Directory.Exists(_rootFolder))
        {
            Directory.Delete(_rootFolder, recursive: true);
        }
    }
}
