using FileManager.Application.Dtos;
using FileManager.Core.Entities;
using FileManager.Core.Interfaces;

namespace FileManager.Application.Services;

public class VirtualFolderAppService : IVirtualFolderAppService
{
    private readonly IVirtualFolderRepository _folderRepository;
    private readonly IVirtualFolderFileRepository _membershipRepository;
    private readonly IFileRepository _fileRepository;

    public VirtualFolderAppService(
        IVirtualFolderRepository folderRepository,
        IVirtualFolderFileRepository membershipRepository,
        IFileRepository fileRepository)
    {
        _folderRepository = folderRepository;
        _membershipRepository = membershipRepository;
        _fileRepository = fileRepository;
    }

    public async Task<IReadOnlyList<VirtualFolderDto>> GetTreeAsync(CancellationToken cancellationToken = default)
    {
        var all = await _folderRepository.GetAllAsync(cancellationToken);

        var dtoById = all.ToDictionary(f => f.Id, MapToDto);

        var roots = new List<VirtualFolderDto>();

        foreach (var folder in all)
        {
            var dto = dtoById[folder.Id];

            if (folder.ParentId is Guid parentId && dtoById.TryGetValue(parentId, out var parentDto))
            {
                parentDto.Children.Add(dto);
            }
            else
            {
                roots.Add(dto);
            }
        }

        return roots;
    }

    public async Task<VirtualFolderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var folder = await _folderRepository.GetByIdAsync(id, cancellationToken);
        return folder is null ? null : MapToDto(folder);
    }

    public async Task<VirtualFolderOperationResult> CreateAsync(CreateVirtualFolderDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return Fail("Folder name is required.");
        }

        if (dto.ParentId.HasValue)
        {
            var parent = await _folderRepository.GetByIdAsync(dto.ParentId.Value, cancellationToken);

            if (parent is null)
            {
                return Fail("Parent folder does not exist.", notFound: true);
            }
        }

        var duplicate = await _folderRepository.ExistsWithNameUnderParentAsync(
            dto.Name.Trim(), dto.ParentId, excludeId: null, cancellationToken);

        if (duplicate)
        {
            return Fail($"A folder named '{dto.Name.Trim()}' already exists here.", conflict: true);
        }

        var now = DateTime.UtcNow;

        var folder = new VirtualFolder
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            ParentId = dto.ParentId,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _folderRepository.AddAsync(folder, cancellationToken);

        return new VirtualFolderOperationResult
        {
            Success = true,
            Folder = MapToDto(folder)
        };
    }

    public async Task<VirtualFolderOperationResult> UpdateAsync(Guid id, UpdateVirtualFolderDto dto, CancellationToken cancellationToken = default)
    {
        var folder = await _folderRepository.GetByIdAsync(id, cancellationToken);

        if (folder is null)
        {
            return Fail("Virtual folder not found.", notFound: true);
        }

        var newName = string.IsNullOrWhiteSpace(dto.Name) ? folder.Name : dto.Name.Trim();
        var newParentId = dto.ParentId;

        if (string.IsNullOrWhiteSpace(newName))
        {
            return Fail("Folder name is required.");
        }

        if (newParentId.HasValue)
        {
            if (newParentId.Value == id)
            {
                return Fail("A folder cannot be its own parent.");
            }

            var parent = await _folderRepository.GetByIdAsync(newParentId.Value, cancellationToken);

            if (parent is null)
            {
                return Fail("Parent folder does not exist.", notFound: true);
            }

            var descendants = await _folderRepository.GetDescendantsAsync(id, cancellationToken);

            if (descendants.Any(d => d.Id == newParentId.Value))
            {
                return Fail("Cannot move a folder under one of its own descendants.");
            }
        }

        var duplicate = await _folderRepository.ExistsWithNameUnderParentAsync(
            newName, newParentId, excludeId: id, cancellationToken);

        if (duplicate)
        {
            return Fail($"A folder named '{newName}' already exists here.", conflict: true);
        }

        folder.Name = newName;
        folder.ParentId = newParentId;
        folder.UpdatedAt = DateTime.UtcNow;

        await _folderRepository.UpdateAsync(folder, cancellationToken);

        return new VirtualFolderOperationResult
        {
            Success = true,
            Folder = MapToDto(folder)
        };
    }

    public async Task<VirtualFolderOperationResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var folder = await _folderRepository.GetByIdAsync(id, cancellationToken);

        if (folder is null)
        {
            return Fail("Virtual folder not found.", notFound: true);
        }

        var hasChildren = await _folderRepository.HasChildrenAsync(id, cancellationToken);

        if (hasChildren)
        {
            return Fail("Cannot delete a folder that contains child folders.", conflict: true);
        }

        // Deleting a virtual folder only removes its metadata and memberships.
        // Physical files are never touched.
        await _membershipRepository.RemoveAllForFolderAsync(id, cancellationToken);
        await _folderRepository.RemoveAsync(id, cancellationToken);

        return new VirtualFolderOperationResult { Success = true };
    }

    public async Task<IReadOnlyList<SearchResultDto>> GetFilesInFolderAsync(Guid folderId, CancellationToken cancellationToken = default)
    {
        var files = await _membershipRepository.GetFilesInFolderAsync(folderId, cancellationToken);

        return files.Select(MapToSearchResultDto).ToList();
    }

    public async Task<VirtualFolderMembershipResult> AddFileAsync(Guid folderId, Guid fileId, CancellationToken cancellationToken = default)
    {
        var folder = await _folderRepository.GetByIdAsync(folderId, cancellationToken);

        if (folder is null)
        {
            return new VirtualFolderMembershipResult
            {
                Success = false,
                IsNotFound = true,
                ErrorMessage = "Virtual folder not found."
            };
        }

        var file = await _fileRepository.GetByIdAsync(fileId, cancellationToken);

        if (file is null)
        {
            return new VirtualFolderMembershipResult
            {
                Success = false,
                IsNotFound = true,
                ErrorMessage = "File not found in the index."
            };
        }

        var exists = await _membershipRepository.ExistsAsync(folderId, fileId, cancellationToken);

        if (exists)
        {
            // Idempotent: dropping the same file twice is not an error.
            return new VirtualFolderMembershipResult { Success = true };
        }

        await _membershipRepository.AddAsync(new Core.Entities.VirtualFolderFile
        {
            Id = Guid.NewGuid(),
            VirtualFolderId = folderId,
            FileEntryId = fileId,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        return new VirtualFolderMembershipResult { Success = true };
    }

    public async Task<VirtualFolderMembershipResult> RemoveFileAsync(Guid folderId, Guid fileId, CancellationToken cancellationToken = default)
    {
        var folder = await _folderRepository.GetByIdAsync(folderId, cancellationToken);

        if (folder is null)
        {
            return new VirtualFolderMembershipResult
            {
                Success = false,
                IsNotFound = true,
                ErrorMessage = "Virtual folder not found."
            };
        }

        // Removes only the virtual membership. The FileEntry and the physical file are untouched.
        await _membershipRepository.RemoveAsync(folderId, fileId, cancellationToken);

        return new VirtualFolderMembershipResult { Success = true };
    }

    public async Task<IReadOnlyList<VirtualFolderDto>> GetFoldersForFileAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var folders = await _membershipRepository.GetFoldersForFileAsync(fileId, cancellationToken);

        return folders.Select(MapToDto).ToList();
    }

    private static VirtualFolderOperationResult Fail(string message, bool conflict = false, bool notFound = false)
    {
        return new VirtualFolderOperationResult
        {
            Success = false,
            ErrorMessage = message,
            IsConflict = conflict,
            IsNotFound = notFound
        };
    }

    private static VirtualFolderDto MapToDto(VirtualFolder folder)
    {
        return new VirtualFolderDto
        {
            Id = folder.Id,
            Name = folder.Name,
            ParentId = folder.ParentId,
            CreatedAt = folder.CreatedAt,
            UpdatedAt = folder.UpdatedAt,
            Children = []
        };
    }

    private static SearchResultDto MapToSearchResultDto(FileEntry file)
    {
        return new SearchResultDto
        {
            Id = file.Id,
            Name = file.Name,
            Path = file.FullPath,
            Size = file.Size,
            Extension = file.Extension,
            Modified = file.LastModified,
            Hash = file.Hash
        };
    }
}
