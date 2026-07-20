using FileManager.Core.Services;
using FileManager.Infrastructure.Persistence;
using FileManager.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FileManager.Cli.Commands;

public class DuplicatesCommand
{
    public async Task ExecuteAsync()
    {
        var options = new DbContextOptionsBuilder<FileManagerDbContext>()
            .UseSqlite("Data Source=filemanager.db")
            .Options;

        using var context = new FileManagerDbContext(options);

        context.Database.Migrate();

        var repository = new FileRepository(context);

        var duplicateReportService = new DuplicateReportService(repository);

        var groups = await duplicateReportService.FindDuplicatesAsync();

        if (groups.Count == 0)
        {
            Console.WriteLine("No duplicates found.");
            return;
        }

        foreach (var group in groups)
        {
            Console.WriteLine($"Hash: {group.Key} ({group.Count()} files)");

            foreach (var file in group)
            {
                Console.WriteLine($"  {file.FullPath}");
            }
        }
    }
}
