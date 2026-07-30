using FileManager.Core.Entities;
using FileManager.Core.Interfaces;
using FileManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FileManager.Infrastructure.Repositories;

public class VirtualFolderFileRepository : IVirtualFolderFileRepository
{
    private readonly FileManagerDbContext _context;

    public VirtualFolderFileRepository(FileManagerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<FileEntry>> GetFilesInFolderAsync(Guid folderId, CancellationToken cancellationToken = default)
    {
        return await _context.VirtualFolderFiles
            .Where(m => m.VirtualFolderId == folderId)
            .Join(_context.Files, m => m.FileEntryId, f => f.Id, (m, f) => f)
            .Where(f => !f.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<VirtualFolder>> GetFoldersForFileAsync(Guid fileEntryId, CancellationToken cancellationToken = default)
    {
        return await _context.VirtualFolderFiles
            .Where(m => m.FileEntryId == fileEntryId)
            .Join(_context.VirtualFolders, m => m.VirtualFolderId, vf => vf.Id, (m, vf) => vf)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid folderId, Guid fileEntryId, CancellationToken cancellationToken = default)
    {
        return await _context.VirtualFolderFiles
            .AnyAsync(m => m.VirtualFolderId == folderId && m.FileEntryId == fileEntryId, cancellationToken);
    }

    public async Task AddAsync(VirtualFolderFile membership, CancellationToken cancellationToken = default)
    {
        _context.VirtualFolderFiles.Add(membership);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(Guid folderId, Guid fileEntryId, CancellationToken cancellationToken = default)
    {
        var membership = await _context.VirtualFolderFiles
            .FirstOrDefaultAsync(m => m.VirtualFolderId == folderId && m.FileEntryId == fileEntryId, cancellationToken);

        if (membership is null)
        {
            return;
        }

        _context.VirtualFolderFiles.Remove(membership);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAllForFolderAsync(Guid folderId, CancellationToken cancellationToken = default)
    {
        var memberships = await _context.VirtualFolderFiles
            .Where(m => m.VirtualFolderId == folderId)
            .ToListAsync(cancellationToken);

        if (memberships.Count == 0)
        {
            return;
        }

        _context.VirtualFolderFiles.RemoveRange(memberships);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> HasAnyFilesAsync(Guid folderId, CancellationToken cancellationToken = default)
    {
        return await _context.VirtualFolderFiles.AnyAsync(m => m.VirtualFolderId == folderId, cancellationToken);
    }
}
