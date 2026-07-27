using FileManager.Core.Entities;

namespace FileManager.Core.Interfaces;

public interface IIndexedRootRepository
{
    Task<IReadOnlyList<IndexedRoot>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IndexedRoot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IndexedRoot?> GetByNormalizedPathAsync(string normalizedPath, CancellationToken cancellationToken = default);

    Task AddAsync(IndexedRoot root, CancellationToken cancellationToken = default);

    Task RemoveAsync(Guid id, CancellationToken cancellationToken = default);

    Task UpdateAsync(IndexedRoot root, CancellationToken cancellationToken = default);
}
