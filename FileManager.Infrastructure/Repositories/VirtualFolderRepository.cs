using FileManager.Core.Entities;
using FileManager.Core.Interfaces;
using FileManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FileManager.Infrastructure.Repositories;

public class VirtualFolderRepository : IVirtualFolderRepository
{
    private readonly FileManagerDbContext _context;

    public VirtualFolderRepository(FileManagerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<VirtualFolder>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.VirtualFolders
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<VirtualFolder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.VirtualFolders
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<VirtualFolder>> GetChildrenAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        return await _context.VirtualFolders
            .Where(f => f.ParentId == parentId)
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<VirtualFolder>> GetDescendantsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var all = await _context.VirtualFolders.ToListAsync(cancellationToken);

        var descendants = new List<VirtualFolder>();
        var stack = new Stack<Guid>();
        stack.Push(id);

        while (stack.Count > 0)
        {
            var currentId = stack.Pop();
            var children = all.Where(f => f.ParentId == currentId).ToList();

            foreach (var child in children)
            {
                descendants.Add(child);
                stack.Push(child.Id);
            }
        }

        return descendants;
    }

    public async Task<bool> ExistsWithNameUnderParentAsync(
        string name,
        Guid? parentId,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        return await _context.VirtualFolders
            .AnyAsync(f =>
                f.ParentId == parentId &&
                f.Name.ToLower() == name.ToLower() &&
                (excludeId == null || f.Id != excludeId),
                cancellationToken);
    }

    public async Task AddAsync(VirtualFolder folder, CancellationToken cancellationToken = default)
    {
        _context.VirtualFolders.Add(folder);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(VirtualFolder folder, CancellationToken cancellationToken = default)
    {
        _context.VirtualFolders.Update(folder);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var folder = await _context.VirtualFolders
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

        if (folder is null)
        {
            return;
        }

        _context.VirtualFolders.Remove(folder);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> HasChildrenAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.VirtualFolders.AnyAsync(f => f.ParentId == id, cancellationToken);
    }
}
