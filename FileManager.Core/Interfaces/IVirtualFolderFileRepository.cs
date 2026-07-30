using FileManager.Core.Entities;

namespace FileManager.Core.Interfaces;

public interface IVirtualFolderFileRepository
{
    Task<IReadOnlyList<FileEntry>> GetFilesInFolderAsync(Guid folderId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VirtualFolder>> GetFoldersForFileAsync(Guid fileEntryId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid folderId, Guid fileEntryId, CancellationToken cancellationToken = default);

    Task AddAsync(VirtualFolderFile membership, CancellationToken cancellationToken = default);

    Task RemoveAsync(Guid folderId, Guid fileEntryId, CancellationToken cancellationToken = default);

    Task RemoveAllForFolderAsync(Guid folderId, CancellationToken cancellationToken = default);

    Task<bool> HasAnyFilesAsync(Guid folderId, CancellationToken cancellationToken = default);
}
