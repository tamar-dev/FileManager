# FileManager — Performance Strategy

> **Constraint:** All performance optimizations described here are read-only operations on the
> physical filesystem. Background workers read file content; they never write to, move, rename,
> or delete any physical file.

---

## Separate Storage from Access Speed

**Persistent storage** (source of truth for metadata):
- SQLite
- Reliable, durable metadata store

**Fast access layer** (optimized for frequent queries):
- In-memory index
- Optimized structures for browsing, search, and duplicate detection

---

## Background Processing

Heavy operations must not block the indexing pipeline or the UI. They run in the background and
interact with the filesystem in a **read-only** manner.

```
File discovery (IIndexSource)          [read-only: enumerate paths]
    |
    v
Metadata saved to index
    |
    v
Background workers:
    - Hash calculation                 [read-only: read file content, write hash to index]
    - Thumbnail generation             [read-only: read file content, write thumbnail to cache]
    - AI enrichment (future)           [read-only: read file content, write metadata to index]
```

Background workers write only to the application's own stores (index database, thumbnail cache,
metadata store). They never write to physical files.

---

## Incremental Updates

Never rebuild the whole index after every change. Only process affected files.

See [ADR-005](ADR/ADR-005-Incremental-Indexing.md) and
[ADR-006](decisions/ADR-006-FileEntryFactory-Hash-Reuse.md) for full rationale.

### Hash Recalculation Avoidance

SHA-256 hashing is the most expensive part of indexing (full file read). `FileEntryFactory`
avoids recomputing hashes when a file's cheap-to-read metadata (`Size`, `LastModified`) matches
what is already persisted for that path.

Decision logic (in `Core`, not `Infrastructure`):
1. Look up the existing `FileEntry` for the path via `IFileRepository`.
2. If `Size` and `LastModified` are unchanged ? reuse the existing hash; skip file read.
3. If metadata changed or no entry exists ? compute a new hash.

This applies to both full initial scans and watcher-driven updates.

---

## Future Performance Work

- Memory/cache access layer for frequent queries (browsing, search, virtual folder membership).
- Replaceable indexing sources (`IIndexSource`) for faster enumeration:
  - NTFS MFT-based reader (avoids recursive directory traversal on Windows).
  - USN Journal-based reader (incremental deltas without full re-scan).
- Incremental update optimizations to reduce per-event latency in the watcher pipeline.
- Operational diagnostics to surface indexing throughput and error rates.
