# FileManager Project Rules

## Goal
Build a local file management engine, not a search engine.

The physical filesystem is the source of truth for physical files.
The application database is the source of truth only for metadata and virtual organization.
The index reflects the filesystem; it must never change physical files automatically.

## Architecture

Projects:
- Core: business logic and abstractions
- Infrastructure: database and OS integrations
- Application: use cases and orchestration
- API: HTTP host and endpoints
- CLI: command-line host and startup only
- React client: presentation and API integration

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

## GitHub Issues and Pull Requests

When creating, triaging, refining, or closing issues, follow `.github/skills/issue-management/SKILL.md`. Search for duplicates, verify current implementation, and define measurable acceptance criteria. Never close an issue solely because code was generated. Follow `.github/PULL_REQUEST_TEMPLATE.md` for reviews.

## Safety

Indexing, monitoring, hashing, duplicate detection and enrichment are read-only on physical user files. Any future file-changing action must be explicit, user-initiated and confirmed. Safety changes require regression tests.

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