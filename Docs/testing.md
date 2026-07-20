# Testing Guide

This document describes the automated test suite for the FileManager solution: how it's structured, how to run it, and what each layer validates.

## Test Project Structure

```
tests/
 ??? FileManager.Core.Tests            Unit tests for Core business logic (no I/O dependencies beyond temp files)
 ??? FileManager.Application.Tests     Unit tests for Application services (use-case orchestration, DTO mapping)
 ??? FileManager.Infrastructure.Tests  Tests for EF Core / SQLite repository implementation
 ??? FileManager.Integration.Tests     End-to-end tests wiring Core + Infrastructure together
```

All test projects target the same framework as the application (`net9.0`) and use:
- **xUnit** — test framework
- **FluentAssertions** — assertion library
- **Moq** — mocking framework (used in Core tests to isolate dependencies via interfaces)

Architecture boundaries are preserved:
- `FileManager.Core.Tests` only depends on `FileManager.Core`. It never references EF Core or SQLite — dependencies like `IFileRepository` and `IFileScanner` are mocked via Moq.
- `FileManager.Application.Tests` only depends on `FileManager.Core` and `FileManager.Application`. It never references EF Core, SQLite, or `FileManager.Infrastructure` — `IFileRepository`/`IFileScanner` are mocked via Moq, and assertions are made against DTOs (`IndexingResultDto`, `DuplicateGroupDto`, `DashboardDto`), never domain entities.
- `FileManager.Infrastructure.Tests` exercises the real `FileRepository` against a real SQLite database (in-memory, via a shared `SqliteConnection`), verifying persistence behavior without a mock.
- `FileManager.Integration.Tests` wires real components together (`FileScanner`, `InitialIndexingService`, `LocalFileWatcher`, `FileChangeWorker`, `FileRepository`) against a real file-backed SQLite database in a temporary folder, validating true end-to-end behavior.

No test-only logic was added to production code. The one production fix made alongside this work (`FileManagerDbContext.OnConfiguring` now respects externally supplied `DbContextOptions`) is a correctness fix, not test scaffolding — it was required because the context previously ignored any connection passed to it.

## Running the Tests

Run all tests in the solution from the repository root:

```powershell
dotnet test FileManager.Engine.sln
```

Run a single test project:

```powershell
dotnet test tests/FileManager.Core.Tests
dotnet test tests/FileManager.Application.Tests
dotnet test tests/FileManager.Infrastructure.Tests
dotnet test tests/FileManager.Integration.Tests
```

Run a filtered subset of tests:

```powershell
dotnet test --filter "FullyQualifiedName~DuplicateDetector"
```

## What Each Layer Validates

### `FileManager.Core.Tests`

- **`FileHasherTests`** — identical content produces identical SHA-256 hashes, different content produces different hashes, empty files hash consistently.
- **`FileEntryFactoryTests`** — `FileEntry` creation from a path populates `FullPath`, `Size`, and a valid SHA-256 hash for existing files; missing files are handled without throwing and produce an empty hash.
- **`DuplicateDetectorTests`** — files with identical hashes are grouped, files with differing hashes are not grouped, groups have the expected membership/counts, files with empty hashes are excluded.
- **`InitialIndexingServiceTests`** — using mocked `IFileScanner`/`IFileRepository`, verifies the service scans the given path and persists every discovered file exactly once, with no repository calls when no files are found.

### `FileManager.Application.Tests`

- **`IndexingAppServiceTests`** — using mocked `IFileScanner`/`IFileRepository`, verifies `IndexDirectoryAsync` persists all scanned files, returns an accurate `IndexingResultDto` (files indexed, success flag), reports per-file progress via `IProgress<string>`, and returns a failure result (not an exception) when cancelled.
- **`DuplicateAppServiceTests`** — using a mocked `IFileRepository`, verifies duplicate groups are correctly mapped to `DuplicateGroupDto` (hash, file count, total size, wasted size, file list), and that no duplicates yields an empty list.
- **`DashboardAppServiceTests`** — using a mocked `IFileRepository`, verifies `DashboardDto` aggregates indexed file count, total size, duplicate group/file counts, and potential storage savings correctly, including the zero-files case.

### `FileManager.Infrastructure.Tests`

- **`FileRepositoryTests`** — using a real SQLite database (in-memory), verifies:
  - Adding a new file persists it.
  - Upserting an existing file updates its fields (hash, size, etc.) rather than duplicating it.
  - Deleting a file removes it; deleting a non-existent file does not throw.
  - `GetAllAsync` returns all persisted files.
  - Hash values round-trip correctly through the database.

### `FileManager.Integration.Tests`

- **`InitialIndexingFlowTests`** — creates a temporary folder with real files, runs `FileScanner` ? `InitialIndexingService` ? SQLite, and verifies all files are discovered, persisted, and have non-empty hash values.
- **`WatcherFlowTests`** — creates a temporary folder, starts `LocalFileWatcher` + `FileChangeQueue` + `FileChangeWorker` + `IndexingService` against a real SQLite database, and verifies:
  - Creating a file adds an entry to the index.
  - Modifying a file updates its hash and size.
  - Deleting a file removes its entry from the index.

  These tests poll the database with a timeout (rather than sleeping a fixed duration) to avoid flakiness while still failing fast if the watcher pipeline breaks.

## Notes on Test Isolation

- Each test class creates its own temporary directory (via `Directory.CreateTempSubdirectory`) and/or its own SQLite database file, and cleans up in `Dispose()`.
- Integration tests keep the *watched/scanned* folder separate from the *database* folder, since `FileSystemWatcher` and `FileScanner` would otherwise pick up the SQLite database's own files (`.db`, `.db-wal`) as part of the monitored directory.
- `WatcherFlowTests` uses a dedicated `DbContext`/connection per read to avoid concurrent access exceptions, since the background `FileChangeWorker` uses its own long-lived `DbContext` on a separate thread.
