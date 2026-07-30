using FileManager.Application.Services;
using FileManager.Application.Dtos;
using FileManager.Core.Entities;
using FileManager.Core.Interfaces;
using FluentAssertions;
using Moq;

namespace FileManager.Application.Tests.Services;

public class SearchAppServiceTests
{
    private static List<FileEntry> BuildFiles()
    {
        return new List<FileEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "report.pdf",
                FullPath = "/Documents/Finance/report.pdf",
                Extension = ".pdf",
                Size = 1000,
                LastModified = new DateTime(2025, 1, 10),
                Hash = "H1"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "notes.docx",
                FullPath = "/Documents/Meetings/notes.docx",
                Extension = ".docx",
                Size = 2000,
                LastModified = new DateTime(2025, 2, 15),
                Hash = "H2"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "photo.jpg",
                FullPath = "/Photos/2025/photo.jpg",
                Extension = ".jpg",
                Size = 3000,
                LastModified = new DateTime(2025, 3, 20),
                Hash = "H3"
            }
        };
    }

    private static SearchAppService CreateService(IReadOnlyList<FileEntry> files)
    {
        var repositoryMock = new Mock<IFileRepository>();
        repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(files);
        return new SearchAppService(repositoryMock.Object);
    }

    [Fact]
    public async Task SearchAsync_EmptyQuery_ReturnsAllFiles()
    {
        var service = CreateService(BuildFiles());

        var results = await service.SearchAsync(new SearchQueryDto());

        results.Should().HaveCount(3);
    }

    [Fact]
    public async Task SearchAsync_FiltersByName()
    {
        var service = CreateService(BuildFiles());

        var results = await service.SearchAsync(new SearchQueryDto { Name = "notes" });

        results.Should().ContainSingle();
        results[0].Name.Should().Be("notes.docx");
    }

    [Fact]
    public async Task SearchAsync_FiltersByExtension()
    {
        var service = CreateService(BuildFiles());

        var results = await service.SearchAsync(new SearchQueryDto { Extension = ".jpg" });

        results.Should().ContainSingle();
        results[0].Name.Should().Be("photo.jpg");
    }

    [Fact]
    public async Task SearchAsync_FiltersByPath()
    {
        var service = CreateService(BuildFiles());

        var results = await service.SearchAsync(new SearchQueryDto { Path = "Finance" });

        results.Should().ContainSingle();
        results[0].Name.Should().Be("report.pdf");
    }

    [Fact]
    public async Task SearchAsync_FiltersByModifiedAfter()
    {
        var service = CreateService(BuildFiles());

        var results = await service.SearchAsync(new SearchQueryDto { ModifiedAfter = new DateTime(2025, 2, 1) });

        results.Should().HaveCount(2);
        results.Should().Contain(f => f.Name == "notes.docx");
        results.Should().Contain(f => f.Name == "photo.jpg");
    }

    [Fact]
    public async Task SearchAsync_FiltersByModifiedBefore()
    {
        var service = CreateService(BuildFiles());

        var results = await service.SearchAsync(new SearchQueryDto { ModifiedBefore = new DateTime(2025, 2, 1) });

        results.Should().ContainSingle();
        results[0].Name.Should().Be("report.pdf");
    }

    [Fact]
    public async Task SearchAsync_MapsToSearchResultDto_Correctly()
    {
        var service = CreateService(BuildFiles());

        var results = await service.SearchAsync(new SearchQueryDto { Name = "report" });

        var result = results.Single();
        result.Name.Should().Be("report.pdf");
        result.Path.Should().Be("/Documents/Finance/report.pdf");
        result.Size.Should().Be(1000);
        result.Extension.Should().Be(".pdf");
        result.Modified.Should().Be(new DateTime(2025, 1, 10));
        result.Hash.Should().Be("H1");
        result.Type.Should().Be("pdf");
    }

    [Theory]
    [InlineData(".pdf", "pdf")]
    [InlineData(".docx", "document")]
    [InlineData(".txt", "document")]
    [InlineData(".jpg", "image")]
    [InlineData(".png", "image")]
    [InlineData(".mp4", "video")]
    [InlineData(".mp3", "audio")]
    [InlineData(".zip", "archive")]
    [InlineData(".xlsx", "spreadsheet")]
    [InlineData(".xyz", "other")]
    public async Task SearchAsync_ClassifiesType_ForRepresentativeExtensions(string extension, string expectedType)
    {
        var files = new List<FileEntry>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = $"file{extension}",
                FullPath = $"/file{extension}",
                Extension = extension,
                Size = 10,
                LastModified = DateTime.UtcNow,
                Hash = "H"
            }
        };

        var service = CreateService(files);

        var results = await service.SearchAsync(new SearchQueryDto());

        results.Single().Type.Should().Be(expectedType);
    }
}
