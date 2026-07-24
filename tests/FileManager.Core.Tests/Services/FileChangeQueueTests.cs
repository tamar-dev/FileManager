using FileManager.Core.Events;
using FileManager.Core.Services;
using FluentAssertions;

namespace FileManager.Core.Tests.Services;

public class FileChangeQueueTests
{
    [Fact]
    public void Enqueue_DuplicateCreatedEventsForSamePath_OnlyKeepsOnePending()
    {
        var queue = new FileChangeQueue();

        queue.Enqueue(new FileChangeEvent { ChangeType = FileChangeType.Created, FullPath = "C:/a.txt" });
        queue.Enqueue(new FileChangeEvent { ChangeType = FileChangeType.Created, FullPath = "C:/a.txt" });
        queue.Enqueue(new FileChangeEvent { ChangeType = FileChangeType.Created, FullPath = "C:/a.txt" });

        queue.Count.Should().Be(1);
    }

    [Fact]
    public void Enqueue_DuplicateChangedEventsForSamePath_OnlyKeepsOnePending()
    {
        var queue = new FileChangeQueue();

        queue.Enqueue(new FileChangeEvent { ChangeType = FileChangeType.Changed, FullPath = "C:/a.txt" });
        queue.Enqueue(new FileChangeEvent { ChangeType = FileChangeType.Changed, FullPath = "C:/a.txt" });

        queue.Count.Should().Be(1);
    }

    [Fact]
    public void Enqueue_DifferentPaths_AreNotCoalesced()
    {
        var queue = new FileChangeQueue();

        queue.Enqueue(new FileChangeEvent { ChangeType = FileChangeType.Changed, FullPath = "C:/a.txt" });
        queue.Enqueue(new FileChangeEvent { ChangeType = FileChangeType.Changed, FullPath = "C:/b.txt" });

        queue.Count.Should().Be(2);
    }

    [Fact]
    public void Enqueue_DeletedEvents_AreNotCoalesced()
    {
        var queue = new FileChangeQueue();

        queue.Enqueue(new FileChangeEvent { ChangeType = FileChangeType.Deleted, FullPath = "C:/a.txt" });
        queue.Enqueue(new FileChangeEvent { ChangeType = FileChangeType.Deleted, FullPath = "C:/a.txt" });

        queue.Count.Should().Be(2);
    }

    [Fact]
    public void Enqueue_AfterDequeue_CanEnqueueSamePathAgain()
    {
        var queue = new FileChangeQueue();

        queue.Enqueue(new FileChangeEvent { ChangeType = FileChangeType.Changed, FullPath = "C:/a.txt" });
        queue.TryDequeue(out _);

        queue.Enqueue(new FileChangeEvent { ChangeType = FileChangeType.Changed, FullPath = "C:/a.txt" });

        queue.Count.Should().Be(1);
    }

    [Fact]
    public void Enqueue_SamePathDifferentCasing_IsCoalesced()
    {
        var queue = new FileChangeQueue();

        queue.Enqueue(new FileChangeEvent { ChangeType = FileChangeType.Changed, FullPath = "C:/A.txt" });
        queue.Enqueue(new FileChangeEvent { ChangeType = FileChangeType.Changed, FullPath = "C:/a.txt" });

        queue.Count.Should().Be(1);
    }
}
