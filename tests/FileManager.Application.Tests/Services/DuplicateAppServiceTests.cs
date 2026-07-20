using FileManager.Application.Services;
using FileManager.Core.Entities;
using FileManager.Core.Interfaces;
using FluentAssertions;
using Moq;

namespace FileManager.Application.Tests.Services;

public class DuplicateAppServiceTests
{
    [Fact]
    public async Task GetDuplicateReportAsync_ReturnsGroupedDuplicates_AsDtos()
    {
        var files = new List<FileEntry>
        {
            new() { FullPath = "a.txt", Hash = "HASH1", Size = 100 },
            new() { FullPath = "b.txt", Hash = "HASH1", Size = 100 },
            new() { FullPath = "c.txt", Hash = "HASH2", Size = 50 }
        };

        var repositoryMock = new Mock<IFileRepository>();
        repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(files);

        var service = new DuplicateAppService(repositoryMock.Object);

        var groups = await service.GetDuplicateReportAsync();

        groups.Should().HaveCount(1);
        groups[0].Hash.Should().Be("HASH1");
        groups[0].FileCount.Should().Be(2);
        groups[0].TotalSize.Should().Be(200);
        groups[0].WastedSize.Should().Be(100);
        groups[0].Files.Should().Contain(f => f.FullPath == "a.txt");
        groups[0].Files.Should().Contain(f => f.FullPath == "b.txt");
    }

    [Fact]
    public async Task GetDuplicateReportAsync_NoDuplicates_ReturnsEmptyList()
    {
        var files = new List<FileEntry>
        {
            new() { FullPath = "a.txt", Hash = "HASH1" },
            new() { FullPath = "b.txt", Hash = "HASH2" }
        };

        var repositoryMock = new Mock<IFileRepository>();
        repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(files);

        var service = new DuplicateAppService(repositoryMock.Object);

        var groups = await service.GetDuplicateReportAsync();

        groups.Should().BeEmpty();
    }
}
