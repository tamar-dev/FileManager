using System.Collections.Generic;
using FileManager.Core.Entities;

namespace FileManager.Core.Interfaces
{
    public interface IFileScanner
    {
        IEnumerable<FileEntry> Scan(string path);
    }
}
