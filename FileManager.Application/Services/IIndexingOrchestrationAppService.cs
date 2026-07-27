namespace FileManager.Application.Services;

public interface IIndexingOrchestrationAppService
{
    bool TryStartIndexing(string path);
}
