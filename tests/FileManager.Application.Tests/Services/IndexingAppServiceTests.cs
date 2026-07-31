using FileManager.Application.Services;
using FileManager.Core.Entities;
using FileManager.Core.Interfaces;
using FileManager.Core.Services;
using FluentAssertions;
using Moq;

namespace FileManager.Application.Tests.Services;

public class IndexingAppServiceTests
{
    [Fact]
    public async Task IndexDirectoryAsync_PersistsAllScannedFiles_AndReturnsSuccess()
    {
        var paths = new[]
        {
            "a.txt",
            "b.txt"
        };

        var indexSourceMock = new Mock<IIndexSource>();

        indexSourceMock
            .Setup(s => s.EnumeratePaths(
                "some/path",
                It.IsAny<CancellationToken>()))
            .Returns(paths);

        var fileRepositoryMock = new Mock<IFileRepository>();

        fileRepositoryMock
            .Setup(r => r.GetByPathsAsync(
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<FileEntry>());

        var fileEntryFactoryMock = new Mock<IFileEntryFactory>();

        fileEntryFactoryMock
            .Setup(f => f.CreateMetadataAsync(
                "a.txt",
                It.IsAny<FileEntry?>()))
            .ReturnsAsync(new FileEntry
            {
                FullPath = "a.txt",
                Hash = "HASH1"
            });

        fileEntryFactoryMock
            .Setup(f => f.CreateMetadataAsync(
                "b.txt",
                It.IsAny<FileEntry?>()))
            .ReturnsAsync(new FileEntry
            {
                FullPath = "b.txt",
                Hash = "HASH2"
            });

        var initialIndexingService = new InitialIndexingService(
            fileRepositoryMock.Object,
            indexSourceMock.Object,
            fileEntryFactoryMock.Object);

        var indexedRootRepositoryMock =
            new Mock<IIndexedRootRepository>();

        indexedRootRepositoryMock
            .Setup(r => r.GetByNormalizedPathAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IndexedRoot?)null);

        var service = new IndexingAppService(
            initialIndexingService,
            indexedRootRepositoryMock.Object);

        var result =
            await service.IndexDirectoryAsync("some/path");

        result.Success.Should().BeTrue();
        result.FilesIndexed.Should().Be(2);
        result.Path.Should().Be("some/path");

        fileRepositoryMock.Verify(
            r => r.UpsertBatchAsync(
                It.Is<IReadOnlyCollection<FileEntry>>(
                    entries => entries.Count == 2),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task IndexDirectoryAsync_NoFiles_ReturnsZeroCount()
    {
        var indexSourceMock = new Mock<IIndexSource>();

        indexSourceMock
            .Setup(s => s.EnumeratePaths(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Array.Empty<string>());

        var fileRepositoryMock =
            new Mock<IFileRepository>();

        var fileEntryFactoryMock =
            new Mock<IFileEntryFactory>();

        var initialIndexingService =
            new InitialIndexingService(
                fileRepositoryMock.Object,
                indexSourceMock.Object,
                fileEntryFactoryMock.Object);

        var indexedRootRepositoryMock =
            new Mock<IIndexedRootRepository>();

        indexedRootRepositoryMock
            .Setup(r => r.GetByNormalizedPathAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IndexedRoot?)null);

        var service = new IndexingAppService(
            initialIndexingService,
            indexedRootRepositoryMock.Object);

        var result =
            await service.IndexDirectoryAsync("empty/path");

        result.Success.Should().BeTrue();
        result.FilesIndexed.Should().Be(0);

        fileRepositoryMock.Verify(
            r => r.UpsertBatchAsync(
                It.IsAny<IReadOnlyCollection<FileEntry>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task IndexDirectoryAsync_ReportsProgressForEachFile()
    {
        var indexSourceMock =
            new Mock<IIndexSource>();

        indexSourceMock
            .Setup(s => s.EnumeratePaths(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(new[]
            {
                "a.txt"
            });

        var fileRepositoryMock =
            new Mock<IFileRepository>();

        fileRepositoryMock
            .Setup(r => r.GetByPathsAsync(
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<FileEntry>());

        var fileEntryFactoryMock =
            new Mock<IFileEntryFactory>();

        fileEntryFactoryMock
            .Setup(f => f.CreateMetadataAsync(
                "a.txt",
                It.IsAny<FileEntry?>()))
            .ReturnsAsync(new FileEntry
            {
                FullPath = "a.txt"
            });

        var initialIndexingService =
            new InitialIndexingService(
                fileRepositoryMock.Object,
                indexSourceMock.Object,
                fileEntryFactoryMock.Object);

        var indexedRootRepositoryMock =
            new Mock<IIndexedRootRepository>();

        indexedRootRepositoryMock
            .Setup(r => r.GetByNormalizedPathAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IndexedRoot?)null);

        var service =
            new IndexingAppService(
                initialIndexingService,
                indexedRootRepositoryMock.Object);

        var reported = new List<string>();

        var progress =
            new ImmediateProgress<string>(
                reported.Add);

        await service.IndexDirectoryAsync(
            "path",
            progress);

        reported.Should().ContainSingle();
        reported.Should().Contain("a.txt");
    }

    [Fact]
    public async Task IndexDirectoryAsync_Cancelled_ReturnsFailureResult()
    {
        var indexSourceMock =
            new Mock<IIndexSource>();

        indexSourceMock
            .Setup(s => s.EnumeratePaths(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(new[]
            {
                "a.txt"
            });

        var fileRepositoryMock =
            new Mock<IFileRepository>();

        var fileEntryFactoryMock =
            new Mock<IFileEntryFactory>();

        var initialIndexingService =
            new InitialIndexingService(
                fileRepositoryMock.Object,
                indexSourceMock.Object,
                fileEntryFactoryMock.Object);

        var indexedRootRepositoryMock =
            new Mock<IIndexedRootRepository>();

        var service =
            new IndexingAppService(
                initialIndexingService,
                indexedRootRepositoryMock.Object);

        using var cts =
            new CancellationTokenSource();

        cts.Cancel();

        var result =
            await service.IndexDirectoryAsync(
                "path",
                cancellationToken: cts.Token);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task IndexDirectoryAsync_WhenIndexedRootExists_UpdatesLastIndexedAt()
    {
        var path =
            Path.Combine(
                Path.GetTempPath(),
                "FileManager-Test");

        var indexSourceMock =
            new Mock<IIndexSource>();

        indexSourceMock
            .Setup(s => s.EnumeratePaths(
                path,
                It.IsAny<CancellationToken>()))
            .Returns(Array.Empty<string>());

        var fileRepositoryMock =
            new Mock<IFileRepository>();

        var fileEntryFactoryMock =
            new Mock<IFileEntryFactory>();

        var initialIndexingService =
            new InitialIndexingService(
                fileRepositoryMock.Object,
                indexSourceMock.Object,
                fileEntryFactoryMock.Object);

        var root = new IndexedRoot
        {
            Id = Guid.NewGuid(),
            Path = path,
            Enabled = true,
            WatchEnabled = true,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var indexedRootRepositoryMock =
            new Mock<IIndexedRootRepository>();

        indexedRootRepositoryMock
            .Setup(r => r.GetByNormalizedPathAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(root);

        var service =
            new IndexingAppService(
                initialIndexingService,
                indexedRootRepositoryMock.Object);

        var before =
            DateTime.UtcNow;

        var result =
            await service.IndexDirectoryAsync(path);

        result.Success.Should().BeTrue();

        root.LastIndexedAt.Should().NotBeNull();
        root.LastIndexedAt.Should().BeOnOrAfter(before);

        indexedRootRepositoryMock.Verify(
            r => r.UpdateAsync(
                root,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private sealed class ImmediateProgress<T> : IProgress<T>
    {
        private readonly Action<T> _handler;

        public ImmediateProgress(Action<T> handler)
        {
            _handler = handler;
        }

        public void Report(T value)
        {
            _handler(value);
        }
    }
}