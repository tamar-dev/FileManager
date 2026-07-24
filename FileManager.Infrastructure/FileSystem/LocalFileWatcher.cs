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
        SafeEnqueue(() => new FileChangeEvent
        {
            ChangeType = FileChangeType.Created,
            FullPath = e.FullPath
        });
    }

    private void OnChanged(object? sender, FileSystemEventArgs e)
    {
        SafeEnqueue(() => new FileChangeEvent
        {
            ChangeType = FileChangeType.Changed,
            FullPath = e.FullPath
        });
    }

    private void OnDeleted(object? sender, FileSystemEventArgs e)
    {
        SafeEnqueue(() => new FileChangeEvent
        {
            ChangeType = FileChangeType.Deleted,
            FullPath = e.FullPath
        });
    }

    private void OnRenamed(object? sender, RenamedEventArgs e)
    {
        SafeEnqueue(() => new FileChangeEvent
        {
            ChangeType = FileChangeType.Renamed,
            FullPath = e.FullPath,
            OldFullPath = e.OldFullPath
        });
    }

    // Notification handlers run on the FileSystemWatcher's own thread pool callback.
    // An unhandled exception here would be swallowed by the runtime and silently stop
    // further processing of that event without any indication of failure, so every
    // failure is caught and logged instead.
    private void SafeEnqueue(Func<FileChangeEvent> createChange)
    {
        try
        {
            _queue.Enqueue(createChange());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LocalFileWatcher] Failed to enqueue file change event: {ex.Message}");
        }
    }

    private void OnError(object? sender, ErrorEventArgs e)
    {
        var exception = e.GetException();
        Console.WriteLine($"[LocalFileWatcher] FileSystemWatcher error: {exception.Message}");

        // FileSystemWatcher disables raising events after certain errors (e.g. an
        // internal buffer overflow caused by a burst of changes exceeding its
        // capacity). Without explicitly re-enabling it, the watcher goes silent and
        // no further changes are ever observed, which is the core reliability issue
        // for long-running usage. Restart raising events so monitoring continues,
        // noting that events that occurred during the overflow window may be missed.
        if (exception is InternalBufferOverflowException)
        {
            Console.WriteLine(
                "[LocalFileWatcher] Internal buffer overflow detected; some file change events may have been missed. Restarting watcher.");
        }

        try
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.EnableRaisingEvents = true;
        }
        catch (Exception restartEx)
        {
            Console.WriteLine($"[LocalFileWatcher] Failed to restart watcher after error: {restartEx.Message}");
        }
    }

    public void Dispose()
    {
        _watcher.Dispose();
    }
}