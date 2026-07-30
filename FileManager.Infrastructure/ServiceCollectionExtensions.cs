using FileManager.Core.Interfaces;
using FileManager.Core.Services;
using FileManager.Infrastructure.Persistence;
using FileManager.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FileManager.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFileManagerInfrastructure(
        this IServiceCollection services,
        string? connectionString = null)
    {
        connectionString ??= FileManagerPaths.ConnectionString;

        services.AddDbContext<FileManagerDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IFileRepository, FileRepository>();
        services.AddScoped<IIndexedRootRepository, IndexedRootRepository>();
        services.AddScoped<IVirtualFolderRepository, VirtualFolderRepository>();
        services.AddScoped<IVirtualFolderFileRepository, VirtualFolderFileRepository>();
        services.AddScoped<IFileScanner, FileScanner>();
        services.AddScoped<IIndexSource, FileSystemIndexSource>();
        services.AddScoped<IFileEntryFactory, FileEntryFactory>();
        services.AddScoped<InitialIndexingService>();

        return services;
    }
}