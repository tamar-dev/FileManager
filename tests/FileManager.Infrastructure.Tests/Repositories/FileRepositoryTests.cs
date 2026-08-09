using FileManager.Core.Entities;
using FileManager.Infrastructure.Persistence;
using FileManager.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FileManager.Infrastructure.Tests.Repositories;

public class FileRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly FileManagerDbContext _context;
    private readonly FileRepository _repository;

    public FileRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<FileManagerDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new FileManagerDbContext(options);
        _context.Database.EnsureCreated();

        _repository = new FileRepository(_context);
    }

    [Fact]
    public async Task UpsertAsync_NewFile_AddsFile()
    {
        var entry = CreateEntry("a.txt", hash: "HASH1");

        await _repository.UpsertAsync(entry);

        var files = await _repository.GetAllAsync();
        files.Should().ContainSingle(f => f.FullPath == "a.txt" && f.Hash == "HASH1");
    }

    [Fact]
    public async Task UpsertAsync_ExistingFile_UpdatesFields()
    {
        var entry = CreateEntry("a.txt", hash: "HASH1", size: 10);
        await _repository.UpsertAsync(entry);

        var updated = CreateEntry("a.txt", hash: "HASH2", size: 20);
        await _repository.UpsertAsync(updated);

        var files = await _repository.GetAllAsync();
        files.Should().ContainSingle();
        files[0].Hash.Should().Be("HASH2");
        files[0].Size.Should().Be(20);
    }

    [Fact]
    public async Task UpsertBatchAsync_MixedEntries_InsertsAndUpdates()
    {
        await _repository.UpsertAsync(CreateEntry("a.txt", hash: "OLD", size: 10));

        var batch = new List<FileEntry>
        {
            CreateEntry("a.txt", hash: "NEW", size: 20),
            CreateEntry("b.txt", hash: "HASH-B", size: 30)
        };

        await _repository.UpsertBatchAsync(batch);

        var files = await _repository.GetAllAsync();

        files.Should().HaveCount(2);
        files.Should().Contain(f => f.FullPath == "a.txt" && f.Hash == "NEW" && f.Size == 20);
        files.Should().Contain(f => f.FullPath == "b.txt" && f.Hash == "HASH-B" && f.Size == 30);
    }

    [Fact]
    public async Task DeleteAsync_ExistingFile_RemovesFile()
    {
        var entry = CreateEntry("a.txt", hash: "HASH1");
        await _repository.UpsertAsync(entry);

        await _repository.DeleteAsync("a.txt");

        var files = await _repository.GetAllAsync();
        files.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_NonExistentFile_DoesNotThrow()
    {
        var act = async () => await _repository.DeleteAsync("does-not-exist.txt");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllPersistedFiles()
    {
        await _repository.UpsertAsync(CreateEntry("a.txt", hash: "HASH1"));
        await _repository.UpsertAsync(CreateEntry("b.txt", hash: "HASH2"));

        var files = await _repository.GetAllAsync();

        files.Should().HaveCount(2);
        files.Select(f => f.FullPath).Should().BeEquivalentTo("a.txt", "b.txt");
    }

    [Fact]
    public async Task UpsertAsync_PersistsHashValueCorrectly()
    {
        var entry = CreateEntry("a.txt", hash: "6600CCE3A93121E79F93AD2175251D6D4C9B2A1241FBC1A8A45058E94E16ECD2");

        await _repository.UpsertAsync(entry);

        var files = await _repository.GetAllAsync();
        files.Single().Hash.Should().Be("6600CCE3A93121E79F93AD2175251D6D4C9B2A1241FBC1A8A45058E94E16ECD2");
    }

    [Fact]
    public async Task GetByPathAsync_ExistingPath_ReturnsFileEntry()
    {
        await _repository.UpsertAsync(CreateEntry("a.txt", hash: "HASH1"));

        var result = await _repository.GetByPathAsync("a.txt");

        result.Should().NotBeNull();
        result!.FullPath.Should().Be("a.txt");
        result.Hash.Should().Be("HASH1");
    }

    [Fact]
    public async Task GetByPathAsync_MissingPath_ReturnsNull()
    {
        var result = await _repository.GetByPathAsync("does-not-exist.txt");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByPathAsync_SoftDeletedEntry_ReturnsNull()
    {
        var entry = CreateEntry("a.txt", hash: "HASH1");
        await _repository.UpsertAsync(entry);

        var persisted = await _context.Files.FirstAsync(f => f.FullPath == "a.txt");
        persisted.IsDeleted = true;
        await _context.SaveChangesAsync();

        var result = await _repository.GetByPathAsync("a.txt");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByPathsAsync_ReturnsOnlyRequestedNonDeletedEntries()
    {
        await _repository.UpsertAsync(CreateEntry("a.txt", hash: "HASH1"));
        await _repository.UpsertAsync(CreateEntry("b.txt", hash: "HASH2"));
        await _repository.UpsertAsync(CreateEntry("c.txt", hash: "HASH3"));

        var deleted = await _context.Files.FirstAsync(f => f.FullPath == "c.txt");
        deleted.IsDeleted = true;
        await _context.SaveChangesAsync();

        var result = await _repository.GetByPathsAsync(["a.txt", "c.txt", "missing.txt"]);

        result.Should().HaveCount(1);
        result.Single().FullPath.Should().Be("a.txt");
    }

    [Fact]
    public async Task GetFilesWithoutHashAfterAsync_ReturnsPendingRowsAfterCursorInOrder()
    {
        await _repository.UpsertAsync(CreateEntry("a.txt", hash: ""));
        await _repository.UpsertAsync(CreateEntry("b.txt", hash: ""));
        await _repository.UpsertAsync(CreateEntry("c.txt", hash: ""));
        await _repository.UpsertAsync(CreateEntry("d.txt", hash: "READY"));

        var result = await _repository.GetFilesWithoutHashAfterAsync(
            limit: 10,
            afterPath: "a.txt");

        result.Select(file => file.FullPath)
            .Should()
            .Equal("b.txt", "c.txt");
    }

    private static FileEntry CreateEntry(string fullPath, string hash, long size = 100)
    {
        return new FileEntry
        {
            FullPath = fullPath,
            Name = Path.GetFileName(fullPath),
            Extension = Path.GetExtension(fullPath),
            Size = size,
            LastModified = DateTime.UtcNow,
            Hash = hash
        };
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}