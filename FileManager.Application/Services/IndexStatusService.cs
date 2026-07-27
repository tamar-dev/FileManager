using FileManager.Application.Dtos;

namespace FileManager.Application.Services;

public class IndexStatusService : IIndexStatusService
{
    private readonly object _lock = new();
    private IndexStatusDto _status = new() { State = IndexingState.Idle };

    public IndexStatusDto GetStatus()
    {
        lock (_lock)
        {
            return new IndexStatusDto
            {
                State = _status.State,
                Path = _status.Path,
                FilesProcessed = _status.FilesProcessed,
                TotalFiles = _status.TotalFiles,
                StartedAt = _status.StartedAt,
                CompletedAt = _status.CompletedAt,
                Error = _status.Error
            };
        }
    }

    public bool TryStart(string path)
    {
        lock (_lock)
        {
            if (_status.State == IndexingState.Running)
            {
                return false;
            }

            _status = new IndexStatusDto
            {
                State = IndexingState.Running,
                Path = path,
                FilesProcessed = 0,
                TotalFiles = null,
                StartedAt = DateTime.UtcNow
            };

            return true;
        }
    }

    public void ReportProgress(int filesProcessed)
    {
        lock (_lock)
        {
            if (_status.State != IndexingState.Running)
            {
                return;
            }

            _status.FilesProcessed = filesProcessed;
        }
    }

    public void Complete()
    {
        lock (_lock)
        {
            _status.State = IndexingState.Completed;
            _status.CompletedAt = DateTime.UtcNow;
        }
    }

    public void Fail(string error)
    {
        lock (_lock)
        {
            _status.State = IndexingState.Failed;
            _status.Error = error;
            _status.CompletedAt = DateTime.UtcNow;
        }
    }
}
