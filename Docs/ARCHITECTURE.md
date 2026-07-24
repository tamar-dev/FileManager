# FileManager — Architecture

## Product Context

FileManager is a local desktop file management application. The architecture exists to support a
product that helps users organize and rediscover their files without changing the physical folder
structure. The indexing engine, virtual organization layer, and future AI enrichment pipeline are
all internal capabilities that serve that user experience.

**The architecture must always enforce the separation between the physical filesystem (which the
application reads but never modifies) and the application's own data (metadata, virtual
organization, AI-generated metadata).**

See [C4 diagrams](c4/C1-CONTEXT.md) for visual representations.

---

## Non-Negotiable Architectural Rules

These rules are not implementation preferences. They constrain every layer of the system and must
never be violated by any new feature, service, or background process.

- **Never delete, move, rename, or overwrite physical user files automatically.**
  Every write operation on the physical filesystem must be an explicit, user-initiated action with
  confirmation. Indexing, change monitoring, duplicate detection, thumbnail generation, and AI
  enrichment are all read-only filesystem operations.

- **The user's filesystem is the source of truth for physical files.**
  The application index reflects the filesystem. The filesystem never reflects the index.

- **The application owns only metadata and virtual organization.**
  Virtual folders, collections, albums, tags, and AI-generated metadata live in the application
  database. They do not create or alter anything on disk.

- **Background services may read files; they must never write to them.**

- **AI enrichment is an optional, additive, read-only metadata layer.**
  AI analysis reads file content and writes only to the application's own metadata store.
  It must not trigger any file operation.

---

## Overview

See C4 diagrams:
- [C1 - System Context](c4/C1-CONTEXT.md)
- [C2 - Container Diagram](c4/C2-CONTAINER.md)

## Purpose
Build a modular desktop file management application over the native filesystem, providing a virtual
organization layer that lets users organize files logically without altering their physical location.

## Layering
- `FileManager.Core`
  - Entities
  - Interfaces
  - Business rules
  - Module contracts
- `FileManager.Infrastructure`
  - SQLite metadata persistence
  - Filesystem adapters (read-only access to physical files)
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
- Never modify, move, rename, or delete any file.

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

| Module | Responsibility |
|---|---|
| Index Engine | Reliable metadata synchronization with the filesystem |
| Metadata Store | Persistent storage for file properties and hashes |
| Virtual Folder Engine | Logical organization independent of physical structure |
| Tag Engine | User-applied and AI-generated labels |
| Duplicate Detection | Identifying duplicate files; user decides on action |
| Collection Engine | Named groupings (albums, smart collections) |
| Query Engine | Powering search and browsing across metadata |
| Thumbnail Cache | Background preview generation (read-only on source files) |
| AI Enrichment (future) | Optional metadata layer: categorization, OCR, recognition |

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
- Infrastructure adapters that touch the filesystem are **read-only**; any future write operation
  on physical files must be gated behind an explicit user action at the Application layer.
