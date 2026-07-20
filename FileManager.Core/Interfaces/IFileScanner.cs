using FileManager.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Interfaces
{
    internal interface IFileScanner
    {
        IEnumerable<FileEntry> Scan(string path);
    }
}
