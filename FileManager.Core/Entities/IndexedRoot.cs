namespace FileManager.Core.Entities;

public class IndexedRoot
{
    public Guid Id { get; set; }

    public string Path { get; set; } = "";

    public bool Enabled { get; set; } = true;

    public bool WatchEnabled { get; set; } = true;

    public DateTime? LastIndexedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
