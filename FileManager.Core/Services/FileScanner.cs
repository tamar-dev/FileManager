using FileManager.Core.Entities;
using FileManager.Core.Interfaces;

namespace FileManager.Core.Services;

public class FileScanner : IFileScanner
{
    private readonly FileHasher _hasher = new();
    public IEnumerable<FileEntry> Scan(string path)
    {
        var files = Directory.EnumerateFiles(
            path,
            "*",
            SearchOption.AllDirectories);


        foreach (var file in files)
        {
            var info = new FileInfo(file);

            yield return new FileEntry
            {
                FullPath = info.FullName,
                Name = info.Name,
                Size = info.Length,
                Extension = info.Extension,
                LastModified = info.LastWriteTime,
                Hash = _hasher.Calculate(info.FullName)
            };
        }
    }
}