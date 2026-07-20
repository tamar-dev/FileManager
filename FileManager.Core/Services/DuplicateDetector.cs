using FileManager.Core.Entities;

namespace FileManager.Core.Services;

public class DuplicateDetector
{
    public IEnumerable<IGrouping<string, FileEntry>> Find(
        IEnumerable<FileEntry> files)
    {
        return files
            .Where(x => !string.IsNullOrEmpty(x.Hash))
            .GroupBy(x => x.Hash)
            .Where(x => x.Count() > 1);
    }
}