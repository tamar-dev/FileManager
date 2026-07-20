using FileManager.Application.Dtos;
using FileManager.Core.Interfaces;
using FileManager.Core.Services;

namespace FileManager.Application.Services;

public class DashboardAppService : IDashboardAppService
{
    private readonly IFileRepository _repository;
    private readonly DuplicateDetector _detector;

    public DashboardAppService(IFileRepository repository, DuplicateDetector? detector = null)
    {
        _repository = repository;
        _detector = detector ?? new DuplicateDetector();
    }

    public async Task<DashboardDto> GetDashboardAsync()
    {
        var files = await _repository.GetAllAsync();

        var duplicateGroups = _detector.Find(files).ToList();

        var duplicateFileCount = duplicateGroups.Sum(g => g.Count());

        var potentialSavings = duplicateGroups.Sum(g =>
        {
            var sizes = g.Select(f => f.Size).ToList();

            return sizes.Sum() - sizes[0];
        });

        return new DashboardDto
        {
            IndexedFileCount = files.Count,
            TotalIndexedSize = files.Sum(f => f.Size),
            DuplicateGroupCount = duplicateGroups.Count,
            DuplicateFileCount = duplicateFileCount,
            PotentialStorageSavings = potentialSavings
        };
    }
}
