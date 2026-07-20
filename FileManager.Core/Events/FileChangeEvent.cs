namespace FileManager.Core.Events;

public class FileChangeEvent
{
    public FileChangeType ChangeType { get; init; }

    public string FullPath { get; init; } = string.Empty;

    public string? OldFullPath { get; init; }

    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}