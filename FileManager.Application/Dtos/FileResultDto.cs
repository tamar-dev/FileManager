namespace FileManager.Application.Dtos;

public class FileResultDto
{
    public string FullPath { get; set; } = "";

    public string Name { get; set; } = "";

    public long Size { get; set; }

    public string Extension { get; set; } = "";

    public DateTime LastModified { get; set; }

    public string Hash { get; set; } = "";
}
