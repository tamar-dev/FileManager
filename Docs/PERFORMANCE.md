# Performance Strategy

The target is an indexing architecture inspired by high performance desktop indexers.

Important concepts:

## Separate storage from access speed

Persistent storage:
- SQLite
- Reliable metadata storage

Fast access layer:
- Memory index
- Optimized structures for frequent operations


## Background Processing

Heavy operations should not block indexing:

File discovery
    |
    v
Metadata saved
    |
    v
Background workers:
- Hash calculation
- Thumbnail generation
- Content analysis


## Incremental Updates

Never rebuild the whole index after every change.
Only process affected files.

## Next Milestone: Incremental Indexing & Hash Recalculation Avoidance

See [ADR-005](ADR/ADR-005-Incremental-Indexing.md) for full rationale.

Goal: avoid recomputing a file's SHA-256 hash when its cheap-to-read metadata
(`Size`, `LastModified`) matches what is already persisted for that path.

Planned approach:
1. Add a single-file lookup to `IFileRepository` (e.g. `GetByPathAsync(string fullPath)`), so
   callers don't need to load the entire index to check one file.
2. Introduce a decision point (in `Core`, replacing/augmenting `FileEntryFactory.Create`) that:
   - Fetches the existing entry for the path, if any.
   - Skips `FileHasher.Calculate` and reuses the existing `Hash` when `Size` and `LastModified`
     are unchanged.
   - Recomputes the hash for new files or files whose metadata changed.
3. Apply this to both:
   - **Initial/full scans** (`FileScanner` / `InitialIndexingService` / `IndexingAppService`) —
     re-scanning an unchanged directory should touch metadata only, not file contents.
   - **Watcher-driven updates** (`FileChangeWorker` ? `IndexingService`) — `Changed` events that
     don't actually change size/mtime should not trigger a re-hash.
4. Measure before/after: full re-index time on a large, mostly-unchanged directory; per-event
   latency in the watcher pipeline.

Explicitly out of scope for this milestone: content-defined chunking, partial hashing for large
files, and rename detection — tracked separately.
