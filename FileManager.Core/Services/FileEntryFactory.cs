using FileManager.Core.Entities;
using FileManager.Core.Interfaces;

namespace FileManager.Core.Services;

public class FileEntryFactory
{
    private readonly FileHasher _hasher;
    private readonly IFileRepository? _repository;

    public FileEntryFactory(FileHasher? hasher = null, IFileRepository? repository = null)
    {
        _hasher = hasher ?? new FileHasher();
        _repository = repository;
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

    public async Task<FileEntry> CreateAsync(string fullPath, FileEntry? existingEntry = null)
    {
        var info = new FileInfo(fullPath);

        if (!info.Exists)
        {
            return new FileEntry
            {
                FullPath = info.FullName,
                Name = info.Name,
                Extension = info.Extension,
                Size = 0,
                LastModified = default,
                Hash = ""
            };
        }

        existingEntry ??= _repository != null
            ? await _repository.GetByPathAsync(info.FullName)
            : null;

        var currentSize = info.Length;
        var currentLastModified = info.LastWriteTimeUtc;

        string hash;
        if (existingEntry != null &&
            existingEntry.Size == currentSize &&
            existingEntry.LastModified == currentLastModified)
        {
            hash = existingEntry.Hash;
        }
        else
        {
            hash = _hasher.Calculate(info.FullName);
        }

        return new FileEntry
        {
            FullPath = info.FullName,
            Name = info.Name,
            Extension = info.Extension,
            Size = currentSize,
            LastModified = currentLastModified,
            Hash = hash
        };
    }
}
