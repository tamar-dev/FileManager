# ADR-004 Indexing Strategy

Status: Accepted

## Context

The system requires fast and reliable file management.
Search performance is not the main goal, but a rich index is required for all features.

## Decision

Build an abstraction based Index Engine.

The engine separates:
- Initial indexing
- Change monitoring
- Metadata persistence

## Consequences

The current simple implementations can later be replaced by optimized Windows-specific implementations without changing business logic.
