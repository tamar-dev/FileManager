using FileManager.Core.Entities;
using FileManager.Core.Interfaces;

namespace FileManager.Core.Services;

public class DuplicateReportService
{
    private readonly IFileRepository _repository;
    private readonly DuplicateDetector _detector;

    public DuplicateReportService(IFileRepository repository, DuplicateDetector? detector = null)
    {
        _repository = repository;
        _detector = detector ?? new DuplicateDetector();
    }

    public async Task<IReadOnlyList<IGrouping<string, FileEntry>>> FindDuplicatesAsync()
    {
        var files = await _repository.GetAllAsync();

        return _detector.Find(files).ToList();
    }
}
