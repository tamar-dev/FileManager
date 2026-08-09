# C2 - Container Diagram

## Purpose

Shows the high-level runtime containers of FileManager and how they collaborate.

FileManager is a local-first file management application built around a strict separation between:

- the user's physical filesystem
- FileManager's indexed metadata and virtual organization

The physical filesystem remains the source of truth for files.

SQLite is the source of truth for FileManager-owned metadata such as:

- indexed file metadata
- indexed roots
- virtual folders
- virtual-folder memberships
- future organization metadata

---

## Current Container Diagram

```mermaid
graph TB
    User([User])

    subgraph "FileManager"
        React["React Client<br/>(client/file-manager-app)<br/>UI and interaction"]
        Api["FileManager.Api<br/>.NET 9 Web API<br/>HTTP boundary / composition root"]
        Cli["FileManager.Cli<br/>.NET 9 Console<br/>Secondary host / diagnostics"]
        Application["FileManager.Application<br/>Use-case orchestration<br/>DTO contracts"]
        Core["FileManager.Core<br/>Domain model, abstractions,<br/>business rules"]
        Infrastructure["FileManager.Infrastructure<br/>EF Core, repositories,<br/>filesystem adapters"]
    end

    Database[("SQLite Database<br/>filemanager.db<br/>Metadata Store")]
    FileSystem[("Windows File System<br/>Physical Files")]

    User -->|"Uses application"| React
    User -->|"Runs CLI commands"| Cli

    React -->|"HTTP / JSON"| Api

    Api -->|"Calls application services"| Application
    Cli -->|"Calls application services"| Application

    Application -->|"Uses domain contracts and rules"| Core

    Infrastructure -.->|"Implements Core abstractions"| Core

    Infrastructure -->|"Reads/writes application metadata"| Database
    Infrastructure -->|"Reads files, metadata, and events"| FileSystem

    style React fill:#1168bd,stroke:#0b4884,color:#ffffff
    style Api fill:#1168bd,stroke:#0b4884,color:#ffffff
    style Cli fill:#438dd5,stroke:#2e6295,color:#ffffff
    style Application fill:#1168bd,stroke:#0b4884,color:#ffffff
    style Core fill:#1168bd,stroke:#0b4884,color:#ffffff
    style Infrastructure fill:#438dd5,stroke:#2e6295,color:#ffffff
    style Database fill:#999999,stroke:#6b6b6b,color:#ffffff
    style FileSystem fill:#999999,stroke:#6b6b6b,color:#ffffff
    style User fill:#08427b,stroke:#052e56,color:#ffffff
````

> Dashed arrows (`-.->`) represent dependency inversion.
>
> Infrastructure implements abstractions owned by Core.
> Core does not depend on Infrastructure.

---

## Containers

### React Client

**Technology:** React + Vite

**Location:**

```text
client/file-manager-app
```

**Responsibilities:**

* User interface
* Navigation
* Explorer-style interaction
* Calling the backend API
* Rendering DTOs returned by the backend
* Loading, error, and empty states

**Rules:**

* Does not access SQLite directly
* Does not access Core or Infrastructure directly
* Does not own persistence
* Does not reimplement backend domain rules
* Does not modify the physical filesystem directly

---

### FileManager.Api

**Technology:** .NET 9 Web API

**Responsibilities:**

* HTTP boundary for the React client
* Dependency injection composition
* Endpoint definitions
* Request/response handling
* HTTP status-code mapping
* Database migration on startup
* Local-development CORS configuration

**Rules:**

* Contains no core business logic
* Delegates use cases to `FileManager.Application`
* Does not access EF Core repositories directly when an Application service exists
* Does not expose persistence implementation details

---

### FileManager.Cli

**Technology:** .NET 9 Console Application

**Responsibilities:**

* Development workflows
* Diagnostics
* Engine operations
* Command routing
* Secondary composition root

Examples include commands such as:

```text
index
watch
duplicates
dashboard
search
```

**Rules:**

* No business logic
* Uses Application services
* Does not duplicate domain behavior already implemented elsewhere

---

### FileManager.Application

**Technology:** .NET 9 Class Library

**Responsibilities:**

* Use-case orchestration
* DTO contracts
* Application-level validation
* Coordinating domain abstractions
* Exposing stable operations to API and CLI

Current registered application services include:

```text
IIndexingAppService
IDuplicateAppService
IDashboardAppService
ISearchAppService
IIndexedLocationAppService
IVirtualFolderAppService
IIndexStatusService
IIndexingOrchestrationAppService
```

**Rules:**

* No UI rendering logic
* No HTTP-specific behavior
* No direct EF Core concerns
* Does not expose `DbContext`
* Presentation layers consume DTOs rather than persistence implementation details

---

### FileManager.Core

**Technology:** .NET 9 Class Library

**Responsibilities:**

* Domain entities
* Core interfaces
* Business rules
* Indexing contracts
* File metadata creation
* Hashing
* Duplicate detection
* Filesystem change-processing logic
* Virtual-folder domain contracts

Current domain concepts include:

```text
FileEntry
IndexedRoot
VirtualFolder
VirtualFolderFile
```

**Rules:**

* No dependency on Infrastructure
* No dependency on API
* No dependency on React
* No EF Core dependency
* Owns abstractions implemented by Infrastructure

---

### FileManager.Infrastructure

**Technology:** .NET 9 + EF Core

**Responsibilities:**

* SQLite persistence
* `FileManagerDbContext`
* Repository implementations
* Filesystem enumeration
* Filesystem event monitoring
* Infrastructure dependency injection registration
* Local database path management

Examples:

```text
FileRepository
IndexedRootRepository
VirtualFolderRepository
VirtualFolderFileRepository
FileSystemIndexSource
LocalFileWatcher
```

**Rules:**

* Implements contracts defined by Core
* Filesystem access is read-only for normal engine/background operations
* Writes only to FileManager-owned persistence unless a future explicit physical-file operation is intentionally introduced

---

### SQLite Database

**Responsibilities:**

Stores FileManager-owned state such as:

* `FileEntry`
* `IndexedRoot`
* `VirtualFolder`
* `VirtualFolderFile`
* future tags
* future collections
* future derived metadata

SQLite is the source of truth for FileManager-owned virtual organization and indexed metadata.

It is not the source of truth for the existence or contents of physical files.

---

### Windows File System

**Responsibilities:**

* Physical file storage
* Physical directory structure
* File contents
* File timestamps and other filesystem metadata
* Filesystem change events

The Windows filesystem is the source of truth for physical files.

Normal FileManager engine flows may read:

```text
paths
metadata
file contents for hashing
filesystem change events
```

They must not automatically:

```text
delete
move
rename
overwrite
modify contents
```

---

## Runtime Flows

### React request

```text
User
  -> React Client
  -> FileManager.Api
  -> FileManager.Application
  -> Core abstraction
  -> Infrastructure implementation
  -> SQLite / Filesystem
```

---

### CLI request

```text
User
  -> FileManager.Cli
  -> FileManager.Application
  -> Core abstraction
  -> Infrastructure implementation
```

---

## Virtual Folder Flow

```text
React Client
  -> FileManager.Api
  -> IVirtualFolderAppService
  -> Core repository contract
  -> Infrastructure repository
  -> SQLite
```

Virtual-folder operations affect application metadata only.

They do not create or modify physical directories.

---

## Dependency Rule

The intended dependency direction is:

```text
React
  |
  v
Api ----+
        |
Cli ----+
        v
Application
    |
    v
Core
    ^
    |
Infrastructure
```

Important:

* Core owns abstractions.
* Infrastructure implements abstractions.
* Application orchestrates use cases.
* API exposes HTTP contracts.
* CLI exposes command-line workflows.
* React provides the user experience.

No presentation layer should bypass Application and directly access persistence logic.

---

## Architectural Boundary

The most important container-level rule is:

```text
Physical Filesystem
      |
      | read / observe
      v
FileManager Engine
      |
      | writes metadata only
      v
SQLite
```

The virtual organization layer is built around physical files without taking ownership of them.
