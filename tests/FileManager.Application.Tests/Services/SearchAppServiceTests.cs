using FileManager.Application.Dtos;
using FileManager.Application.Services;
using FileManager.Core.Entities;
using FileManager.Core.Interfaces;
using FluentAssertions;
using Moq;

namespace FileManager.Application.Tests.Services;

public class SearchAppServiceTests
{
    [Fact]
    public async Task SearchAsync_MapsQueryToRepository_AndReturnsDtos()
    {
        var files = new List<FileEntry>
        {
            new()
            {
                FullPath = "C:\\docs\\report.txt",
                Name = "report.txt",
                Extension = ".txt",
                Size = 123,
                LastModified = new DateTime(2024, 1, 1),
                Hash = "HASH1"
            }
        };

        var repositoryMock = new Mock<IFileRepository>();
        repositoryMock
            .Setup(r => r.SearchAsync(
                "report",
                ".txt",
                "docs",
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(files);

        var service = new SearchAppService(repositoryMock.Object);

        var query = new SearchQueryDto
        {
            Name = "report",
            Extension = ".txt",
            Path = "docs"
        };

        var results = await service.SearchAsync(query);

        results.Should().HaveCount(1);
        results[0].FullPath.Should().Be("C:\\docs\\report.txt");
        results[0].Hash.Should().Be("HASH1");

        repositoryMock.Verify(r => r.SearchAsync(
            "report",
            ".txt",
            "docs",
            null,
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_NoMatches_ReturnsEmptyList()
    {
        var repositoryMock = new Mock<IFileRepository>();
        repositoryMock
            .Setup(r => r.SearchAsync(
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FileEntry>());

        var service = new SearchAppService(repositoryMock.Object);

        var results = await service.SearchAsync(new SearchQueryDto { Name = "missing" });

        results.Should().BeEmpty();
    }
}
