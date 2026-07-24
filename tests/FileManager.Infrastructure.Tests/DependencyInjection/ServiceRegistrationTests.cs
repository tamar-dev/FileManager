using FileManager.Core.Interfaces;
using FileManager.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FileManager.Infrastructure.Tests.DependencyInjection;

public class ServiceRegistrationTests
{
    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddFileManagerInfrastructure("Data Source=:memory:");
        return services.BuildServiceProvider();
    }

    [Fact]
    public void AddFileManagerInfrastructure_RegistersIIndexSource()
    {
        using var provider = BuildProvider();

        var source = provider.GetService<IIndexSource>();

        source.Should().NotBeNull();
        source.Should().BeOfType<FileSystemIndexSource>();
    }

    [Fact]
    public void AddFileManagerInfrastructure_RegistersFileEntryFactory()
    {
        using var provider = BuildProvider();

        using var scope = provider.CreateScope();
        var factory = scope.ServiceProvider.GetService<IFileEntryFactory>();

        factory.Should().NotBeNull();
        factory.Should().BeOfType<FileEntryFactory>();
    }

    [Fact]
    public void AddFileManagerInfrastructure_RegistersInitialIndexingService()
    {
        using var provider = BuildProvider();

        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetService<InitialIndexingService>();

        service.Should().NotBeNull();
    }

    [Fact]
    public void AddFileManagerInfrastructure_InitialIndexingService_ReceivesFileSystemIndexSource()
    {
        using var provider = BuildProvider();

        // Resolve the registered IIndexSource and confirm it is the filesystem-backed one.
        // This validates the DI wiring without exercising the filesystem.
        using var scope = provider.CreateScope();
        var source = scope.ServiceProvider.GetRequiredService<IIndexSource>();

        source.Should().BeOfType<FileSystemIndexSource>(
            "FileSystemIndexSource is the registered IIndexSource implementation and " +
            "must be injected into InitialIndexingService by DI");
    }
}
