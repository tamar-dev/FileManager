using FileManager.Core.Services;
using FileManager.Application.Services;

namespace FileManager.Cli.Commands;

public class DuplicatesCommand
{
    private readonly IDuplicateAppService _duplicateAppService;

    public DuplicatesCommand(IDuplicateAppService duplicateAppService)
    {
        _duplicateAppService = duplicateAppService;
    }

    public async Task ExecuteAsync()
    {
        var groups = await _duplicateAppService.GetDuplicateReportAsync();

        if (groups.Count == 0)
        {
            Console.WriteLine("No duplicates found.");
            return;
        }

        foreach (var group in groups)
        {
            Console.WriteLine($"Hash: {group.Hash} ({group.FileCount} files)");

            foreach (var file in group.Files)
            {
                Console.WriteLine($"  {file.FullPath}");
            }
        }
    }
}
