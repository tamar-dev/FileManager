using FileManager.Core.Entities;

namespace FileManager.Core.Interfaces;

public interface IVirtualFolderRepository
{
    Task<IReadOnlyList<VirtualFolder>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<VirtualFolder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VirtualFolder>> GetChildrenAsync(Guid parentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VirtualFolder>> GetDescendantsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsWithNameUnderParentAsync(
        string name,
        Guid? parentId,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(VirtualFolder folder, CancellationToken cancellationToken = default);

    Task UpdateAsync(VirtualFolder folder, CancellationToken cancellationToken = default);

    Task RemoveAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> HasChildrenAsync(Guid id, CancellationToken cancellationToken = default);
}
