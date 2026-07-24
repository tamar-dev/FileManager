using FileManager.Core.Entities;
using FileManager.Core.Interfaces;

namespace FileManager.Core.Services;

public class InitialIndexingService
{
    private const int BatchSize = 500;

    private readonly IFileRepository _repository;
    private readonly FileEntryFactory _fileEntryFactory;
    private readonly IIndexSource _indexSource;

    public InitialIndexingService(
        IFileRepository repository,
        IIndexSource indexSource,
        FileEntryFactory fileEntryFactory)
    {
        _repository = repository;
        _indexSource = indexSource;
        _fileEntryFactory = fileEntryFactory;
    }

    public async Task IndexDirectoryAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        var pendingPaths = new List<string>(BatchSize);

        foreach (var filePath in _indexSource.EnumeratePaths(path, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            pendingPaths.Add(filePath);

            if (pendingPaths.Count == BatchSize)
            {
                await ProcessBatchAsync(pendingPaths, cancellationToken);
                pendingPaths.Clear();
            }
        }

        if (pendingPaths.Count > 0)
        {
            await ProcessBatchAsync(pendingPaths, cancellationToken);
        }
    }

    private async Task ProcessBatchAsync(
        IReadOnlyCollection<string> filePaths,
        CancellationToken cancellationToken)
    {
        var existingEntries = await _repository.GetByPathsAsync(filePaths, cancellationToken);
        var existingByPath = existingEntries.ToDictionary(entry => entry.FullPath, StringComparer.OrdinalIgnoreCase);

        var batchEntries = new List<FileEntry>(filePaths.Count);

        foreach (var filePath in filePaths)
        {
            cancellationToken.ThrowIfCancellationRequested();

            existingByPath.TryGetValue(filePath, out var existingEntry);

            var entry = await _fileEntryFactory.CreateAsync(filePath, existingEntry);

            batchEntries.Add(entry);
            Console.WriteLine($"Indexed: {entry.FullPath}");
        }

        await _repository.UpsertBatchAsync(batchEntries, cancellationToken);
    }
}
