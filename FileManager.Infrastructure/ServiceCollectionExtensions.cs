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
        string connectionString = "Data Source=filemanager.db")
    {
        services.AddDbContext<FileManagerDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IFileRepository, FileRepository>();
        services.AddScoped<IFileScanner, FileScanner>();

        return services;
    }
}
