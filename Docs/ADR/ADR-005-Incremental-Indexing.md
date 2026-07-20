# ADR-005: Incremental Indexing & Hash Recalculation Avoidance

Status: Proposed

## Context

`FileEntryFactory.Create` currently computes a SHA-256 hash (`FileHasher.Calculate`) for **every**
file it processes, unconditionally:

- Full initial scans (`FileScanner.Scan` ? `InitialIndexingService`/`IndexingAppService`) hash every
  file in the target directory, even if it was indexed in a previous run and never changed on disk.
- The watcher pipeline (`LocalFileWatcher` ? `FileChangeQueue` ? `FileChangeWorker` ? `IndexingService`)
  re-hashes a file on every `Changed` notification, including notifications that do not represent a
  real content change (e.g. metadata-only writes, or duplicate OS notifications for a single save).

Hashing is the most expensive part of indexing (full file read + SHA-256), and re-running it
unconditionally makes both full re-scans and change monitoring scale poorly with library size.

## Decision

Introduce **incremental indexing**: skip hash recalculation when a file's cheap-to-read metadata
(size + last-write-time) matches what is already persisted for that path.

1. Before creating/hashing a `FileEntry`, look up the existing persisted entry for the same
   `FullPath` (already available via `IFileRepository`).
2. If an existing entry is found and `Size` and `LastModified` are unchanged, reuse the existing
   `Hash` and skip `FileHasher.Calculate` entirely.
3. Only recompute the hash when:
   - No existing entry exists (new file), or
   - `Size` or `LastModified` differ from the persisted entry (content likely changed).
4. This check happens in `Core` (business rule), not `Infrastructure` — the repository merely
   exposes a lookup; the decision to skip hashing is domain logic that must remain replaceable.

## Consequences

- Full re-scans of an already-indexed, unchanged directory become dramatically cheaper — no file
  content is read, only metadata comparisons against the index (the source of truth).
- The watcher pipeline stops re-hashing on spurious/duplicate change notifications when the file's
  metadata is unchanged.
- Requires `IFileRepository` to expose an efficient single-file lookup (`GetByPathAsync` or similar)
  in addition to `GetAllAsync`, so the initial-scan path doesn't need to hold the entire index in
  memory to compare against.
- `FileEntryFactory` (or a new orchestrating service) needs access to "the previously known state"
  for a path — this must be injected as a dependency (interface), not hardcoded to EF Core, keeping
  `Core` independent from `Infrastructure`.
- Metadata precision matters: `LastModified` must be compared with a resolution that matches what
  the filesystem actually reports (avoid false negatives from serialization/precision mismatches
  when round-tripping through SQLite).
- This is purely an optimization of *when* to hash — it does not change the schema, the `FileEntry`
  contract, or any Application-layer DTOs.

## Out of Scope (future ADRs)

- Content-defined chunking / partial hashing for very large files.
- Renamed-file detection via hash reuse (moving a file currently looks like delete+create).
- Persisting a separate lightweight "fast index" structure for size/mtime lookups instead of
  querying the full `FileEntry` table.
