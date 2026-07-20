using FileManager.Application.Dtos;
using FileManager.Core.Entities;
using FileManager.Core.Interfaces;
using FileManager.Core.Services;

namespace FileManager.Application.Services;

public class DuplicateAppService : IDuplicateAppService
{
    private readonly IFileRepository _repository;
    private readonly DuplicateDetector _detector;

    public DuplicateAppService(IFileRepository repository, DuplicateDetector? detector = null)
    {
        _repository = repository;
        _detector = detector ?? new DuplicateDetector();
    }

    public async Task<IReadOnlyList<DuplicateGroupDto>> GetDuplicateReportAsync()
    {
        var files = await _repository.GetAllAsync();

        var groups = _detector.Find(files);

        return groups.Select(MapToDto).ToList();
    }

    private static DuplicateGroupDto MapToDto(IGrouping<string, FileEntry> group)
    {
        var files = group.Select(MapToFileResult).ToList();

        var totalSize = files.Sum(f => f.Size);

        var wastedSize = files.Count > 1
            ? totalSize - files[0].Size
            : 0;

        return new DuplicateGroupDto
        {
            Hash = group.Key,
            FileCount = files.Count,
            TotalSize = totalSize,
            WastedSize = wastedSize,
            Files = files
        };
    }

    private static FileResultDto MapToFileResult(FileEntry entry)
    {
        return new FileResultDto
        {
            FullPath = entry.FullPath,
            Name = entry.Name,
            Size = entry.Size,
            Extension = entry.Extension,
            LastModified = entry.LastModified,
            Hash = entry.Hash
        };
    }
}
