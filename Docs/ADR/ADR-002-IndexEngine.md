# ADR-002: Index Engine Abstraction

Status: Accepted

## Context

FileManager needs a reliable, continuously updated metadata index to power browsing, virtual
folders, duplicate detection, search, and future AI enrichment. The initial approach would be to
wire the indexing pipeline directly to the filesystem (e.g., `Directory.EnumerateFiles` and
`FileSystemWatcher`). This creates a hard dependency on specific OS APIs and makes it impossible
to improve indexing performance without changing business logic.

Additionally, the Index Engine must be a **read-only consumer of the filesystem**. It must never
modify, move, rename, or delete any physical file.

## Decision

Build the Index Engine around abstractions rather than concrete filesystem APIs:

- `IIndexSource` — abstracts file path enumeration for initial indexing.
- `IFileWatcher` / `ChangeMonitor` — abstracts filesystem change notifications.
- `IFileRepository` — abstracts metadata persistence.

`InitialIndexingService` depends only on `IIndexSource` and `IFileRepository`. It has no direct
dependency on the filesystem.

Current implementations (`FileSystemIndexSource`, `LocalFileWatcher`) are registered through DI
and are replaceable without changing any business logic.

## Consequences

- The indexing pipeline is testable with in-memory or mock sources.
- Faster indexing strategies (NTFS MFT reader, USN Journal reader) can be introduced by registering
  a new `IIndexSource` implementation — no changes to `InitialIndexingService` or any other service.
- `FileSystemWatcher` is an implementation detail, not an architectural dependency.
- All implementations of `IIndexSource` and `IFileWatcher` are contractually read-only: they
  enumerate or observe; they never write to the filesystem.
