namespace FileManager.Core.Entities;

public class FileEntry
{
    public Guid Id { get; set; }

    public string FullPath { get; set; } = "";

    public string Name { get; set; } = "";

    public long Size { get; set; }

    public string Extension { get; set; } = "";

    public DateTime LastModified { get; set; }

    public DateTime IndexedAt { get; set; }

    public bool IsDeleted { get; set; }

    public string Hash { get; set; } = "";
}