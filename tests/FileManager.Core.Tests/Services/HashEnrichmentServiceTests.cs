using FileManager.Core.Entities;
using FileManager.Core.Interfaces;
using FileManager.Core.Services;
using FluentAssertions;
using Moq;

namespace FileManager.Core.Tests.Services;

public class HashEnrichmentServiceTests : IDisposable
{
    private readonly string _tempDirectory;

    public HashEnrichmentServiceTests()
    {
        _tempDirectory = Directory.CreateTempSubdirectory("HashEnrichmentServiceTests").FullName;
    }

    [Fact]
    public async Task EnrichNextBatchAsync_AccessibleFile_PersistsHash()
    {
        var path = Path.Combine(_tempDirectory, "file.txt");
        await File.WriteAllTextAsync(path, "content");

        var entry = new FileEntry { FullPath = path, Hash = "" };
        var repositoryMock = new Mock<IFileRepository>();
        repositoryMock
            .Setup(r => r.GetFilesWithoutHashAfterAsync(
                101,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([entry]);

        IReadOnlyCollection<FileEntry>? persisted = null;
        repositoryMock
            .Setup(r => r.UpsertBatchAsync(
                It.IsAny<IReadOnlyCollection<FileEntry>>(),
                It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyCollection<FileEntry>, CancellationToken>(
                (files, _) => persisted = files)
            .Returns(Task.CompletedTask);

        var service = new HashEnrichmentService(
            repositoryMock.Object,
            new FileHasher());

        var enriched = await service.EnrichNextBatchAsync();

        enriched.Should().Be(1);
        persisted.Should().NotBeNull();
        persisted!.Single().Hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task EnrichMissingHashesAsync_FailedFirstBatch_DoesNotStarveLaterFile()
    {
        var validPath = Path.Combine(_tempDirectory, "999-valid.txt");
        await File.WriteAllTextAsync(validPath, "valid content");

        var failedEntries = Enumerable.Range(0, 100)
            .Select(index => new FileEntry
            {
                FullPath = Path.Combine(
                    _tempDirectory,
                    $"{index:D3}-missing.txt"),
                Hash = ""
            })
            .ToList();

        var validEntry = new FileEntry
        {
            FullPath = validPath,
            Hash = ""
        };

        var firstPage = failedEntries
            .Append(validEntry)
            .ToList();

        var repositoryMock = new Mock<IFileRepository>();
        repositoryMock
            .SetupSequence(r => r.GetFilesWithoutHashAfterAsync(
                101,
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(firstPage)
            .ReturnsAsync([validEntry]);

        var persisted = new List<FileEntry>();
        repositoryMock
            .Setup(r => r.UpsertBatchAsync(
                It.IsAny<IReadOnlyCollection<FileEntry>>(),
                It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyCollection<FileEntry>, CancellationToken>(
                (files, _) => persisted.AddRange(files))
            .Returns(Task.CompletedTask);

        var service = new HashEnrichmentService(
            repositoryMock.Object,
            new FileHasher());

        var enriched = await service.EnrichMissingHashesAsync();

        enriched.Should().Be(1);
        persisted.Should().ContainSingle();
        persisted.Single().FullPath.Should().Be(validPath);
        persisted.Single().Hash.Should().NotBeNullOrEmpty();

        repositoryMock.Verify(
            r => r.GetFilesWithoutHashAfterAsync(
                101,
                failedEntries[^1].FullPath,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EnrichNextBatchWithCursorAsync_Cancelled_ThrowsOperationCanceledException()
    {
        var entry = new FileEntry
        {
            FullPath = Path.Combine(_tempDirectory, "missing.txt"),
            Hash = ""
        };

        var repositoryMock = new Mock<IFileRepository>();
        repositoryMock
            .Setup(r => r.GetFilesWithoutHashAfterAsync(
                101,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([entry]);

        var service = new HashEnrichmentService(
            repositoryMock.Object,
            new FileHasher());

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var act = async () => await service.EnrichNextBatchWithCursorAsync(
            null,
            cancellationToken: cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }
}
