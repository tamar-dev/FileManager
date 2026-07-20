using FileManager.Core.Entities;

namespace FileManager.Core.Services;

public class FileEntryFactory
{
    private readonly FileHasher _hasher;

    public FileEntryFactory(FileHasher? hasher = null)
    {
        _hasher = hasher ?? new FileHasher();
    }

    public FileEntry Create(string fullPath)
    {
        var info = new FileInfo(fullPath);

        return new FileEntry
        {
            FullPath = info.FullName,
            Name = info.Name,
            Extension = info.Extension,
            Size = info.Exists ? info.Length : 0,
            LastModified = info.Exists ? info.LastWriteTimeUtc : default,
            Hash = info.Exists ? _hasher.Calculate(info.FullName) : ""
        };
    }
}
