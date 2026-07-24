# Index Engine Design

## Goal
Maintain a complete and continuously updated representation of files.

## Architecture

```
            IndexEngine
                 |
    +------------+-------------+
    |                          |
    v                          v
InitialIndexer            ChangeMonitor
    |                          |
    v                          v
IIndexSource            Change Events
    |
    v
Metadata Index
```

## Initial Indexing

The initial build creates a snapshot of filesystem metadata.

`InitialIndexingService` coordinates the pipeline:
1. Requests paths from the injected `IIndexSource`.
2. Groups paths into batches to reduce repository overhead.
3. Reuses existing hashes for unchanged files (same size + last-modified timestamp).
4. Persists new or updated entries via `IFileRepository`.

The service has no direct dependency on the filesystem. All file discovery is
delegated to the `IIndexSource` implementation resolved through DI.

### `IIndexSource`
`IIndexSource` is the abstraction that separates file discovery from the indexing
pipeline. Implementations enumerate file paths under a root directory and support
cooperative cancellation.

Current implementation:
- `FileSystemIndexSource` — recursive `Directory.EnumerateFiles` scan.

Future implementations (no pipeline changes required):
- NTFS MFT-based source for faster enumeration on Windows.
- USN Journal-based source for incremental or delta indexing.
- Virtual or test sources for isolated unit testing.

Replacing or extending the source requires only registering a different
`IIndexSource` implementation in the DI container — `InitialIndexingService`
and the rest of the pipeline remain unchanged.

## Incremental Updates

After initial indexing:

```
Created -> Add metadata
Changed -> Update metadata
Deleted -> Remove metadata
Renamed -> Update identity/path
```

## Important

The engine must not depend on `FileSystemWatcher`.

`FileSystemWatcher` is only one possible implementation of `ChangeMonitor`.

`FileSystemIndexSource` is only one possible implementation of `IIndexSource`.
