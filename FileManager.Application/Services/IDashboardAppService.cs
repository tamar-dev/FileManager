using FileManager.Application.Dtos;

namespace FileManager.Application.Services;

public interface IDashboardAppService
{
    Task<DashboardDto> GetDashboardAsync();
}
