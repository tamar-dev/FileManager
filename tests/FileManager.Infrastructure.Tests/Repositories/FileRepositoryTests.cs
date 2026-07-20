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
