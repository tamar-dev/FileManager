# FileManager — Data Flow

> **Constraint:** Every flow described below is a read-only interaction with the physical
> filesystem. No flow modifies, moves, renames, or deletes any physical file.
> Any future flow that affects physical files must be an explicit, user-initiated action.

---

## Startup

```
Start application (CLI / UI composition root)
  -> Wire DI: AddFileManagerInfrastructure() + AddFileManagerApplication()
  -> Run pending migrations
  -> Load existing index
  -> Run initial indexing if needed (via IIndexingAppService)
  -> Start change monitoring
```

---

## Initial Indexing (on demand, e.g. `index <path>`)

```
Caller (CLI command / UI)
  -> IIndexingAppService.IndexDirectoryAsync(path, progress)
  -> IIndexSource.EnumeratePaths(path)       [read-only filesystem scan]
  -> FileEntryFactory.CreateAsync(path)      [reads metadata + computes hash; no writes]
  -> IFileRepository.UpsertBatchAsync(...)   [writes to application database only]
  -> IndexingResultDto returned to caller    (files indexed, success, error)
```

The Application service reports per-file progress via `IProgress<string>` and never leaks
`FileEntry` or EF Core types back to the caller.

---

## File Created (change monitor event)

```
Physical filesystem change
  -> LocalFileWatcher (Infrastructure, IFileWatcher)    [read-only observer]
  -> FileChangeQueue (Core)
  -> FileChangeWorker (Core)
  -> IndexingService.AddAsync(path)
  -> FileEntryFactory.CreateAsync(path)                 [reads file; no writes]
  -> IFileRepository.UpsertAsync(...)                   [writes to application database only]
```

---

## File Modified (change monitor event)

```
Physical filesystem change
  -> LocalFileWatcher
  -> FileChangeQueue
  -> FileChangeWorker
  -> IndexingService.UpdateAsync(path)
  -> FileEntryFactory.CreateAsync(path)    [re-reads metadata; reuses hash if size+mtime unchanged]
  -> IFileRepository.UpsertAsync(...)      [writes to application database only]
```

---

## File Deleted (change monitor event)

```
Physical filesystem change
  -> LocalFileWatcher
  -> FileChangeQueue
  -> FileChangeWorker
  -> IndexingService.RemoveAsync(path)
  -> IFileRepository.DeleteAsync(path)     [removes entry from application database only]
```

The physical file was already deleted by the OS or the user. The application only removes
its own metadata record. The application does not delete any file in response to an index event.

---

## File Renamed (change monitor event)

```
Physical filesystem change
  -> LocalFileWatcher
  -> FileChangeQueue
  -> FileChangeWorker
  -> IndexingService.RenameAsync(oldPath, newPath)
  -> IFileRepository.UpdatePathAsync(...)    [updates path in application database only]
```

---

## Duplicate Report (on demand, e.g. `duplicates`)

```
Caller (CLI command / UI)
  -> IDuplicateAppService.GetDuplicateReportAsync()
  -> IFileRepository.GetAllAsync()
  -> Group entries by Hash
  -> DuplicateGroupDto[] returned to caller
```

The report identifies duplicates. No file is deleted or modified. The user decides on any action.

---

## Virtual Folder Operations (future)

```
Caller (CLI command / UI)
  -> IVirtualFolderAppService.AddFileToFolderAsync(fileId, folderId)
  -> VirtualFolderRepository.AddItemAsync(...)    [writes to application database only]
```

Virtual folder operations read and write the application database only. No physical file
or directory is created, moved, renamed, or deleted.
