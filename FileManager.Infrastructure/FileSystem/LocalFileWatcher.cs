using FileManager.Core.Events;
using FileManager.Core.Services;

namespace FileManager.Infrastructure.FileSystem;

public class LocalFileWatcher : IDisposable
{
    private readonly FileSystemWatcher _watcher;
    private readonly FileChangeQueue _queue;

    public LocalFileWatcher(string path, FileChangeQueue queue)
    {
        _queue = queue;

        _watcher = new FileSystemWatcher(path)
        {
            IncludeSubdirectories = true,
            NotifyFilter =
                NotifyFilters.FileName |
                NotifyFilters.DirectoryName |
                NotifyFilters.LastWrite
        };

        _watcher.Created += OnCreated;
        _watcher.Changed += OnChanged;
        _watcher.Deleted += OnDeleted;
        _watcher.Renamed += OnRenamed;
        _watcher.Error += OnError;
    }

    public void Start()
    {
        _watcher.EnableRaisingEvents = true;
    }

    public void Stop()
    {
        _watcher.EnableRaisingEvents = false;
    }

    private void OnCreated(object? sender, FileSystemEventArgs e)
    {
        _queue.Enqueue(new FileChangeEvent
        {
            ChangeType = FileChangeType.Created,
            FullPath = e.FullPath
        });
    }

    private void OnChanged(object? sender, FileSystemEventArgs e)
    {
        _queue.Enqueue(new FileChangeEvent
        {
            ChangeType = FileChangeType.Changed,
            FullPath = e.FullPath
        });
    }

    private void OnDeleted(object? sender, FileSystemEventArgs e)
    {
        _queue.Enqueue(new FileChangeEvent
        {
            ChangeType = FileChangeType.Deleted,
            FullPath = e.FullPath
        });
    }

    private void OnRenamed(object? sender, RenamedEventArgs e)
    {
        _queue.Enqueue(new FileChangeEvent
        {
            ChangeType = FileChangeType.Renamed,
            FullPath = e.FullPath,
            OldFullPath = e.OldFullPath
        });
    }

    private void OnError(object? sender, ErrorEventArgs e)
    {
        Console.WriteLine($"[LocalFileWatcher] FileSystemWatcher error: {e.GetException().Message}");
    }

    public void Dispose()
    {
        _watcher.Dispose();
    }
}