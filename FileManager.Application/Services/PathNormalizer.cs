namespace FileManager.Application.Services;

public static class PathNormalizer
{
    public static string Normalize(string rawPath)
    {
        if (rawPath is null)
        {
            throw new ArgumentNullException(nameof(rawPath));
        }

        var trimmed = rawPath.Trim();

        if (trimmed.Length >= 2 &&
            ((trimmed[0] == '"' && trimmed[^1] == '"') ||
             (trimmed[0] == '\'' && trimmed[^1] == '\'')))
        {
            trimmed = trimmed[1..^1].Trim();
        }

        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new ArgumentException("Path is empty.", nameof(rawPath));
        }

        string fullPath;

        try
        {
            fullPath = Path.GetFullPath(trimmed);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            throw new ArgumentException($"Path '{trimmed}' is not a valid path.", nameof(rawPath));
        }

        var root = Path.GetPathRoot(fullPath);

        if (!string.IsNullOrEmpty(root) && fullPath.Length > root.Length)
        {
            fullPath = fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        return fullPath;
    }
}
