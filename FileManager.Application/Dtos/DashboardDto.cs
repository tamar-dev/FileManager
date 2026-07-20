namespace FileManager.Application.Dtos;

public class DashboardDto
{
    public int IndexedFileCount { get; set; }

    public long TotalIndexedSize { get; set; }

    public int DuplicateGroupCount { get; set; }

    public int DuplicateFileCount { get; set; }

    public long PotentialStorageSavings { get; set; }
}
