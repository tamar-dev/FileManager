using FileManager.Application.Dtos;
using FileManager.Core.Interfaces;
using FileManager.Core.Services;

namespace FileManager.Application.Services;

public class IndexingAppService : IIndexingAppService
{
    private readonly InitialIndexingService _initialIndexingService;
    private readonly IIndexedRootRepository _indexedRootRepository;

    public IndexingAppService(
        InitialIndexingService initialIndexingService,
        IIndexedRootRepository indexedRootRepository)
    {
        _initialIndexingService = initialIndexingService;
        _indexedRootRepository = indexedRootRepository;
    }

    public async Task<IndexingResultDto> IndexDirectoryAsync(
        string path,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var filesIndexed =
                await _initialIndexingService.IndexDirectoryAsync(
                    path,
                    progress,
                    cancellationToken);

            await MarkIndexedAsync(
                path,
                cancellationToken);

            return new IndexingResultDto
            {
                Path = path,
                FilesIndexed = filesIndexed,
                Success = true
            };
        }
        catch (OperationCanceledException)
        {
            return new IndexingResultDto
            {
                Path = path,
                FilesIndexed = 0,
                Success = false,
                ErrorMessage = "Indexing was cancelled."
            };
        }
        catch (Exception ex)
        {
            return new IndexingResultDto
            {
                Path = path,
                FilesIndexed = 0,
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private async Task MarkIndexedAsync(
        string path,
        CancellationToken cancellationToken)
    {
        var normalizedPath = PathNormalizer.Normalize(path);

        var root =
            await _indexedRootRepository
                .GetByNormalizedPathAsync(
                    normalizedPath,
                    cancellationToken);

        if (root is null)
        {
            return;
        }

        root.LastIndexedAt = DateTime.UtcNow;

        await _indexedRootRepository.UpdateAsync(
            root,
            cancellationToken);
    }
}