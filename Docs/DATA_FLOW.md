# Data Flow

## Startup

Start Engine (Cli/UI composition root)
 -> Wire DI: AddFileManagerInfrastructure() + AddFileManagerApplication()
 -> Run pending migrations
 -> Load existing index
 -> Run initial indexing if needed (via IIndexingAppService)
 -> Start change monitoring


## Initial Indexing (on demand, e.g. `index <path>`)

Caller (Cli command / UI)
 -> IIndexingAppService.IndexDirectoryAsync(path, progress)
 -> IFileScanner.Scan(path)          (Infrastructure: FileScanner, Core: FileEntryFactory)
 -> IFileRepository.UpsertAsync      (Infrastructure: FileRepository -> SQLite)
 -> IndexingResultDto returned to caller (files indexed, success, error)

The Application service reports per-file progress via `IProgress<string>` and never leaks
`FileEntry` or EF Core types back to the caller.


## File Created

Filesystem
 -> LocalFileWatcher (Infrastructure, IFileWatcher)
 -> FileChangeQueue (Core)
 -> FileChangeWorker (Core)
 -> IndexingService (Core)
 -> IFileRepository (Infrastructure)
 -> Database


## File Modified

Filesystem
 -> LocalFileWatcher
 -> FileChangeQueue
 -> FileChangeWorker
 -> IndexingService
 -> Update metadata (including hash recalculation today; see Performance milestone)


## File Deleted

Filesystem
 -> LocalFileWatcher
 -> FileChangeQueue
 -> FileChangeWorker
 -> IndexingService.RemoveAsync
 -> Remove from index


## Duplicate Report (on demand, e.g. `duplicates`)

Caller (Cli command / UI)
 -> IDuplicateAppService.GetDuplicateReportAsync()
 -> IFileRepository.GetAllAsync()
 -> DuplicateDetector.Find (Core, groups by Hash)
 -> Mapped to IReadOnlyList<DuplicateGroupDto>


## Dashboard (on demand)

Caller (Cli command / UI)
 -> IDashboardAppService.GetDashboardAsync()
 -> IFileRepository.GetAllAsync()
 -> DuplicateDetector.Find (Core)
 -> Aggregated into DashboardDto (indexed count, total size, duplicate count, potential savings)
