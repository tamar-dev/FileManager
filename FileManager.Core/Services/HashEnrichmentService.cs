using FileManager.Core.Entities;
using FileManager.Core.Interfaces;

namespace FileManager.Core.Services;

public class HashEnrichmentService
{
    private const int BatchSize = 100;

    private readonly IFileRepository _repository;
    private readonly FileHasher _fileHasher;

    public HashEnrichmentService(
        IFileRepository repository,
        FileHasher fileHasher)
    {
        _repository = repository;
        _fileHasher = fileHasher;
    }

    public async Task<int> EnrichMissingHashesAsync(
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var files = await _repository.GetAllAsync();

        var pending = files
            .Where(file =>
                !file.IsDeleted &&
                string.IsNullOrWhiteSpace(file.Hash))
            .ToList();

        var batch = new List<FileEntry>(BatchSize);
        var enrichedCount = 0;

        foreach (var file in pending)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!File.Exists(file.FullPath))
            {
                continue;
            }

            try
            {
                file.Hash = _fileHasher.Calculate(file.FullPath);
                batch.Add(file);
                enrichedCount++;
                progress?.Report(file.FullPath);

                if (batch.Count >= BatchSize)
                {
                    await _repository.UpsertBatchAsync(
                        batch,
                        cancellationToken);

                    batch.Clear();
                }
            }
            catch (Exception ex)
                when (ex is IOException or UnauthorizedAccessException)
            {
                Console.WriteLine(
                    $"Skipping hash for '{file.FullPath}': " +
                    $"{ex.GetType().Name}: {ex.Message}");
            }
        }

        if (batch.Count > 0)
        {
            await _repository.UpsertBatchAsync(
                batch,
                cancellationToken);
        }

        return enrichedCount;
    }
}
