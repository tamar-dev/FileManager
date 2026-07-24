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

    [Fact]
    public void EnumeratePaths_SubdirectoryDeletedDuringScan_DoesNotThrow()
    {
        // Arrange: create a sibling subdirectory that exists at enumeration time,
        // then delete it before the recursive step reaches it.
        // This simulates an IOException / DirectoryNotFoundException that the safe
        // enumerator must swallow without aborting the whole scan.
        var accessibleDir = Path.Combine(_tempDirectory, "accessible");
        Directory.CreateDirectory(accessibleDir);
        var accessibleFile = Path.Combine(accessibleDir, "file.txt");
        File.WriteAllText(accessibleFile, "content");

        var vanishingDir = Path.Combine(_tempDirectory, "vanishing");
        Directory.CreateDirectory(vanishingDir);
        File.WriteAllText(Path.Combine(vanishingDir, "ghost.txt"), "content");

        // Delete the directory so the enumerator encounters a missing path mid-scan.
        Directory.Delete(vanishingDir, recursive: true);

        var source = new FileSystemIndexSource();

        var act = () => source.EnumeratePaths(_tempDirectory).ToList();

        // Must not throw even though one subdirectory disappeared between discovery
        // and enumeration.
        act.Should().NotThrow();
    }

    [Fact]
    public void EnumeratePaths_SubdirectoryDeletedDuringScan_StillReturnsAccessibleFiles()
    {
        var accessibleDir = Path.Combine(_tempDirectory, "accessible");
        Directory.CreateDirectory(accessibleDir);
        var accessibleFile = Path.Combine(accessibleDir, "file.txt");
        File.WriteAllText(accessibleFile, "content");

        var vanishingDir = Path.Combine(_tempDirectory, "vanishing");
        Directory.CreateDirectory(vanishingDir);
        Directory.Delete(vanishingDir, recursive: true);

        var source = new FileSystemIndexSource();

        var paths = source.EnumeratePaths(_tempDirectory).ToList();

        paths.Should().Contain(accessibleFile);
    }

    [Fact]
    public void EnumeratePaths_MultipleSubdirectories_InaccessibleOneDoesNotPreventOthers()
    {
        // dir-a: accessible, has a file
        var dirA = Path.Combine(_tempDirectory, "dir-a");
        Directory.CreateDirectory(dirA);
        var fileA = Path.Combine(dirA, "a.txt");
        File.WriteAllText(fileA, "content-a");

        // dir-b: will vanish before enumeration reaches it
        var dirB = Path.Combine(_tempDirectory, "dir-b");
        Directory.CreateDirectory(dirB);
        Directory.Delete(dirB, recursive: true);

        // dir-c: accessible, has a file
        var dirC = Path.Combine(_tempDirectory, "dir-c");
        Directory.CreateDirectory(dirC);
        var fileC = Path.Combine(dirC, "c.txt");
        File.WriteAllText(fileC, "content-c");

        var source = new FileSystemIndexSource();

        var paths = source.EnumeratePaths(_tempDirectory).ToList();

        paths.Should().Contain(fileA);
        paths.Should().Contain(fileC);
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
