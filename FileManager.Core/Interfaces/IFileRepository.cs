using FileManager.Core.Entities;

namespace FileManager.Core.Interfaces;

public interface IFileRepository
{
    Task UpsertAsync(FileEntry file);

    Task DeleteAsync(string fullPath);

    Task<IReadOnlyList<FileEntry>> GetAllAsync();

    Task<FileEntry?> GetByPathAsync(string fullPath);
}