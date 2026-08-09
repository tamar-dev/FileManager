using FileManager.Core.Entities;
using FileManager.Core.Interfaces;
using FileManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FileManager.Infrastructure.Repositories;

public class FileRepository : IFileRepository
{
    private readonly FileManagerDbContext _context;

    public FileRepository(FileManagerDbContext context)
    {
        _context = context;
    }

    public void AddRange(IEnumerable<FileEntry> files)
    {
        _context.Files.AddRange(files);
        _context.SaveChanges();
    }

    public Task UpsertAsync(FileEntry file)
    {
        return UpsertBatchAsync([file]);
    }

    public async Task UpsertBatchAsync(
        IReadOnlyCollection<FileEntry> files,
        CancellationToken cancellationToken = default)
    {
        if (files.Count == 0)
        {
            return;
        }

        var paths = files
            .Select(file => file.FullPath)
            .Distinct()
            .ToList();

        var existingEntries = await _context.Files
            .Where(f => paths.Contains(f.FullPath))
            .ToListAsync(cancellationToken);

        var existingByPath = existingEntries
            .ToDictionary(file => file.FullPath, StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            if (!existingByPath.TryGetValue(file.FullPath, out var existing))
            {
                _context.Files.Add(file);
                continue;
            }

            existing.Name = file.Name;
            existing.Extension = file.Extension;
            existing.Size = file.Size;
            existing.LastModified = file.LastModified;
            existing.Hash = file.Hash;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(string fullPath)
    {
        var file = await _context.Files
            .FirstOrDefaultAsync(f => f.FullPath == fullPath);

        if (file == null)
            return;

        _context.Files.Remove(file);

        await _context.SaveChangesAsync();
    }

    public int Count()
    {
        return _context.Files.Count();
    }

    public async Task<IReadOnlyList<FileEntry>> GetAllAsync()
    {
        return await _context.Files
            .Where(f => !f.IsDeleted)
            .ToListAsync();
    }

    public async Task<FileEntry?> GetByPathAsync(string fullPath)
    {
        return await _context.Files
            .FirstOrDefaultAsync(f => f.FullPath == fullPath && !f.IsDeleted);
    }

    public async Task<FileEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Files
            .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<FileEntry>> GetByPathsAsync(
        IReadOnlyCollection<string> fullPaths,
        CancellationToken cancellationToken = default)
    {
        if (fullPaths.Count == 0)
        {
            return [];
        }

        return await _context.Files
            .Where(f => fullPaths.Contains(f.FullPath) && !f.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public Task<IReadOnlyList<FileEntry>> GetFilesWithoutHashAsync(
        int limit,
        CancellationToken cancellationToken = default) =>
        GetFilesWithoutHashAfterAsync(limit, null, cancellationToken);

    public async Task<IReadOnlyList<FileEntry>> GetFilesWithoutHashAfterAsync(
        int limit,
        string? afterPath,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Files
            .Where(file =>
                !file.IsDeleted &&
                string.IsNullOrEmpty(file.Hash));

        if (!string.IsNullOrEmpty(afterPath))
        {
            query = query.Where(file => string.Compare(file.FullPath, afterPath) > 0);
        }

        return await query
            .OrderBy(file => file.FullPath)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}
