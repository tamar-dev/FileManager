using FileManager.Core.Entities;
using FileManager.Core.Interfaces;

namespace FileManager.Core.Services;

public class IndexingService
{
    private readonly IFileRepository _repository;

    public IndexingService(IFileRepository repository)
    {
        _repository = repository;
    }


    public async Task IndexAsync(string fullPath)
    {
        if (!File.Exists(fullPath))
            return;

        var info = new FileInfo(fullPath);

        var entry = new FileEntry
        {
            FullPath = info.FullName,
            Name = info.Name,
            Extension = info.Extension,
            Size = info.Length,
            LastModified = info.LastWriteTimeUtc,
            Hash = ""
        };

        await _repository.UpsertAsync(entry);
        Console.WriteLine(
    $"Indexed: {entry.FullPath}");
    }


    public async Task RemoveAsync(string fullPath)
    {
        await _repository.DeleteAsync(fullPath);
    }
}