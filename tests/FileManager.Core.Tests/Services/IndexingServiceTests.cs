using FileManager.Core.Entities;
using FileManager.Core.Interfaces;
using FileManager.Core.Services;
using FluentAssertions;

namespace FileManager.Core.Tests.Services;

public class IndexingServiceTests
{
    private sealed class FakeFileEntryFactory : IFileEntryFactory
    {
        private readonly Exception _exception;
        public int CallCount { get; private set; }

        public FakeFileEntryFactory(Exception exception)
        {
            _exception = exception;
        }

        public FileEntry Create(string fullPath) => throw new NotImplementedException();

        public Task<FileEntry> CreateAsync(string fullPath, FileEntry? existingEntry = null)
        {
            CallCount++;
            throw _exception;
        }
        public Task<FileEntry> CreateMetadataAsync(string fullPath, FileEntry? existingEntry = null)
        {
            return Task.FromResult(
                existingEntry ?? new FileEntry
                {
                    FullPath = fullPath,
                    Name = Path.GetFileName(fullPath),
                    Extension = Path.GetExtension(fullPath),
                    IndexedAt = DateTime.UtcNow
                });
        }
    }

    private sealed class FakeRepository : IFileRepository
    {
        public bool UpsertCalled { get; private set; }

        public Task UpsertAsync(FileEntry file)
        {
            UpsertCalled = true;
            return Task.CompletedTask;
        }

        public Task UpsertBatchAsync(IReadOnlyCollection<FileEntry> files, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task DeleteAsync(string fullPath) => Task.CompletedTask;

        public Task<IReadOnlyList<FileEntry>> GetAllAsync() =>
            Task.FromResult<IReadOnlyList<FileEntry>>(new List<FileEntry>());

        public Task<FileEntry?> GetByPathAsync(string fullPath) => Task.FromResult<FileEntry?>(null);

        public Task<FileEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<FileEntry?>(null);

        public Task<IReadOnlyList<FileEntry>> GetByPathsAsync(IReadOnlyCollection<string> fullPaths, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<FileEntry>>(new List<FileEntry>());

        public Task<IReadOnlyList<FileEntry>> GetFilesWithoutHashAsync(int limit, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<FileEntry>>(new List<FileEntry>());
    }

    [Fact]
    public async Task IndexAsync_AllRetriesExhaustedWithIOException_DoesNotThrowAndDoesNotUpsert()
    {
        var tempFile = Path.GetTempFileName();

        try
        {
            var factory = new FakeFileEntryFactory(new IOException("locked"));
            var repository = new FakeRepository();
            var service = new IndexingService(repository, factory);

            await service.IndexAsync(tempFile);

            factory.CallCount.Should().Be(3);
            repository.UpsertCalled.Should().BeFalse();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task IndexAsync_UnauthorizedAccessException_RetriesAndDoesNotThrow()
    {
        var tempFile = Path.GetTempFileName();

        try
        {
            var factory = new FakeFileEntryFactory(new UnauthorizedAccessException("denied"));
            var repository = new FakeRepository();
            var service = new IndexingService(repository, factory);

            await service.IndexAsync(tempFile);

            factory.CallCount.Should().Be(3);
            repository.UpsertCalled.Should().BeFalse();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
