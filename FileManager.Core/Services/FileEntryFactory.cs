using FileManager.Core.Entities;
using FileManager.Core.Interfaces;

namespace FileManager.Core.Services;

public class FileEntryFactory : IFileEntryFactory
{
    private readonly FileHasher _hasher;
    private readonly IFileRepository? _repository;

    public FileEntryFactory(
        FileHasher? hasher = null,
        IFileRepository? repository = null)
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
            LastModified = info.Exists
                ? info.LastWriteTimeUtc
                : default,
            Hash = info.Exists
                ? _hasher.Calculate(info.FullName)
                : "",
            IndexedAt = DateTime.UtcNow
        };
    }

    public async Task<FileEntry> CreateAsync(
        string fullPath,
        FileEntry? existingEntry = null)
    {
        var info = new FileInfo(fullPath);

        if (!info.Exists)
        {
            return CreateMissingEntry(info);
        }

        // Full CreateAsync may still be used outside the batch indexing flow,
        // for example by the watcher, so repository lookup is kept here.
        existingEntry ??=
            _repository != null
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

        return CreateEntry(
            info,
            currentSize,
            currentLastModified,
            hash);
    }

    public Task<FileEntry> CreateMetadataAsync(
        string fullPath,
        FileEntry? existingEntry = null)
    {
        var info = new FileInfo(fullPath);

        if (!info.Exists)
        {
            return Task.FromResult(
                CreateMissingEntry(info));
        }

        // IMPORTANT:
        // Do not query the repository here.
        // InitialIndexingService already loaded all existing entries
        // for the current batch with GetByPathsAsync.

        var currentSize = info.Length;
        var currentLastModified = info.LastWriteTimeUtc;

        var unchanged =
            existingEntry != null &&
            existingEntry.Size == currentSize &&
            existingEntry.LastModified == currentLastModified;

        var hash = unchanged
            ? existingEntry!.Hash
            : "";

        return Task.FromResult(
            CreateEntry(
                info,
                currentSize,
                currentLastModified,
                hash));
    }

    private static FileEntry CreateEntry(
        FileInfo info,
        long size,
        DateTime lastModified,
        string hash)
    {
        return new FileEntry
        {
            FullPath = info.FullName,
            Name = info.Name,
            Extension = info.Extension,
            Size = size,
            LastModified = lastModified,
            Hash = hash,
            IndexedAt = DateTime.UtcNow
        };
    }

    private static FileEntry CreateMissingEntry(
        FileInfo info)
    {
        return new FileEntry
        {
            FullPath = info.FullName,
            Name = info.Name,
            Extension = info.Extension,
            Size = 0,
            LastModified = default,
            Hash = "",
            IndexedAt = DateTime.UtcNow
        };
    }
}