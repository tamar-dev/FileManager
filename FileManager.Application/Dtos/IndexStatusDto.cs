namespace FileManager.Application.Dtos;

public static class IndexingState
{
    public const string Idle = "idle";
    public const string Running = "running";
    public const string Completed = "completed";
    public const string Failed = "failed";
}

public class IndexStatusDto
{
    public string State { get; set; } = IndexingState.Idle;

    public string? Path { get; set; }

    public int FilesProcessed { get; set; }

    public int? TotalFiles { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? Error { get; set; }
}
