# FileManager Project Rules

## Goal
Build a local file management engine, not a search engine.

The index is the source of truth.
Filesystem is the data source.

## Architecture

Projects:
- Core: business logic and abstractions
- Infrastructure: database and OS integrations
- CLI: startup only

Rules:
- Do not put business logic in CLI.
- Keep Core independent from Infrastructure.
- Prefer interfaces for replaceable components.

## Indexing

Separate:
- Initial indexing
- Change monitoring
- Metadata persistence

FileSystemWatcher is only one implementation.

Design for future faster indexing mechanisms.

## Coding Style

- Small focused classes.
- Avoid duplicate logic.
- Preserve existing behavior when refactoring.