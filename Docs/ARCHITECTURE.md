# Architecture

## Overview

See C4 diagrams:
- [C1 - System Context](c4/C1-CONTEXT.md)
- [C2 - Container Diagram](c4/C2-CONTAINER.md)

## Purpose
Build a modular desktop file management engine over the native filesystem.

## Layering
- `FileManager.Core`
  - Entities
  - Interfaces
  - Business rules
  - Module contracts
- `FileManager.Infrastructure`
  - SQLite metadata persistence
  - Filesystem adapters
  - OS integrations (e.g., change notifications)
  - `AddFileManagerInfrastructure()` DI registration (DbContext, `IFileRepository`, `IFileScanner`, `IIndexSource`, `FileEntryFactory`, `InitialIndexingService`)
- `FileManager.Application`
  - Use-case orchestration only (no EF Core, no UI concerns)
  - Exposes app services (`IIndexingAppService`, `IDuplicateAppService`, `IDashboardAppService`)
	that wrap Core services and translate domain entities into DTOs
  - Defines the DTO contract (`FileResultDto`, `DuplicateGroupDto`, `DashboardDto`, `IndexingResultDto`)
	consumed by any front end
  - `AddFileManagerApplication()` DI registration
  - Depends only on `FileManager.Core`
- `FileManager.Cli`
  - Composition root and startup only
  - No business logic
  - Wires `AddFileManagerInfrastructure()` + `AddFileManagerApplication()`, resolves commands via DI
  - Commands consume Application services (`IIndexingAppService`, `IDuplicateAppService`) instead of
	constructing Core/Infrastructure types directly
- Future `FileManager.UI` (or similar desktop UI project)
  - Will reference only `FileManager.Application` (+ the same DI composition helpers)
  - Never references `FileManager.Infrastructure`, EF Core types, or `DbContext` directly

## Application Layer Responsibility
The Application layer is the **single clean boundary** between any front end (CLI today, desktop UI later)
and the business/persistence layers. Rules:
- May depend on `FileManager.Core` only — never on `FileManager.Infrastructure` or EF Core packages.
- Never returns Core entities (e.g. `FileEntry`) to callers — always maps to DTOs.
- Contains orchestration/use-case logic only (calling Core services, aggregating results); it does not
  contain filesystem access, persistence logic, or UI rendering concerns.
- Any new user-facing feature (search, virtual folders, duplicate management, photo organization,
  thumbnail cache) is exposed as a new Application service + DTOs, backed by new Core interfaces and
  Infrastructure implementations — without requiring changes to how the CLI or UI is wired.

## Index Source Abstraction

### `IIndexSource` Responsibilities
`IIndexSource` (in `FileManager.Core/Interfaces`) is the sole contract for supplying file paths to the
indexing pipeline. It exposes a single method:

```csharp
IEnumerable<string> EnumeratePaths(string rootPath, CancellationToken cancellationToken = default);
```

Responsibilities:
- Enumerate every file path that should be considered for indexing under `rootPath`.
- Support cooperative cancellation via `CancellationToken`.
- Return only paths — no metadata, no hashing, no persistence.

### `FileSystemIndexSource` — Current Implementation
`FileSystemIndexSource` (in `FileManager.Core/Services`) is the default implementation registered in DI.
It performs a recursive `Directory.EnumerateFiles` scan and yields paths one at a time, checking for
cancellation between each path.

It is registered in `AddFileManagerInfrastructure()` as:
```csharp
services.AddScoped<IIndexSource, FileSystemIndexSource>();
```

### Why `InitialIndexingService` No Longer Depends on the Filesystem
`InitialIndexingService` receives its `IIndexSource` through constructor injection. It never references
`FileSystemIndexSource`, `Directory`, or any other filesystem API directly. The service only knows
about the `IIndexSource` contract.

This means the entire indexing pipeline — batching, hash reuse, repository persistence — is driven
purely by whatever paths the injected source provides. Swapping the source does not require any change
to the service or the pipeline.

### Introducing a Future Index Source
To add an alternative source (for example, an NTFS MFT-based reader for faster initial indexing on
Windows):

1. Create a new class in `FileManager.Infrastructure` (or a new project) that implements `IIndexSource`.
2. Register it in `AddFileManagerInfrastructure()` instead of (or conditionally alongside)
   `FileSystemIndexSource`:
   ```csharp
   services.AddScoped<IIndexSource, MftIndexSource>();
   ```
3. No changes are required in `InitialIndexingService`, `FileEntryFactory`, `IFileRepository`, or
   any Application/CLI layer.

## Module Boundaries (Target)
- Index Engine
- Metadata Store
- Virtual Folder Engine
- Tag Engine
- Duplicate Detection
- Collection Engine
- Query Engine
- Thumbnail Cache

Each module has a single responsibility and stable contracts in `Core`.

## Rules
- Keep `Core` independent from `Infrastructure`.
- Keep `Core` and `Infrastructure` independent from any UI/CLI.
- Keep indexing implementation replaceable via `IIndexSource`.
- Separate:
  - Initial indexing
  - Change monitoring
  - Metadata persistence
- `FileSystemWatcher` is an implementation detail, not an architectural dependency.
- `FileSystemIndexSource` is an implementation detail, not a business-logic dependency.
- All front ends consume business logic exclusively through `FileManager.Application` services.
