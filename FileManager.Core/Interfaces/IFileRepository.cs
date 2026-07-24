using FileManager.Core.Entities;

namespace FileManager.Core.Interfaces;

public interface IFileRepository
{
    Task UpsertAsync(FileEntry file);

    Task UpsertBatchAsync(
        IReadOnlyCollection<FileEntry> files,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string fullPath);

    Task<IReadOnlyList<FileEntry>> GetAllAsync();

    Task<FileEntry?> GetByPathAsync(string fullPath);

    Task<IReadOnlyList<FileEntry>> GetByPathsAsync(
        IReadOnlyCollection<string> fullPaths,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FileEntry>> SearchAsync(
        string? name = null,
        string? extension = null,
        string? path = null,
        DateTime? modifiedAfter = null,
        DateTime? modifiedBefore = null,
        CancellationToken cancellationToken = default);
}