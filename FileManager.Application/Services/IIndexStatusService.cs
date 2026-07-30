using FileManager.Application.Dtos;
using FileManager.Application.Dtos;

namespace FileManager.Application.Services;

public interface IIndexStatusService
{
    IndexStatusDto GetStatus();

    bool TryStart(string path);

    void ReportProgress(int filesProcessed);

    void Complete();

    void Fail(string error);
}
