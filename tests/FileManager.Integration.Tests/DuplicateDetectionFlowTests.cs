using FileManager.Application.Services;
using FileManager.Core.Services;
using FileManager.Infrastructure.Persistence;
using FileManager.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FileManager.Integration.Tests;

public class DuplicateDetectionFlowTests : IDisposable
{
    private readonly string _rootFolder;
    private readonly string _dataFolder;
    private readonly string _dbPath;
    private readonly FileManagerDbContext _context;

    public DuplicateDetectionFlowTests()
    {
        _rootFolder = Directory
            .CreateTempSubdirectory("DuplicateDetectionFlowTests")
            .FullName;

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
    public async Task IndexAndHashEnrichment_WithDuplicateFiles_ReportsDuplicateGroupEndToEnd()
    {
        File.WriteAllText(
            Path.Combine(_dataFolder, "original.txt"),
            "duplicate content");

        File.WriteAllText(
            Path.Combine(_dataFolder, "copy.txt"),
            "duplicate content");

        File.WriteAllText(
            Path.Combine(_dataFolder, "unique.txt"),
            "unique content");

        var repository = new FileRepository(_context);
        var duplicateAppService = new DuplicateAppService(repository);

        var indexingService = new InitialIndexingService(
            repository,
            new FileSystemIndexSource(),
            new FileEntryFactory());

        await indexingService.IndexDirectoryAsync(_dataFolder);

        var indexedFiles = await repository.GetAllAsync();

        indexedFiles.Should().HaveCount(3);
        indexedFiles.Should().OnlyContain(
            file => string.IsNullOrEmpty(file.Hash));

        var hashEnrichmentService = new HashEnrichmentService(
            repository,
            new FileHasher());

        var enrichedCount = await hashEnrichmentService
            .EnrichMissingHashesAsync();

        enrichedCount.Should().Be(3);

        var enrichedFiles = await repository.GetAllAsync();

        enrichedFiles.Should().OnlyContain(
            file => !string.IsNullOrEmpty(file.Hash));

        var groups = await duplicateAppService.GetDuplicateReportAsync();

        groups.Should().ContainSingle();
        groups[0].FileCount.Should().Be(2);
        groups[0].Files.Should().Contain(f => f.Name == "original.txt");
        groups[0].Files.Should().Contain(f => f.Name == "copy.txt");
        groups[0].Files.Should().NotContain(f => f.Name == "unique.txt");
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
