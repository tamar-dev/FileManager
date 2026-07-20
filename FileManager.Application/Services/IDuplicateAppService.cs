using FileManager.Application.Dtos;

namespace FileManager.Application.Services;

public interface IDuplicateAppService
{
    Task<IReadOnlyList<DuplicateGroupDto>> GetDuplicateReportAsync();
}
