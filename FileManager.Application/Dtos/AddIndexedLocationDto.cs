namespace FileManager.Application.Dtos;

public class AddIndexedLocationDto
{
    public string Path { get; set; } = "";

    public bool WatchEnabled { get; set; } = true;
}
