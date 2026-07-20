using System.Collections.Concurrent;
using FileManager.Core.Events;

namespace FileManager.Core.Services;

public class FileChangeQueue
{
    private readonly ConcurrentQueue<FileChangeEvent> _queue = new();

    public void Enqueue(FileChangeEvent change)
    {
        _queue.Enqueue(change);
    }

    public bool TryDequeue(out FileChangeEvent? change)
    {
        return _queue.TryDequeue(out change);
    }

    public int Count => _queue.Count;
}