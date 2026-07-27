using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FileManager.Infrastructure.Persistence;

public class FileManagerDbContextFactory : IDesignTimeDbContextFactory<FileManagerDbContext>
{
    public FileManagerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FileManagerDbContext>();
        optionsBuilder.UseSqlite(FileManagerPaths.ConnectionString);

        return new FileManagerDbContext(optionsBuilder.Options);
    }
}
