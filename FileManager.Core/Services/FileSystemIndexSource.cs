using FileManager.Core.Interfaces;

namespace FileManager.Core.Services;

/// <summary>
/// An <see cref="IIndexSource"/> implementation backed by a recursive
/// filesystem scan. This is today's default discovery mechanism and can
/// later be replaced or complemented by faster sources (e.g. an NTFS MFT
/// or USN Journal based source) without changing the indexing pipeline.
/// </summary>
public class FileSystemIndexSource : IIndexSource
{
    public IEnumerable<string> EnumeratePaths(
        string rootPath,
        CancellationToken cancellationToken = default)
    {
        foreach (var filePath in Directory.EnumerateFiles(rootPath, "*", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return filePath;
        }
    }
}
