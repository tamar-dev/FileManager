using System.Collections.Concurrent;
using FileManager.Core.Events;

namespace FileManager.Core.Services;

public class FileChangeQueue
{
    private readonly ConcurrentQueue<FileChangeEvent> _queue = new();

    // Tracks Created/Changed events that are enqueued but not yet processed, so that
    // rapid duplicate notifications for the same path (a well known FileSystemWatcher
    // behavior, e.g. multiple "Changed" events for a single write) are coalesced into a
    // single pending item instead of piling up redundant work on the worker.
    private readonly ConcurrentDictionary<PendingKey, byte> _pending = new();

    public void Enqueue(FileChangeEvent change)
    {
        if (change.ChangeType is FileChangeType.Created or FileChangeType.Changed)
        {
            var key = new PendingKey(change.FullPath, change.ChangeType);

            if (!_pending.TryAdd(key, 0))
            {
                // An equivalent event for this path is already queued/pending; skip.
                return;
            }
        }

        _queue.Enqueue(change);
    }

    public bool TryDequeue(out FileChangeEvent? change)
    {
        if (!_queue.TryDequeue(out change))
        {
            return false;
        }

        if (change!.ChangeType is FileChangeType.Created or FileChangeType.Changed)
        {
            _pending.TryRemove(new PendingKey(change.FullPath, change.ChangeType), out _);
        }

        return true;
    }

    public int Count => _queue.Count;

    private readonly record struct PendingKey(string FullPath, FileChangeType ChangeType)
    {
        public bool Equals(PendingKey other) =>
            ChangeType == other.ChangeType &&
            string.Equals(FullPath, other.FullPath, StringComparison.OrdinalIgnoreCase);

        public override int GetHashCode() =>
            HashCode.Combine(FullPath.ToUpperInvariant(), ChangeType);
    }
}