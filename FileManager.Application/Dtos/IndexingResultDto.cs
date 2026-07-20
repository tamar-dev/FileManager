namespace FileManager.Application.Dtos;

public class IndexingResultDto
{
    public string Path { get; set; } = "";

    public int FilesIndexed { get; set; }

    public bool Success { get; set; }

    public string? ErrorMessage { get; set; }
}
