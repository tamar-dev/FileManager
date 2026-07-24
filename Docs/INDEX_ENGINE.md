# FileManager — Index Engine

## Purpose

The Index Engine is internal infrastructure that supports the product experience. Its job is to
maintain a complete, accurate, and continuously updated representation of file metadata so that
the rest of the application — browsing, virtual folders, duplicate detection, search, thumbnail
generation, and future AI enrichment — can work without reading the filesystem on demand.

**The Index Engine is a read-only consumer of the physical filesystem.** It enumerates paths,
reads metadata, and computes hashes. It never creates, modifies, moves, renames, or deletes any
physical file.

---

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

---

## Initial Indexing

The initial build creates a snapshot of filesystem metadata.

`InitialIndexingService` coordinates the pipeline:
1. Requests paths from the injected `IIndexSource`.
2. Groups paths into batches to reduce repository overhead.
3. Reuses existing hashes for unchanged files (same size + last-modified timestamp).
4. Persists new or updated entries via `IFileRepository`.

The service has no direct dependency on the filesystem. All file discovery is delegated to the
`IIndexSource` implementation resolved through DI.

### `IIndexSource`

`IIndexSource` is the abstraction that separates file discovery from the indexing pipeline.
Implementations enumerate file paths under a root directory and support cooperative cancellation.
**All implementations must be read-only — they must not modify, move, rename, or delete any file.**

Current implementation:
- `FileSystemIndexSource` — recursive `Directory.EnumerateFiles` scan.

Future implementations (no pipeline changes required):
- NTFS MFT-based source for faster enumeration on Windows.
- USN Journal-based source for incremental or delta indexing.
- Virtual or test sources for isolated unit testing.

Replacing or extending the source requires only registering a different `IIndexSource`
implementation in the DI container — `InitialIndexingService` and the rest of the pipeline remain
unchanged.

---

## Incremental Updates

After initial indexing, the change monitor keeps the index in sync with filesystem events:

```
Created  ->  Add metadata to index
Changed  ->  Update metadata in index
Deleted  ->  Remove metadata from index
Renamed  ->  Update path in index
```

The index is updated to reflect what happened on the filesystem. The filesystem is never modified
in response to an index update.

---

## Constraints

- **The engine must not modify physical files.** All filesystem interactions are read-only.
- **The engine must not depend on `FileSystemWatcher`.** `FileSystemWatcher` is one possible
  implementation of `ChangeMonitor`, not an architectural dependency.
- **`FileSystemIndexSource` is an implementation detail**, not a business-logic dependency.
  `InitialIndexingService` depends on `IIndexSource` only.
- Background indexing workers (hash calculation, thumbnail generation) are also read-only
  consumers of the filesystem.
