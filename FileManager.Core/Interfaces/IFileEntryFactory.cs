using FileManager.Core.Entities;

namespace FileManager.Core.Interfaces;

public interface IFileEntryFactory
{
    FileEntry Create(string fullPath);

    Task<FileEntry> CreateAsync(string fullPath, FileEntry? existingEntry = null);
}
