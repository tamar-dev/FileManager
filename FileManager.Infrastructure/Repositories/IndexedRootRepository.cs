using FileManager.Core.Entities;
using FileManager.Core.Interfaces;
using FileManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FileManager.Infrastructure.Repositories;

public class IndexedRootRepository : IIndexedRootRepository
{
    private readonly FileManagerDbContext _context;

    public IndexedRootRepository(FileManagerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<IndexedRoot>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.IndexedRoots
            .OrderBy(r => r.Path)
            .ToListAsync(cancellationToken);
    }

    public async Task<IndexedRoot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.IndexedRoots
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IndexedRoot?> GetByNormalizedPathAsync(string normalizedPath, CancellationToken cancellationToken = default)
    {
        var roots = await _context.IndexedRoots.ToListAsync(cancellationToken);

        return roots.FirstOrDefault(r =>
            string.Equals(r.Path, normalizedPath, StringComparison.OrdinalIgnoreCase));
    }

    public async Task AddAsync(IndexedRoot root, CancellationToken cancellationToken = default)
    {
        _context.IndexedRoots.Add(root);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var root = await _context.IndexedRoots
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (root is null)
        {
            return;
        }

        _context.IndexedRoots.Remove(root);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(IndexedRoot root, CancellationToken cancellationToken = default)
    {
        _context.IndexedRoots.Update(root);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
