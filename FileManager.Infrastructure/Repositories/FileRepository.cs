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

    public async Task UpsertAsync(FileEntry file)
    {
        var existing = await _context.Files
            .FirstOrDefaultAsync(f => f.FullPath == file.FullPath);

        if (existing == null)
        {
            _context.Files.Add(file);
        }
        else
        {
            existing.Name = file.Name;
            existing.Extension = file.Extension;
            existing.Size = file.Size;
            existing.LastModified = file.LastModified;
            existing.Hash = file.Hash;
        }

        await _context.SaveChangesAsync();
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
}