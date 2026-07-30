using FileManager.Application.Dtos;
using FileManager.Application.Dtos;
using FileManager.Application.Services;
using FileManager.Core.Entities;
using FileManager.Core.Interfaces;
using FluentAssertions;
using Moq;

namespace FileManager.Application.Tests.Services;

public class IndexedLocationAppServiceTests
{
    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "FileManagerTests_" + Guid.NewGuid());
        Directory.CreateDirectory(path);
        return path;
    }

    [Fact]
    public async Task AddAsync_PlainValidPath_Succeeds()
    {
        var tempDir = CreateTempDirectory();
        try
        {
            var repositoryMock = new Mock<IIndexedRootRepository>();
            repositoryMock
                .Setup(r => r.GetByNormalizedPathAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IndexedRoot?)null);

            var service = new IndexedLocationAppService(repositoryMock.Object);

            var result = await service.AddAsync(new AddIndexedLocationDto { Path = tempDir });

            result.Success.Should().BeTrue();
            result.Location.Should().NotBeNull();
            result.Location!.Path.Should().Be(Path.GetFullPath(tempDir).TrimEnd(Path.DirectorySeparatorChar));

            repositoryMock.Verify(r => r.AddAsync(It.IsAny<IndexedRoot>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task AddAsync_PathWithSurroundingQuotes_NormalizesAndSucceeds()
    {
        var tempDir = CreateTempDirectory();
        try
        {
            var repositoryMock = new Mock<IIndexedRootRepository>();
            repositoryMock
                .Setup(r => r.GetByNormalizedPathAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IndexedRoot?)null);

            var service = new IndexedLocationAppService(repositoryMock.Object);

            var result = await service.AddAsync(new AddIndexedLocationDto { Path = $"\"{tempDir}\"" });

            result.Success.Should().BeTrue();
            result.Location!.Path.Should().Be(Path.GetFullPath(tempDir).TrimEnd(Path.DirectorySeparatorChar));
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task AddAsync_PathWithSurroundingWhitespace_NormalizesAndSucceeds()
    {
        var tempDir = CreateTempDirectory();
        try
        {
            var repositoryMock = new Mock<IIndexedRootRepository>();
            repositoryMock
                .Setup(r => r.GetByNormalizedPathAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IndexedRoot?)null);

            var service = new IndexedLocationAppService(repositoryMock.Object);

            var result = await service.AddAsync(new AddIndexedLocationDto { Path = $"   {tempDir}   " });

            result.Success.Should().BeTrue();
            result.Location!.Path.Should().Be(Path.GetFullPath(tempDir).TrimEnd(Path.DirectorySeparatorChar));
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task AddAsync_PathWithTrailingSlash_NormalizesAndSucceeds()
    {
        var tempDir = CreateTempDirectory();
        try
        {
            var repositoryMock = new Mock<IIndexedRootRepository>();
            repositoryMock
                .Setup(r => r.GetByNormalizedPathAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IndexedRoot?)null);

            var service = new IndexedLocationAppService(repositoryMock.Object);

            var result = await service.AddAsync(new AddIndexedLocationDto { Path = tempDir + Path.DirectorySeparatorChar });

            result.Success.Should().BeTrue();
            result.Location!.Path.Should().Be(Path.GetFullPath(tempDir).TrimEnd(Path.DirectorySeparatorChar));
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task AddAsync_PathWithDotSegment_NormalizesToEquivalentPath()
    {
        var tempDir = CreateTempDirectory();
        try
        {
            var repositoryMock = new Mock<IIndexedRootRepository>();
            repositoryMock
                .Setup(r => r.GetByNormalizedPathAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IndexedRoot?)null);

            var service = new IndexedLocationAppService(repositoryMock.Object);

            var pathWithDot = Path.Combine(tempDir, ".");

            var result = await service.AddAsync(new AddIndexedLocationDto { Path = pathWithDot });

            result.Success.Should().BeTrue();
            result.Location!.Path.Should().Be(Path.GetFullPath(tempDir).TrimEnd(Path.DirectorySeparatorChar));
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task AddAsync_DuplicateNormalizedPath_ReturnsConflict()
    {
        var tempDir = CreateTempDirectory();
        try
        {
            var normalized = Path.GetFullPath(tempDir).TrimEnd(Path.DirectorySeparatorChar);

            var repositoryMock = new Mock<IIndexedRootRepository>();
            repositoryMock
                .Setup(r => r.GetByNormalizedPathAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new IndexedRoot { Id = Guid.NewGuid(), Path = normalized });

            var service = new IndexedLocationAppService(repositoryMock.Object);

            var result = await service.AddAsync(new AddIndexedLocationDto { Path = tempDir + Path.DirectorySeparatorChar });

            result.Success.Should().BeFalse();
            result.IsDuplicate.Should().BeTrue();

            repositoryMock.Verify(r => r.AddAsync(It.IsAny<IndexedRoot>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task AddAsync_NonexistentDirectory_ReturnsFailure()
    {
        var repositoryMock = new Mock<IIndexedRootRepository>();
        repositoryMock
            .Setup(r => r.GetByNormalizedPathAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IndexedRoot?)null);

        var service = new IndexedLocationAppService(repositoryMock.Object);

        var nonexistentPath = Path.Combine(Path.GetTempPath(), "DoesNotExist_" + Guid.NewGuid());

        var result = await service.AddAsync(new AddIndexedLocationDto { Path = nonexistentPath });

        result.Success.Should().BeFalse();
        result.IsDuplicate.Should().BeFalse();
        result.ErrorMessage.Should().Contain("does not exist");
    }

    [Fact]
    public async Task AddAsync_EmptyPath_ReturnsFailure()
    {
        var repositoryMock = new Mock<IIndexedRootRepository>();
        var service = new IndexedLocationAppService(repositoryMock.Object);

        var result = await service.AddAsync(new AddIndexedLocationDto { Path = "   " });

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("required");

        repositoryMock.Verify(r => r.AddAsync(It.IsAny<IndexedRoot>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddAsync_InvalidPathSyntax_ReturnsFailure()
    {
        var repositoryMock = new Mock<IIndexedRootRepository>();
        var service = new IndexedLocationAppService(repositoryMock.Object);

        var invalidPath = "D:\\Some|Invalid<Path>";

        var result = await service.AddAsync(new AddIndexedLocationDto { Path = invalidPath });

        result.Success.Should().BeFalse();

        repositoryMock.Verify(r => r.AddAsync(It.IsAny<IndexedRoot>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
