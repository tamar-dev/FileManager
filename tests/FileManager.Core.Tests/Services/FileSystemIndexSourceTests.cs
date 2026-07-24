using FileManager.Core.Services;
using FluentAssertions;

namespace FileManager.Core.Tests.Services;

public class FileSystemIndexSourceTests : IDisposable
{
    private readonly string _tempDirectory;

    public FileSystemIndexSourceTests()
    {
        _tempDirectory = Directory.CreateTempSubdirectory("FileSystemIndexSourceTests").FullName;
    }

    [Fact]
    public void EnumeratePaths_ReturnsAllFilesRecursively()
    {
        var rootFile = CreateFile("root.txt");
        var nestedDir = Path.Combine(_tempDirectory, "nested");
        Directory.CreateDirectory(nestedDir);
        var nestedFile = Path.Combine(nestedDir, "nested.txt");
        File.WriteAllText(nestedFile, "nested content");

        var source = new FileSystemIndexSource();

        var paths = source.EnumeratePaths(_tempDirectory).ToList();

        paths.Should().BeEquivalentTo([rootFile, nestedFile]);
    }

    [Fact]
    public void EnumeratePaths_EmptyDirectory_ReturnsNoPaths()
    {
        var source = new FileSystemIndexSource();

        var paths = source.EnumeratePaths(_tempDirectory).ToList();

        paths.Should().BeEmpty();
    }

    [Fact]
    public void EnumeratePaths_CancellationRequested_ThrowsOperationCanceledException()
    {
        CreateFile("a.txt");
        CreateFile("b.txt");

        var source = new FileSystemIndexSource();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = () => source.EnumeratePaths(_tempDirectory, cts.Token).ToList();

        act.Should().Throw<OperationCanceledException>();
    }

    private string CreateFile(string name)
    {
        var path = Path.Combine(_tempDirectory, name);
        File.WriteAllText(path, "content");
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
