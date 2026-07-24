using FileManager.Core.Entities;
using FileManager.Infrastructure.Persistence;
using FileManager.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FileManager.Integration.Tests;

public class SearchFlowTests : IDisposable
{
    private readonly string _rootFolder;
    private readonly string _dbPath;
    private readonly FileManagerDbContext _context;
    private readonly FileRepository _repository;

    public SearchFlowTests()
    {
        _rootFolder = Directory.CreateTempSubdirectory("SearchFlowTests").FullName;
        _dbPath = Path.Combine(_rootFolder, "test.db");

        var options = new DbContextOptionsBuilder<FileManagerDbContext>()
            .UseSqlite($"Data Source={_dbPath}")
            .Options;

        _context = new FileManagerDbContext(options);
        _context.Database.EnsureCreated();

        _repository = new FileRepository(_context);
    }

    [Fact]
    public async Task SearchAsync_ByName_ReturnsMatchingFiles()
    {
        await SeedFilesAsync();

        var results = await _repository.SearchAsync(name: "report");

        results.Should().ContainSingle();
        results[0].Name.Should().Be("report.txt");
    }

    [Fact]
    public async Task SearchAsync_ByExtension_ReturnsMatchingFiles()
    {
        await SeedFilesAsync();

        var results = await _repository.SearchAsync(extension: ".log");

        results.Should().ContainSingle();
        results[0].Name.Should().Be("system.log");
    }

    [Fact]
    public async Task SearchAsync_ByPath_ReturnsMatchingFiles()
    {
        await SeedFilesAsync();

        var results = await _repository.SearchAsync(path: "logs");

        results.Should().ContainSingle();
        results[0].Name.Should().Be("system.log");
    }

    [Fact]
    public async Task SearchAsync_ByModifiedDateRange_ReturnsMatchingFiles()
    {
        await SeedFilesAsync();

        var results = await _repository.SearchAsync(
            modifiedAfter: new DateTime(2024, 1, 1),
            modifiedBefore: new DateTime(2024, 6, 30));

        results.Should().ContainSingle();
        results[0].Name.Should().Be("report.txt");
    }

    [Fact]
    public async Task SearchAsync_ExcludesDeletedFiles()
    {
        await SeedFilesAsync();

        var results = await _repository.SearchAsync(name: "deleted");

        results.Should().BeEmpty();
    }

    private async Task SeedFilesAsync()
    {
        await _repository.UpsertBatchAsync(new List<FileEntry>
        {
            new()
            {
                FullPath = "C:\\docs\\report.txt",
                Name = "report.txt",
                Extension = ".txt",
                LastModified = new DateTime(2024, 3, 15)
            },
            new()
            {
                FullPath = "C:\\logs\\system.log",
                Name = "system.log",
                Extension = ".log",
                LastModified = new DateTime(2025, 1, 1)
            }
        });

        _context.Files.Add(new FileEntry
        {
            FullPath = "C:\\docs\\deleted.txt",
            Name = "deleted.txt",
            Extension = ".txt",
            IsDeleted = true
        });

        await _context.SaveChangesAsync();
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
