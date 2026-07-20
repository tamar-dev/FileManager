using FileManager.Application.Dtos;
using FileManager.Core.Interfaces;

namespace FileManager.Application.Services;

public class IndexingAppService : IIndexingAppService
{
    private readonly IFileScanner _scanner;
    private readonly IFileRepository _repository;

    public IndexingAppService(IFileScanner scanner, IFileRepository repository)
    {
        _scanner = scanner;
        _repository = repository;
    }

    public async Task<IndexingResultDto> IndexDirectoryAsync(
        string path,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var filesIndexed = 0;

        try
        {
            foreach (var entry in _scanner.Scan(path))
            {
                cancellationToken.ThrowIfCancellationRequested();

                await _repository.UpsertAsync(entry);

                filesIndexed++;

                progress?.Report(entry.FullPath);
            }

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
                FilesIndexed = filesIndexed,
                Success = false,
                ErrorMessage = "Indexing was cancelled."
            };
        }
        catch (Exception ex)
        {
            return new IndexingResultDto
            {
                Path = path,
                FilesIndexed = filesIndexed,
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
