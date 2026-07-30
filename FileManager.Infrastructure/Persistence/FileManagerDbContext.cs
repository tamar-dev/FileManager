using FileManager.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileManager.Infrastructure.Persistence;

public class FileManagerDbContext : DbContext
{
    public DbSet<FileEntry> Files => Set<FileEntry>();

    public DbSet<IndexedRoot> IndexedRoots => Set<IndexedRoot>();

    public DbSet<VirtualFolder> VirtualFolders => Set<VirtualFolder>();

    public DbSet<VirtualFolderFile> VirtualFolderFiles => Set<VirtualFolderFile>();

    public FileManagerDbContext(
        DbContextOptions<FileManagerDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<VirtualFolder>(entity =>
        {
            entity.HasIndex(f => new { f.ParentId, f.Name });
        });

        modelBuilder.Entity<VirtualFolderFile>(entity =>
        {
            entity.HasIndex(f => new { f.VirtualFolderId, f.FileEntryId }).IsUnique();
        });
    }
}