namespace FileManager.Infrastructure;

public static class FileManagerPaths
{
    public static string AppDataDirectory
    {
        get
        {
            var localAppData =
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            var directory = Path.Combine(localAppData, "FileManager");

            Directory.CreateDirectory(directory);

            return directory;
        }
    }

    public static string DatabasePath =>
        Path.Combine(AppDataDirectory, "filemanager.db");

    public static string ConnectionString =>
        $"Data Source={DatabasePath}";
}