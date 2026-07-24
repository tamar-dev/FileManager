using FileManager.Core.Entities;
using FileManager.Core.Interfaces;

namespace FileManager.Core.Services;

public class FileScanner : IFileScanner
{
    private readonly IFileEntryFactory _fileEntryFactory;

    public FileScanner(IFileEntryFactory? fileEntryFactory = null)
    {
        _fileEntryFactory = fileEntryFactory ?? new FileEntryFactory();
    }

    public IEnumerable<FileEntry> Scan(string path)
    {
        var files = Directory.EnumerateFiles(
            path,
            "*",
            SearchOption.AllDirectories);


        foreach (var file in files)
        {
            yield return _fileEntryFactory.Create(file);
        }
    }
}