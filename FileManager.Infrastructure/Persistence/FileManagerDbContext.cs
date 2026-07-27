using FileManager.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileManager.Infrastructure.Persistence;

public class FileManagerDbContext : DbContext
{
    public DbSet<FileEntry> Files => Set<FileEntry>();

    public DbSet<IndexedRoot> IndexedRoots => Set<IndexedRoot>();

    public FileManagerDbContext(
        DbContextOptions<FileManagerDbContext> options)
        : base(options)
    {
    }
}