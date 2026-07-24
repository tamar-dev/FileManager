using FileManager.Core.Entities;
using FileManager.Core.Interfaces;
using FileManager.Core.Services;
using FluentAssertions;
using Moq;

namespace FileManager.Core.Tests.Services;

public class InitialIndexingServiceTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly FileSystemIndexSource _fileSystemIndexSource = new();
    private readonly FileEntryFactory _fileEntryFactory = new();

    public InitialIndexingServiceTests()
    {
        _tempDirectory = Directory.CreateTempSubdirectory("InitialIndexingServiceTests").FullName;
    }

    [Fact]
    public async Task IndexDirectoryAsync_PersistsAllFilesInDirectory()
    {
        CreateFile("a.txt", "A");
        CreateFile("b.txt", "B");

        var repositoryMock = new Mock<IFileRepository>();
        repositoryMock
            .Setup(r => r.GetByPathsAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var service = new InitialIndexingService(repositoryMock.Object, _fileSystemIndexSource, _fileEntryFactory);

        await service.IndexDirectoryAsync(_tempDirectory);

        repositoryMock.Verify(r => r.UpsertBatchAsync(
            It.Is<IReadOnlyCollection<FileEntry>>(entries => entries.Count == 2),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IndexDirectoryAsync_UsesInjectedIndexSource_InsteadOfFilesystem()
    {
        // No files are actually created on disk; the fake index source
        // reports paths that don't exist to prove the service doesn't
        // scan the filesystem directly.
        var fakePath = Path.Combine(_tempDirectory, "virtual.txt");

        var indexSourceMock = new Mock<IIndexSource>();
        indexSourceMock
            .Setup(s => s.EnumeratePaths(_tempDirectory, It.IsAny<CancellationToken>()))
            .Returns([fakePath]);

        var repositoryMock = new Mock<IFileRepository>();
        repositoryMock
            .Setup(r => r.GetByPathsAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var service = new InitialIndexingService(repositoryMock.Object, indexSourceMock.Object, _fileEntryFactory);

        await service.IndexDirectoryAsync(_tempDirectory);

        indexSourceMock.Verify(
            s => s.EnumeratePaths(_tempDirectory, It.IsAny<CancellationToken>()),
            Times.Once);

        repositoryMock.Verify(r => r.GetByPathsAsync(
            It.Is<IReadOnlyCollection<string>>(paths => paths.Contains(fakePath)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IndexDirectoryAsync_EmptyDirectory_DoesNotCallRepository()
    {
        var repositoryMock = new Mock<IFileRepository>();

        var service = new InitialIndexingService(repositoryMock.Object, _fileSystemIndexSource, _fileEntryFactory);

        await service.IndexDirectoryAsync(_tempDirectory);

        repositoryMock.Verify(r => r.GetByPathsAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()), Times.Never);
        repositoryMock.Verify(r => r.UpsertBatchAsync(It.IsAny<IReadOnlyCollection<FileEntry>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task IndexDirectoryAsync_ReindexUnchangedFolder_ReusesExistingHashes()
    {
        var path = CreateFile("same.txt", "same content");
        var info = new FileInfo(path);

        const string existingHash = "REUSED_HASH";

        var repositoryMock = new Mock<IFileRepository>();
        repositoryMock
            .Setup(r => r.GetByPathsAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new FileEntry
                {
                    FullPath = path,
                    Size = info.Length,
                    LastModified = info.LastWriteTimeUtc,
                    Hash = existingHash
                }
            ]);

        IReadOnlyCollection<FileEntry>? upserted = null;
        repositoryMock
            .Setup(r => r.UpsertBatchAsync(It.IsAny<IReadOnlyCollection<FileEntry>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyCollection<FileEntry>, CancellationToken>((entries, _) => upserted = entries)
            .Returns(Task.CompletedTask);

        var service = new InitialIndexingService(repositoryMock.Object, _fileSystemIndexSource, _fileEntryFactory);

        await service.IndexDirectoryAsync(_tempDirectory);

        upserted.Should().NotBeNull();
        upserted!.Should().ContainSingle();
        upserted.Single().Hash.Should().Be(existingHash);
    }

    [Fact]
    public async Task IndexDirectoryAsync_ModifiedFiles_GetNewHash()
    {
        var path = CreateFile("modified.txt", "new content");
        var info = new FileInfo(path);

        const string oldHash = "OLD_HASH";

        var repositoryMock = new Mock<IFileRepository>();
        repositoryMock
            .Setup(r => r.GetByPathsAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new FileEntry
                {
                    FullPath = path,
                    Size = info.Length + 1,
                    LastModified = info.LastWriteTimeUtc.AddMinutes(-1),
                    Hash = oldHash
                }
            ]);

        IReadOnlyCollection<FileEntry>? upserted = null;
        repositoryMock
            .Setup(r => r.UpsertBatchAsync(It.IsAny<IReadOnlyCollection<FileEntry>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyCollection<FileEntry>, CancellationToken>((entries, _) => upserted = entries)
            .Returns(Task.CompletedTask);

        var service = new InitialIndexingService(repositoryMock.Object, _fileSystemIndexSource, _fileEntryFactory);

        await service.IndexDirectoryAsync(_tempDirectory);

        upserted.Should().NotBeNull();
        upserted!.Should().ContainSingle();

        var entry = upserted.Single();
        entry.Hash.Should().NotBe(oldHash);
        entry.Hash.Should().MatchRegex("^[0-9A-F]{64}$");
    }

    [Fact]
    public async Task IndexDirectoryAsync_NewFiles_GetCalculatedHashes()
    {
        CreateFile("new.txt", "new file content");

        var repositoryMock = new Mock<IFileRepository>();
        repositoryMock
            .Setup(r => r.GetByPathsAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        IReadOnlyCollection<FileEntry>? upserted = null;
        repositoryMock
            .Setup(r => r.UpsertBatchAsync(It.IsAny<IReadOnlyCollection<FileEntry>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyCollection<FileEntry>, CancellationToken>((entries, _) => upserted = entries)
            .Returns(Task.CompletedTask);

        var service = new InitialIndexingService(repositoryMock.Object, _fileSystemIndexSource, _fileEntryFactory);

        await service.IndexDirectoryAsync(_tempDirectory);

        upserted.Should().NotBeNull();
        upserted!.Should().ContainSingle();
        upserted.Single().Hash.Should().MatchRegex("^[0-9A-F]{64}$");
    }

    private string CreateFile(string name, string content)
    {
        var path = Path.Combine(_tempDirectory, name);
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
