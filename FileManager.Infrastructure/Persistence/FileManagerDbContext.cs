using FileManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace FileManager.Infrastructure.Persistence;

public class FileManagerDbContext : DbContext
{
    public DbSet<FileEntry> Files => Set<FileEntry>();

    public FileManagerDbContext(
      DbContextOptions<FileManagerDbContext> options)
      : base(options)
    {
    }

    protected override void OnConfiguring(
    DbContextOptionsBuilder optionsBuilder)
    {
        var dbPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "filemanager.db");

        optionsBuilder.UseSqlite(
            $"Data Source={dbPath}");
    }
}