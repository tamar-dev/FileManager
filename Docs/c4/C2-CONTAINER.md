# C2 - Container Diagram

## Purpose
Shows the high-level building blocks of the FileManager metadata engine and how they collaborate to keep a persistent metadata index synchronized with the physical file system.

FileManager is a **metadata-driven file management engine**, not a search engine.
Search, if introduced later, would be one additional consumer of the metadata store — not a core container.

## Diagram (Current Implementation)

```mermaid
graph TB
    User([User])

    subgraph "FileManager Engine"
        CLI["CLI Host<br/>(FileManager.Cli)<br/>Composition root & command entry point"]
        Core["Metadata Engine<br/>(FileManager.Core)<br/>Business rules, orchestration,<br/>defines abstractions"]
        Persistence["Persistence Adapter<br/>(FileManager.Infrastructure)<br/>Implements IFileRepository"]
        Watcher["File System Watcher Adapter<br/>(FileManager.Infrastructure)<br/>Implements IFileWatcher"]
    end

    Database[("SQLite Database<br/>filemanager.db<br/>Metadata Store")]
    FileSystem[("Windows File System<br/>Physical Files")]

    User -->|"Runs commands (e.g. watch)"| CLI
    CLI -->|"Starts & wires dependencies"| Core
    CLI -->|"Injects concrete implementations"| Persistence
    CLI -->|"Injects concrete implementations"| Watcher

    Persistence -.->|"implements"| Core
    Watcher -.->|"implements"| Core

    Persistence -->|"Reads/writes metadata"| Database
    Watcher -->|"Reads file events & metadata"| FileSystem

    style CLI fill:#1168bd,stroke:#0b4884,color:#ffffff
    style Core fill:#1168bd,stroke:#0b4884,color:#ffffff
    style Persistence fill:#438dd5,stroke:#2e6295,color:#ffffff
    style Watcher fill:#438dd5,stroke:#2e6295,color:#ffffff
    style Database fill:#999999,stroke:#6b6b6b,color:#ffffff
    style FileSystem fill:#999999,stroke:#6b6b6b,color:#ffffff
    style User fill:#08427b,stroke:#052e56,color:#ffffff
```

> Dashed arrows (`-.->`) represent **dependency inversion**: Infrastructure adapters implement abstractions owned by Core. Core has no outgoing dependency on Infrastructure.

## Containers

### CLI Host (FileManager.Cli)
**Technology:** .NET 6 Console Application

**Responsibilities:**
- Composition root: constructs and wires concrete Infrastructure implementations into Core abstractions
- Command routing (e.g., `watch`)

**Rules:**
- No business logic
- Only depends on Core abstractions and Infrastructure implementations for wiring

### Metadata Engine (FileManager.Core)
**Technology:** .NET 6 Class Library

**Responsibilities:**
- Business entities (`FileEntry`)
- Orchestration services (`IndexingService`, `FileChangeWorker`, `FileHasher`, `DuplicateDetector`)
- Abstractions consumed by the engine (`IFileRepository`, `IFileWatcher`, `IFileScanner`, `IFileIndexer`)
- Domain events (`FileChangeEvent`, `FileChangeQueue`)

**Rules:**
- Has zero dependency on Infrastructure
- Depends only on its own abstractions
- Contains all business/orchestration logic

### Persistence Adapter (FileManager.Infrastructure)
**Technology:** .NET 6 / EF Core

**Responsibilities:**
- Implements `IFileRepository`
- Persists and retrieves file metadata (`FileManagerDbContext`, `FileRepository`)

### File System Watcher Adapter (FileManager.Infrastructure)
**Technology:** .NET 6 / `System.IO.FileSystemWatcher`

**Responsibilities:**
- Implements `IFileWatcher`
- Detects file system changes and forwards them to Core (`LocalFileWatcher`)

### SQLite Database
**Responsibilities:**
- Persistent metadata store
- Source of truth for logical organization and application metadata

### Windows File System
**Responsibilities:**
- Physical file storage
- Source of truth for physical files

## Current Indexing Flow

1. User runs `watch <path>` via the CLI Host.
2. CLI Host composes `FileChangeQueue`, `IndexingService`, `FileChangeWorker`, and the Watcher Adapter, injecting them behind Core's abstractions.
3. Watcher Adapter detects file system events and enqueues them.
4. `FileChangeWorker` (Core) consumes the queue and invokes `IndexingService`.
5. `IndexingService` (Core) updates metadata through the `IFileRepository` abstraction.
6. Persistence Adapter fulfills `IFileRepository` and writes to SQLite.

## Target Architecture (Not Yet Implemented)

The diagram above intentionally excludes containers that are **planned but not built**, per `ARCHITECTURE.md` module boundaries:

- Virtual Folder Engine
- Tag Engine
- Collection Engine
- Query Engine
- Thumbnail Cache
- Alternative high-performance Index Engine implementations (e.g. NTFS-native)

These remain future modules behind Core abstractions and are documented in `ARCHITECTURE.md`, not in this container diagram, to avoid describing unbuilt functionality as current architecture.

---

## Architectural Improvements Made

1. **Fixed dependency direction violating Clean Architecture.** The previous diagram showed `Core --> Infrastructure` ("Defines Interfaces") and a duplicate contradictory arrow. Corrected to `Infrastructure -.-> Core` ("implements"), since Infrastructure depends on Core, never the reverse.
2. **Elevated abstraction level above the Visual Studio solution structure.** Instead of one box per `.csproj`, Infrastructure is split into two containers by responsibility — Persistence Adapter and File System Watcher Adapter — since they serve distinct purposes and have independent external dependencies (SQLite vs. OS file events).
3. **Renamed "Core" to "Metadata Engine"** in the diagram label to reinforce the project's real purpose (metadata-driven engine) rather than a generic layer name.
4. **Removed CLI → Infrastructure "business" framing.** Clarified that CLI's relationship to Infrastructure is purely compositional (DI wiring), not usage of business capability.
5. **Separated current vs. target architecture** into two explicit sections, keeping the Mermaid diagram limited to what is actually implemented today.
6. **Removed speculative containers** that were implied elsewhere in docs (Index Engine, Hash Worker as standalone containers) since they are currently implementation details inside the Metadata Engine, not independent deployable/architectural containers.

---

## Observations

These are concerns and potential future improvements. They do not require any code changes now.

- **`DuplicateDetector` and `FileHasher` currently live inside Core as plain services**, not behind abstractions. If duplicate detection or hashing grows in complexity (e.g., different hashing strategies, async background workers), consider introducing `IDuplicateDetector` / `IFileHasher` abstractions so alternative implementations (e.g., a background hash worker) can be swapped in without modifying orchestration logic.
- **No explicit "Index Engine" container exists yet.** `INDEX_ENGINE.md` and ADR-004 describe a target design with `InitialIndexer` and `ChangeMonitor` as distinct concepts. Today, `IndexingService` and `FileChangeWorker` conflate some of these responsibilities. Revisit once initial bulk-indexing is implemented, since it doesn't exist yet (`watch` only reacts to changes).
- **CLI directly constructs `DbContextOptionsBuilder` and concrete classes** (`WatchCommand.cs`), rather than using a DI container. This works at the current scale but will not scale cleanly as more commands/services are added. Consider introducing `Microsoft.Extensions.DependencyInjection` in the CLI composition root.
- **Search is not yet represented anywhere**, which is correct per project vision — but if/when introduced, it should be modeled as a consumer container reading from the Metadata Store, not as a new source of truth or a redesign of the Metadata Engine.
- **No container currently represents background/async processing** (e.g., future hash workers, thumbnail generation from `PERFORMANCE.md`). When these are implemented, they should appear as distinct containers depending on Core abstractions, not as Infrastructure-embedded logic.

