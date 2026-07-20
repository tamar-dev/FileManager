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

# Git workflow rules:

## Do not create commits automatically.

Before any git commit:
1. Show a summary of changed files.
2. Show the proposed commit message.
3. Wait for my explicit approval.

Only commit after I confirm.

You may:
- modify files
- run builds
- run tests
- report git status

But do not:
- git add
- git commit
- create branches
- merge changes

without explicit approval.