using FileManager.Core.Interfaces;

namespace FileManager.Core.Services;

public class InitialIndexingService
{
    private readonly IFileScanner _scanner;
    private readonly IFileRepository _repository;

    public InitialIndexingService(IFileScanner scanner, IFileRepository repository)
    {
        _scanner = scanner;
        _repository = repository;
    }

    public async Task IndexDirectoryAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        foreach (var entry in _scanner.Scan(path))
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _repository.UpsertAsync(entry);

            Console.WriteLine($"Indexed: {entry.FullPath}");
        }
    }
}
