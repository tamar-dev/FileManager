# FileManager — Roadmap

## Principle

FileManager is a local-first file management application.

Its goal is to help users rediscover, organize, and manage their files without requiring them to
change the physical folder structure on disk.

The roadmap reflects the priorities defined in the [PRD](PRD.md):

1. Reliable indexing engine
2. Rich metadata extraction
3. Timeline-based browsing
4. Virtual folders
5. Collections and albums
6. Duplicate detection and management
7. Thumbnail generation
8. AI enrichment

**Search is an enabling capability, not the product goal.**

Fast and accurate metadata search supports browsing, organization, and discovery, but the defining
product value is the virtual organization layer built around the user's existing filesystem.

**No roadmap item may introduce automatic modification of physical user files.**

Any future operation that affects files on disk must remain explicit, user-initiated, and clearly confirmed.

---

## Current Implementation Snapshot

The following capabilities are already present in the current architecture:

- Local SQLite metadata persistence
- Initial indexing
- Incremental filesystem monitoring
- File metadata and SHA-256 hashing
- Hash reuse for unchanged files
- Search over indexed metadata
- Duplicate detection and reporting
- Dashboard/application summary data
- Indexed locations
- Index orchestration/status services
- Virtual-folder persistence
- Nested virtual folders
- Virtual-folder file membership
- React client
- HTTP API between React and the .NET application

The current implementation focus is completing the persisted Virtual Explorer experience in the React client.

This snapshot is intentionally high-level.

Detailed implementation state should be determined from the codebase, tests, and active issues rather than
using this roadmap as a task tracker.

---

## Phase 1 — Foundation

**Goal:** a stable, reliable local metadata index that tracks the filesystem.

Key capabilities:

- Reliable initial indexing
- Clear separation between initial indexing and incremental monitoring
- Stable metadata schema
- SQLite persistence
- Filesystem synchronization for:
  - created files
  - modified files
  - deleted files
  - renamed or moved files
- Hash-based file identity and duplicate-detection groundwork
- Replaceable indexing source abstraction
- Indexed-root persistence

The filesystem remains the source of truth for physical files.

---

## Phase 2 — Virtual Organization

**Goal:** allow users to organize files logically without moving them physically.

Key capabilities:

- Nested virtual folders
- One physical file in multiple logical locations
- Persistent virtual-folder membership
- Explorer-like browsing
- Physical-location visibility alongside virtual organization
- Safe metadata-only create, rename, move, and delete operations
- Virtual-folder membership management

This phase is central to the product's differentiation.

A virtual folder must never be treated as a physical directory.

---

## Phase 3 — Browsing and Discovery

**Goal:** make indexed files easy to rediscover through multiple meaningful views.

Planned capabilities:

- Timeline-based browsing
- Date-oriented navigation
- Recent files
- Favorites
- Metadata-driven filtering
- Improved navigation around virtual organization
- Richer file properties and contextual views

Search continues to support these experiences rather than replace them.

---

## Phase 4 — Collections and Media Experience

**Goal:** provide richer ways to organize and browse visual and curated content.

Planned capabilities:

- Collections
- Albums
- Photo-oriented views
- Thumbnail generation
- Thumbnail cache
- Image metadata improvements
- Capture-date and media metadata support

All generated previews and metadata remain application-owned.

Source files remain unchanged.

---

## Phase 5 — Performance and Operational Maturity

**Goal:** improve scalability, indexing performance, resilience, and maintainability.

Potential work includes:

- Faster Windows-specific indexing sources
- NTFS/MFT-based enumeration behind `IIndexSource`
- Incremental indexing optimizations
- Background work coordination
- Hashing optimizations
- Query performance improvements
- Cache strategy
- Operational diagnostics
- Recovery from interrupted indexing
- Better handling of large file libraries

Performance improvements must preserve existing architectural boundaries.

---

## Phase 6 — Advanced Organization

**Goal:** extend the virtual organization model beyond folders.

Potential capabilities:

- Tags
- File-tag relationships
- Curated collections
- Rule-based collections
- Smart collections
- Saved metadata filters

These features should follow the same model as virtual folders:

```text
FileEntry
    |
    +---- application-owned metadata
````

They must not require moving or duplicating physical files.

---

## Phase 7 — AI Enrichment

**Goal:** add an optional intelligent metadata layer on top of the existing index.

Potential capabilities:

* Automatic categorization
* OCR
* Semantic search
* Content-based labels
* Image understanding
* Smart collections based on derived metadata

AI enrichment must remain:

* optional
* additive
* explainable where practical
* user-controlled
* read-only with respect to physical files

AI may write derived metadata only to FileManager-owned storage.

---

## Product Priority Order

When choosing between competing features, prefer work that strengthens:

```text
1. Reliability
2. Data safety
3. Virtual organization
4. Browsing and rediscovery
5. Performance
6. Rich metadata
7. Advanced automation
8. AI enrichment
```

A feature that weakens filesystem safety or makes the architecture harder to maintain should not be accepted
simply because it appears useful.

---

## What the Roadmap Is Not

This document is not:

* a sprint board
* an issue tracker
* a release checklist
* a source of exact implementation status

Those belong in GitHub Issues, project boards, and code/tests.

The roadmap defines product and architectural direction.

---

## Long-Term Vision

The long-term goal is an intelligent personal file library that sits above the native filesystem.

Users should be able to understand the same physical file through multiple logical views:

```text
Physical File
    |
    v
Indexed Metadata
    |
    +---- Virtual Folders
    +---- Timeline
    +---- Collections
    +---- Tags
    +---- Search
    +---- Duplicate Analysis
    +---- Thumbnails
    +---- Future AI Metadata
```

The filesystem remains intact throughout.

FileManager adds organization and understanding without taking control away from the user.
