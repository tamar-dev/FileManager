using FileManager.Application.Services;

namespace FileManager.Cli.Commands;

public class DashboardCommand
{
    private readonly IDashboardAppService _dashboardAppService;

    public DashboardCommand(IDashboardAppService dashboardAppService)
    {
        _dashboardAppService = dashboardAppService;
    }

    public async Task ExecuteAsync()
    {
        var dashboard = await _dashboardAppService.GetDashboardAsync();

        Console.WriteLine($"Indexed files: {dashboard.IndexedFileCount}");
        Console.WriteLine($"Total indexed size: {dashboard.TotalIndexedSize} bytes");
        Console.WriteLine($"Duplicate groups: {dashboard.DuplicateGroupCount}");
        Console.WriteLine($"Duplicate files: {dashboard.DuplicateFileCount}");
        Console.WriteLine($"Potential storage savings: {dashboard.PotentialStorageSavings} bytes");
    }
}
