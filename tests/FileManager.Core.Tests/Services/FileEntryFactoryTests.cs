using FileManager.Core.Services;
using FluentAssertions;
using Moq;
using FileManager.Core.Entities;
using FileManager.Core.Interfaces;

namespace FileManager.Core.Tests.Services;

public class FileEntryFactoryTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly FileEntryFactory _factory = new();

    public FileEntryFactoryTests()
    {
        _tempDirectory = Directory.CreateTempSubdirectory("FileEntryFactoryTests").FullName;
    }

    [Fact]
    public void Create_ExistingFile_PopulatesFullPath()
    {
        var path = CreateFile("hello world");

        var entry = _factory.Create(path);

        entry.FullPath.Should().Be(path);
    }

    [Fact]
    public void Create_ExistingFile_PopulatesSize()
    {
        var content = "hello world";
        var path = CreateFile(content);

        var entry = _factory.Create(path);

        entry.Size.Should().Be(content.Length);
    }

    [Fact]
    public void Create_ExistingFile_GeneratesSha256Hash()
    {
        var path = CreateFile("hello world");

        var entry = _factory.Create(path);

        entry.Hash.Should().NotBeNullOrEmpty();
        entry.Hash.Should().MatchRegex("^[0-9A-F]{64}$");
    }

    [Fact]
    public void Create_MissingFile_DoesNotThrowAndReturnsEmptyHash()
    {
        var path = Path.Combine(_tempDirectory, "does-not-exist.txt");

        var entry = _factory.Create(path);

        entry.Hash.Should().BeEmpty();
        entry.FullPath.Should().Be(path);
    }

    [Fact]
    public async Task CreateAsync_UnchangedFile_ReusesExistingHash()
    {
        var path = CreateFile("hello world");
        var fileInfo = new FileInfo(path);

        var existingHash = "EXISTING_HASH_VALUE_1234567890ABCDEF1234567890ABCDEF1234567890ABCDEF1234567890ABCDEF";
        var existingEntry = new FileEntry
        {
            FullPath = fileInfo.FullName,
            Size = fileInfo.Length,
            LastModified = fileInfo.LastWriteTimeUtc,
            Hash = existingHash
        };

        var mockRepository = new Mock<IFileRepository>();
        mockRepository
            .Setup(r => r.GetByPathAsync(fileInfo.FullName))
            .ReturnsAsync(existingEntry);

        var factory = new FileEntryFactory(repository: mockRepository.Object);

        var entry = await factory.CreateAsync(path);

        entry.Hash.Should().Be(existingHash);
        mockRepository.Verify(r => r.GetByPathAsync(fileInfo.FullName), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ModifiedFile_RecalculatesHash()
    {
        var path = CreateFile("hello world");
        var fileInfo = new FileInfo(path);

        var oldHash = "OLD_HASH_VALUE_1234567890ABCDEF1234567890ABCDEF1234567890ABCDEF1234567890ABCDEF";
        var existingEntry = new FileEntry
        {
            FullPath = fileInfo.FullName,
            Size = fileInfo.Length - 1,
            LastModified = fileInfo.LastWriteTimeUtc.AddMinutes(-1),
            Hash = oldHash
        };

        var mockRepository = new Mock<IFileRepository>();
        mockRepository
            .Setup(r => r.GetByPathAsync(fileInfo.FullName))
            .ReturnsAsync(existingEntry);

        var factory = new FileEntryFactory(repository: mockRepository.Object);

        var entry = await factory.CreateAsync(path);

        entry.Hash.Should().NotBe(oldHash);
        entry.Hash.Should().NotBeNullOrEmpty();
        entry.Hash.Should().MatchRegex("^[0-9A-F]{64}$");
        mockRepository.Verify(r => r.GetByPathAsync(fileInfo.FullName), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_NewFile_CalculatesHash()
    {
        var path = CreateFile("hello world");
        var fileInfo = new FileInfo(path);

        var mockRepository = new Mock<IFileRepository>();
        mockRepository
            .Setup(r => r.GetByPathAsync(fileInfo.FullName))
            .ReturnsAsync((FileEntry?)null);

        var factory = new FileEntryFactory(repository: mockRepository.Object);

        var entry = await factory.CreateAsync(path);

        entry.Hash.Should().NotBeNullOrEmpty();
        entry.Hash.Should().MatchRegex("^[0-9A-F]{64}$");
        mockRepository.Verify(r => r.GetByPathAsync(fileInfo.FullName), Times.Once);
    }

    private string CreateFile(string content)
    {
        var path = Path.Combine(_tempDirectory, Guid.NewGuid().ToString("N") + ".txt");
        File.WriteAllText(path, content);
        return path;
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }
}
