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
- `FileManager.Cli`
  - Composition root and startup only
  - No business logic

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
- Keep indexing implementation replaceable.
- Separate:
  - Initial indexing
  - Change monitoring
  - Metadata persistence
- `FileSystemWatcher` is an implementation detail, not an architectural dependency.
