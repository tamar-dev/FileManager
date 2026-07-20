using FileManager.Core.Services;
using FluentAssertions;

namespace FileManager.Core.Tests.Services;

public class FileEntryFactoryTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly FileEntryFactory _factory = new();

    public FileEntryFactoryTests()
    {
        _tempDirectory = Directory.CreateTempSubdirectory("FileEntryFactoryTests").FullName;
    }

    [Fact]
    public void Create_ExistingFile_PopulatesFullPath()
    {
        var path = CreateFile("hello world");

        var entry = _factory.Create(path);

        entry.FullPath.Should().Be(path);
    }

    [Fact]
    public void Create_ExistingFile_PopulatesSize()
    {
        var content = "hello world";
        var path = CreateFile(content);

        var entry = _factory.Create(path);

        entry.Size.Should().Be(content.Length);
    }

    [Fact]
    public void Create_ExistingFile_GeneratesSha256Hash()
    {
        var path = CreateFile("hello world");

        var entry = _factory.Create(path);

        entry.Hash.Should().NotBeNullOrEmpty();
        entry.Hash.Should().MatchRegex("^[0-9A-F]{64}$");
    }

    [Fact]
    public void Create_MissingFile_DoesNotThrowAndReturnsEmptyHash()
    {
        var path = Path.Combine(_tempDirectory, "does-not-exist.txt");

        var entry = _factory.Create(path);

        entry.Hash.Should().BeEmpty();
        entry.FullPath.Should().Be(path);
    }

    private string CreateFile(string content)
    {
        var path = Path.Combine(_tempDirectory, Guid.NewGuid().ToString("N") + ".txt");
        File.WriteAllText(path, content);
        return path;
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }
}
