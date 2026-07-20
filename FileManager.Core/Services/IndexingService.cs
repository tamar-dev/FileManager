using FileManager.Core.Entities;
using FileManager.Core.Interfaces;

namespace FileManager.Core.Services;

public class IndexingService
{
    private readonly IFileRepository _repository;
    private readonly FileEntryFactory _fileEntryFactory;

    public IndexingService(IFileRepository repository, FileEntryFactory? fileEntryFactory = null)
    {
        _repository = repository;
        _fileEntryFactory = fileEntryFactory ?? new FileEntryFactory();
    }


    public async Task IndexAsync(string fullPath)
    {
        if (!File.Exists(fullPath))
            return;

        var entry = await CreateEntryWithRetryAsync(fullPath);

        if (entry == null)
            return;

        await _repository.UpsertAsync(entry);
        Console.WriteLine(
    $"Indexed: {entry.FullPath}");
    }


    private async Task<Entities.FileEntry?> CreateEntryWithRetryAsync(string fullPath, int maxAttempts = 3)
    {
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return _fileEntryFactory.Create(fullPath);
            }
            catch (IOException) when (attempt < maxAttempts)
            {
                await Task.Delay(100 * attempt);
            }
        }

        return null;
    }


    public async Task RemoveAsync(string fullPath)
    {
        await _repository.DeleteAsync(fullPath);
    }
}