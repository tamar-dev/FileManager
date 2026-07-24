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
        return EnumerateSafe(rootPath, cancellationToken);
    }

    private static IEnumerable<string> EnumerateSafe(
        string directory,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IEnumerable<string> files;
        try
        {
            files = Directory.EnumerateFiles(directory);
        }
        catch (UnauthorizedAccessException)
        {
            yield break;
        }
        catch (IOException)
        {
            yield break;
        }

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return file;
        }

        IEnumerable<string> subdirectories;
        try
        {
            subdirectories = Directory.EnumerateDirectories(directory);
        }
        catch (UnauthorizedAccessException)
        {
            yield break;
        }
        catch (IOException)
        {
            yield break;
        }

        foreach (var subdirectory in subdirectories)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var file in EnumerateSafe(subdirectory, cancellationToken))
            {
                yield return file;
            }
        }
    }
}
