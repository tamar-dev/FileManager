# ADR-006: FileEntryFactory metadata-based hash reuse

Status: Accepted

## Context
Incremental indexing should avoid unnecessary SHA-256 recalculation for files that have not changed.
The index is the source of truth, so existing `FileEntry` metadata can be used to decide whether hash computation is needed.

## Decision
`FileEntryFactory` now supports metadata-based hash reuse.
Before computing SHA-256, it retrieves an existing `FileEntry` via `IFileRepository` and compares:
- `Size`
- `LastModified`

If both values are unchanged, the existing hash is reused.
If metadata differs or no entry exists, a new hash is calculated.

`FileEntryFactory` now optionally depends on `IFileRepository` for this lookup.

## Rationale
This dependency is acceptable because:
- it is expressed through a Core interface (`IFileRepository`), not an Infrastructure implementation
- it keeps the optimization close to `FileEntry` creation, minimizing duplicated logic in indexing flows
- it preserves current architecture boundaries (Core remains independent of Infrastructure details)

## Future alternatives
If indexing orchestration grows more complex, hash reuse decisions can be moved out of `FileEntryFactory` into a dedicated indexing component (for example, a hash reuse policy or incremental indexing coordinator) that:
- loads existing metadata once per batch
- decides reuse centrally
- keeps `FileEntryFactory` focused on object construction only
