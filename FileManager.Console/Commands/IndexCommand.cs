using FileManager.Core.Services;
using FileManager.Infrastructure.Persistence;
using FileManager.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FileManager.Cli.Commands;

public class IndexCommand
{
    public async Task ExecuteAsync(string path)
    {
        var options = new DbContextOptionsBuilder<FileManagerDbContext>()
            .UseSqlite("Data Source=filemanager.db")
            .Options;

        using var context = new FileManagerDbContext(options);

        context.Database.Migrate();

        var repository = new FileRepository(context);

        var scanner = new FileScanner();

        var initialIndexingService = new InitialIndexingService(scanner, repository);

        Console.WriteLine($"Indexing: {path}");

        await initialIndexingService.IndexDirectoryAsync(path);

        Console.WriteLine("Indexing complete.");
    }
}
