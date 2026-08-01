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
        var totalEnriched = 0;

        while (true)
        {
            var enriched = await EnrichNextBatchAsync(
                progress,
                cancellationToken);

            totalEnriched += enriched;

            if (enriched < BatchSize)
            {
                return totalEnriched;
            }
        }
    }

    public async Task<int> EnrichNextBatchAsync(
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var pending = await _repository.GetFilesWithoutHashAsync(
            BatchSize,
            cancellationToken);

        var enrichedFiles = new List<FileEntry>(pending.Count);

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
                enrichedFiles.Add(file);
                progress?.Report(file.FullPath);
            }
            catch (Exception ex)
                when (ex is IOException or UnauthorizedAccessException)
            {
                Console.WriteLine(
                    $"Skipping hash for '{file.FullPath}': " +
                    $"{ex.GetType().Name}: {ex.Message}");
            }
        }

        if (enrichedFiles.Count > 0)
        {
            await _repository.UpsertBatchAsync(
                enrichedFiles,
                cancellationToken);
        }

        return enrichedFiles.Count;
    }
}
