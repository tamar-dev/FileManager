# FileManager — Architecture

## Product Context

FileManager is a local-first file management application.

The architecture exists to support a product that helps users organize and rediscover their files
without requiring them to change the physical folder structure on disk.

The indexing engine, virtual organization layer, search capabilities, duplicate detection,
and future AI enrichment pipeline are internal capabilities that serve that user experience.

**The architecture must always enforce the separation between the physical filesystem and the
application's own data.**

The filesystem is the source of truth for physical files.

The application owns only:

- indexed metadata
- virtual organization
- application state
- derived metadata

See [C4 diagrams](c4/C1-CONTEXT.md) for visual representations.

---

## Non-Negotiable Architectural Rules

These rules are not implementation preferences.

They constrain every layer of the system and must never be violated by any feature,
service, or background process.

- **Never delete, move, rename, overwrite, or otherwise modify physical user files automatically.**

  Indexing, monitoring, duplicate detection, thumbnail generation, and future AI enrichment
  must remain read-only with respect to source files.

- **The user's filesystem is the source of truth for physical files.**

  The application index reflects the filesystem.
  The filesystem does not reflect the application index.

- **The application owns metadata and virtual organization.**

  Virtual folders, collections, tags, and other logical organization belong to the
  application database.

- **Background services may read physical files but must not modify them.**

- **Any future physical file operation must be explicit and user initiated.**

- **AI enrichment, when introduced, is additive metadata only.**

  AI processing may read file content and write derived information to the application's
  metadata store, but it must not initiate physical file operations.

---

## Overview

Current high-level runtime flow:

```text
React Client
    |
    | HTTP / JSON
    v
FileManager.Api
    |
    v
FileManager.Application
    |
    v
FileManager.Core
    ^
    |
FileManager.Infrastructure
    |
    +---- SQLite
    |
    +---- Physical Filesystem
````

The React client does not access SQLite or the physical filesystem directly.

The API and CLI are separate hosts over the same Application and Infrastructure layers.

---

## Purpose

Build a modular file management application over the native filesystem, providing a virtual
organization layer that lets users organize files logically without altering their physical location.

---

## Layering

### `FileManager.Core`

Contains the application's domain model, abstractions, and core business behavior.

Responsibilities include:

* Domain entities
* Repository contracts
* Filesystem/indexing abstractions
* Indexing services
* File hashing and metadata creation
* Duplicate detection
* Filesystem change-processing pipeline
* Core business rules

Examples of current domain concepts include:

* `FileEntry`
* `IndexedRoot`
* `VirtualFolder`
* `VirtualFolderFile`

Rules:

* Must not depend on `FileManager.Infrastructure`
* Must not depend on EF Core
* Must not depend on the API
* Must not depend on the React client
* Must not contain presentation logic

---

### `FileManager.Infrastructure`

Contains persistence and external-system adapters.

Responsibilities include:

* SQLite metadata persistence
* EF Core
* `FileManagerDbContext`
* Repository implementations
* Filesystem enumeration
* Filesystem change monitoring
* Infrastructure dependency injection registration

Current infrastructure includes implementations for:

* File persistence
* Indexed-root persistence
* Virtual-folder persistence
* Virtual-folder membership persistence
* Filesystem indexing sources
* Filesystem monitoring

`AddFileManagerInfrastructure()` registers concrete infrastructure implementations for the
abstractions owned by Core.

Infrastructure code that accesses source files is read-only.

---

### `FileManager.Application`

Contains use-case orchestration and the contracts consumed by application hosts.

The Application layer provides the main boundary between presentation code and
Core/Infrastructure behavior.

Current registered services include:

* `IIndexingAppService`
* `IDuplicateAppService`
* `IDashboardAppService`
* `ISearchAppService`
* `IIndexedLocationAppService`
* `IVirtualFolderAppService`
* `IIndexStatusService`
* `IIndexingOrchestrationAppService`

Responsibilities:

* Orchestrate application use cases
* Coordinate Core abstractions
* Perform use-case validation where appropriate
* Translate domain data into DTOs
* Expose stable contracts to API and CLI callers

Rules:

* Must not expose EF Core or `DbContext` to callers
* Must not contain UI rendering logic
* Must not contain HTTP-specific behavior
* Must not bypass Core abstractions for persistence or filesystem access

Application DTOs form the contract exposed to presentation layers.

---

### `FileManager.Api`

`FileManager.Api` is the HTTP host used by the React client.

Responsibilities:

* Application startup
* Dependency injection composition
* HTTP endpoint definitions
* Request/response handling
* HTTP status-code translation
* Database migration at startup
* Local-development CORS configuration

It registers:

```csharp
AddFileManagerInfrastructure();
AddFileManagerApplication();
```

The API does not contain core business logic.

Business rules belong in Core or Application.

---

### React Client

The current client is located under:

```text
client/file-manager-app
```

It is a React/Vite application.

Responsibilities:

* User interaction
* Navigation
* Explorer-style presentation
* Calling `FileManager.Api`
* Rendering API DTOs
* Loading, empty, and error states

The client must not:

* Access SQLite directly
* Implement persistence
* Access the physical filesystem as a replacement for the backend
* Duplicate backend business rules

The client communicates with the backend through HTTP.

---

### `FileManager.Cli`

`FileManager.Cli` is a secondary application host.

It remains useful for:

* Development
* Diagnostics
* Engine operations
* Command-line workflows

The CLI is a composition root, but it is no longer the only application host.

It wires:

```csharp
AddFileManagerInfrastructure();
AddFileManagerApplication();
```

and resolves commands through dependency injection.

CLI commands consume Application services rather than constructing Infrastructure or Core
implementations directly.

The CLI contains no business logic of its own.

---

## Application Layer Responsibility

The Application layer is the clean boundary used by application hosts.

Current presentation/runtime paths include:

```text
React
  -> FileManager.Api
  -> FileManager.Application

CLI
  -> FileManager.Application
```

Rules:

* Application may depend on Core contracts and domain types as required.
* Presentation layers should consume DTOs rather than persistence entities.
* Application orchestration must not depend directly on EF Core or SQLite.
* New user-facing features should normally be exposed through an Application service and DTO contract.
* Infrastructure-specific implementation details remain behind Core abstractions.

---

## Dependency Direction

The intended dependency direction is:

```text
FileManager.Api -----------+
                           |
FileManager.Cli -----------+
                           v
                FileManager.Application
                           |
                           v
                    FileManager.Core
                           ^
                           |
               FileManager.Infrastructure
```

Infrastructure implements contracts owned by Core.

Core does not depend on Infrastructure.

The React client is outside the .NET dependency graph and communicates with
`FileManager.Api` over HTTP.

---

## Index Source Abstraction

### `IIndexSource` Responsibilities

`IIndexSource` is the contract responsible for supplying candidate file paths to the indexing pipeline.

It exposes:

```csharp
IEnumerable<string> EnumeratePaths(
    string rootPath,
    CancellationToken cancellationToken = default);
```

Responsibilities:

* Enumerate every file path that should be considered for indexing under `rootPath`
* Support cooperative cancellation
* Return paths only
* Avoid persistence responsibilities
* Never modify, move, rename, or delete files

---

### `FileSystemIndexSource`

`FileSystemIndexSource` is the current filesystem-based implementation.

It performs recursive filesystem enumeration and yields paths to the indexing pipeline.

The rest of the indexing pipeline does not need to know how paths were discovered.

---

### Why `InitialIndexingService` Depends on `IIndexSource`

`InitialIndexingService` receives `IIndexSource` through dependency injection.

It therefore does not need to depend directly on:

* `Directory`
* a specific scanning algorithm
* NTFS-specific implementation details

The indexing pipeline can evolve without rewriting the higher-level indexing orchestration.

---

### Future Index Sources

A future optimized source, such as an NTFS/MFT-based implementation, can implement
`IIndexSource`.

For example:

```csharp
services.AddScoped<IIndexSource, MftIndexSource>();
```

The objective is that changing the enumeration strategy should not require changes to:

* `InitialIndexingService`
* `FileEntryFactory`
* repository contracts
* Application services
* API callers
* CLI callers

---

## Indexing Flow

Conceptually:

```text
Indexed Root
    |
    v
IIndexSource
    |
    v
InitialIndexingService
    |
    v
FileEntryFactory
    |
    v
IFileRepository
    |
    v
SQLite
```

`FileEntryFactory` centralizes creation of indexed file metadata.

Hash reuse is supported when existing metadata proves the source file has not changed.

---

## Incremental Filesystem Monitoring

Filesystem monitoring follows a separate flow from initial indexing.

Conceptually:

```text
Filesystem event
    |
    v
LocalFileWatcher
    |
    v
FileChangeQueue
    |
    v
FileChangeWorker
    |
    v
IndexingService
    |
    v
IFileRepository
```

The watcher observes changes made externally by the user or operating system.

It does not initiate physical file changes.

---

## Virtual Folder Architecture

Virtual folders belong entirely to the application's metadata layer.

A virtual folder does not correspond to a physical directory.

`VirtualFolder` supports hierarchy through a nullable parent reference.

Conceptually:

```text
VirtualFolder
    |
    +---- child VirtualFolder
    |
    +---- VirtualFolderFile
              |
              +---- FileEntry
```

`VirtualFolderFile` represents the association between a virtual folder and an indexed file.

This creates a many-to-many relationship:

```text
VirtualFolder N <----> N FileEntry
```

As a result, one physical file can appear in multiple virtual folders without being copied or moved.

Operations such as:

* creating a virtual folder
* renaming a virtual folder
* changing its parent
* deleting a virtual folder
* adding file membership
* removing file membership

modify application metadata only.

They must not modify the physical file.

---

## Physical vs Virtual Ownership

The distinction between physical and application-owned data is fundamental.

### Physical

Owned by the operating system / user filesystem:

* Files
* Physical directories
* File contents
* Physical paths

FileManager indexes this information but does not own it.

### Application-owned

Stored in the application database:

* Indexed metadata
* Indexed roots
* Virtual folders
* Virtual-folder memberships
* Future tags
* Future collections
* Derived metadata

Application-owned data may be changed without changing the physical file.

---

## Module Boundaries

| Module                | Responsibility                                                   |
| --------------------- | ---------------------------------------------------------------- |
| Index Engine          | Reliable metadata synchronization with the filesystem            |
| Metadata Store        | Persistent storage for file properties and hashes                |
| Indexed Locations     | Tracking user-selected indexing roots                            |
| Search                | Querying indexed metadata                                        |
| Duplicate Detection   | Identifying duplicate files; user decides on any physical action |
| Virtual Folder Engine | Logical organization independent of physical structure           |
| Tag Engine            | User-applied and future generated labels                         |
| Collection Engine     | Named logical groupings                                          |
| Thumbnail Cache       | Background preview generation                                    |
| AI Enrichment         | Future optional metadata enrichment                              |

Search is an enabling capability rather than the defining product goal.

---

## Composition Roots

There are currently two .NET composition roots.

### `FileManager.Api`

Primary HTTP host for the React client.

### `FileManager.Cli`

Command-line host for development and engine workflows.

Both reuse the same Application and Infrastructure registration methods.

This keeps business behavior consistent between hosts.

---

## Architectural Rules

* Keep `Core` independent from `Infrastructure`.
* Keep domain logic independent from presentation layers.
* Keep indexing implementation replaceable through abstractions such as `IIndexSource`.
* Separate initial indexing from incremental filesystem monitoring.
* Treat `FileSystemWatcher` as an implementation detail.
* Treat `FileSystemIndexSource` as an implementation detail.
* Keep EF Core and SQLite concerns in Infrastructure.
* Keep HTTP concerns in `FileManager.Api`.
* Keep UI concerns in the React client.
* Expose user-facing use cases through Application services.
* Never let virtual organization operations alter physical files.
* Do not automatically perform destructive filesystem operations.

---

## Architectural Direction

The architecture is designed so that organization features can evolve without changing the
fundamental ownership model.

The current virtual organization direction is:

```text
Physical files
      |
      v
Indexed FileEntry
      |
      +---- Virtual Folder A
      |
      +---- Virtual Folder B
      |
      +---- Future Tags / Collections
```

The physical file remains in one physical location while the application may expose it through
multiple logical views.

Future capabilities such as tags, collections, timeline browsing, thumbnail generation,
and AI enrichment should extend the metadata layer rather than weaken this separation.


