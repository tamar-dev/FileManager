using FileManager.Application.Services;
using FileManager.Core.Entities;
using FileManager.Core.Interfaces;
using FluentAssertions;
using Moq;

namespace FileManager.Application.Tests.Services;

public class DashboardAppServiceTests
{
    [Fact]
    public async Task GetDashboardAsync_ComputesCountsSizesAndSavings()
    {
        var files = new List<FileEntry>
        {
            new() { FullPath = "a.txt", Hash = "HASH1", Size = 100 },
            new() { FullPath = "b.txt", Hash = "HASH1", Size = 100 },
            new() { FullPath = "c.txt", Hash = "HASH2", Size = 50 }
        };

        var repositoryMock = new Mock<IFileRepository>();
        repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(files);

        var service = new DashboardAppService(repositoryMock.Object);

        var dashboard = await service.GetDashboardAsync();

        dashboard.IndexedFileCount.Should().Be(3);
        dashboard.TotalIndexedSize.Should().Be(250);
        dashboard.DuplicateGroupCount.Should().Be(1);
        dashboard.DuplicateFileCount.Should().Be(2);
        dashboard.PotentialStorageSavings.Should().Be(100);
    }

    [Fact]
    public async Task GetDashboardAsync_NoFiles_ReturnsZeroedDashboard()
    {
        var repositoryMock = new Mock<IFileRepository>();
        repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<FileEntry>());

        var service = new DashboardAppService(repositoryMock.Object);

        var dashboard = await service.GetDashboardAsync();

        dashboard.IndexedFileCount.Should().Be(0);
        dashboard.TotalIndexedSize.Should().Be(0);
        dashboard.DuplicateGroupCount.Should().Be(0);
        dashboard.DuplicateFileCount.Should().Be(0);
        dashboard.PotentialStorageSavings.Should().Be(0);
    }
}
