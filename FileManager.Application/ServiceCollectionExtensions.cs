using FileManager.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FileManager.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFileManagerApplication(this IServiceCollection services)
    {
        services.AddScoped<IIndexingAppService, IndexingAppService>();
        services.AddScoped<IDuplicateAppService, DuplicateAppService>();
        services.AddScoped<IDashboardAppService, DashboardAppService>();
        services.AddScoped<ISearchAppService, SearchAppService>();
        services.AddScoped<IIndexedLocationAppService, IndexedLocationAppService>();
        services.AddSingleton<IIndexStatusService, IndexStatusService>();
        services.AddSingleton<IIndexingOrchestrationAppService, IndexingOrchestrationAppService>();

        return services;
    }
}
