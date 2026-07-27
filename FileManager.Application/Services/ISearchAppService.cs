using FileManager.Application.Dtos;

namespace FileManager.Application.Services;

public interface ISearchAppService
{
    Task<IReadOnlyList<SearchResultDto>> SearchAsync(SearchQueryDto query);
}
