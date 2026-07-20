using FileManager.Core.Services;
using FileManager.Application.Services;

namespace FileManager.Cli.Commands;

public class IndexCommand
{
    private readonly IIndexingAppService _indexingAppService;

    public IndexCommand(IIndexingAppService indexingAppService)
    {
        _indexingAppService = indexingAppService;
    }

    public async Task ExecuteAsync(string path)
    {
        Console.WriteLine($"Indexing: {path}");

        var progress = new Progress<string>(file => Console.WriteLine($"Indexed: {file}"));

        var result = await _indexingAppService.IndexDirectoryAsync(path, progress);

        if (!result.Success)
        {
            Console.WriteLine($"Indexing failed: {result.ErrorMessage}");
            return;
        }

        Console.WriteLine($"Indexing complete. {result.FilesIndexed} files indexed.");
    }
}
