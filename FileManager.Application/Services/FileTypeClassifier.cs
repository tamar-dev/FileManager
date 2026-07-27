namespace FileManager.Application.Services;

/// <summary>
/// Pure helper that classifies a file into a UI-friendly type category
/// based on its extension. Centralized here so the classification logic
/// is not duplicated across application services.
/// </summary>
public static class FileTypeClassifier
{
    private static readonly HashSet<string> DocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".doc", ".docx", ".txt", ".rtf", ".odt", ".md"
    };

    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".svg", ".webp", ".tiff", ".heic"
    };

    private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".mov", ".avi", ".mkv", ".wmv", ".flv", ".webm", ".m4v"
    };

    private static readonly HashSet<string> AudioExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3", ".wav", ".flac", ".aac", ".ogg", ".m4a", ".wma"
    };

    private static readonly HashSet<string> ArchiveExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".zip", ".rar", ".7z", ".tar", ".gz", ".tar.gz"
    };

    private static readonly HashSet<string> SpreadsheetExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".xls", ".xlsx", ".csv", ".ods"
    };

    public static string Classify(string extension)
    {
        if (string.IsNullOrEmpty(extension))
        {
            return "other";
        }

        if (extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return "pdf";
        }

        if (DocumentExtensions.Contains(extension))
        {
            return "document";
        }

        if (ImageExtensions.Contains(extension))
        {
            return "image";
        }

        if (VideoExtensions.Contains(extension))
        {
            return "video";
        }

        if (AudioExtensions.Contains(extension))
        {
            return "audio";
        }

        if (ArchiveExtensions.Contains(extension))
        {
            return "archive";
        }

        if (SpreadsheetExtensions.Contains(extension))
        {
            return "spreadsheet";
        }

        return "other";
    }
}
