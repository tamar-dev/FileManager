using FileManager.Application.Dtos;

namespace FileManager.Application.Services;

public interface IIndexingAppService
{
    Task<IndexingResultDto> IndexDirectoryAsync(
        string path,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default);
}
