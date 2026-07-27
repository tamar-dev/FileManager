using FileManager.Application.Dtos;
using FileManager.Application.Dtos;
using FileManager.Core.Interfaces;

namespace FileManager.Application.Services;

public class SearchAppService : ISearchAppService
{
    private readonly IFileRepository _repository;

    public SearchAppService(IFileRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<SearchResultDto>> SearchAsync(SearchQueryDto query)
    {
        var files = await _repository.GetAllAsync();

        var results = files.AsEnumerable();

        if (!string.IsNullOrEmpty(query.Name))
        {
            results = results.Where(f => f.Name.Contains(query.Name, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(query.Extension))
        {
            results = results.Where(f => f.Extension.Equals(query.Extension, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(query.Path))
        {
            results = results.Where(f => f.FullPath.Contains(query.Path, StringComparison.OrdinalIgnoreCase));
        }

        if (query.ModifiedAfter.HasValue)
        {
            results = results.Where(f => f.LastModified >= query.ModifiedAfter.Value);
        }

        if (query.ModifiedBefore.HasValue)
        {
            results = results.Where(f => f.LastModified <= query.ModifiedBefore.Value);
        }

        return results
            .Select(f => new SearchResultDto
            {
                Id = f.Id,
                Name = f.Name,
                Path = f.FullPath,
                Size = f.Size,
                Extension = f.Extension,
                Modified = f.LastModified,
                Type = FileTypeClassifier.Classify(f.Extension),
                Hash = f.Hash
            })
            .ToList();
    }
}
