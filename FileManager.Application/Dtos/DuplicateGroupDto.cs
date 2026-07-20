namespace FileManager.Application.Dtos;

public class DuplicateGroupDto
{
    public string Hash { get; set; } = "";

    public int FileCount { get; set; }

    public long TotalSize { get; set; }

    public long WastedSize { get; set; }

    public IReadOnlyList<FileResultDto> Files { get; set; } = [];
}
