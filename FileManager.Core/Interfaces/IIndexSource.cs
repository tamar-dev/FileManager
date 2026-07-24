namespace FileManager.Core.Interfaces;

/// <summary>
/// Represents a source capable of enumerating file paths to be indexed.
/// Implementations may back this by different discovery mechanisms
/// (e.g. recursive filesystem scanning today, NTFS MFT/USN Journal in the future)
/// without requiring changes to the indexing pipeline that consumes it.
/// </summary>
public interface IIndexSource
{
    /// <summary>
    /// Enumerates the full paths of files found under <paramref name="rootPath"/>.
    /// </summary>
    IEnumerable<string> EnumeratePaths(
        string rootPath,
        CancellationToken cancellationToken = default);
}
