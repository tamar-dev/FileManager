using FileManager.Core.Entities;

namespace FileManager.Core.Interfaces;

public interface IFileIndexer
{
    Task IndexAsync(
        string path,
        CancellationToken cancellationToken = default);


    Task UpdateAsync(
        string path,
        CancellationToken cancellationToken = default);


    Task RemoveAsync(
        string path,
        CancellationToken cancellationToken = default);
}