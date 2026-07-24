using FileManager.Application.Dtos;

namespace FileManager.Application.Services;

public interface ISearchAppService
{
    Task<IReadOnlyList<FileResultDto>> SearchAsync(
        SearchQueryDto query,
        CancellationToken cancellationToken = default);
}
