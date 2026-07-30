using FileManager.Application.Dtos;

namespace FileManager.Application.Services;

public interface IVirtualFolderAppService
{
    Task<IReadOnlyList<VirtualFolderDto>> GetTreeAsync(CancellationToken cancellationToken = default);

    Task<VirtualFolderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<VirtualFolderOperationResult> CreateAsync(CreateVirtualFolderDto dto, CancellationToken cancellationToken = default);

    Task<VirtualFolderOperationResult> UpdateAsync(Guid id, UpdateVirtualFolderDto dto, CancellationToken cancellationToken = default);

    Task<VirtualFolderOperationResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SearchResultDto>> GetFilesInFolderAsync(Guid folderId, CancellationToken cancellationToken = default);

    Task<VirtualFolderMembershipResult> AddFileAsync(Guid folderId, Guid fileId, CancellationToken cancellationToken = default);

    Task<VirtualFolderMembershipResult> RemoveFileAsync(Guid folderId, Guid fileId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VirtualFolderDto>> GetFoldersForFileAsync(Guid fileId, CancellationToken cancellationToken = default);
}
