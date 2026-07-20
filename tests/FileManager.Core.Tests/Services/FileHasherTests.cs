using FileManager.Core.Services;
using FluentAssertions;

namespace FileManager.Core.Tests.Services;

public class FileHasherTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly FileHasher _hasher = new();

    public FileHasherTests()
    {
        _tempDirectory = Directory.CreateTempSubdirectory("FileHasherTests").FullName;
    }

    [Fact]
    public void Calculate_SameContent_ProducesSameHash()
    {
        var path1 = CreateFile("same content");
        var path2 = CreateFile("same content");

        var hash1 = _hasher.Calculate(path1);
        var hash2 = _hasher.Calculate(path2);

        hash1.Should().Be(hash2);
    }

    [Fact]
    public void Calculate_DifferentContent_ProducesDifferentHash()
    {
        var path1 = CreateFile("content A");
        var path2 = CreateFile("content B");

        var hash1 = _hasher.Calculate(path1);
        var hash2 = _hasher.Calculate(path2);

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void Calculate_EmptyFile_ReturnsConsistentHash()
    {
        var path1 = CreateFile(string.Empty);
        var path2 = CreateFile(string.Empty);

        var hash1 = _hasher.Calculate(path1);
        var hash2 = _hasher.Calculate(path2);

        hash1.Should().NotBeNullOrEmpty();
        hash1.Should().Be(hash2);
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
