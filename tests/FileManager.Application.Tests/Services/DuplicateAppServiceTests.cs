using FileManager.Application.Services;
using FileManager.Application.Dtos;
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
            new() { FullPath = "a.txt", Hash = "HASH1", Size = 100, Extension = ".txt" },
            new() { FullPath = "b.txt", Hash = "HASH1", Size = 100, Extension = ".txt" },
            new() { FullPath = "c.txt", Hash = "HASH2", Size = 50, Extension = ".txt" }
        };

        var repositoryMock = new Mock<IFileRepository>();
        repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(files);

        var service = new DuplicateAppService(repositoryMock.Object);

        var groups = await service.GetDuplicateReportAsync();

        groups.Should().HaveCount(1);
        groups[0].Id.Should().Be("HASH1");
        groups[0].Hash.Should().Be("HASH1");
        groups[0].FileCount.Should().Be(2);
        groups[0].TotalSize.Should().Be(200);
        groups[0].WastedSize.Should().Be(100);
        groups[0].Type.Should().Be("document");
        groups[0].Files.Should().Contain(f => f.FullPath == "a.txt");
        groups[0].Files.Should().Contain(f => f.FullPath == "b.txt");
        groups[0].Files.Should().OnlyContain(f => f.Id != "");
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

    [Fact]
    public async Task GetDuplicateReportAsync_MultipleGroups_ReturnsCorrectGroupsAndSavings()
    {
        var files = new List<FileEntry>
        {
            new() { FullPath = "a.jpg", Hash = "HASH1", Size = 100, Extension = ".jpg" },
            new() { FullPath = "b.jpg", Hash = "HASH1", Size = 100, Extension = ".jpg" },
            new() { FullPath = "c.mp4", Hash = "HASH2", Size = 300, Extension = ".mp4" },
            new() { FullPath = "d.mp4", Hash = "HASH2", Size = 300, Extension = ".mp4" },
            new() { FullPath = "e.mp4", Hash = "HASH2", Size = 300, Extension = ".mp4" },
            new() { FullPath = "f.pdf", Hash = "HASH3", Size = 50, Extension = ".pdf" }
        };

        var repositoryMock = new Mock<IFileRepository>();
        repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(files);

        var service = new DuplicateAppService(repositoryMock.Object);

        var groups = await service.GetDuplicateReportAsync();

        groups.Should().HaveCount(2);

        var hash1Group = groups.Single(g => g.Hash == "HASH1");
        hash1Group.FileCount.Should().Be(2);
        hash1Group.TotalSize.Should().Be(200);
        hash1Group.WastedSize.Should().Be(100);
        hash1Group.Type.Should().Be("image");

        var hash2Group = groups.Single(g => g.Hash == "HASH2");
        hash2Group.FileCount.Should().Be(3);
        hash2Group.TotalSize.Should().Be(900);
        hash2Group.WastedSize.Should().Be(600);
        hash2Group.Type.Should().Be("video");
    }

    [Fact]
    public async Task GetDuplicateReportAsync_MapsFileDetails_ForEachFileInGroup()
    {
        var modified = new DateTime(2025, 7, 15);
        var files = new List<FileEntry>
        {
            new() { FullPath = "/docs/report.pdf", Name = "report.pdf", Hash = "HASH1", Size = 1024, Extension = ".pdf", LastModified = modified },
            new() { FullPath = "/desktop/report_copy.pdf", Name = "report_copy.pdf", Hash = "HASH1", Size = 1024, Extension = ".pdf", LastModified = modified }
        };

        var repositoryMock = new Mock<IFileRepository>();
        repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(files);

        var service = new DuplicateAppService(repositoryMock.Object);

        var groups = await service.GetDuplicateReportAsync();

        var file = groups[0].Files.Single(f => f.Name == "report.pdf");
        file.Id.Should().Be("/docs/report.pdf");
        file.FullPath.Should().Be("/docs/report.pdf");
        file.Size.Should().Be(1024);
        file.Extension.Should().Be(".pdf");
        file.LastModified.Should().Be(modified);
        file.Type.Should().Be("pdf");
    }
}
