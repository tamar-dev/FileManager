using FileManager.Application.Dtos;
using FileManager.Application.Dtos;
using FileManager.Core.Entities;
using FileManager.Core.Interfaces;

namespace FileManager.Application.Services;

public class IndexedLocationAppService : IIndexedLocationAppService
{
    private readonly IIndexedRootRepository _repository;

    public IndexedLocationAppService(IIndexedRootRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<IndexedLocationDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var roots = await _repository.GetAllAsync(cancellationToken);

        return roots.Select(MapToDto).ToList();
    }

    public async Task<IndexedLocationAddResult> AddAsync(AddIndexedLocationDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Path))
        {
            return new IndexedLocationAddResult
            {
                Success = false,
                ErrorMessage = "Path is required."
            };
        }

        string normalizedPath;

        try
        {
            normalizedPath = PathNormalizer.Normalize(dto.Path);
        }
        catch (ArgumentException ex)
        {
            return new IndexedLocationAddResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }

        if (!Directory.Exists(normalizedPath))
        {
            return new IndexedLocationAddResult
            {
                Success = false,
                ErrorMessage = $"Directory '{normalizedPath}' does not exist."
            };
        }

        var existing = await _repository.GetByNormalizedPathAsync(normalizedPath, cancellationToken);

        if (existing is not null)
        {
            return new IndexedLocationAddResult
            {
                Success = false,
                ErrorMessage = $"Location '{normalizedPath}' is already indexed.",
                IsDuplicate = true
            };
        }

        var root = new IndexedRoot
        {
            Id = Guid.NewGuid(),
            Path = normalizedPath,
            Enabled = true,
            WatchEnabled = dto.WatchEnabled,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(root, cancellationToken);

        return new IndexedLocationAddResult
        {
            Success = true,
            Location = MapToDto(root)
        };
    }

    public async Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);

        if (existing is null)
        {
            return false;
        }

        await _repository.RemoveAsync(id, cancellationToken);
        return true;
    }

    private static IndexedLocationDto MapToDto(IndexedRoot root)
    {
        return new IndexedLocationDto
        {
            Id = root.Id,
            Path = root.Path,
            Enabled = root.Enabled,
            WatchEnabled = root.WatchEnabled,
            LastIndexedAt = root.LastIndexedAt,
            CreatedAt = root.CreatedAt
        };
    }
}
