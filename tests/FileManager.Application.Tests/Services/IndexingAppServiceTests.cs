using FileManager.Application.Services;
using FileManager.Core.Entities;
using FileManager.Core.Interfaces;
using FluentAssertions;
using Moq;

namespace FileManager.Application.Tests.Services;

public class IndexingAppServiceTests
{
    [Fact]
    public async Task IndexDirectoryAsync_PersistsAllScannedFiles_AndReturnsSuccess()
    {
        var scannedFiles = new[]
        {
            new FileEntry { FullPath = "a.txt", Hash = "HASH1" },
            new FileEntry { FullPath = "b.txt", Hash = "HASH2" }
        };

        var scannerMock = new Mock<IFileScanner>();
        scannerMock.Setup(s => s.Scan("some/path")).Returns(scannedFiles);

        var repositoryMock = new Mock<IFileRepository>();

        var service = new IndexingAppService(scannerMock.Object, repositoryMock.Object);

        var result = await service.IndexDirectoryAsync("some/path");

        result.Success.Should().BeTrue();
        result.FilesIndexed.Should().Be(2);
        result.Path.Should().Be("some/path");

        repositoryMock.Verify(r => r.UpsertAsync(It.IsAny<FileEntry>()), Times.Exactly(2));
    }

    [Fact]
    public async Task IndexDirectoryAsync_NoFiles_ReturnsZeroCount()
    {
        var scannerMock = new Mock<IFileScanner>();
        scannerMock.Setup(s => s.Scan(It.IsAny<string>())).Returns(Enumerable.Empty<FileEntry>());

        var repositoryMock = new Mock<IFileRepository>();

        var service = new IndexingAppService(scannerMock.Object, repositoryMock.Object);

        var result = await service.IndexDirectoryAsync("empty/path");

        result.Success.Should().BeTrue();
        result.FilesIndexed.Should().Be(0);

        repositoryMock.Verify(r => r.UpsertAsync(It.IsAny<FileEntry>()), Times.Never);
    }

    [Fact]
    public async Task IndexDirectoryAsync_ReportsProgressForEachFile()
    {
        var scannedFiles = new[]
        {
            new FileEntry { FullPath = "a.txt" }
        };

        var scannerMock = new Mock<IFileScanner>();
        scannerMock.Setup(s => s.Scan(It.IsAny<string>())).Returns(scannedFiles);

        var repositoryMock = new Mock<IFileRepository>();

        var service = new IndexingAppService(scannerMock.Object, repositoryMock.Object);

        var reported = new List<string>();
        var progress = new Progress<string>(reported.Add);

        await service.IndexDirectoryAsync("path", progress);

        await Task.Yield();

        reported.Should().Contain("a.txt");
    }

    [Fact]
    public async Task IndexDirectoryAsync_Cancelled_ReturnsFailureResult()
    {
        var scannerMock = new Mock<IFileScanner>();
        scannerMock.Setup(s => s.Scan(It.IsAny<string>()))
            .Returns(new[] { new FileEntry { FullPath = "a.txt" } });

        var repositoryMock = new Mock<IFileRepository>();

        var service = new IndexingAppService(scannerMock.Object, repositoryMock.Object);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await service.IndexDirectoryAsync("path", cancellationToken: cts.Token);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }
}
