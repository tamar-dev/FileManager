using FileManager.Application.Dtos;

namespace FileManager.Application.Services;

public interface IIndexedLocationAppService
{
    Task<IReadOnlyList<IndexedLocationDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IndexedLocationAddResult> AddAsync(AddIndexedLocationDto dto, CancellationToken cancellationToken = default);

    Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken = default);
}

public class IndexedLocationAddResult
{
    public bool Success { get; set; }

    public string? ErrorMessage { get; set; }

    public bool IsDuplicate { get; set; }

    public IndexedLocationDto? Location { get; set; }
}
