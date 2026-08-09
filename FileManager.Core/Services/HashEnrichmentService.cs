using FileManager.Core.Entities;
using FileManager.Core.Interfaces;

namespace FileManager.Core.Services;

public sealed record HashEnrichmentBatchResult(
    int EnrichedCount,
    int ExaminedCount,
    string? LastExaminedPath,
    bool HasMore);

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
        string? cursor = null;

        while (true)
        {
            var result = await EnrichNextBatchWithCursorAsync(
                cursor,
                progress,
                cancellationToken);

            totalEnriched += result.EnrichedCount;

            if (!result.HasMore || result.LastExaminedPath is null)
            {
                return totalEnriched;
            }

            cursor = result.LastExaminedPath;
        }
    }

    public async Task<int> EnrichNextBatchAsync(
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var result = await EnrichNextBatchWithCursorAsync(
            afterPath: null,
            progress,
            cancellationToken);

        return result.EnrichedCount;
    }

    public async Task<HashEnrichmentBatchResult> EnrichNextBatchWithCursorAsync(
        string? afterPath,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var candidates = await _repository.GetFilesWithoutHashAfterAsync(
            BatchSize + 1,
            afterPath,
            cancellationToken);

        var hasMore = candidates.Count > BatchSize;
        var pending = candidates.Take(BatchSize).ToList();
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

        return new HashEnrichmentBatchResult(
            EnrichedCount: enrichedFiles.Count,
            ExaminedCount: pending.Count,
            LastExaminedPath: pending.LastOrDefault()?.FullPath,
            HasMore: hasMore);
    }
}
