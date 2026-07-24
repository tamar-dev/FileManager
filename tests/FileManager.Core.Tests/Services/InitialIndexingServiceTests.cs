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

    // -------------------------------------------------------------------------
    // Stub helper — lets individual tests control which paths succeed or throw.
    // Avoids Moq proxy issues with a concrete class that has no parameterless ctor.
    // -------------------------------------------------------------------------
    private sealed class StubFileEntryFactory : FileEntryFactory
    {
        private readonly Dictionary<string, Func<Task<FileEntry>>> _overrides = new();

        public void SetupThrows<TException>(string path, TException exception)
            where TException : Exception
        {
            _overrides[path] = () => throw exception;
        }

        public void SetupReturns(string path, FileEntry entry)
        {
            _overrides[path] = () => Task.FromResult(entry);
        }

        public override Task<FileEntry> CreateAsync(string fullPath, FileEntry? existingEntry = null)
        {
            if (_overrides.TryGetValue(fullPath, out var handler))
                return handler();

            return base.CreateAsync(fullPath, existingEntry);
        }
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
    public async Task IndexDirectoryAsync_ReindexUnchangedFolder_ReuseingHashes()
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

    [Fact]
    public async Task IndexDirectoryAsync_OneFileThrowsIOException_SkipsFileAndPersistsRemainder()
    {
        var goodPath1 = CreateFile("good1.txt", "content1");
        var badPath   = CreateFile("bad.txt",   "content");
        var goodPath2 = CreateFile("good2.txt", "content2");

        var indexSourceMock = new Mock<IIndexSource>();
        indexSourceMock
            .Setup(s => s.EnumeratePaths(_tempDirectory, It.IsAny<CancellationToken>()))
            .Returns([goodPath1, badPath, goodPath2]);

        var stub = new StubFileEntryFactory();
        stub.SetupReturns(goodPath1, new FileEntry { FullPath = goodPath1, Hash = "HASH1" });
        stub.SetupThrows(badPath, new IOException("File is locked"));
        stub.SetupReturns(goodPath2, new FileEntry { FullPath = goodPath2, Hash = "HASH2" });

        var repositoryMock = new Mock<IFileRepository>();
        repositoryMock
            .Setup(r => r.GetByPathsAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        IReadOnlyCollection<FileEntry>? upserted = null;
        repositoryMock
            .Setup(r => r.UpsertBatchAsync(It.IsAny<IReadOnlyCollection<FileEntry>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyCollection<FileEntry>, CancellationToken>((entries, _) => upserted = entries)
            .Returns(Task.CompletedTask);

        var service = new InitialIndexingService(repositoryMock.Object, indexSourceMock.Object, stub);

        await service.IndexDirectoryAsync(_tempDirectory);

        upserted.Should().NotBeNull();
        upserted!.Should().HaveCount(2);
        upserted.Select(e => e.FullPath).Should().BeEquivalentTo([goodPath1, goodPath2]);
    }

    [Fact]
    public async Task IndexDirectoryAsync_OneFileThrowsUnauthorizedAccess_SkipsFileAndPersistsRemainder()
    {
        var goodPath       = CreateFile("good.txt",       "content");
        var restrictedPath = CreateFile("restricted.txt", "content");

        var indexSourceMock = new Mock<IIndexSource>();
        indexSourceMock
            .Setup(s => s.EnumeratePaths(_tempDirectory, It.IsAny<CancellationToken>()))
            .Returns([goodPath, restrictedPath]);

        var stub = new StubFileEntryFactory();
        stub.SetupReturns(goodPath, new FileEntry { FullPath = goodPath, Hash = "HASH_GOOD" });
        stub.SetupThrows(restrictedPath, new UnauthorizedAccessException("Access denied"));

        var repositoryMock = new Mock<IFileRepository>();
        repositoryMock
            .Setup(r => r.GetByPathsAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        IReadOnlyCollection<FileEntry>? upserted = null;
        repositoryMock
            .Setup(r => r.UpsertBatchAsync(It.IsAny<IReadOnlyCollection<FileEntry>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyCollection<FileEntry>, CancellationToken>((entries, _) => upserted = entries)
            .Returns(Task.CompletedTask);

        var service = new InitialIndexingService(repositoryMock.Object, indexSourceMock.Object, stub);

        await service.IndexDirectoryAsync(_tempDirectory);

        upserted.Should().NotBeNull();
        upserted!.Should().ContainSingle();
        upserted.Single().FullPath.Should().Be(goodPath);
    }

    [Fact]
    public async Task IndexDirectoryAsync_AllFilesFailInBatch_DoesNotCallUpsert()
    {
        var path1 = CreateFile("bad1.txt", "content");
        var path2 = CreateFile("bad2.txt", "content");

        var indexSourceMock = new Mock<IIndexSource>();
        indexSourceMock
            .Setup(s => s.EnumeratePaths(_tempDirectory, It.IsAny<CancellationToken>()))
            .Returns([path1, path2]);

        var stub = new StubFileEntryFactory();
        stub.SetupThrows(path1, new IOException("Locked"));
        stub.SetupThrows(path2, new IOException("Locked"));

        var repositoryMock = new Mock<IFileRepository>();
        repositoryMock
            .Setup(r => r.GetByPathsAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var service = new InitialIndexingService(repositoryMock.Object, indexSourceMock.Object, stub);

        await service.IndexDirectoryAsync(_tempDirectory);

        repositoryMock.Verify(
            r => r.UpsertBatchAsync(It.IsAny<IReadOnlyCollection<FileEntry>>(), It.IsAny<CancellationToken>()),
            Times.Never);
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
            Directory.Delete(_tempDirectory, recursive: true);
    }
}
