using FileManager.Core.Entities;
using FileManager.Core.Interfaces;

namespace FileManager.Core.Services;

public class IndexingService
{
    private readonly IFileRepository _repository;
    private readonly IFileEntryFactory _fileEntryFactory;

    public IndexingService(IFileRepository repository, IFileEntryFactory? fileEntryFactory = null)
    {
        _repository = repository;
        _fileEntryFactory = fileEntryFactory ?? new FileEntryFactory(repository: repository);
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
        Exception? lastException = null;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return await _fileEntryFactory.CreateAsync(fullPath);
            }
            catch (IOException ex) when (attempt < maxAttempts)
            {
                lastException = ex;
                await Task.Delay(100 * attempt);
            }
            catch (UnauthorizedAccessException ex) when (attempt < maxAttempts)
            {
                lastException = ex;
                await Task.Delay(100 * attempt);
            }
            catch (IOException ex)
            {
                lastException = ex;
            }
            catch (UnauthorizedAccessException ex)
            {
                lastException = ex;
            }
        }

        // All retries were exhausted. This must not be silent: log so that the
        // failure is visible instead of the file quietly falling out of the index.
        Console.WriteLine(
            $"[IndexingService] Failed to index '{fullPath}' after {maxAttempts} attempts: {lastException?.Message}");

        return null;
    }


    public async Task RemoveAsync(string fullPath)
    {
        await _repository.DeleteAsync(fullPath);
    }
}