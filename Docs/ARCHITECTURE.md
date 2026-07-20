	# Architecture
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
  - `AddFileManagerInfrastructure()` DI registration (DbContext, `IFileRepository`, `IFileScanner`)
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
- Keep indexing implementation replaceable.
- Separate:
  - Initial indexing
  - Change monitoring
  - Metadata persistence
- `FileSystemWatcher` is an implementation detail, not an architectural dependency.
- All front ends consume business logic exclusively through `FileManager.Application` services.
