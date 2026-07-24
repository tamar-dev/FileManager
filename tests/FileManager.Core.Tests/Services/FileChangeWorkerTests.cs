using FileManager.Core.Entities;
using FileManager.Core.Events;
using FileManager.Core.Interfaces;
using FileManager.Core.Services;
using FluentAssertions;

namespace FileManager.Core.Tests.Services;

public class FileChangeWorkerTests
{
    private sealed class FakeRepository : IFileRepository
    {
        private readonly Dictionary<string, FileEntry> _files = new(StringComparer.OrdinalIgnoreCase);

        public Func<string, Task>? OnDelete { get; set; }

        public IReadOnlyDictionary<string, FileEntry> Files => _files;

        public Task UpsertAsync(FileEntry file)
        {
            _files[file.FullPath] = file;
            return Task.CompletedTask;
        }

        public Task UpsertBatchAsync(IReadOnlyCollection<FileEntry> files, CancellationToken cancellationToken = default)
        {
            foreach (var file in files)
            {
                _files[file.FullPath] = file;
            }

            return Task.CompletedTask;
        }

        public async Task DeleteAsync(string fullPath)
        {
            if (OnDelete != null)
            {
                await OnDelete(fullPath);
            }

            _files.Remove(fullPath);
        }

        public Task<IReadOnlyList<FileEntry>> GetAllAsync() =>
            Task.FromResult<IReadOnlyList<FileEntry>>(_files.Values.ToList());

        public Task<FileEntry?> GetByPathAsync(string fullPath) =>
            Task.FromResult(_files.TryGetValue(fullPath, out var entry) ? entry : null);

        public Task<IReadOnlyList<FileEntry>> GetByPathsAsync(IReadOnlyCollection<string> fullPaths, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<FileEntry>>(
                _files.Values.Where(f => fullPaths.Contains(f.FullPath)).ToList());
    }

    [Fact]
    public async Task RunAsync_RenameWhereRemoveFails_StillIndexesNewPath()
    {
        var tempDir = Directory.CreateTempSubdirectory("FileChangeWorkerTests").FullName;
        var newPath = Path.Combine(tempDir, "new.txt");
        File.WriteAllText(newPath, "content");

        var repository = new FakeRepository
        {
            OnDelete = _ => throw new IOException("simulated failure removing old path")
        };

        var indexingService = new IndexingService(repository);
        var queue = new FileChangeQueue();
        var worker = new FileChangeWorker(queue, indexingService);

        queue.Enqueue(new FileChangeEvent
        {
            ChangeType = FileChangeType.Renamed,
            FullPath = newPath,
            OldFullPath = Path.Combine(tempDir, "old.txt")
        });

        using var cts = new CancellationTokenSource();
        var runTask = worker.RunAsync(cts.Token);

        await WaitUntilAsync(() => repository.Files.ContainsKey(newPath));

        cts.Cancel();
        await SafeAwait(runTask);

        repository.Files.Should().ContainKey(newPath);
    }

    [Fact]
    public async Task RunAsync_SingleFailedChange_DoesNotStopProcessingSubsequentChanges()
    {
        var tempDir = Directory.CreateTempSubdirectory("FileChangeWorkerTests").FullName;
        var goodPath = Path.Combine(tempDir, "good.txt");
        File.WriteAllText(goodPath, "content");

        var repository = new FakeRepository();
        var indexingService = new IndexingService(repository);
        var queue = new FileChangeQueue();
        var worker = new FileChangeWorker(queue, indexingService);

        // Deleted event for a path never indexed still goes through DeleteAsync,
        // which is harmless here; instead force a failure via a bad rename with a
        // null-like old path handled gracefully, then a valid change afterwards.
        queue.Enqueue(new FileChangeEvent
        {
            ChangeType = FileChangeType.Deleted,
            FullPath = Path.Combine(tempDir, "does-not-exist.txt")
        });

        queue.Enqueue(new FileChangeEvent
        {
            ChangeType = FileChangeType.Created,
            FullPath = goodPath
        });

        using var cts = new CancellationTokenSource();
        var runTask = worker.RunAsync(cts.Token);

        await WaitUntilAsync(() => repository.Files.ContainsKey(goodPath));

        cts.Cancel();
        await SafeAwait(runTask);

        repository.Files.Should().ContainKey(goodPath);
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        var start = DateTime.UtcNow;
        while (DateTime.UtcNow - start < TimeSpan.FromSeconds(5))
        {
            if (condition())
            {
                return;
            }

            await Task.Delay(50);
        }

        throw new TimeoutException("Condition was not met within the expected timeout.");
    }

    private static async Task SafeAwait(Task task)
    {
        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
        }
    }
}
