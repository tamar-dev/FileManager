using FileManager.Application.Dtos;
using FileManager.Application.Services;

namespace FileManager.Cli.Commands;

public class SearchCommand
{
    private readonly ISearchAppService _searchAppService;

    public SearchCommand(ISearchAppService searchAppService)
    {
        _searchAppService = searchAppService;
    }

    public async Task ExecuteAsync(string[] args)
    {
        var query = ParseArgs(args);

        var results = await _searchAppService.SearchAsync(query);

        if (results.Count == 0)
        {
            Console.WriteLine("No files found.");
            return;
        }

        foreach (var file in results)
        {
            Console.WriteLine($"{file.FullPath} ({file.Size} bytes, modified {file.LastModified})");
        }
    }

    private static SearchQueryDto ParseArgs(string[] args)
    {
        var query = new SearchQueryDto();

        for (var i = 0; i < args.Length - 1; i++)
        {
            var option = args[i].ToLowerInvariant();
            var value = args[i + 1];

            switch (option)
            {
                case "--name":
                    query.Name = value;
                    i++;
                    break;
                case "--ext":
                case "--extension":
                    query.Extension = value;
                    i++;
                    break;
                case "--path":
                    query.Path = value;
                    i++;
                    break;
                case "--after":
                    if (DateTime.TryParse(value, out var after))
                    {
                        query.ModifiedAfter = after;
                    }
                    i++;
                    break;
                case "--before":
                    if (DateTime.TryParse(value, out var before))
                    {
                        query.ModifiedBefore = before;
                    }
                    i++;
                    break;
            }
        }

        return query;
    }
}
