# Architecture

High level:

Filesystem
    |
    v
Index Engine
    |
    +-- Initial Indexer
    +-- Change Monitor
    +-- Metadata Store
    +-- Hash Worker
    +-- Virtual Folder Engine


Projects:

FileManager.Core
- Entities
- Interfaces
- Business rules

FileManager.Infrastructure
- SQLite
- Filesystem implementations
- External APIs

FileManager.Cli
- Application startup only


Rules:
- Do not put business logic in CLI.
- Keep components replaceable.
- Prefer dependency injection.
