# ADR-001: Project Layer Separation

Status: Accepted

## Context

FileManager is a local desktop file management application. As the product grows to include virtual
folders, metadata extraction, duplicate detection, thumbnail generation, and AI enrichment, mixing
business rules with infrastructure or UI concerns would make the system brittle and hard to extend.

## Decision

Separate the solution into distinct layers with strict dependency rules:

- `FileManager.Core` — entities, interfaces, and business rules. No dependency on any
  infrastructure, persistence, or UI technology.
- `FileManager.Infrastructure` — SQLite persistence, filesystem adapters, OS integrations.
  Depends on `Core`; never referenced by `Application` or `CLI` directly (only through interfaces).
- `FileManager.Application` — use-case orchestration. Depends on `Core` only; never on
  `Infrastructure` or EF Core packages. All communication with front ends uses DTOs.
- `FileManager.Cli` — composition root and startup only. No business logic.
- Future `FileManager.UI` — follows the same rule as `Cli`: references only `Application`.

## Consequences

- Business logic in `Core` is testable without any infrastructure or UI dependency.
- Infrastructure implementations (database, filesystem, OS APIs) are replaceable without
  changing business rules.
- Any front end (CLI today, desktop UI later) connects to business logic exclusively through
  `Application` services and DTOs.
- The physical filesystem is accessed only inside `Infrastructure` adapters; no other layer
  touches the filesystem directly.
