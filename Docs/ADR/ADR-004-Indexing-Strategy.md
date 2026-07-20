# ADR-004: Indexing Strategy

Status: Accepted

## Context
The system must provide fast and reliable metadata synchronization for file-management features.
Search is a secondary consumer of metadata, not the primary system purpose.

## Decision
Adopt an abstraction-based Index Engine with explicit separation of:
- Initial indexing
- Change monitoring
- Metadata persistence

The implementation must remain replaceable (current and future Windows-optimized strategies).

## Consequences
- Business logic remains stable while indexing implementations evolve.
- `FileSystemWatcher` can be used now, but is not a mandatory dependency.
- Performance improvements can be introduced without Core architectural changes
