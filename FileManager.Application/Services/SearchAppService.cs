using FileManager.Application.Dtos;
using FileManager.Core.Entities;
using FileManager.Core.Interfaces;

namespace FileManager.Application.Services;

public class SearchAppService : ISearchAppService
{
    private readonly IFileRepository _repository;

    public SearchAppService(IFileRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<FileResultDto>> SearchAsync(
        SearchQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var results = await _repository.SearchAsync(
            query.Name,
            query.Extension,
            query.Path,
            query.ModifiedAfter,
            query.ModifiedBefore,
            cancellationToken);

        return results.Select(MapToDto).ToList();
    }

    private static FileResultDto MapToDto(FileEntry entry)
    {
        return new FileResultDto
        {
            FullPath = entry.FullPath,
            Name = entry.Name,
            Size = entry.Size,
            Extension = entry.Extension,
            LastModified = entry.LastModified,
            Hash = entry.Hash
        };
    }
}
