using FileManager.Core.Entities;
using FileManager.Core.Services;
using FluentAssertions;

namespace FileManager.Core.Tests.Services;

public class DuplicateDetectorTests
{
    private readonly DuplicateDetector _detector = new();

    [Fact]
    public void Find_FilesWithIdenticalHashes_AreGroupedTogether()
    {
        var files = new[]
        {
            new FileEntry { FullPath = "a.txt", Hash = "HASH1" },
            new FileEntry { FullPath = "b.txt", Hash = "HASH1" },
            new FileEntry { FullPath = "c.txt", Hash = "HASH2" }
        };

        var groups = _detector.Find(files).ToList();

        groups.Should().HaveCount(1);
        groups[0].Key.Should().Be("HASH1");
        groups[0].Select(f => f.FullPath).Should().BeEquivalentTo("a.txt", "b.txt");
    }

    [Fact]
    public void Find_FilesWithDifferentHashes_AreNotGrouped()
    {
        var files = new[]
        {
            new FileEntry { FullPath = "a.txt", Hash = "HASH1" },
            new FileEntry { FullPath = "b.txt", Hash = "HASH2" }
        };

        var groups = _detector.Find(files).ToList();

        groups.Should().BeEmpty();
    }

    [Fact]
    public void Find_ReturnsExpectedDuplicateGroupCounts()
    {
        var files = new[]
        {
            new FileEntry { FullPath = "a.txt", Hash = "HASH1" },
            new FileEntry { FullPath = "b.txt", Hash = "HASH1" },
            new FileEntry { FullPath = "c.txt", Hash = "HASH1" },
            new FileEntry { FullPath = "d.txt", Hash = "HASH2" },
            new FileEntry { FullPath = "e.txt", Hash = "HASH2" },
            new FileEntry { FullPath = "f.txt", Hash = "HASH3" }
        };

        var groups = _detector.Find(files).ToList();

        groups.Should().HaveCount(2);
        groups.Single(g => g.Key == "HASH1").Should().HaveCount(3);
        groups.Single(g => g.Key == "HASH2").Should().HaveCount(2);
    }

    [Fact]
    public void Find_IgnoresFilesWithEmptyHash()
    {
        var files = new[]
        {
            new FileEntry { FullPath = "a.txt", Hash = "" },
            new FileEntry { FullPath = "b.txt", Hash = "" }
        };

        var groups = _detector.Find(files).ToList();

        groups.Should().BeEmpty();
    }
}
