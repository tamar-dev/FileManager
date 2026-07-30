namespace FileManager.Application.Dtos;

public class IndexedLocationDto
{
    public Guid Id { get; set; }

    public string Path { get; set; } = "";

    public bool Enabled { get; set; }

    public bool WatchEnabled { get; set; }

    public DateTime? LastIndexedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
