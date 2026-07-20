using FileManager.Core.Entities;
using FileManager.Core.Interfaces;
using FileManager.Core.Services;
using FluentAssertions;
using Moq;

namespace FileManager.Core.Tests.Services;

public class InitialIndexingServiceTests
{
    [Fact]
    public async Task IndexDirectoryAsync_PersistsAllScannedFiles()
    {
        var scannedFiles = new[]
        {
            new FileEntry { FullPath = "a.txt", Hash = "HASH1" },
            new FileEntry { FullPath = "b.txt", Hash = "HASH2" }
        };

        var scannerMock = new Mock<IFileScanner>();
        scannerMock.Setup(s => s.Scan("some/path")).Returns(scannedFiles);

        var repositoryMock = new Mock<IFileRepository>();

        var service = new InitialIndexingService(scannerMock.Object, repositoryMock.Object);

        await service.IndexDirectoryAsync("some/path");

        repositoryMock.Verify(r => r.UpsertAsync(It.Is<FileEntry>(f => f.FullPath == "a.txt")), Times.Once);
        repositoryMock.Verify(r => r.UpsertAsync(It.Is<FileEntry>(f => f.FullPath == "b.txt")), Times.Once);
        repositoryMock.Verify(r => r.UpsertAsync(It.IsAny<FileEntry>()), Times.Exactly(2));
    }

    [Fact]
    public async Task IndexDirectoryAsync_CallsScannerWithGivenPath()
    {
        var scannerMock = new Mock<IFileScanner>();
        scannerMock.Setup(s => s.Scan(It.IsAny<string>())).Returns(Enumerable.Empty<FileEntry>());

        var repositoryMock = new Mock<IFileRepository>();

        var service = new InitialIndexingService(scannerMock.Object, repositoryMock.Object);

        await service.IndexDirectoryAsync("target/path");

        scannerMock.Verify(s => s.Scan("target/path"), Times.Once);
    }

    [Fact]
    public async Task IndexDirectoryAsync_NoFilesFound_DoesNotCallRepository()
    {
        var scannerMock = new Mock<IFileScanner>();
        scannerMock.Setup(s => s.Scan(It.IsAny<string>())).Returns(Enumerable.Empty<FileEntry>());

        var repositoryMock = new Mock<IFileRepository>();

        var service = new InitialIndexingService(scannerMock.Object, repositoryMock.Object);

        await service.IndexDirectoryAsync("empty/path");

        repositoryMock.Verify(r => r.UpsertAsync(It.IsAny<FileEntry>()), Times.Never);
    }
}
