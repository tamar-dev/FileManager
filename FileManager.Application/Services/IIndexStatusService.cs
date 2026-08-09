using FileManager.Application.Dtos;

namespace FileManager.Application.Services;

public interface IIndexStatusService
{
    IndexStatusDto GetStatus();

    bool TryStart(string path);

    void ReportProgress(int filesProcessed, string? currentFilePath);

    void Complete();

    void Fail(string error);
}
