using FileManager.Core.Services;
using FileManager.Infrastructure.Persistence;
using FileManager.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FileManager.Integration.Tests;

public class InitialIndexingFlowTests : IDisposable
{
    private readonly string _rootFolder;
    private readonly string _dataFolder;
    private readonly string _dbPath;
    private readonly FileManagerDbContext _context;

    public InitialIndexingFlowTests()
    {
        _rootFolder = Directory.CreateTempSubdirectory("InitialIndexingFlowTests").FullName;
        _dataFolder = Path.Combine(_rootFolder, "data");
        Directory.CreateDirectory(_dataFolder);

        _dbPath = Path.Combine(_rootFolder, "test.db");

        var options = new DbContextOptionsBuilder<FileManagerDbContext>()
            .UseSqlite($"Data Source={_dbPath}")
            .Options;

        _context = new FileManagerDbContext(options);
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task IndexDirectoryAsync_DiscoversAndPersistsAllFiles()
    {
        File.WriteAllText(Path.Combine(_dataFolder, "a.txt"), "content A");
        File.WriteAllText(Path.Combine(_dataFolder, "b.txt"), "content B");

        var repository = new FileRepository(_context);
        var service = new InitialIndexingService(repository, new FileSystemIndexSource(), new FileEntryFactory());

        await service.IndexDirectoryAsync(_dataFolder);

        var files = await repository.GetAllAsync();

        files.Should().HaveCount(2);
        files.Select(f => f.Name).Should().BeEquivalentTo("a.txt", "b.txt");
    }

    [Fact]
    public async Task IndexDirectoryAsync_PersistsNonEmptyHashValues()
    {
        File.WriteAllText(Path.Combine(_dataFolder, "a.txt"), "content A");

        var repository = new FileRepository(_context);
        var service = new InitialIndexingService(repository, new FileSystemIndexSource(), new FileEntryFactory());

        await service.IndexDirectoryAsync(_dataFolder);

        var files = await repository.GetAllAsync();

        files.Should().ContainSingle();
        files[0].Hash.Should().NotBeNullOrEmpty();
    }

    public void Dispose()
    {
        _context.Dispose();

        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

        if (Directory.Exists(_rootFolder))
        {
            Directory.Delete(_rootFolder, recursive: true);
        }
    }
}
